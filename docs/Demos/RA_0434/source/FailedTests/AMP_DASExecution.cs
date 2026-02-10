using System;
using System.Threading.Tasks;
using Teradyne.ARCM.RA.FailedTests.AMPCommunication;
using Teradyne.ARCM.RA.FailedTests.AMPMessages;

namespace Teradyne.ARCM.RA.FailedTests
{
    /// <summary>
    /// This is the main engine accepting and processing messages coming from the testing environment
    /// </summary>
    internal class AMP_DASExecution
    {
        /// <summary>
        /// URL address for this AMP DAS
        /// </summary>
        public const string DAS_URL = "http://+:4200/FailedTests/";

        /// <summary>
        /// Low-level component handling communication (receiving messages and sending back status codes and responses)
        /// </summary>
        protected DASListener failedTestsDAS;        

        /// <summary>
        /// Application specific message processing
        /// </summary>
        protected AMP_Messages_Processor myAMPProcessor;

        /// <summary>
        /// Initialization of the components and connections
        /// </summary>
        public AMP_DASExecution()
        {
            // DAS Server creation and setup
            failedTestsDAS = new DASListener(DAS_URL);            

            // Messages processor
            myAMPProcessor = new AMP_Messages_Processor();

            // Subscribe to messages
            failedTestsDAS.subscribeToListOfMessages(myAMPProcessor.AMP_Message_Names);

            // DAS events management
            failedTestsDAS.Connected += failedTestsDAS_Connected;
            failedTestsDAS.Error += failedTestsDAS_Error;
            failedTestsDAS.Disconnected += failedTestsDAS_Disconnected;
            failedTestsDAS.NewMessage += failedTestsDAS_NewMessage;
        }

        /// <summary>
        /// Top-level start of the DAS application
        /// </summary>
        /// <returns>Task</returns>
        public async Task Run()
        {
            // Starting the asynchronous DAS
            await failedTestsDAS.Start();
        }

        /// <summary>
        /// Request to stop the DAS
        /// </summary>
        public void StopDAS()
        {
            failedTestsDAS.Stop();
        }

        /// <summary>
        /// A new AMP message has been received.
        /// A debug information is output and the message is processed by the application specific component.
        /// </summary>
        /// <param name="origin">Message producer</param>
        /// <param name="msg">Message object after initial JSON parsing</param>
        private void failedTestsDAS_NewMessage(string origin, AMPMessageModel msg)
        {
            if (msg.MessageName == "STATUS") return;
            if (msg.MessageName == "T_STATUS") return;

            Console.WriteLine($"We received a new message: {msg.MessageName}");
            // Schedule the message for asynchronous processing (added to a queue)
            Task.Run(() => myAMPProcessor.ScheduleMessageForProcessing(origin, msg));
        }

        /// <summary>
        /// DAS Connection event.
        /// The application specific component is informed to start processing messages
        /// </summary>
        private void failedTestsDAS_Connected()
        {
            Console.WriteLine("The DAS is Connected");
            myAMPProcessor?.StartProcessing();
        }

        /// <summary>
        /// DAS disconnection event.
        /// The application specific component is informed to stop processing messages
        /// </summary>
        private void failedTestsDAS_Disconnected()
        {
            myAMPProcessor?.StopProcessing();            
            Console.WriteLine("The DAS is disconnected");
        }

        /// <summary>
        /// Error management.
        /// In this example, the exception is simply displayed.
        /// </summary>
        /// <param name="exception">Error that was detected</param>
        private void failedTestsDAS_Error(Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
        }


    }
}
