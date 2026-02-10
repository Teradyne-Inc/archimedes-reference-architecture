// This code is provided 'as is' without any guarantee of its performance, suitability, or reliability.
// The code is not intended to be used for production purposes and has not been optimized. 
// It has also not been implemented following all state-of-the-art practices. 
// The purpose of this code is to demonstrate some capabilities of UltraEdge software. 
// It is intended for demonstration purposes only. The author shall not be held liable 
// for any damages arising from the use of this code. Use of this code is solely at the user's own risk.
// Teradyne company will not be responsible for any damages arising during the use of this code. 
// The code should be treated as confidential and is protected by intellectual property rights.

#include "DASMessageListener.h"
#include <iostream>
#include <thread>
#include <atomic>
#include <chrono>
#include <sstream>
#include <iomanip>
#include <algorithm>
#include <set>

// Note: This implementation uses a simplified HTTP server for demonstration
// In production, consider using libraries like cpp-httplib, Boost.Beast, or Poco

namespace Teradyne {
namespace Archimedes {
namespace DAS {

// Simple HTTP server implementation
class SimpleHttpServer {
public:
    SimpleHttpServer(const std::string& host, int port)
        : host_(host), port_(port), running_(false) {}

    bool start(std::function<void(const std::string&, const std::string&, const std::string&)> handler) {
        if (running_) return false;
        
        handler_ = handler;
        running_ = true;
        
        // In a real implementation, this would use sockets
        // For demonstration purposes, we'll simulate the server
        serverThread_ = std::thread([this]() {
            std::cout << "HTTP Server started on " << host_ << ":" << port_ << std::endl;
            while (running_) {
                std::this_thread::sleep_for(std::chrono::milliseconds(100));
            }
        });
        
        return true;
    }

    void stop() {
        if (!running_) return;
        running_ = false;
        if (serverThread_.joinable()) {
            serverThread_.join();
        }
    }

    bool isRunning() const { return running_; }

    // Simulate receiving a message (for demonstration)
    void simulateMessage(const std::string& method, const std::string& path, const std::string& body) {
        if (handler_ && running_) {
            handler_(method, path, body);
        }
    }

private:
    std::string host_;
    int port_;
    std::atomic<bool> running_;
    std::thread serverThread_;
    std::function<void(const std::string&, const std::string&, const std::string&)> handler_;
};

// Private implementation
class DASMessageListener::Impl {
public:
    Impl(DASMessageListener* parent, const std::string& dasUrl)
        : parent_(parent), dasUrl_(dasUrl), running_(false) {
        parseUrl(dasUrl);
    }

    ~Impl() {
        stop();
    }

    bool start() {
        if (running_) return false;

        server_ = std::make_unique<SimpleHttpServer>(host_, port_);
        
        bool started = server_->start([this](const std::string& method, 
                                              const std::string& path, 
                                              const std::string& body) {
            handleRequest(method, path, body);
        });

        if (started) {
            running_ = true;
            parent_->invokeConnected();
        }

        return started;
    }

    void stop() {
        if (!running_) return;
        
        if (server_) {
            server_->stop();
            server_.reset();
        }
        
        running_ = false;
        parent_->invokeDisconnected();
    }

    bool isRunning() const {
        return running_;
    }

    void subscribeToMessages(const std::vector<std::string>& messageNames, bool resetFirst) {
        if (resetFirst) {
            subscribedMessages_.clear();
        }
        
        for (const auto& msg : messageNames) {
            subscribedMessages_.insert(msg);
        }
    }

private:
    void parseUrl(const std::string& url) {
        // Simple URL parsing (http://host:port/path/)
        std::string temp = url;
        
        // Remove protocol
        size_t pos = temp.find("://");
        if (pos != std::string::npos) {
            temp = temp.substr(pos + 3);
        }
        
        // Extract host and port
        pos = temp.find(":");
        if (pos != std::string::npos) {
            host_ = temp.substr(0, pos);
            temp = temp.substr(pos + 1);
            
            pos = temp.find("/");
            if (pos != std::string::npos) {
                port_ = std::stoi(temp.substr(0, pos));
                basePath_ = temp.substr(pos);
            } else {
                port_ = std::stoi(temp);
            }
        } else {
            pos = temp.find("/");
            if (pos != std::string::npos) {
                host_ = temp.substr(0, pos);
                basePath_ = temp.substr(pos);
            } else {
                host_ = temp;
            }
            port_ = 80;
        }
    }

