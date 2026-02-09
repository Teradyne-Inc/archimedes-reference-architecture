// This code is provided 'as is' without any guarantee of its performance, suitability, or reliability.
// The code is not intended to be used for production purposes and has not been optimized. 
// It has also not been implemented following all state-of-the-art practices. 
// The purpose of this code is to demonstrate some capabilities of UltraEdge software. 
// It is intended for demonstration purposes only. The author shall not be held liable 
// for any damages arising from the use of this code. Use of this code is solely at the user's own risk.
// Teradyne company will not be responsible for any damages arising during the use of this code. 
// The code should be treated as confidential and is protected by intellectual property rights.

using System.Net;
using System.Text.Json;
using Serilog;
using Teradyne.Archimedes.AMPCommunication.Messages;
using Teradyne.Archimedes.AMPCommunication.Models;
using static Teradyne.Archimedes.AMPCommunication.AMPEvents;

namespace Teradyne.Archimedes.AMPCommunication.Services
{
    /// <summary>
    /// Handles incoming HTTP requests, parses them into AMPMessageModel,
    /// triggers events, and returns appropriate HTTP responses.
    /// </summary>
    /// <author>Teradyne DIA</author>
    internal class AMPMessageProcessor
    {
        private bool _isFirstMessage = true;
        private readonly AMPMessageManager _messageList;

        /// <summary>
        /// Event triggered when a new AMP message is received.
        /// </summary>
        public event NewMessageDlg NewMessage = delegate { };

        /// <summary>
        /// Initializes a new instance of AMPMessageProcessor.
        /// </summary>
        /// <param name="messageList">Shared AMPMessageManager instance.</param>
        public AMPMessageProcessor(AMPMessageManager messageList)
        {
            _messageList = messageList ?? throw new ArgumentNullException(nameof(messageList));
        }

        /// <summary>
        /// Extracts a timestamp from the incoming JSON message.
        /// </summary>
        /// <param name="message">The message in JSON format.</param>
        /// <returns>A DateTime object extracted or the current UTC time if parsing fails.</returns>
        private DateTime ExtractTimeStamp(string message)
        {
            try
            {
                using JsonDocument document = JsonDocument.Parse(message);
                JsonElement root = document.RootElement;

                if (root.TryGetProperty("TIME_STAMP", out JsonElement timeStampElement))
                {
                    string? timeStampString = timeStampElement.GetString();

                    if (!string.IsNullOrWhiteSpace(timeStampString) &&
                        DateTime.TryParse(timeStampString, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime parsedDate))
                    {
                        return parsedDate;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error extracting timestamp from message.");
            }

            return DateTime.UtcNow;
        }

        /// <summary>
        /// Parses the URL and raw JSON into an AMPMessageModel.
        /// </summary>
        /// <param name="uri">The request URI.</param>
        /// <param name="message">The message content.</param>
        /// <returns>A populated AMPMessageModel.</returns>
        private AMPMessageModel AnalyzeMessage(Uri uri, string message)
        {
            string messageName = uri.Segments.Last();
            string testCellId = uri.Segments.Length > 3 ? uri.Segments[3].TrimEnd('/') : "Unknown";

            return new AMPMessageModel
            {
                Message = message,
                TestCellID = testCellId,
                URL = uri.Segments,
                MessageName = messageName,
                TimeStamp = ExtractTimeStamp(message)
            };
        }

        /// <summary>
        /// Handles and interprets the incoming AMP message.
        /// Returns the corresponding response object.
        /// </summary>
        /// <param name="message">Parsed AMP message.</param>
        /// <returns>HTTP response object indicating success or failure.</returns>
        private HttpResponse HandleMessage(AMPMessageModel message)
        {
            try
            {
                if (_isFirstMessage)
                {
                    _isFirstMessage = false;

                    if (message.IsInitialization)
                    {
                        var msgList = new AMPMessageManager();
                        msgList.ExtractMessageListFromInitMessage(message);
                        _messageList.UpdateListToMatchAvailableMessages(msgList);
                        return HttpResponse.Success(_messageList.ToJson());
                    }

                    return HttpResponse.Error(HttpStatusCode.InternalServerError);
                }

                return HttpResponse.Success(_messageList.ToJson());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing message.");
                return HttpResponse.Error(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Main entry point for handling incoming HTTP requests asynchronously.
        /// </summary>
        /// <param name="context">HTTP context with request and response streams.</param>
        public async Task ProcessRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            try
            {
                if (request.HttpMethod != "POST")
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }

                string message;
                using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    message = await reader.ReadToEndAsync();
                }

                if (request.Url == null)
                {
                    Log.Error("Request URL is null. Cannot process message.");
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }

                AMPMessageModel ampMessage = AnalyzeMessage(request.Url, message);
                HttpResponse responseMessage = HandleMessage(ampMessage);

                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    try { NewMessage(ampMessage.TestCellID, ampMessage); }
                    catch (Exception ex) { Log.Warning(ex, "Error while invoking NewMessage event."); }
                }

                response.StatusCode = (int)responseMessage.StatusCode;
                if (!string.IsNullOrEmpty(responseMessage.Content))
                {
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseMessage.Content);
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing HTTP request.");
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            finally
            {
                response.Close();
            }
        }
    }

    /// <summary>
    /// Represents a structured HTTP response with status and content.
    /// </summary>
    /// <author>Teradyne DIA</author>
    internal class HttpResponse
    {
        public HttpStatusCode StatusCode { get; }
        public string Content { get; }

        private HttpResponse(HttpStatusCode statusCode, string content = "")
        {
            StatusCode = statusCode;
            Content = content;
        }

        public static HttpResponse Success(string content) => new HttpResponse(HttpStatusCode.OK, content);
        public static HttpResponse Error(HttpStatusCode statusCode) => new HttpResponse(statusCode);
    }
}
