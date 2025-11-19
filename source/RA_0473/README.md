# RA_0473 PowerShell DAS Listener - Source Files

This folder contains the core PowerShell scripts and resources for the RA_0473 Data Application Service (DAS) Listener reference implementation. These files enable you to simulate and process DAS messages using PowerShell, as described in the main documentation.

## File Overview

- **DASMessage.ps1**  
  Defines the `DASMessage` class, which wraps incoming HTTP messages with properties for the URL, message name, and JSON body.

- **DASListener.ps1**  
  Implements the main HTTP listener logic. It receives HTTP requests, parses them, and enqueues messages for processing.

- **CallbackExample.ps1**  
  Provides a sample callback function (`MyCallback`) to handle and display received DAS messages. You can customize this for your own processing needs.

- **Run-DAS.ps1**  
  A convenience script to launch the listener and consumer together, displaying messages in real time and supporting graceful shutdown.

- **RA_0473.zip**  
  A zip archive containing all the above scripts for easy download and distribution.

## Usage

Refer to the main documentation in the parent folder for setup and usage instructions. These scripts are modular and can be adapted for custom DAS workflows or integration with AMP systems.

---
*Part of the Teradyne Archimedes Reference Architecture series.*
