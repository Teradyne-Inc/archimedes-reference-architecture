add_executable(cppdas
    main.cpp
    Connection.cpp
)

# Require C++23 for this target
target_compile_features(cppdas PUBLIC cxx_std_23)

# Link Boost libraries you need (filesystem, system, etc.)
target_link_libraries(cppdas
    PRIVATE
        Boost::asio
        Boost::beast
)

# Include current directory for headers
target_include_directories(cppdas PRIVATE ${CMAKE_CURRENT_SOURCE_DIR})

# Recommended warnings
if (MSVC)
    target_compile_options(cppdas PRIVATE /W4 /permissive-)
else()
    target_compile_options(cppdas PRIVATE -Wall -Wextra -Wpedantic)
endif()

