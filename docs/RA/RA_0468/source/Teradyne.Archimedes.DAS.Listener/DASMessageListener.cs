// This code is provided 'as is' without any guarantee of its performance, suitability, or reliability.
// The code is not intended to be used for production purposes and has not been optimized. 
// It has also not been implemented following all state-of-the-art practices. 
// The purpose of this code is to demonstrate some capabilities of UltraEdge software. 
// It is intended for demonstration purposes only. The author shall not be held liable 
// for any damages arising from the use of this code. Use of this code is solely at the user's own risk.
// Teradyne company will not be responsible for any damages arising during the use of this code. 
// The code should be treated as confidential and is protected by intellectual property rights.

using Serilog;
using System.Net;
using Teradyne.Archimedes.AMPCommunication.Messages;
using Teradyne.Archimedes.AMPCommunication.Models;
using Teradyne.Archimedes.AMPCommunication.Services;
using static Teradyne.Archimedes.AMPCommunication.AMPEvents;

namespace Teradyne.Archimedes.DAS.Listener
{
    /// <summary>
    /// DASMessageListener is responsible for starting and stopping the DAS HTTP listener,
    /// receiving incoming HTTP requests, and dispatching them to a processor. It manages
    /// event notifications for connection, disconnection, errors, and new messages.
    /// </summary>
    /// <author>Teradyne DIA</author>
    public class DASMessageListener : IDisposable
    {
        /// <summary>
        /// Delegate used for the event related to the DAS
        /// </summary>
        public delegate void DasEventHandler();

        /// <summary>
        /// Delegate when the DAS is facing to an error
        /// </summary>
        /// <param name="exception"></param>
        public delegate void DasErrorHandler(Exception exception);

        /// <summary>
        /// Event when the DAS is connected
        /// </summary>
        public event DasEventHandler Connected = delegate { };

        /// <summary>
        /// Event when the DAS encouterred and error
        /// </summary>
        public event DasErrorHandler Error = delegate { };

        /// <summary>
        /// Event when the DAS is disconnected
        /// </summary>
        public event DasEventHandler Disconnected = delegate { };

        /// <summary>
        /// Event when the DAS received a new message
        /// </summary>
        public event NewMessageDlg NewMessage = delegate { };

        /// <summary>
        /// System for managing the list of messages we want to receive
        /// </summary>
        protected AMPMessageManager _messageList;

        private readonly string _dasurl;
        private HttpListener? _listener;
        private CancellationTokenSource? _ctSource;

        /// <summary>
        /// Gets the DAS listener URL.
        /// </summary>
        public string DASURL => _dasurl;

        /// <summary>
        /// Initializes a new instance of the DASMessageListener class with a DAS URL.
        /// </summary>
        /// <param name="dasurl">The URL to listen to.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public DASMessageListener(string dasurl)
        {
            _messageList = new AMPMessageManager();
            _dasurl = dasurl ?? throw new ArgumentNullException(nameof(dasurl));
        }

        /// <summary>
        /// Starts the HTTP listener and begins processing incoming requests.
        /// </summary>
        /// <returns>True if started successfully, false otherwise.</returns>
        public async Task<bool> Start()
        {
            _ctSource = new CancellationTokenSource();
            CancellationToken cancellationToken = _ctSource.Token;
            _listener = new HttpListener();

            try
            {
                if (string.IsNullOrWhiteSpace(_dasurl))
                {
                    Log.Error("DAS URL is invalid.");
                    return false;
                }

                _listener.Prefixes.Add(DASURL);
                _listener.Start();

                try { Connected(); } catch (Exception ex) { Log.Warning(ex, "Error while invoking Connected event."); }

                AMPMessageProcessor requestProcessor = new(_messageList);
                requestProcessor.NewMessage += RequestProcessor_NewMessage;

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        HttpListenerContext context = await _listener.GetContextAsync();
                        await requestProcessor.ProcessRequest(context);
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected when cancelling token
                    }
                    catch (HttpListenerException hlex)
                    {
                        Log.Error(hlex, "HTTP Listener exception.");
                        try { Error(hlex); } catch (Exception ex) { Log.Warning(ex, "Error while invoking Error event."); }
                        Stop();
                        return false;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Unexpected exception in DASListener.");
                        try { Error(ex); } catch (Exception ex2) { Log.Warning(ex2, "Error while invoking Error event."); }
                        Stop();
                        return false;
                    }
                }

                return true;
            }
            finally
            {
                Stop();
            }
        }

        /// <summary>
        /// Stops the HTTP listener and cancels all ongoing tasks.
        /// </summary>
        public void Stop()
        {
            _ctSource?.Cancel();
            _ctSource = null;

            if (_listener != null)
            {
                _listener.Stop();
                _listener.Close();
                _listener = null;
                try { Disconnected(); } catch (Exception ex) { Log.Warning(ex, "Error while invoking Disconnected event."); }
            }
        }

        /// <summary>
        /// Handles new messages received by the message processor and raises the NewMessage event.
        /// </summary>
        /// <param name="cellid">The ID of the cell that sent the message.</param>
        /// <param name="message">The message payload.</param>
        private void RequestProcessor_NewMessage(string cellid, AMPMessageModel message)
        {
            try { NewMessage(cellid, message); } catch (Exception ex) { Log.Warning(ex, "Error while invoking NewMessage event."); }
        }

        /// <summary>
        /// Subscribes to a set of message names, optionally resetting previous subscriptions.
        /// </summary>
        /// <param name="messageNames">List of message names to subscribe to.</param>
        /// <param name="resetFirst">Whether to clear existing subscriptions first.</param>
        public void SubscribeToMessages(List<string> messageNames, bool resetFirst = true)
        {
            try
            {
                if (resetFirst) _messageList.UnsubscribeAll();
                _messageList.Subscribe(messageNames);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to subscribe to messages.");
            }
        }

        /// <summary>
        /// Disposes the listener and stops it if necessary.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }
}
