cmake_minimum_required(VERSION 3.20)

# ---- Project ----
project(cppdas VERSION 0.1.0 LANGUAGES CXX)

# Export compile_commands.json for tooling (clangd, VS Code, etc.)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

# Prefer modern CMake behavior where possible
set(CMAKE_POLICY_DEFAULT_CMP0077 NEW)

# Enable folders in IDEs
set_property(GLOBAL PROPERTY USE_FOLDERS ON)

include(FetchContent)

# ---- Fetch Boost ----
# We'll use Boost's official GitHub repo and build only the needed parts.
FetchContent_Declare(
  boost
  GIT_REPOSITORY https://github.com/boostorg/boost.git
  GIT_TAG boost-1.89.0  # pick a stable release tag
)

# Fetch and make available
FetchContent_MakeAvailable(boost)

# Initialize Boost submodules (important!)
execute_process(
  COMMAND git submodule update --init --recursive
  WORKING_DIRECTORY ${boost_SOURCE_DIR}
)

# Add our source directory
add_subdirectory(src)
