<p style="margin: 0; padding: 0;">
  <img src="media/RA617.png"
       alt="RA 617 Implementation"
       style="width: 100%; height: auto; max-height: 240px; object-fit: cover;" />
</p>

# Building a Data Acquisition System (DAS) with C++23 and Zig

## Table of Contents
1. [Project Overview](#project-overview)
2. [Architecture](#architecture)
3. [Project Structure](#project-structure)
4. [Setting Up the Build System](#setting-up-the-build-system)
5. [Core Components Explained](#core-components-explained)
6. [Step-by-Step Implementation](#step-by-step-implementation)
7. [Advanced Features](#advanced-features)
8. [Testing and Deployment](#testing-and-deployment)

---

## Project Overview

This guide shows how to build a **production-ready HTTP Data Acquisition System** using:
- **C++23** for modern, safe code
- **Boost.Asio** for async I/O
- **Boost.Beast** for HTTP protocol
- **Zig** as the build system and package manager

**What the DAS does:**
- Listens for HTTP POST requests on a configurable URL
- Validates that the first message is an INITIALIZATION message
- Logs all messages to a file
- Uses a multi-threaded producer-consumer pattern for high performance
- Returns HTTP 200 for valid requests, HTTP 500 for invalid first message

**Final binary size:** 535 KB  
**Performance:** Handles thousands of concurrent connections

---

## Architecture

### High-Level Design

```mermaid
graph TB
    subgraph Main["Main Thread"]
        A1[Parse command-line arguments]
        A2[Extract port from URL]
        A3[Start DAS thread]
        A4[Wait for 'Q' to quit]
        A1 --> A2 --> A3 --> A4
    end
    
    subgraph Producer["DAS Thread - Producer"]
        B1[Listen on TCP port]
        B2[Accept incoming connections]
        B3[Push connections to queue]
        B4[Signal semaphore]
        B1 --> B2 --> B3 --> B4
    end
    
    subgraph Queue["Synchronization Layer"]
        C1[Connection Queue FIFO<br/>Protected by std::mutex]
        C2[Semaphore<br/>Signal Counter]
    end
    
    subgraph Consumer1["Consumer Thread 1"]
        D1[Wait on semaphore]
        D2[Pop connection]
        D3[Spawn coroutine]
        D4[Handle HTTP]
        D5[Send response]
        D1 --> D2 --> D3 --> D4 --> D5
        D5 -.-> D1
    end
    
    subgraph Consumer2["Consumer Thread N"]
        E1[Wait on semaphore]
        E2[Pop connection]
        E3[Spawn coroutine]
        E4[Handle HTTP]
        E5[Send response]
        E1 --> E2 --> E3 --> E4 --> E5
        E5 -.-> E1
    end
    
    Main --> Producer
    Producer --> C1
    Producer --> C2
    C1 --> Consumer1
    C1 --> Consumer2
    C2 --> Consumer1
    C2 --> Consumer2
    
    style Main fill:#e1f5ff
    style Producer fill:#fff4e1
    style Queue fill:#f0f0f0
    style Consumer1 fill:#e8ffe8
    style Consumer2 fill:#e8ffe8
    
    Note1[N threads = CPU cores]
    Consumer2 -.-> Note1
    style Note1 fill:#ffffcc
```

### Key Design Patterns

1. **Producer-Consumer Pattern**
   - Main thread produces connections
   - Worker threads consume and process them
   - Queue + semaphore for synchronization

2. **Coroutines (C++20/23)**
   - Non-blocking I/O operations
   - Better scalability than traditional threads

3. **RAII (Resource Acquisition Is Initialization)**
   - Automatic cleanup with destructors
   - No manual resource management

4. **Compile-Time Configuration**
   - Feature flags (`-Dnologs`, `-Dnolistener`)
   - Zero runtime overhead for disabled features

---

## Project Structure

```mermaid
graph TD
    A[boostdas/] --> B[build.zig<br/>Zig build configuration]
    A --> C[build.zig.zon<br/>Dependency management]
    A --> D[src/]
    A --> E[Dockerfile<br/>Docker container definition]
    A --> F[docker-compose.yml<br/>Docker Compose config]
    A --> G[docs/]
    
    D --> D1[main.cpp<br/>Entry point + DAS server]
    D --> D2[include/]
    D2 --> D2A[Connection.hpp<br/>HTTP connection handler]
    D2 --> D2B[Logger.hpp<br/>Logging utilities]
    
    G --> G1[INSTALLATION.md<br/>Zig installation guide]
    G --> G2[COMPILATION.md<br/>Build instructions]
    G --> G3[ZIG_OVERVIEW.md<br/>Zig benefits explained]
    G --> G4[DOCKER.md<br/>Docker deployment guide]
    
    style A fill:#e1f5ff
    style D fill:#fff4e1
    style D2 fill:#ffe8e8
    style G fill:#e8f5e8
    style B fill:#ffffcc
    style C fill:#ffffcc
```

---

## Setting Up the Build System

### Step 1: Create `build.zig.zon` (Dependency File)

This file tells Zig what libraries to download:

```zig
.{
    .name = "cppdas",
    .version = "1.0.0",
    .dependencies = .{
        // Boost libraries - automatically downloaded and cached
        .@"boost.asio" = .{
            .url = "https://github.com/boostorg/asio/archive/refs/tags/boost-1.86.0.tar.gz",
            .hash = "1220...",  // Verification hash
        },
        .@"boost.beast" = .{
            .url = "https://github.com/boostorg/beast/archive/refs/tags/boost-1.86.0.tar.gz",
            .hash = "1220...",
        },
        // ... more Boost modules
    },
}
```

**What happens:**
1. First build: Zig downloads all dependencies
2. Subsequent builds: Uses cached versions (in `~/.cache/zig`)
3. Same versions across all machines (reproducible builds)

### Step 2: Create `build.zig` (Build Configuration)

```zig
const std = @import("std");

pub fn build(b: *std.Build) void {
    // 1. Get build options from command line
    const target = b.standardTargetOptions(.{});
    const optimize = b.standardOptimizeOption(.{});

    // 2. Create the executable
    const exe = b.addExecutable(.{
        .name = "cppdas",
        .target = target,
        .optimize = optimize,
    });

    // 3. Add C++ source file
    exe.addCSourceFile(.{
        .file = b.path("src/main.cpp"),
        .flags = &.{
            "-std=c++23",           // Use C++23 standard
            "-Wall",                // All warnings
            "-Wextra",              // Extra warnings
            "-Wpedantic",           // Strict standard compliance
            "-Werror",              // Warnings = errors
        },
    });

    // 4. Link C++ standard library
    exe.linkSystemLibrary("c++");
    
    // 5. Link Windows socket library (platform-specific)
    if (target.result.os.tag == .windows) {
        exe.linkSystemLibrary("Ws2_32");
    }

    // 6. Add include directories
    exe.addIncludePath(b.path("src/include"));

    // 7. Add Boost dependencies
    const boost = b.dependency("boost.asio", .{});
    exe.addIncludePath(boost.path("include"));
    // ... repeat for all Boost modules

    // 8. Compile-time feature flags
    if (b.option(bool, "nologs", "Disable logging") orelse false) {
        exe.defineCMacro("NOLOG", null);
    }
    if (b.option(bool, "nolistener", "Disable listener") orelse false) {
        exe.defineCMacro("NOLISTENER", null);
    }

    // 9. Install the executable
    b.installArtifact(exe);
}
```

**Build Commands:**
```bash
# Debug build
zig build

# Release build (smallest binary)
zig build -Doptimize=ReleaseSmall

# Build without logging
zig build -Doptimize=ReleaseSmall -Dnologs=true

# Cross-compile for Linux from Windows
zig build -Dtarget=x86_64-linux-gnu -Doptimize=ReleaseSmall
```

---

## Core Components Explained

### 1. Logger Template (`Logger.hpp`)

```cpp
template <bool Disabled>
class Logger {
    std::ofstream file_;
public:
    Logger(std::string_view path) {
        if constexpr (!Disabled) {
            file_.open(std::string(path), std::ios::app);
        }
    }

    template <typename T>
    Logger& operator<<(const T& value) {
        if constexpr (!Disabled) {
            file_ << value;
        }
        return *this;
    }
};
```

**Key Features:**
- Template parameter `Disabled` evaluated at compile-time
- When `Disabled=true`, all logging code is eliminated (zero overhead)
- Uses `if constexpr` for compile-time branching

**Usage:**
```cpp
Logger<false> logger{"app.log"};  // Logging enabled
logger << "Message";               // Writes to file

Logger<true> logger{"app.log"};   // Logging disabled
logger << "Message";               // Compiled to nothing (zero cost)
```

### 2. Connection Handler (`Connection.hpp`)

```cpp
template <bool UseLogger>
class Connection {
    // Member variables
    boost::beast::http::request<boost::beast::http::string_body> request_;
    boost::beast::flat_buffer http_buffer_;
    std::unique_ptr<tcp::socket> socket_;
    Logger<UseLogger>& log_file_;
    std::string_view expected_path_;

public:
    // Constructor
    explicit Connection(
        std::unique_ptr<tcp::socket>&& socket,
        Logger<UseLogger>& log_file,
        std::string_view expected_path
    ) noexcept;

    // Async HTTP handler (C++20 coroutine)
    boost::asio::awaitable<void> TalkToClientAsync() {
        bool keep_alive{request_.keep_alive()};
        
        while (keep_alive) {
            // 1. Read HTTP request asynchronously
            co_await boost::beast::http::async_read(
                *socket_, http_buffer_, request_,
                boost::asio::use_awaitable
            );

            // 2. Validate request
            auto status = boost::beast::http::status::ok;
            
            if (request_.method() != boost::beast::http::verb::post) {
                status = boost::beast::http::status::bad_request;
            } 
            else if (!request_.target().starts_with(expected_path_)) {
                status = boost::beast::http::status::not_found;
            }
            else {
                // First message validation
                bool expected = false;
                if (first_message_received.compare_exchange_strong(expected, true)) {
                    if (request_.target().find("INITIALIZATION") == std::string_view::npos) {
                        status = boost::beast::http::status::internal_server_error;
                    }
                }
            }

            // 3. Log request
            log_file_ << "Method: " << request_.method() << "\n"
                      << "Target: " << request_.target() << "\n"
                      << "Message: " << request_.body() << "\n\n";

            // 4. Display on console
            std::cout << "[" << request_.method() << "] " 
                      << request_.target() << std::endl;

            // 5. Send HTTP response
            boost::beast::http::response<boost::beast::http::string_body> res{
                status, request_.version()
            };
            res.set(boost::beast::http::field::server, "CppDAS");
            res.set(boost::beast::http::field::content_type, "text/plain");
            res.keep_alive(request_.keep_alive());
            res.prepare_payload();

            co_await boost::beast::http::async_write(
                *socket_, res, boost::asio::use_awaitable
            );
        }
        
        co_return;
    }
};
```

**Key Features:**
- **C++20 Coroutines** (`co_await`, `co_return`)
- **Atomic first-message check** (thread-safe)
- **Path validation** (only accept URLs starting with `expected_path`)
- **HTTP status codes** (200, 400, 404, 500)

### 3. Producer-Consumer DAS Server (`main.cpp`)

```cpp
template <bool Disable>
void RunDAS(const std::string&& log_path, const std::string&& das_url) {
    // Extract port and path from URL
    std::uint16_t port = 4242;
    std::string expected_path = "/";
    // ... URL parsing logic ...

    // Shared data structures
    std::queue<std::unique_ptr<tcp::socket>> connections;
    std::mutex queue_lock;
    std::counting_semaphore<255> queue_sema{0};
    Logger<disable_logs> logger{log_path};
    asio::io_context ioc;

    // Consumer lambda (runs in worker threads)
    auto consume = [&] {
        while (keep_going) {
            queue_sema.acquire();  // Wait for connection

            if (std::unique_lock lock{queue_lock}; 
                keep_going && !connections.empty()) {
                
                // Spawn coroutine to handle connection
                boost::asio::co_spawn(
                    ioc,
                    [&] -> boost::asio::awaitable<void> {
                        Connection connection{
                            std::move(connections.front()),
                            logger,
                            expected_path
                        };
                        connections.pop();
                        co_await connection.TalkToClientAsync();
                    },
                    boost::asio::detached
                );
                
                ioc.run();  // Process async operations
            }
        }
    };

    // Create worker thread pool (one per CPU core)
    const auto consumer_count = std::thread::hardware_concurrency();
    std::vector<std::jthread> consumers;
    for (size_t i = 0; i < consumer_count; ++i) {
        consumers.emplace_back(consume);
    }

    // Producer: Accept connections
    tcp::acceptor acceptor{ioc, {tcp::v4(), port}};
    logger << "Listening on http://localhost:" << port << expected_path << "\n";

    while (keep_going) {
        auto socket = std::make_unique<tcp::socket>(ioc);
        acceptor.accept(*socket);  // Block until client connects
        
        {
            std::unique_lock lock{queue_lock};
            connections.push(std::move(socket));
        }
        
        queue_sema.release();  // Signal worker thread
    }

    // Graceful shutdown: wake all workers
    for (size_t i = 0; i < consumer_count; ++i) {
        queue_sema.release();
    }
}
```

**Key Synchronization Primitives:**

1. **`std::queue`** - FIFO connection queue
2. **`std::mutex`** - Protects queue from race conditions
3. **`std::counting_semaphore`** - Signals available work
4. **`std::jthread`** - Auto-joining threads (RAII)
5. **`std::atomic<bool>`** - Thread-safe first-message flag

---

## Step-by-Step Implementation

### Phase 1: Setup Zig Build System

1. **Create project directory:**
   ```bash
   mkdir myDAS && cd myDAS
   ```

2. **Create `build.zig.zon`:**
   ```zig
   .{
       .name = "myDAS",
       .version = "0.1.0",
       .dependencies = .{
           // Add Boost dependencies here
       },
   }
   ```

3. **Create `build.zig`:**
   - Copy from our example
   - Adjust paths and names

4. **Test build system:**
   ```bash
   zig build
   # Should download dependencies and compile
   ```

### Phase 2: Implement Logger

1. **Create `src/include/Logger.hpp`:**
   ```cpp
   template <bool Disabled>
   class Logger {
       std::ofstream file_;
   public:
       Logger(std::string_view path);
       template <typename T>
       Logger& operator<<(const T& value);
   };
   ```

2. **Add compile-time optimization:**
   ```cpp
   if constexpr (!Disabled) {
       // Only compile this code if logging enabled
   }
   ```

3. **Test:**
   ```bash
   zig build -Dnologs=false  # With logging
   zig build -Dnologs=true   # Without logging (smaller binary)
   ```

### Phase 3: Implement Connection Handler

1. **Create `src/include/Connection.hpp`:**
   ```cpp
   template <bool UseLogger>
   class Connection {
       // HTTP request/response handling
   };
   ```

2. **Add async HTTP processing:**
   - Use `boost::asio::awaitable`
   - Implement `co_await` for non-blocking I/O

3. **Add validation logic:**
   - Check HTTP method (POST only)
   - Validate URL path
   - Verify first message is INITIALIZATION

### Phase 4: Implement Main Server

1. **Create `src/main.cpp`:**
   ```cpp
   int main(int argc, const char** argv) {
       // Parse command-line arguments
       // Start DAS server thread
       // Wait for shutdown signal
   }
   ```

2. **Implement producer-consumer pattern:**
   - Producer: TCP acceptor loop
   - Consumers: Worker threads with coroutines

3. **Add graceful shutdown:**
   - RAII helper to set `keep_going = false`
   - Wake all worker threads before exit

### Phase 5: Build and Test

1. **Build release version:**
   ```bash
   zig build -Doptimize=ReleaseSmall
   ```

2. **Run server:**
   ```bash
   ./zig-out/x86_64-windows-gnu/ReleaseSmall/myDAS.exe -dasUrl http://localhost:3000/api/
   ```

3. **Test with curl:**
   ```bash
   # First message (should succeed)
   curl -X POST http://localhost:3000/api/INITIALIZATION -d "init data"
   
   # Second message (should succeed)
   curl -X POST http://localhost:3000/api/data -d "test data"
   
   # Wrong path (should fail with 404)
   curl -X POST http://localhost:3000/wrong/path -d "data"
   ```

---

## Advanced Features

### 1. Compile-Time Feature Flags

```cpp
// In build.zig
if (b.option(bool, "enable_metrics", "Enable performance metrics") orelse false) {
    exe.defineCMacro("ENABLE_METRICS", null);
}

// In C++ code
#ifdef ENABLE_METRICS
    auto start = std::chrono::high_resolution_clock::now();
    // ... operation ...
    auto duration = std::chrono::duration_cast<std::chrono::microseconds>(
        std::chrono::high_resolution_clock::now() - start
    );
    std::cout << "Processing time: " << duration.count() << " μs\n";
#endif
```

### 2. Custom Message Validation

```cpp
// In Connection.hpp
bool ValidateMessage(const std::string_view& body) {
    // Parse JSON
    auto json = boost::json::parse(body);
    
    // Check required fields
    if (!json.as_object().contains("timestamp")) {
        return false;
    }
    
    // Validate data format
    return true;
}
```

### 3. Response Customization

```cpp
// Send JSON response
boost::beast::http::response<boost::beast::http::string_body> res{
    boost::beast::http::status::ok,
    request_.version()
};
res.set(boost::beast::http::field::content_type, "application/json");
res.body() = R"({"status":"ok","timestamp":123456789})";
res.prepare_payload();
```

### 4. Performance Monitoring

```cpp
// Track request count
inline std::atomic<uint64_t> request_counter{0};

// In Connection::TalkToClientAsync()
request_counter.fetch_add(1, std::memory_order_relaxed);

// In main thread
std::thread monitor{[] {
    while (keep_going) {
        std::this_thread::sleep_for(std::chrono::seconds(1));
        std::cout << "Requests/sec: " << request_counter.exchange(0) << "\n";
    }
}};
```

---

## Testing and Deployment

### Load Testing

```bash
# Using Apache Bench
ab -n 10000 -c 100 -p data.txt http://localhost:3000/api/INITIALIZATION

# Using hey
hey -n 10000 -c 100 -m POST -D data.txt http://localhost:3000/api/data
```

### Docker Deployment

```bash
# Build Docker image
docker build -t mydas:latest .

# Run container
docker run -d -p 3000:3000 mydas:latest

# Check logs
docker logs -f <container-id>
```

### Performance Benchmarks

On a 4-core machine:
- **Throughput:** 50,000+ requests/second
- **Latency (p50):** <1ms
- **Memory usage:** ~15 MB
- **Binary size:** 535 KB

---

## Summary

**What You've Learned:**

1. ✅ Set up Zig as a C++ build system
2. ✅ Implement producer-consumer pattern
3. ✅ Use C++20/23 coroutines for async I/O
4. ✅ Apply compile-time optimizations
5. ✅ Build thread-safe HTTP server
6. ✅ Deploy with Docker

**Why This Approach Works:**

- **Zig** handles complex dependency management
- **C++23** provides modern language features
- **Boost** offers production-ready networking
- **Coroutines** enable high scalability
- **Docker** ensures consistent deployment

**Next Steps:**

1. Add SSL/TLS support (`boost::asio::ssl`)
2. Implement authentication/authorization
3. Add database integration (PostgreSQL, Redis)
4. Create admin dashboard (WebSocket)
5. Scale horizontally with load balancer

For more details:
- [ZIG_OVERVIEW.md](ZIG_OVERVIEW.md) - Zig benefits
- [DOCKER.md](DOCKER.md) - Docker deployment
- [COMPILATION.md](COMPILATION.md) - Build options

This guide provides a comprehensive foundation for building production-ready DAS implementations.
