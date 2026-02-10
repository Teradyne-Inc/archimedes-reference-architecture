#!/usr/local/bin/python3

"""
PyDAS application file to be run in a Docker on the UltraEdge.
This script sends a poem to FIFO and logs the process.

DISCLAIMER:
This code is provided "as is" without any guarantees or warranties.
It is intended for demonstration purposes only.
The author shall not be held liable for any damages arising from its use.
Use at your own risk.
(c) 2024 Teradyne DIA - All rights reserved.
"""

import logging
import time
import sys

# Constants
FIFO_OUTPUT = "/app/exec/fifo_to_TP"  # Path to the FIFO output file
SEND_DELAY = 0.1  # Delay between sending lines in seconds

# Poem Data
POEM = [
    "She walks in beauty, like the night",
    "Of cloudless climes and starry skies;",
    "And all that’s best of dark and bright",
    "Meet in her aspect and her eyes;",
    "Thus mellowed to that tender light",
    "Which heaven to gaudy day denies.",
    "",
    "One shade the more, one ray the less,",
    "Had half impaired the nameless grace",
    "Which waves in every raven tress,",
    "Or softly lightens o’er her face;",
    "Where thoughts serenely sweet express,",
    "How pure, how dear their dwelling-place.",
    "",
    "And on that cheek, and o’er that brow,",
    "So soft, so calm, yet eloquent,",
    "The smiles that win, the tints that glow,",
    "But tell of days in goodness spent,",
    "A mind at peace with all below,",
    "A heart whose love is innocent!",
    "",
    "Good Bye from the UltraEdge"
]

def setup_logging():
    """
    Configure logging settings.
    Logs will be written to stdout for easier debugging.
    """
    logging.basicConfig(
        format="%(asctime)s - %(levelname)s - %(message)s",
        level=logging.INFO,
        handlers=[logging.StreamHandler(sys.stdout)]
    )

def send_to_fifo():
    """
    Sends the poem to FIFO line by line.
    Each line is followed by a null terminator (\0) and written to FIFO.
    """
    try:
        with open(FIFO_OUTPUT, 'w', encoding='utf-8') as output_stream:
            for line in POEM:
                output_stream.write(line + "\0")
                output_stream.flush()
                logging.info(f"Sent to FIFO: {line}")
    except Exception as e:
        logging.error(f"Error writing to FIFO: {e}", exc_info=True)

def main():
    """
    Main entry point of the application.
    Sets up logging and starts the FIFO communication.
    """
    setup_logging()
    logging.info("Starting PyDAS FIFO sender...")
    send_to_fifo()
    logging.info("Poem transmission completed.")

if __name__ == '__main__':
    main()
