// Standard library includes
#include <cstdint>        // For fixed-width integer types
#include <cstdlib>        // For EXIT_SUCCESS/EXIT_FAILURE
#include <expected>       // For std::expected error handling
#include <iostream>       // For console I/O
#include <limits>         // For numeric limits
#include <memory>         // For smart pointers
#include <mutex>          // For thread synchronization
#include <queue>          // For connection queue
#include <semaphore>      // For counting semaphore
#include <span>           // For non-owning array views
#include <string>         // For string operations
#include <string_view>    // For non-owning string views
#include <thread>         // For multi-threading
#include <utility>        // For std::exchange, std::forward
#include <vector>         // For dynamic arrays

// Project-specific headers
#include "include/Connection.hpp"  // HTTP connection handler
#include "include/Logger.hpp"      // Logging utilities

// Boost.Asio for async I/O
#include <boost/asio.hpp>
#include <boost/asio/awaitable.hpp>          // For coroutine support
#include <boost/asio/co_spawn.hpp>           // For spawning coroutines
#include <boost/asio/detached.hpp>           // For fire-and-forget coroutines
#include <boost/beast.hpp>                   // For HTTP protocol support
#include <boost/system/detail/error_code.hpp> // For error code details
#include <boost/system/system_error.hpp>     // For system error exceptions
#include <boost/throw_exception.hpp>         // For exception throwing

// ************************* Comptime Configuration ************************* //
// C macros (which C++ is stuck with for now) can't express anything at the type
// system level directly. For proper type-safe comptime routine and class
// selections, we will define constexpr bools to thinly wrap macro presence,
// then use these in template specializations.
// ************************************************************************** //

namespace {
// Compile-time flag to disable logging (set via -Dnologs)
inline constexpr bool disable_logs{
#ifdef NOLOG
    true
#else
    false
#endif
};

// Compile-time flag to disable HTTP listener (set via -Dnolistener)
inline constexpr bool disable_listener{
#ifdef NOLISTENER
    true
#else
    false
#endif
};
} // namespace

// Namespace aliases for convenience
namespace asio = boost::asio;
using tcp = asio::ip::tcp;

