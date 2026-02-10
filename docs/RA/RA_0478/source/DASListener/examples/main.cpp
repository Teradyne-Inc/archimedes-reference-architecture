// This code is provided 'as is' without any guarantee of its performance, suitability, or reliability.
// Example application demonstrating the DASMessageListener usage

#include "DASMessageListener.h"
#include <iostream>
#include <thread>
#include <chrono>
#include <csignal>

using namespace Teradyne::Archimedes::DAS;

// Global flag for graceful shutdown
volatile std::sig_atomic_t stopFlag = 0;

void signalHandler(int signal) {
    std::cout << "\nReceived signal " << signal << ", shutting down..." << std::endl;
    stopFlag = 1;
}

int main() {
    // Register signal handler for Ctrl+C
    std::signal(SIGINT, signalHandler);
    std::signal(SIGTERM, signalHandler);

    std::cout << "========================================" << std::endl;
    std::cout << "  DAS Message Listener Example (C++)" << std::endl;
    std::cout << "========================================" << std::endl;
    std::cout << std::endl;

    // Create DAS listener instance
    DASMessageListener listener("http://localhost:3000/tems/");

    // Register event callbacks
    listener.onConnected([]() {
        std::cout << "[EVENT] Connected to DAS" << std::endl;
    });

    listener.onDisconnected([]() {
        std::cout << "[EVENT] Disconnected from DAS" << std::endl;
    });

    listener.onError([](const std::string& error) {
        std::cerr << "[ERROR] " << error << std::endl;
    });

    listener.onNewMessage([](const std::string& cellId, const AMPMessage& message) {
        std::cout << "========================================" << std::endl;
        std::cout << "[MESSAGE] Received from Cell: " << cellId << std::endl;
        std::cout << "  Message Name: " << message.messageName << std::endl;
        std::cout << "  Timestamp: " << message.timestamp << std::endl;
        std::cout << "  URL: " << message.url << std::endl;
        std::cout << "  Payload Length: " << message.payload.length() << " bytes" << std::endl;
        
        if (!message.payload.empty() && message.payload.length() < 500) {
            std::cout << "  Payload: " << message.payload << std::endl;
        }
        std::cout << "========================================" << std::endl;
    });

    // Subscribe to specific messages (optional)
    // If not subscribed to any messages, all messages will be received
    std::vector<std::string> messagesToSubscribe = {
        "TEST_START",
        "TEST_END",
        "TEST_DATA",
        "BIN_UPDATE"
    };
    listener.subscribeToMessages(messagesToSubscribe);

    std::cout << "Starting DAS listener on: " << listener.getDASURL() << std::endl;
    std::cout << "Subscribed to messages: ";
    for (const auto& msg : messagesToSubscribe) {
        std::cout << msg << " ";
    }
    std::cout << std::endl;
    std::cout << "Press Ctrl+C to stop..." << std::endl;
    std::cout << std::endl;

    // Start the listener
    if (!listener.start()) {
        std::cerr << "Failed to start DAS listener!" << std::endl;
        return 1;
    }

    // Keep the application running until interrupted
    while (!stopFlag && listener.isRunning()) {
        std::this_thread::sleep_for(std::chrono::seconds(1));
    }

    // Stop the listener
    std::cout << "Stopping DAS listener..." << std::endl;
    listener.stop();

    std::cout << "DAS listener stopped. Goodbye!" << std::endl;
    return 0;
}
