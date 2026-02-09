using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FailedTests.Utilities;

namespace Teradyne.ARCM.RA.FailedTests
{
    internal class Program
    {
        /// <summary>
        /// Top-level entry point for this AMP DAS executable.
        /// </summary>
        /// <param name="args">N/A</param>
        /// <returns>N/A</returns>
        static async Task Main(string[] args)
        {
            List<string> welcome = new List<string>()
                {
                "Welcome to the Archimedes Reference Architecture AMP Failed Tests example.",
                "",
                "An AMP Data Application Server (DAS) has just been started.",
                "Its purpose is to grab all failed tests and save them to a text data file.",
                "",
                "Note: 'STATUS' and 'T_STATUS' messages are not displayed for clarity.",
                "      Only subscribed and mandatory messages are displayed."
                };

            Utils.OutputHeader(welcome);

            // Starting the main engine accepting and processing messages
            AMP_DASExecution execution = new AMP_DASExecution();            
            await execution.Run();
        }
    }
}
