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
using Teradyne.Archimedes.AMPCommunication.Models;

namespace Teradyne.Archimedes.AMPCommunication.Messages
{
    /// <summary>
    /// Manages AMP message subscriptions and interactions.
    /// It is mandatory to return a list of message names as a response to the INITIALIZATION message 
    /// and recommended for each message.
    /// </summary>
    /// <author>Teradyne DIA</author>
    public class AMPMessageManager 
    {
        /// <summary>
        /// Predefined list of all valid AMP message types.
        /// </summary>
        private static readonly HashSet<string> AMP_MESSAGES = new()
        {
            "STATUS", "INITIALIZATION", "LICENSE", "T_STATUS", "SHUTDOWN", "HALT", "ALARM", "LOCATION", "USER_ACCOUNT",
            "DTR", "IGXL_DATA", "MST_DATA", "QUALITY_MONITOR_DATA", "TEST_END", "TESTER_OS", "TEST_PROGRAM_LOAD",
            "TEST_START", "CONFIGURATION", "BINNAME", "RESULTFILE", "LOT_END", "LOT_START", "SUBLOT_END", "SUBLOT_START",
            "PERIPHERAL", "MAINTENANCE", "MAINTENANCE_DATA", "CZ_DATA_POINT", "CZ_END", "CZ_SETUP", "CZ_START",
            "CZ_START_POINT", "CZ_SUMMARY", "TEST_DATA", "T_STATUS", "ADAPTIVE_CHANGE"
        };

        /// <summary>
        /// Current list of subscribed messages.
        /// </summary>
        public HashSet<string> MessageList { get; private set; } = new(AMP_MESSAGES);



        /// <summary>
        /// Extracts the message list from an INITIALIZATION message.
        /// </summary>
        /// <param name="message">The INITIALIZATION message model.</param>
        public void ExtractMessageListFromInitMessage(AMPMessageModel message)
        {
            if (!message.IsInitialization)
                return;

            try
            {
                var jsonData = JsonConvert.DeserializeObject<JObject>(message.Message);

                if (jsonData == null || !jsonData.ContainsKey("TCS_MESSAGE_LIST"))
                {
                    MessageList.Clear();
                    return;
                }

                var extractedMessages = jsonData["TCS_MESSAGE_LIST"]?.ToObject<List<string>>() ?? Enumerable.Empty<string>();
                MessageList = new HashSet<string>(extractedMessages.Intersect(AMP_MESSAGES));
            }
            catch (Exception ex)
            {
                if (Log.Logger != null && Log.IsEnabled(Serilog.Events.LogEventLevel.Error))
                {
                    Log.Error(ex, "Failed to extract message list from INITIALIZATION message.");
                }
            }
        }

        /// <summary>
        /// Subscribes to a single message name if it is valid.
        /// </summary>
        /// <param name="messageName">The name of the message to subscribe to.</param>
        /// <returns>True if subscribed, false if already present or invalid.</returns>
        public bool Subscribe(string messageName)
        {
            if (!AMP_MESSAGES.Contains(messageName)) return false;
            return MessageList.Add(messageName);
        }

        /// <summary>
        /// Subscribes to multiple message names.
        /// </summary>
        /// <param name="messageNames">A collection of message names to subscribe to.</param>
        public void Subscribe(IEnumerable<string> messageNames)
        {
            if (messageNames == null) return;

            foreach (var messageName in messageNames)
            {
                Subscribe(messageName);
            }
        }

        /// <summary>
        /// Clears the current message subscription list.
        /// </summary>
        public void UnsubscribeAll()
        {
            MessageList.Clear();
        }

        /// <summary>
        /// Subscribes to all predefined AMP messages.
        /// </summary>
        public void SubscribeAll()
        {
            MessageList.Clear();
            MessageList.UnionWith(AMP_MESSAGES);
        }

        /// <summary>
        /// Intersects the message list with another manager's list.
        /// Only shared messages are kept.
        /// </summary>
        /// <param name="available">Manager with the available message list.</param>
        public void UpdateListToMatchAvailableMessages(AMPMessageManager available)
        {
            if (available == null) return;
            MessageList.IntersectWith(available.MessageList);
        }

        /// <summary>
        /// Serializes the message list into a JSON string.
        /// </summary>
        /// <returns>A JSON string containing the MESSAGE_LIST.</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(new { MESSAGE_LIST = MessageList });
        }
    }
}
