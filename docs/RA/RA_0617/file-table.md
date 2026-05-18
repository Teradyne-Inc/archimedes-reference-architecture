<p style="margin: 0; padding: 0;">
  <img src="media/RA617.png"
       alt="RA 617 File List"
       style="width: 100%; height: auto; max-height: 240px; object-fit: cover;" />
</p>
---

| 📄 Description      | 🔗 Download Link                                  |
|---------------------|---------------------------------------------------|
| Source Code         | [GitHub Repository](https://github.com/Teradyne-Inc/archimedes-reference-architecture/tree/main/docs/RA/RA_0617)  |


## Source Code Files

The BoostDAS project includes:

- **build.zig** - Zig build system configuration with cross-compilation support
- **build.zig.zon** - Dependency management (Boost libraries)
- **src/** - C++23 source code for the DAS implementation
- **INSTALLATION.md** - Step-by-step installation instructions
- **COMPILATION.md** - Comprehensive build guide with all options
- **IMPLEMENTATION_GUIDE.md** - Developer guide for customization
- **DOCKER.md** - Docker deployment instructions
- **ZIG_OVERVIEW.md** - Introduction to the Zig build system

## Quick Start

1. Download the project or clone from GitHub
2. Install Zig 0.15.2 ([Installation Guide](INSTALLATION.md))
3. Build with `zig build`
4. Run with `zig build run`

For cross-compilation to Linux UltraEdge:

```sh
zig build -Dtarget=x86_64-linux-musl -Doptimize=ReleaseSmall
```

More details about the [source code here](source/readme.md)

*This documentation is part of the Teradyne Archimedes Reference Architecture series.*
