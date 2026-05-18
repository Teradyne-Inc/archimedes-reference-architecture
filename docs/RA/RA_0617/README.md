# boostdas
A DAS written in standard C++23 using the C++ boost libraries.

## Building And Running

The DAS uses the zig build system <em><b>0.15.2</b></em> (ignore the CMake files,
those were for early prototyping). This is straightforward to install ((link)[https://ziglang.org/download/], be sure to use
0.15.2, other versions may not work!). The executable is
standalone, so you could even add it to the project folder and invoke it directly
(or add the downloaded folder to your PATH for easy access to zig.exe from
powershell). This build system allows out of the box
cross-compilation (even to ARM MacOS! Which we don't need, but it highlights the
versatility of the tool). You may run it locally on your windows machine for
testing, then build for Linux to deploy on the UE, allowing you to
utilize the performance of a compiled DAS with no .NET runtime.

With some testing, some build configurations can yield a completely statically linked binary around 600KB in size.

The DAS URL and log file path can be configured via command-line arguments. By default,
it listens on port 4242 with path "/cppdas/", and logs to "DasLog.log". The DAS even 
works when running on a different computer than AMP (from my initial testing).

All builds are placed in the "zig-out" folder. When I put something in
\[brackets\], this means optionally not-required flags you may pass to the build.

To simply build the DAS for your local environment:

```sh
zig build [OTHER OPTIONS]
```

To run it in your local environment through zig build system:

```sh
zig build run [OTHER OPTIONS]
```

Of course running the executable directly works as you would expect:
```sh
./path/to/das/cppdas.exe
```
However, running with the zig build system allows you to pass build options and
run the resulting exe in one step, which is convenient for testing.

To run it with command line arguments:
```sh
zig build run [OTHER OPTIONS] -- [ARGS]
```
Mind the '--' delimitting build arguments from runtime arguments!

### Runtime Options

To display help for runtime command-line arguments:
```sh
zig build run -- -h
```
or
```sh
zig build run -- --help
```

Available runtime options:
- `-dasLog [file_name]`: Specify the path to the DAS log file (default: "DasLog.log")
- `-dasUrl [url]`: Specify the complete DAS URL including port and path (default: "http://localhost:4242/cppdas/")

Example with custom log and URL:
```sh
zig build run -- -dasLog mylog.txt -dasUrl "http://localhost:8080/mydas/"
```

To stop the DAS server, type 'Q' in the terminal.

### Compile-Time Build Options

Please keep in mind that as this prototype is developed further, build and
runtime options may change and not be reflected here. For the single absolute
source of truth for the build, run the build with the help flag:

```sh
zig build -h
```

There are various parts of the DAS that can be configured at
<em>compile time</em> to be excluded for performance reasons. For example,
you can disable logging entirely at compile-time, or disable the HTTP listener
portion if needed.

To disable logging at compile-time (for performance, gives a smaller binary):
```sh
zig build -Dnologs [OTHER OPTIONS]
```

To disable the DAS HTTP listener at compile-time:
```sh
zig build -Dnolistener [OTHER OPTIONS]
```

To cross compile to another target:

```sh
zig build -Dtarget=ARCH-OS[-ABI] [OTHER OPTIONS]
```

Where ARCH is the cpu architecture (you will likely only use 'x86_64', but you
can run 'zig targets' to see all possibilities), OS is the target OS (like
'windows' or 'linux') and the ABI is the linking strategy used. For linux, you
can use 'gnu' for a traditional dynamically linked exe, or 'musl' for a
standalone exe that is completely statically linked. On windows, you should
always use 'gnu', but if you develop on windows you can skip this flag.

For example (assuming a windows development machine):

```sh
zig build                              # Native build. Implicitly targets x86_64-windows-gnu if you are on classic windows
zig build -Dtarget=x86_64-windows-gnu  # Explicitly target classic windows
zig build -Dtarget=x86_64-windows      # Note the optional abi
zig build -Dtarget=aarch64-windows-gnu # Even targeting ARM on windows works!
zig build -Dtarget=x86_64-linux-gnu    # Target the UE with a traditional linux exe
zig build -Dtarget=x86_64-linux-musl   # Target the UE with a statically linked exe
zig build -Dtarget=aarch64-macos       # Even MacOS target is possible! Note the missing ABI (though not needed for us)
# ... And many more combinations desired ...
```

Please note that some combos do not work. For example, windows+musl is invalid,
as musl is a linux only ABI.

To build in release mode:

```sh
zig build                 # No flag; builds in debug mode
zig build --release=small # builds optimizing for size using -Os
zig build --release=fast  # builds optimizing for speed using -O2
```

All of these examples can be combined. For example, to build a statically linked
DAS for the UE optimizing for size with no logging:

```sh
zig build -Dtarget=x86_64-linux-musl --release=small -Dnologs
```

In fact, this is the configuration that gives us a small binary of around 600KB.

## Documentation

To build and serve the http doc site, run

```sh
zig build docs
```

This will print to stdout the port the docs will be listening to. Simply
go to your browser and enter

```sh
localhost:PORT
```

with the proper PORT number reported on the console.
