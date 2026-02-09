// This code is provided 'as is' without any guarantee of its performance, suitability, or reliability.
// The code is not intended to be used for production purposes and has not been optimized. 
// It has also not been implemented following all state-of-the-art practices. 
// The purpose of this code is to demonstrate some capabilities of UltraEdge software. 
// It is intended for demonstration purposes only. The author shall not be held liable 
// for any damages arising from the use of this code. Use of this code is solely at the user's own risk.
// Teradyne company will not be responsible for any damages arising during the use of this code. 
// The code should be treated as confidential and is protected by intellectual property rights.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Teradyne.Archimedes.AMPCommunication.Models
{
    /// <summary>
    /// Represents an AMP message with metadata, test cell ID, timestamps,
    /// and JSON message formatting.
    /// </summary>
    /// <author>Teradyne DIA</author>
    public class AMPMessageModel
    {
        /// <summary>
        /// The URL segments associated with the message.
        /// </summary>
        public string[] URL { get; set; } = Array.Empty<string>();

        /// <summary>
        /// The name of the message extracted from the URL.
        /// </summary>
        public string MessageName { get; set; } = string.Empty;

        /// <summary>
        /// The identifier of the test cell associated with the message.
        /// </summary>
        public string TestCellID { get; set; } = string.Empty;

        /// <summary>
        /// The timestamp of the message, either parsed or defaulting to UTC now.
        /// </summary>
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        private string _formattedMessage = string.Empty;

        /// <summary>
        /// The JSON-formatted version of the message. Falls back to raw message if invalid.
        /// </summary>
        public string FormattedMessage => _formattedMessage;

        private string _rawMessage = string.Empty;

        /// <summary>
        /// The raw JSON message string. When set, attempts to format it with indentation.
        /// </summary>
        public string Message
        {
            get => _rawMessage;
            set
            {
                _rawMessage = value;
                try
                {
                    var jsonObject = JObject.Parse(value);
                    _formattedMessage = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
                }
                catch (Exception ex)
                {
                    if (Log.Logger != null && Log.IsEnabled(Serilog.Events.LogEventLevel.Error))
                    {
                        Log.Error(ex, "Failed to parse JSON message in AMPMessageModel.");
                    }
                    _formattedMessage = value;
                }
            }
        }

        /// <summary>
        /// Determines whether the message name indicates an initialization command.
        /// </summary>
        public bool IsInitialization => MessageName == "INITIALIZATION";
    }
}
