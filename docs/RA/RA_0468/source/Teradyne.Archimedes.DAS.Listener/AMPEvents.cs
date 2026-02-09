// This code is provided 'as is' without any guarantee of its performance, suitability, or reliability.
// The code is not intended to be used for production purposes and has not been optimized. 
// It has also not been implemented following all state-of-the-art practices. 
// The purpose of this code is to demonstrate some capabilities of UltraEdge software. 
// It is intended for demonstration purposes only. The author shall not be held liable 
// for any damages arising from the use of this code. Use of this code is solely at the user's own risk.
// Teradyne company will not be responsible for any damages arising during the use of this code. 
// The code should be treated as confidential and is protected by intellectual property rights.

using Teradyne.Archimedes.AMPCommunication.Models;

namespace Teradyne.Archimedes.AMPCommunication
{
    /// <summary>
    /// Provides utility definitions for the DAS system, including event delegate declarations.
    /// </summary>
    /// <author>Teradyne DIA</author>
    public class AMPEvents
    {
        /// <summary>
        /// Delegate for handling new message events.
        /// Triggered when a new <see cref="AMPMessageModel"/> is received from a test cell.
        /// </summary>
        /// <param name="cellid">The identifier of the test cell that received the message.</param>
        /// <param name="message">The AMP message payload.</param>
        public delegate void NewMessageDlg(string cellid, AMPMessageModel message);
    }
}
