using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teradyne.ARCM.RA.FailedTests.Models;

namespace Teradyne.ARCM.RA.FailedTests.DataFiles
{
    /// <summary>
    /// The failed tests data file is a text file.
    /// Each touchdown starts with a line containing the string "[Touchdown]"
    /// For each touchdown, there is one line per reported failed test.
    /// </summary>
    internal class FailedTestsDataFile
    {
        /// <summary>
        /// Output folder. Arbitrarily chosen to be C:\temp.
        /// For a production use, this would be made configurable.
        /// </summary>
        public const string OUTPUT_DIRECTORY = @"C:\temp\";

        /// <summary>
        /// Output filename
        /// </summary>
        protected string filename;

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="sublot_id">Sublot ID used in the filename (the id comes from the SUBLOT_START message)</param>
        public FailedTestsDataFile(String sublot_id)
        {
            if (string.IsNullOrWhiteSpace(sublot_id))
            {
                sublot_id = "UNKNOWN_SUBLOT_ID";
            }
            else
            {
                sublot_id = SanitizeFileName(sublot_id);
            }
            filename = BuildFileName(sublot_id);
        }

        protected string SanitizeFileName(string input)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(input.Select(ch => invalidChars.Contains(ch) ? '-' : ch).ToArray());
            return sanitized;
        }

        /// <summary>
        /// Use the current date and time to build a string timestamp
        /// </summary>
        /// <returns>String representation of the current date-time</returns>
        protected string BuildTimeStamp()
        {
            return DateTime.Now.ToString("yyyyMMdd_HHmmss");
        }


      

        /// <summary>
        /// Arbitrary choice for the output filename format
        /// </summary>
        /// <param name="sublot_id">Sublot ID to use for the filename</param>
        /// <returns>Filename</returns>
        protected string BuildFileName(string sublot_id)
        {
            return $"{OUTPUT_DIRECTORY}AMPDAS_FAILEDTESTS_{sublot_id}_{BuildTimeStamp()}.txt";
        }

        /// <summary>
        /// Save all the failed tests data to the data file.
        /// Used when the SUBLOT_END message is received.
        /// </summary>
        /// <param name="tdList">List of touchdowns to save</param>
        public void saveToFile(TouchdownsList tdList)
        {
            if (tdList == null) return;

            try
            {
                int counter = 1;
                StreamWriter sw = new StreamWriter(filename);
                foreach (Touchdown td in tdList.touchdowns)
                {
                    sw.WriteLine($"[Touchdown_{counter.ToString("000")}]");
                    counter++;
                    foreach (FailedTestModel ftm in td.failedTests)
                        sw.WriteLine(ftm.ToDataFile());
                }                    
                sw.Close();
                Console.WriteLine();
                Console.WriteLine($"Failed tests data has been written to {filename}.");
            }
            catch { }
        }
    }
}
