using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text.Json;
using System.Threading.Tasks;
using Teradyne.ARCM.RA.FailedTests.AMPMessages;
using static Teradyne.ARCM.RA.FailedTests.AMPCommunication.AMPEvents;

namespace Teradyne.ARCM.RA.FailedTests.AMPCommunication
{
    // This code is provided 'as is' without any guarantee of its performance, suitability, or reliability.
    // The code is not intended to be used for production purposes and has not been optimized. 
    // It has also not been implemented following all state-of-the-art practices. 
    // The purpose of this code is to demonstrate some capabilities of AMP software. 
    // It is intended for demonstration purposes only. The author shall not be held liable 
    // for any damages arising from the use of this code. Use of this code is solely at the user's own risk.
    // Teradyne company will not be responsible for any damages arising during the use of this code. 
    // The code should be treated as confidential and is protected by intellectual property rights. 

    /// <summary>
    /// Processes HTTP requests, manages message handlers, and invokes events for new messages.
    /// </summary>
    internal class RequestProcessor
    {
        //private MessageHandler _MessageHandler;
        private bool _isFirstMessage = true;
        private AMP_MESSAGE_LIST _messageList = null;

        /// <summary>
        /// Event triggered when a new message is processed.
        /// </summary>
        public event NewMessageDlg NewMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestProcessor"/> class.
        /// </summary>
        /// <param name="messageList">Handle to message list managed by the DasListener</param>
        public RequestProcessor(AMP_MESSAGE_LIST messageList)
        {
            _messageList = messageList;
        }

        /// <summary>
        /// Extracts the timestamp from a message.
        /// </summary>
        /// <param name="message">The JSON message from which the timestamp is extracted.</param>
        /// <returns>
        /// The extracted timestamp as a <see cref="DateTime"/> object, or the current time if no timestamp is found.
        /// </returns>
        private DateTime ExtractTimeStamp(string message)
        {
            try
            {
                using (JsonDocument document = JsonDocument.Parse(message))
                {
                    JsonElement root = document.RootElement;

                    if (root.TryGetProperty("TIME_STAMP", out JsonElement timeStampElement))
                    {
                        string timeStampString = timeStampElement.GetString();
                        return DateTime.Parse(timeStampString, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error for debugging
                Console.WriteLine($"Error extracting timestamp: {ex.Message}");
            }

            return DateTime.Now;
        }

        private AMPMessageModel AnalyzeMessage(Uri uri, string message)
        {
            string messageName = uri.Segments.Last();
            string testCellId = uri.Segments[3].TrimEnd('/'); // Assuming the TEST_CELL name is the third segment

            return new AMPMessageModel
            {
                Message = message,
                TestCellID = testCellId,
                URL = uri.Segments,
                MessageName = messageName,
                TimeStamp = ExtractTimeStamp(message)
            };
        }

        private Tuple<HttpStatusCode, string> HandleMessage(AMPMessageModel message)
        {
            try
            {
                if (_messageList == null) _messageList = new AMP_MESSAGE_LIST();

                if (_isFirstMessage)
                {
                    _isFirstMessage = false;
                    if (message.isInitialization)
                    {
                        AMP_MESSAGE_LIST msgList = new AMP_MESSAGE_LIST();
                        msgList.ExtractMessageListFromInitMessage(message);
                        _messageList.updateListToMatchAvailableMessages(msgList);

                        return new Tuple<HttpStatusCode, string>(HttpStatusCode.OK, _messageList.ToJSon());
                    }
                    else
                    {
                        return new Tuple<HttpStatusCode, string>(HttpStatusCode.InternalServerError, String.Empty);
                    }
                }

                return new Tuple<HttpStatusCode, string>(HttpStatusCode.OK, _messageList.ToJSon());
            }                        
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error processing initialization message: {ex.Message}");
                return new Tuple<HttpStatusCode, string>(HttpStatusCode.InternalServerError, String.Empty);
            }
        }

        /// <summary>
        /// Processes an incoming HTTP request.
        /// </summary>
        /// <param name="context">The HTTP context containing the request and response.</param>
        public async Task ProcessRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            try
            {
                if (request.HttpMethod == "POST")
                {
                    // Read request details
                    StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding);
                    string message = await reader.ReadToEndAsync();
                    
                    // Extract information
                    AMPMessageModel ampMessage = AnalyzeMessage(request.Url, message);

                    // Process the message using the handler
                    Tuple<HttpStatusCode, string> responseMessage = HandleMessage(ampMessage);

                    if (responseMessage.Item1 == HttpStatusCode.OK)
                        // Trigger the NewMessage event if there was no error
                        NewMessage?.Invoke(ampMessage.TestCellID, ampMessage);

                    // Send the response
                    response.StatusCode = (int)responseMessage.Item1;// (int)HttpStatusCode.OK;
                    if (!string.IsNullOrEmpty(responseMessage.Item2))
                    {
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseMessage.Item2);
                        response.ContentLength64 = buffer.Length;                        
                        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    }
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                Console.WriteLine($"Error processing request: {ex.Message}");
            }
            finally
            {
                response.Close();
            }
        }
    }
}
