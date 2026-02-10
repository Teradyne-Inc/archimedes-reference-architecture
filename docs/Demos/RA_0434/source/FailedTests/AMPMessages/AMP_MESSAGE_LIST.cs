using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Teradyne.ARCM.RA.FailedTests.AMPMessages
{
    /// <summary>
    /// Managing the list of AMP Messages.
    /// It is mandatory to return a list of message names as a response to the INITIALIZATION message and recommended for each message.    
    /// </summary>
    public class AMP_MESSAGE_LIST
    {
        /// <summary>
        /// All existing AMP messages
        /// </summary>
        protected readonly List<string> AMP_MESSAGES = new List<string>()
        {
            "STATUS","INITIALIZATION","LICENSE","T_STATUS","SHUTDOWN","HALT","ALARM","LOCATION","USER_ACCOUNT","DTR","IGXL_DATA","MST_DATA","QUALITY_MONITOR_DATA","TEST_END","TESTER_OS","TEST_PROGRAM_LOAD","TEST_START","CONFIGURATION","BINNAME","RESULTFILE","LOT_END","LOT_START","SUBLOT_END","SUBLOT_START","PERIPHERAL","MAINTENANCE","MAINTENANCE_DATA","CZ_DATA_POINT","CZ_END","CZ_SETUP","CZ_START","CZ_START_POINT","CZ_SUMMARY","TEST_DATA"
        };

        /// <summary>
        /// Current list of messages used by the DAS application
        /// </summary>
        public List<String> MessageList { get; private set; }

        /// <summary>
        /// Initialization. By default, all messages are selected.
        /// </summary>
        public AMP_MESSAGE_LIST()
        {
            MessageList = new List<string>(AMP_MESSAGES);
        }

        /// <summary>
        /// The INITIALIZATION message sends the list of available messages.
        /// </summary>
        /// <param name="message">INITIALIZATION message</param>
        public void ExtractMessageListFromInitMessage(AMPMessageModel message)
        {
            if (!message.isInitialization) return;

            try
            {
                // Parses the JSON message
                JObject jsonData = JsonConvert.DeserializeObject<JObject>(message.Message);

                // Forces the list of messages to be empty if the INITIALIZATION message is incorrect
                if (jsonData == null)
                    MessageList = new List<string>();

                // Extract the list of messages sent by the DAS client
                if (jsonData.ContainsKey("TCS_MESSAGE_LIST"))
                    MessageList = jsonData["TCS_MESSAGE_LIST"]?.ToObject<List<string>>();                
            }
            catch
            {
            }
        }

        /// <summary>
        /// Adds a message name to the list of messages we are interested in.
        /// </summary>
        /// <param name="messageName">Name of the message to subscribe to</param>
        /// <returns>Success or failure</returns>
        public bool subscribe(string messageName)
        {
            if (MessageList == null) MessageList = new List<string>();
            
            if (!AMP_MESSAGES.Contains(messageName)) return false;

            MessageList.Add(messageName);

            return true;
        }

        /// <summary>
        /// Subscribe to a list of messages.
        /// Passing an empty list is not an error but has no effect.
        /// </summary>
        /// <param name="messageNames">List of messages to subscribe to</param>
        public void subscribe(List<string> messageNames)
        {
            if (messageNames == null) return;
            foreach (string messageName in messageNames)
            {
                subscribe(messageName);
            }
        }

        /// <summary>
        /// Empty the list of messages we are interested in.
        /// </summary>
        public void unsubscribeAll() { MessageList = new List<string>(); }

        /// <summary>
        /// We are interested in all existing messages.
        /// </summary>
        public void subscribeAll() { MessageList = new List<string>(AMP_MESSAGES); }

        /// <summary>
        /// Intersect lists to only keep the messages that are both available and of interest to the DAS application.
        /// </summary>
        /// <param name="available">List of available messages as provided by the INITIALIZATION message</param>
        public void updateListToMatchAvailableMessages(AMP_MESSAGE_LIST available)
        {
            if (available == null) return;
            MessageList = MessageList.Intersect(available.MessageList).ToList();
        }

        /// <summary>
        /// Creates a JSON string based on the list of message names currently subscribed to.
        /// </summary>
        /// <returns>the JSON string to send as a response</returns>
        public string ToJSon()
        {
            if (MessageList == null)
                MessageList = new List<string>();

            return JsonConvert.SerializeObject(new { MESSAGE_LIST = MessageList });
        }
    }
}
