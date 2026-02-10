// This code is provided 'as is' without any guarantee of its performance, suitability, or reliability.
// The code is not intended to be used for production purposes and has not been optimized. 
// It has also not been implemented following all state-of-the-art practices. 
// The purpose of this code is to demonstrate some capabilities of UltraEdge software. 
// It is intended for demonstration purposes only. The author shall not be held liable 
// for any damages arising from the use of this code. Use of this code is solely at the user's own risk.
// Teradyne company will not be responsible for any damages arising during the use of this code. 
// The code should be treated as confidential and is protected by intellectual property rights.

#ifndef DAS_MESSAGE_LISTENER_H
#define DAS_MESSAGE_LISTENER_H

#include <string>
#include <functional>
#include <memory>
#include <vector>

namespace Teradyne {
namespace Archimedes {
namespace DAS {

/**
 * @brief Represents an AMP message received by the DAS
 */
struct AMPMessage {
    std::string cellId;        // Test cell identifier
    std::string messageName;   // AMP message name (e.g., TEST_START, TEST_END)
    std::string url;           // Full URL of the request
    std::string payload;       // JSON payload
    std::string timestamp;     // ISO 8601 timestamp
};

/**
 * @brief DASMessageListener is responsible for starting and stopping the DAS HTTP listener,
 * receiving incoming HTTP requests, and dispatching them to registered callbacks.
 * 
 * This class provides event-based notifications for connection, disconnection, errors,
 * and new messages received from the test cell.
 * 
 * @author Teradyne DIA
 */
class DASMessageListener {
public:
    // Event callbacks
    using ConnectedCallback = std::function<void()>;
    using DisconnectedCallback = std::function<void()>;
    using ErrorCallback = std::function<void(const std::string& error)>;
    using NewMessageCallback = std::function<void(const std::string& cellId, const AMPMessage& message)>;

    /**
     * @brief Constructs a DAS listener with the specified URL
     * @param dasUrl The URL to listen on (e.g., "http://localhost:3000/tems/")
     */
    explicit DASMessageListener(const std::string& dasUrl);
    
    /**
     * @brief Destructor - ensures the listener is stopped
     */
    ~DASMessageListener();

    // Prevent copying
    DASMessageListener(const DASMessageListener&) = delete;
    DASMessageListener& operator=(const DASMessageListener&) = delete;

    /**
     * @brief Starts the HTTP listener asynchronously
     * @return true if started successfully, false otherwise
     */
    bool start();

    /**
     * @brief Stops the HTTP listener
     */
    void stop();

    /**
     * @brief Subscribe to specific message names
     * @param messageNames List of message names to listen for
     * @param resetFirst If true, clears existing subscriptions first
     */
    void subscribeToMessages(const std::vector<std::string>& messageNames, bool resetFirst = true);

    /**
     * @brief Check if the listener is currently running
     * @return true if running, false otherwise
     */
    bool isRunning() const;

    /**
     * @brief Get the DAS URL
     * @return The configured DAS URL
     */
    std::string getDASURL() const { return dasUrl_; }

    // Event registration
    void onConnected(ConnectedCallback callback) { connectedCallback_ = callback; }
    void onDisconnected(DisconnectedCallback callback) { disconnectedCallback_ = callback; }
    void onError(ErrorCallback callback) { errorCallback_ = callback; }
    void onNewMessage(NewMessageCallback callback) { newMessageCallback_ = callback; }

private:
    class Impl;
    std::unique_ptr<Impl> pImpl_;
    
    std::string dasUrl_;
    ConnectedCallback connectedCallback_;
    DisconnectedCallback disconnectedCallback_;
    ErrorCallback errorCallback_;
    NewMessageCallback newMessageCallback_;
    
    void invokeConnected();
    void invokeDisconnected();
    void invokeError(const std::string& error);
    void invokeNewMessage(const std::string& cellId, const AMPMessage& message);
};

} // namespace DAS
} // namespace Archimedes
} // namespace Teradyne

#endif // DAS_MESSAGE_LISTENER_H
