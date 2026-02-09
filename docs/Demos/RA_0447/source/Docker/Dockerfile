# This container runs a Python application for UltraEdge
#
# DISCLAIMER:
# This Dockerfile is provided "as is" without any guarantees or warranties.
# It is intended for demonstration purposes only.
# The author shall not be held liable for any damages arising from its use.
# Use at your own risk.
# (c) 2024 Teradyne DIA - All rights reserved.
# ------------------------------------------------------------------------------

# Use a lightweight Python base image for reduced container size
FROM python:3.10-slim

# ------------------------------------------------------------------------------
# Define Application Variables
# ------------------------------------------------------------------------------
# Define a build-time variable (can be overridden at build time)
ARG APP_NAME=pydas.py

# Convert the build-time variable into a runtime environment variable
ENV APP_NAME=${APP_NAME}

# Prevents Python from writing .pyc files (improves performance)
ENV PYTHONDONTWRITEBYTECODE=1

# Disables output buffering for real-time logs
ENV PYTHONUNBUFFERED=1

# ------------------------------------------------------------------------------
# Set up Application Directory
# ------------------------------------------------------------------------------
# Create an application directory inside the container
RUN mkdir -p /app

# Set the working directory to /app
WORKDIR /app

# ------------------------------------------------------------------------------
# Copy Application Files
# ------------------------------------------------------------------------------
# Copy the main Python script into the container
COPY ${APP_NAME} /app/

# ------------------------------------------------------------------------------
# Install Dependencies (if any)
# ------------------------------------------------------------------------------
# If the application has dependencies, uncomment the following lines:
# COPY requirements.txt /app/
# RUN pip install --no-cache-dir -r /app/requirements.txt

# ------------------------------------------------------------------------------
# Set File Permissions (Optional)
# ------------------------------------------------------------------------------
# Ensure the Python script is executable (not required if using `python3`)
RUN chmod a+x /app/${APP_NAME}

# Expose the AMP DAS port to be used
EXPOSE 5001

# ------------------------------------------------------------------------------
# Define Container Startup Command
# ------------------------------------------------------------------------------
CMD ["/app/${APP_NAME}"]

#