    void handleRequest(const std::string& method, const std::string& path, const std::string& body) {
        if (method != "POST") return;

        // Parse the URL path to extract cell ID and message name
        // Expected format: /basePath/TEST_CELL/cellid/LOT/lotid/SUBLOT/sublotid/MESSAGE_NAME
        
        std::string cellId = extractCellId(path);
        std::string messageName = extractMessageName(path);
        
        // Check if we're subscribed to this message
        if (!subscribedMessages_.empty() && 
            subscribedMessages_.find(messageName) == subscribedMessages_.end()) {
            return; // Not subscribed to this message
        }

        AMPMessage message;
        message.cellId = cellId;
        message.messageName = messageName;
        message.url = path;
        message.payload = body;
        message.timestamp = getCurrentTimestamp();

        parent_->invokeNewMessage(cellId, message);
    }

    std::string extractCellId(const std::string& path) {
        // Look for TEST_CELL/{cellid} pattern
        size_t pos = path.find("TEST_CELL/");
        if (pos != std::string::npos) {
            std::string temp = path.substr(pos + 10);
            size_t end = temp.find("/");
            if (end != std::string::npos) {
                return temp.substr(0, end);
            }
        }
        return "UNKNOWN";
    }

    std::string extractMessageName(const std::string& path) {
        // The message name is typically the last segment of the URL
        size_t pos = path.rfind('/');
        if (pos != std::string::npos && pos < path.length() - 1) {
            return path.substr(pos + 1);
        }
        return "UNKNOWN";
    }

    std::string getCurrentTimestamp() {
        auto now = std::chrono::system_clock::now();
        auto time = std::chrono::system_clock::to_time_t(now);
        auto ms = std::chrono::duration_cast<std::chrono::milliseconds>(
            now.time_since_epoch()) % 1000;

        std::stringstream ss;
        ss << std::put_time(std::gmtime(&time), "%Y-%m-%dT%H:%M:%S");
        ss << '.' << std::setfill('0') << std::setw(3) << ms.count() << 'Z';
        return ss.str();
    }

    DASMessageListener* parent_;
    std::string dasUrl_;
    std::string host_;
    int port_;
    std::string basePath_;
    std::atomic<bool> running_;
    std::unique_ptr<SimpleHttpServer> server_;
    std::set<std::string> subscribedMessages_;
};

// DASMessageListener implementation

DASMessageListener::DASMessageListener(const std::string& dasUrl)
    : dasUrl_(dasUrl),
      pImpl_(std::make_unique<Impl>(this, dasUrl)) {
}

DASMessageListener::~DASMessageListener() {
    stop();
}

bool DASMessageListener::start() {
    return pImpl_->start();
}

void DASMessageListener::stop() {
    pImpl_->stop();
}

void DASMessageListener::subscribeToMessages(const std::vector<std::string>& messageNames, bool resetFirst) {
    pImpl_->subscribeToMessages(messageNames, resetFirst);
}

bool DASMessageListener::isRunning() const {
    return pImpl_->isRunning();
}

void DASMessageListener::invokeConnected() {
    if (connectedCallback_) {
        try {
            connectedCallback_();
        } catch (const std::exception& e) {
            std::cerr << "Error in Connected callback: " << e.what() << std::endl;
        }
    }
}

void DASMessageListener::invokeDisconnected() {
    if (disconnectedCallback_) {
        try {
            disconnectedCallback_();
        } catch (const std::exception& e) {
            std::cerr << "Error in Disconnected callback: " << e.what() << std::endl;
        }
    }
}

void DASMessageListener::invokeError(const std::string& error) {
    if (errorCallback_) {
        try {
            errorCallback_(error);
        } catch (const std::exception& e) {
            std::cerr << "Error in Error callback: " << e.what() << std::endl;
        }
    }
}

void DASMessageListener::invokeNewMessage(const std::string& cellId, const AMPMessage& message) {
    if (newMessageCallback_) {
        try {
            newMessageCallback_(cellId, message);
        } catch (const std::exception& e) {
            std::cerr << "Error in NewMessage callback: " << e.what() << std::endl;
        }
    }
}

} // namespace DAS
} // namespace Archimedes
} // namespace Teradyne
