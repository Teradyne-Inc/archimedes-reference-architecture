# Multi-stage Docker build using Zig
# Stage 1: Build the application using Zig
FROM alpine:latest AS builder

# Install Zig
ARG ZIG_VERSION=0.15.2
RUN apk add --no-cache wget tar \
    && wget https://ziglang.org/download/${ZIG_VERSION}/zig-linux-x86_64-${ZIG_VERSION}.tar.xz \
    && tar -xf zig-linux-x86_64-${ZIG_VERSION}.tar.xz \
    && mv zig-linux-x86_64-${ZIG_VERSION} /usr/local/zig \
    && ln -s /usr/local/zig/zig /usr/local/bin/zig \
    && rm zig-linux-x86_64-${ZIG_VERSION}.tar.xz

# Set working directory
WORKDIR /build

# Copy project files
COPY build.zig build.zig.zon ./
COPY src/ ./src/

# Build the application
RUN zig build -Doptimize=ReleaseSmall -Dtarget=x86_64-linux-gnu

# Stage 2: Create minimal runtime image
FROM alpine:latest

# Install only runtime dependencies (libstdc++ for C++ runtime)
RUN apk add --no-cache libstdc++

# Create application directory
WORKDIR /app

# Copy the compiled binary from builder stage
COPY --from=builder /build/zig-out/x86_64-linux-gnu/ReleaseSmall/cppdas /app/cppdas

# Create log directory
RUN mkdir -p /app/logs

# Expose default port
EXPOSE 4242

# Set environment variables
ENV DAS_LOG=/app/logs/das.log
ENV DAS_URL=http://0.0.0.0:4242/cppdas/

# Run the application
ENTRYPOINT ["/app/cppdas"]
CMD ["-dasUrl", "http://0.0.0.0:4242/cppdas/", "-dasLog", "/app/logs/das.log"]
