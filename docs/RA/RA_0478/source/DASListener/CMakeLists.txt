cmake_minimum_required(VERSION 3.14)
project(DASMessageListener VERSION 1.0.0 LANGUAGES CXX)

# Set C++ standard
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

# Library source files
set(LIBRARY_SOURCES
    src/DASMessageListener.cpp
)

set(LIBRARY_HEADERS
    include/DASMessageListener.h
)

# Create the library
add_library(DASListener STATIC ${LIBRARY_SOURCES} ${LIBRARY_HEADERS})

# Include directories
target_include_directories(DASListener
    PUBLIC
        $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/include>
        $<INSTALL_INTERFACE:include>
)

# Compiler options
if(MSVC)
    target_compile_options(DASListener PRIVATE /W4)
else()
    target_compile_options(DASListener PRIVATE -Wall -Wextra -Wpedantic)
endif()

# Threading support
find_package(Threads REQUIRED)
target_link_libraries(DASListener PUBLIC Threads::Threads)

# Example executable
add_executable(DASListenerExample examples/main.cpp)
target_link_libraries(DASListenerExample PRIVATE DASListener)

# Installation
install(TARGETS DASListener DASListenerExample
    ARCHIVE DESTINATION lib
    LIBRARY DESTINATION lib
    RUNTIME DESTINATION bin
)

install(FILES ${LIBRARY_HEADERS}
    DESTINATION include
)