namespace {
// Global flag to control server shutdown
bool keep_going{true};

/**
 * @brief Main DAS HTTP server function
 * @tparam Disable If true, this function does nothing (compile-time optimization)
 * @param log_path Path to the log file
 * @param das_url Complete DAS URL (e.g., http://localhost:4242/cppdas/)
 * 
 * Creates a multi-threaded HTTP server using Boost.Asio coroutines.
 * Uses a producer-consumer pattern with a thread pool.
 */
template <bool Disable>
[[maybe_unused]] void RunDAS(const std::string &&log_path, const std::string &&das_url) {  // Parse URL to extract port and path
  std::uint16_t port = 4242; // Default port
  std::string expected_path = "/"; // Default path
  
  auto port_pos = das_url.find_last_of(':');
  if (port_pos != std::string::npos) {
    // Find the end of the port number (before next '/' or end of string)
    auto slash_pos = das_url.find('/', port_pos);
    if (slash_pos != std::string::npos) {
      std::string port_str = das_url.substr(port_pos + 1, slash_pos - port_pos - 1);
      expected_path = das_url.substr(slash_pos); // Everything after the port
      try {
        port = static_cast<std::uint16_t>(std::stoi(port_str));
      } catch (...) {
        std::cerr << "Invalid port in URL, using default 4242" << std::endl;
      }
    }
  }
    // Queue of incoming client connections
  std::queue<std::unique_ptr<tcp::socket>> connections{};
  // Mutex to protect the connection queue
  std::mutex queue_lock{};
  // Semaphore to signal available connections to worker threads
  std::counting_semaphore<std::numeric_limits<std::uint8_t>::max()> queue_sema{
      0};
  // Logger instance (can be disabled at compile-time)
  Logger<disable_logs> logger{log_path};
  // Asio I/O context for async operations
  asio::io_context ioc{};
  
  // Consumer lambda: processes connections from the queue
  auto consume = [&connections, &queue_sema, &queue_lock, &logger, &ioc, &expected_path] {
    while (keep_going) {
      // Wait for a connection to be available
      queue_sema.acquire();

      // Lock the queue and check if there's work to do
      if (std::unique_lock<std::mutex> lock{queue_lock};
          keep_going && !connections.empty()) {

        try {
          // Spawn a coroutine to handle the HTTP connection asynchronously
          boost::asio::co_spawn(
              ioc,
              [&connections, &logger, &expected_path] -> boost::asio::awaitable<void> {
                // Create connection handler with the socket
                Connection connection{
                    std::forward<std::unique_ptr<tcp::socket>>(
                        std::exchange(connections.front(), nullptr)),
                    logger,
                    expected_path};
                connections.pop();
                // Handle HTTP request/response asynchronously
                co_await connection.TalkToClientAsync();
              },
              boost::asio::detached);  // Fire-and-forget
          ioc.run();
        } catch (const boost::wrapexcept<boost::system::system_error> &ex) {
          // Handle Boost-specific system errors
          std::cerr << "Boost error: " << ex.what() << "\n";
        } catch (std::exception &ex) {
          // Handle standard exceptions
          std::cerr << "General Exception: " << ex.what() << "\n";
        } catch (...) {
          // Catch-all for any other errors
          std::cerr << "Uncatchable connection error occurred" << "\n";
        }
      }
    }
  };

  // Create a thread pool with one thread per CPU core
  const auto consumer_count{std::thread::hardware_concurrency()};
  std::vector<std::jthread> consumers{};
  consumers.reserve(consumer_count);
  using consumer_count_t = std::remove_cv_t<decltype(consumer_count)>;
  for (consumer_count_t t{}; t < consumer_count; ++t) {
    consumers.emplace_back(consume);
  }

  // Create TCP acceptor to listen for incoming connections
  tcp::acceptor acceptor{ioc, {tcp::v4(), port}};

  logger << "Listening on http://localhost:" << port << expected_path << "\n";
  logger << "Expected path: " << expected_path << "\n";

  // Main accept loop (producer)
  while (keep_going) {
    // Create a new socket for the incoming connection
    std::unique_ptr<tcp::socket> socket{std::make_unique<tcp::socket>(ioc)};
    // Block until a client connects
    acceptor.accept(*socket);
    logger << "Made connection with " << socket->remote_endpoint() << "\n";
    // Add the connection to the queue (thread-safe)
    {
      std::unique_lock lock{queue_lock};
      connections.emplace(std::move(socket));
    }
    // Signal one consumer thread that work is available
    queue_sema.release();
  }

  // Wake up all consumer threads so they can exit gracefully
  for (consumer_count_t t{}; t < consumer_count; ++t) {
    queue_sema.release();
  }
}

/// <summary>
/// Specialization disabling (at compile time) running the listening
/// portion of the DAS.
/// </summary>
template <> [[maybe_unused]] void RunDAS<true>([[maybe_unused]] const std::string &&_, [[maybe_unused]] const std::string &&__) {}
} // namespace

namespace {

/**
 * @brief Command-line arguments structure
 * 
 * Holds parsed command-line arguments with defaults.
 * Move-only to enforce efficient passing.
 */
struct CommandLineArgs final {
  std::string_view das_log{"DasLog.log"};                   // Path to DAS log file
  std::string_view das_url{"http://localhost:4242/cppdas/"}; // DAS HTTP URL
  bool help{false};                                          // Help flag

