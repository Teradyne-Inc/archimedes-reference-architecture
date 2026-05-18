<p style="margin: 0; padding: 0;">
  <img src="media/RA617.png"
       alt="RA 617 Overview"
       style="width: 100%; height: auto; max-height: 240px; object-fit: cover;" />
</p>

# How to implement a high-performance DAS in C++23 using Boost

#### Reference : RA 617

Welcome to the documentation for the **RA_0617 BoostDAS**.  
This reference architecture demonstrates how to build a high-performance Data Analytics Solution (DAS) using modern C++23 and the Boost libraries, with cross-platform compilation support via the Zig build system.

## General Technical Info
| Component      | Version          |
|----------------|------------------|
| Language       | C++23            |
| Libraries      | Boost (Beast, ASIO, JSON) |
| Build System   | Zig 0.15.2       |
| Environment    | Windows 10/11, Linux |
| IGXL           | > 10.30.10       |
| MST            | > 2016C          |
| IDE            | Visual Studio Code |


## Key Features

- **High Performance**: Native C++ implementation with optimized builds as small as 600KB
- **Cross-Platform**: Compile for Windows and Linux from any development machine
- **HTTP Listener**: Real-time message reception using Boost.Beast
- **Configurable Logging**: Enable or disable logging at compile-time for optimal performance
- **Flexible Deployment**: Run locally or deploy to UltraEdge without .NET runtime
- **Command-Line Configuration**: Customize DAS URL and log file path via runtime arguments

## Architecture Overview

The BoostDAS is a lightweight HTTP server built with Boost.Beast that receives messages from test programs. It features compile-time configurability for logging and listener components, allowing you to optimize the binary for specific deployment scenarios.

### Performance Characteristics

- **Statically Linked Build**: ~600KB with all logging disabled
- **Debug Build**: ~5.5MB with full symbols
- **Optimized Release**: ~740KB with size optimization
- **No Runtime Dependencies**: Fully standalone executable on Linux (musl build)

---

## Example: Quick Start

### Building the DAS

To build for your local environment:

```sh
zig build
```

To build an optimized version for deployment:

```sh
zig build -Doptimize=ReleaseSmall
```

To build for Linux UltraEdge (from Windows):

```sh
zig build -Dtarget=x86_64-linux-musl -Doptimize=ReleaseSmall
```

### Running the DAS

Run with default settings (port 4242, path "/cppdas/"):

```sh
zig build run
```

Run with custom configuration:

```sh
zig build run -- -dasUrl "http://localhost:8080/mydas/" -dasLog mylog.txt
```

Run the compiled executable directly:

```sh
./zig-out/x86_64-windows-gnu/Debug/cppdas.exe
```

To stop the DAS, type 'Q' in the terminal.

---

## Build Options

### Compile-Time Flags

Disable logging for maximum performance:

```sh
zig build -Dnologs
```

Disable HTTP listener at compile-time:

```sh
zig build -Dnolistener
```

### Cross-Compilation Targets

```sh
# Windows (default on Windows hosts)
zig build -Dtarget=x86_64-windows-gnu

# Linux with dynamic linking
zig build -Dtarget=x86_64-linux-gnu

# Linux with static linking (standalone)
zig build -Dtarget=x86_64-linux-musl
```

### Runtime Options

- `-dasLog [file_name]`: Specify log file path (default: "DasLog.log")
- `-dasUrl [url]`: Specify DAS URL with port and path (default: "http://localhost:4242/cppdas/")
- `-h` or `--help`: Display help information

---

## Documentation

Build and serve the full API documentation:

```sh
zig build docs
```

Then open your browser to the URL displayed in the console.

---

*This documentation is part of the Teradyne Archimedes Reference Architecture series.*
