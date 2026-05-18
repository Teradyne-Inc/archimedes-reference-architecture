#ifndef CPPDAS_SRC_INCLUDE_LOGGER_HPP_
#define CPPDAS_SRC_INCLUDE_LOGGER_HPP_

#include <fstream>
#include <future>
#include <string>
#include <string_view>

/// If logging is opted in at compile time, this specialization will be selected
/// and will have all real definitions.
template <bool Disable> class Logger final {
public:
  Logger(const std::string &log_path) : log_file_{log_path} {}

  void LogAsync(std::string_view log) {
    // We fire and forget, so need to specify std::launch::async to prevent
    // deferred execution
    const auto _{std::async(std::launch::async,
                            [this, log] { this->log_file_ << log; })};
  }

  template <class T> Logger &operator<<(T arg) {
    log_file_ << arg;
    log_file_.flush();
    return *this;
  }

  // An object of this class should exist once and need not be copied or
  // moved
  ~Logger() = default;
  Logger(const Logger &) = delete;
  Logger &operator=(const Logger &) = delete;
  Logger(Logger &&) = delete;
  Logger &operator=(Logger &&) = delete;

private:
  std::ofstream log_file_;
};

/// The false specialization will have everything defined but do
/// nothing to allow compilation to succeed while opting out of logging at
/// compile time.
template <> class Logger<true> final {
public:
  Logger(const std::string &_) noexcept {}
  void LogAsync(std::string_view _) noexcept {}
  template <class T> Logger &operator<<(T _) noexcept { return *this; }
};

#endif // CPPDAS_SRC_INCLUDE_LOGGER_HPP_