  CommandLineArgs() = default;
  // Enforce copy elision - prevent accidental copies
  CommandLineArgs(const CommandLineArgs &) = delete;
  CommandLineArgs &operator=(const CommandLineArgs &) = delete;
  CommandLineArgs(CommandLineArgs &&) = default;
  CommandLineArgs &operator=(CommandLineArgs &&) = default;
  ~CommandLineArgs() = default;
};

// Error types for command-line parsing
enum struct CommandLineError : std::uint8_t {
  unexpected_value,  // Unknown argument encountered
};

// Type aliases for better readability
using ErrorInfo = std::pair<CommandLineError, std::string_view>;
using Expected = std::expected<CommandLineArgs, ErrorInfo>;

/**
 * @brief Parse command-line arguments
 * @param args Span of command-line argument strings
 * @return Expected containing either parsed arguments or error info
 * 
 * Uses a simple state machine to parse arguments.
 */
Expected Parse(std::span<const char *> args) {

  CommandLineArgs vals{};

  // Parser state machine states
  enum struct State : std::uint8_t {
    searching,         // Looking for a flag
    searching_das_log, // Expecting log file path
    searching_das_url, // Expecting URL
  };

  State state{State::searching};

  // Recognized command-line flags
  constexpr const char *DAS_LOG{"-dasLog"};
  constexpr const char *DAS_URL{"-dasUrl"};
  constexpr const char *SHORT_HELP{"-h"};
  constexpr const char *LONG_HELP{"--help"};

  // Parse each argument
  for (std::string_view arg : args) {
    if (arg.starts_with('-')) {
      // This is a flag - transition state
      if (arg == DAS_LOG) {
        state = State::searching_das_log;
      } else if (arg == DAS_URL) {
        state = State::searching_das_url;
      } else if (arg == SHORT_HELP || arg == LONG_HELP) {
        state = State::searching;
        vals.help = true;
        return vals;
      } else {
        // Unknown flag encountered
        // HACK: Clang can't seem to disambiguate std::unexpected in
        // <exception> and <expected>
        return Expected{Expected::unexpected_type{
            std::make_pair(CommandLineError::unexpected_value, arg)}};
      }
    } else {
      // This is a value - store it based on current state
      switch (state) {
      case State::searching:
        // Unexpected value (not after a flag)
        return Expected{Expected::unexpected_type{
            std::make_pair(CommandLineError::unexpected_value, arg)}};
        break;
      case State::searching_das_log:
        vals.das_log = arg;
        break;
      case State::searching_das_url:
        vals.das_url = arg;
        break;
      }

      // Reset state after capturing value
      state = State::searching;
    }
  }

  return vals;
}
} // namespace

/**
 * @brief Main entry point for the DAS HTTP server
 * 
 * Parses command-line arguments, extracts port from URL,
 * and starts the HTTP server in a separate thread.
 */
int main(int argc, const char **argv) {
  constexpr auto usage{
      "Usage: cppdas [-dasLog [file_name]] [-dasUrl [url]]"};

  // Too many arguments provided
  if (argc > 5) {
    std::cerr << usage << std::endl;
    return EXIT_FAILURE;
  }

  // Parse command-line arguments
  std::expected<CommandLineArgs, ErrorInfo> args{
      Parse(std::span<const char *>{argv + 1, argv + argc})};

  // Handle parsing errors
  if (!args.has_value()) {
    const ErrorInfo &err{args.error()};
    switch (err.first) {
    case CommandLineError::unexpected_value:
      std::cerr << "Encountered unexpected value: " << err.second << std::endl;
      return 1;
      break;
    }
  }

  const CommandLineArgs &correct_args{args.value()};
  // Display help and exit if requested
  if (correct_args.help) {
    std::cerr << usage << std::endl;
    return EXIT_SUCCESS;
  }

  // Display configuration
  if constexpr (disable_logs) {
    std::cout << "Logging has been disabled!" << std::endl;
  } else {
    std::cout << "DAS messages being logged to: " << correct_args.das_log
              << std::endl;
  }

  std::cout << "DAS HTTP URL: " << correct_args.das_url << std::endl;

  try {
    // RAII helper to ensure keep_going is set to false on exit
    struct Flag {
      bool &value{keep_going};
      ~Flag() { value = false; }  // Triggers graceful shutdown
    };
    Flag flag{};

    // Start DAS HTTP server in a separate thread
    std::jthread das_thread{RunDAS<disable_listener>,
                            std::string{correct_args.das_log},
                            std::string{correct_args.das_url}};
    
    // Main loop: wait for 'Q' command to quit
    std::string input{};
    while (flag.value) {
      std::cin >> input;
      if (input == "Q") {
        flag.value = false;  // Signal shutdown
      }
    }

    std::cout << "Shutting down, waiting for last message...\n";
    // Note: das_thread joins automatically when going out of scope (std::jthread)
  } catch (std::exception &e) {
    std::cerr << "Error: " << e.what() << "\n";
  } catch (...) {
    std::cerr << "Uncatchable error occurred" << "\n";
  }
}
