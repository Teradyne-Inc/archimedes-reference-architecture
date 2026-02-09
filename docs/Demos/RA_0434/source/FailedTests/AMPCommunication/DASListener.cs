using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Teradyne.ARCM.RA.FailedTests.AMPMessages;
using static Teradyne.ARCM.RA.FailedTests.AMPCommunication.AMPEvents;

// This code is provided 'as is' without any guarantee of its performance, suitability, or reliability.
// The code is not intended to be used for production purposes and has not been optimized. 
// It has also not been implemented following all state-of-the-art practices. 
// The purpose of this code is to demonstrate some capabilities of OI.NET software. 
// It is intended for demonstration purposes only. The author shall not be held liable 
// for any damages arising from the use of this code. Use of this code is solely at the user's own risk.
// Teradyne company will not be responsible for any damages arising during the use of this code. 
// The code should be treated as confidential and is protected by intellectual property rights. 

namespace Teradyne.ARCM.RA.FailedTests.AMPCommunication
{
    /// <summary>
    /// Represents a DAS listener for managing incoming requests and events.
    /// </summary>
    public class DASListener : IDisposable
    {
        /// <summary>
        /// Delegate for DAS connection and disconnection events.
        /// </summary>
        public delegate void DasEventHandler();

        /// <summary>
        /// Delegate for handling errors during DAS operations.
        /// </summary>
        /// <param name="exception">The exception that occurred.</param>
        public delegate void DasErrorHandler(Exception exception);

        /// <summary>
        /// Event triggered when a connection is established.
        /// </summary>
        public event DasEventHandler Connected;

        /// <summary>
        /// Event triggered when an error occurs.
        /// </summary>
        public event DasErrorHandler Error;

        /// <summary>
        /// Event triggered when the listener is disconnected.
        /// </summary>
        public event DasEventHandler Disconnected;

        /// <summary>
        /// Event triggered when a new message is received.
        /// </summary>
        public event NewMessageDlg NewMessage;

        /// <summary>
        /// List of messages managed by this application
        /// </summary>
        protected AMP_MESSAGE_LIST _messageList;

        // DAS URL
        private string _dasurl;

        // HTTP communication object
        private HttpListener _listener;

        // Flag to stop the processing thread
        private CancellationTokenSource _ctSource;        

        /// <summary>
        /// Gets or sets the DAS URL for the listener.
        /// </summary>
        public string DASURL
        {
            get => _dasurl;
            set => _dasurl = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DASListener"/> class with the specified URL.
        /// </summary>
        /// <param name="dasurl">The URL for the DAS listener.</param>
        public DASListener(string dasurl)
        {            
            _messageList = new AMP_MESSAGE_LIST();
            _dasurl = dasurl;
            _listener = null;
            _ctSource = null;
        }

        /// <summary>
        /// Starts the DAS listener asynchronously.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// Returns true if the listener was started successfully; otherwise, false.
        /// </returns>
        /// <exception cref="HttpListenerException">Thrown when the HTTP listener encounters an error.</exception>
        public async Task<bool> Start()
        {
            // Init loop async control
            _ctSource = new CancellationTokenSource();
            CancellationToken cancellationToken = _ctSource.Token;
            
            // Init HTTP listener
            _listener = new HttpListener();

            try
            {
                // Check URL
                if (string.IsNullOrWhiteSpace(_dasurl))
                {
                    Console.WriteLine("DAS URL is invalid.");
                    return false;
                }

                // Setup HTTP connection
                _listener.Prefixes.Add(DASURL);
                _listener.Start();
                Connected?.Invoke();

                RequestProcessor requestProcessor = new RequestProcessor(_messageList);
                requestProcessor.NewMessage += RequestProcessor_NewMessage;

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // Get input from the HTTP listener
                        Task<HttpListenerContext> contextTask = _listener.GetContextAsync();

                        try
                        {
                            contextTask.Wait(cancellationToken); // complete the Task (get an input) or wait until the cancellationtoken was canceled
                            HttpListenerContext context = contextTask.Result;
                            await requestProcessor.ProcessRequest(context);
                        }
                        catch (OperationCanceledException)
                        {
                            // nothing to do, the cancelation will be seen by the while loop condition
                        }
                    }
                    catch (HttpListenerException hlex)
                    {
                        Error?.Invoke(hlex);
                        Console.WriteLine($"HTTP Listener exception: {hlex.Message}");
                        // Cancellation requested
                        _ctSource.Cancel();
                        return false;
                    }
                    catch (Exception ex)
                    {
                        Error?.Invoke(ex);
                        Console.WriteLine($"Unexpected exception: {ex.Message}");
                        // Cancellation requested
                        _ctSource.Cancel();
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
        /// Stops the DAS listener and cleans up resources.
        /// </summary>
        public void Stop()
        {
            if (_ctSource != null)
            {
                _ctSource.Cancel();
                _ctSource = null;
            }

            if (_listener != null)
            {
                _listener.Stop();
                _listener.Close();
                _listener = null;
                Disconnected?.Invoke();
            }
        }

        /// <summary>
        /// Handles new messages from the request processor.
        /// </summary>
        /// <param name="cellid">The cell ID associated with the message.</param>
        /// <param name="message">The message model.</param>
        private void RequestProcessor_NewMessage(string cellid, AMPMessageModel message)
        {
            if (NewMessage != null)
            {
                NewMessage.Invoke(cellid, message);
            }
        }

        /// <summary>
        /// Allows DAS applications to specify the list of messages to subscribe to.
        /// </summary>
        /// <param name="messageNames">List of messages to subscribe to</param>
        /// <param name="resetFirst">Possibility to unsubscribe from all messages first (enabled by default)</param>
        public void subscribeToListOfMessages(List<string> messageNames, bool resetFirst = true)
        {
            if (resetFirst) _messageList.unsubscribeAll();
            _messageList.subscribe(messageNames);
        }

        /// <summary>
        /// Releases all resources used by the DASListener.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }
}
