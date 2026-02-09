#!/bin/bash

# ------------------------------------------------------------------------------
# Docker Build and Export Script for UltraEdge
#
# This script builds a Docker image, saves it as a compressed file, 
# and moves it to the TestPrograms directory.
#
# DISCLAIMER:
# This script is provided "as is" without any guarantees or warranties.
# It is intended for demonstration purposes only.
# The author shall not be held liable for any damages arising from its use.
# Use at your own risk.
# (c) 2025 Teradyne DIA - All rights reserved.
# ------------------------------------------------------------------------------

set -e  # Exit on error
set -o pipefail  # Ensure pipe failures are caught

# Constants
APPNAME="pydas"
IMAGE_FILE="$APPNAME.tar.gz"

# Step 1: Build the Docker image
echo "Building Docker image: $APPNAME..."
if docker build -t "$APPNAME" .; then
    echo "✅ Docker build successful."
else
    echo "❌ Docker build failed!" >&2
    exit 1
fi

# Step 2: Save the Docker image as a compressed file
echo "Saving Docker image as: $IMAGE_FILE..."
if docker image save "$APPNAME:latest" | gzip > "$IMAGE_FILE"; then
    echo "✅ Docker image saved successfully."
else
    echo "❌ Failed to save Docker image!" >&2
    exit 1
fi

# Step 3: Remove the Docker image
echo "Removing Docker image: $APPNAME..."
if docker image rm "$APPNAME:latest"; then
    echo "✅ Docker image removed successfully."
else
    echo "⚠️ Warning: Could not remove Docker image. It might not exist."
fi

# Step 4: Display Docker images
echo "Displaying Docker image list:"
docker image list

echo "🎉 Docker build and export process completed successfully!"
