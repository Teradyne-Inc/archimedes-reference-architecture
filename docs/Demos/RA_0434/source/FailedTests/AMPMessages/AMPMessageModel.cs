using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Teradyne.ARCM.RA.FailedTests.AMPMessages
{
    // This code is provided 'as is' without any guarantee of its performance, suitability, or reliability.
    // The code is not intended to be used for production purposes and has not been optimized. 
    // It has also not been implemented following all state-of-the-art practices. 
    // The purpose of this code is to demonstrate some capabilities of OI.NET software. 
    // It is intended for demonstration purposes only. The author shall not be held liable 
    // for any damages arising from the use of this code. Use of this code is solely at the user's own risk.
    // Teradyne company will not be responsible for any damages arising during the use of this code. 
    // The code should be treated as confidential and is protected by intellectual property rights. 

    /// <summary>
    /// Represents a AMP message model containing details about a message, its metadata, and test cell information.
    /// </summary>
    public class AMPMessageModel
    {
        /// <summary>
        /// The URL segments associated with the message.
        /// </summary>
        public string[] URL { get; set; }

        /// <summary>
        /// The name of the message.
        /// </summary>
        public string MessageName { get; set; }

        /// <summary>
        /// The identifier for the test cell associated with the message.
        /// </summary>
        public string TestCellID { get; set; }

        /// <summary>
        /// The timestamp of the message.
        /// </summary>
        public DateTime TimeStamp { get; set; }

        private string _formattedMessage;

        /// <summary>
        /// The formatted (pretty-printed) JSON message if valid, or the original message if not.
        /// </summary>
        public string BMessage => _formattedMessage;

        private string _rawMessage;

        /// <summary>
        /// The raw message content in JSON format.
        /// When set, attempts to parse and format the message as indented JSON.
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
                catch (Exception)
                {
                    // If parsing fails, retain the raw message
                    _formattedMessage = value;
                }
            }
        }

        /// <summary>
        /// This is the INITIALIZATION message
        /// </summary>
        public bool isInitialization
        {
            get => MessageName == "INITIALIZATION";
        }
    }
}
