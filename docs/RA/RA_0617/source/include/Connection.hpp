#ifndef CPPDAS_SRC_INCLUDE_SERVER_HPP_
#define CPPDAS_SRC_INCLUDE_SERVER_HPP_

#include <atomic>
#include <iostream>
#include <memory>
#include <utility>

#include "Logger.hpp"

#include <boost/asio.hpp>
#include <boost/asio/awaitable.hpp>
#include <boost/asio/ip/tcp.hpp>
#include <boost/asio/use_awaitable.hpp>
#include <boost/beast.hpp>
#include <boost/beast/core/flat_buffer.hpp>
#include <boost/beast/http.hpp>
#include <boost/beast/http/message_fwd.hpp>
#include <boost/beast/http/status.hpp>
#include <boost/beast/http/string_body_fwd.hpp>
#include <boost/beast/http/verb.hpp>

// Global variable to track if the first message has been received
inline std::atomic<bool> first_message_received{false};

template <bool UseLogger> class Connection {
public:
  explicit Connection(std::unique_ptr<boost::asio::ip::tcp::socket> &&socket,
                      Logger<UseLogger> &log_file,
                      std::string_view expected_path) noexcept
      : request_{}, http_buffer_{}, socket_{std::exchange(socket, nullptr)},
        log_file_{log_file}, expected_path_{expected_path} {}

  // rule of 5 triggered due to dtor
  ~Connection() noexcept {
    if (socket_) {
      try {
        socket_->shutdown(boost::asio::ip::tcp::socket::shutdown_send);
      } catch (const boost::wrapexcept<boost::system::system_error> &ex) {
        std::cerr << "Boost error: " << ex.what() << "\n";
      }
    }
  }
  Connection(const Connection &) = delete;
  Connection &operator=(const Connection &) = delete;
  Connection(Connection &&) = delete;
  Connection &operator=(Connection &&) = delete;

  boost::asio::awaitable<void> TalkToClientAsync() {
    //
    bool keep_alive{request_.keep_alive()};
    while (keep_alive) {
      http_buffer_.clear();
      co_await boost::beast::http::async_read(*socket_, http_buffer_, request_,
                                              boost::asio::use_awaitable);

      auto status = boost::beast::http::status::ok;
      if (request_.method() != boost::beast::http::verb::post) {
        status = boost::beast::http::status::bad_request;
        std::cerr << "BAD REQUEST: received a non-POST message\n";
      } else if (!request_.target().starts_with(expected_path_)) {
        status = boost::beast::http::status::not_found;
        std::cerr << "NOT FOUND: target '" << request_.target() 
                  << "' does not match expected path '" << expected_path_ << "'\n";
      } else {
        // First message validation
        bool expected = false;
        if (first_message_received.compare_exchange_strong(expected, true)) {
          // This is the first message - must be INITIALIZATION
          if (request_.target().find("INITIALIZATION") == std::string_view::npos) {
            status = boost::beast::http::status::internal_server_error;
            std::cerr << "ERROR: First message must be INITIALIZATION, got: " 
                      << request_.target() << "\n";
          }
        }
      }

      log_file_ << "Method: " << request_.method() << "\n"
                << "Target: " << request_.target() << "\n"
                << "Message: " << request_.body() << "\n\n";
      
      // Console display of received message
      std::cout << "[" << request_.method() << "] " << request_.target() << std::endl;

      boost::beast::http::response<boost::beast::http::string_body> res{
          status, request_.version()};
      res.set(boost::beast::http::field::server, "CppDAS");
      res.set(boost::beast::http::field::content_type, "text/plain");
      res.keep_alive(request_.keep_alive());
      res.prepare_payload();

      co_await boost::beast::http::async_write(*socket_, res,
                                               boost::asio::use_awaitable);
    }

    co_return;
  }

  void TalkToClient() {

    http_buffer_.clear();
    boost::beast::http::read(*socket_, http_buffer_, request_);

    auto status = boost::beast::http::status::ok;
    if (request_.method() != boost::beast::http::verb::post) {
      status = boost::beast::http::status::bad_request;
      std::println(std::cerr, "BAD REQUEST: received a non-POST message");
    } else if (!request_.target().starts_with(expected_path_)) {
      status = boost::beast::http::status::not_found;
      std::cerr << "NOT FOUND: target '" << request_.target() 
                << "' does not match expected path '" << expected_path_ << "'\n";
    } else {
      // First message validation
      bool expected = false;
      if (first_message_received.compare_exchange_strong(expected, true)) {
        // This is the first message - must be INITIALIZATION
        if (request_.target().find("INITIALIZATION") == std::string_view::npos) {
          status = boost::beast::http::status::internal_server_error;
          std::cerr << "ERROR: First message must be INITIALIZATION, got: " 
                    << request_.target() << "\n";
        }
      }
    }

    log_file_ << "Method: " << request_.method() << "\n"
              << "Target: " << request_.target() << "\n"
              << "Message: " << request_.body() << "\n\n";
    
    // Console display of received message
    std::cout << "[" << request_.method() << "] " << request_.target() << std::endl;

    boost::beast::http::response<boost::beast::http::string_body> res{
        status, request_.version()};
    res.set(boost::beast::http::field::server, "CppDAS");
    res.set(boost::beast::http::field::content_type, "text/plain");
    res.prepare_payload();

    boost::beast::http::write(*socket_, res);
  }

private:
  boost::beast::http::request<boost::beast::http::string_body> request_{};
  boost::beast::flat_buffer http_buffer_{};
  std::unique_ptr<boost::asio::ip::tcp::socket> socket_{};
  /// Be careful! Lifetime must live as long as this owning class
  Logger<UseLogger> &log_file_;
  std::string_view expected_path_;
};

#endif // CPPDAS_SRC_INCLUDE_SERVER_HPP_
