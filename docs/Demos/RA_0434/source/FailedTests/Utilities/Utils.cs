using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailedTests.Utilities
{
    internal class Utils
    {
        /// <summary>
        /// Function to display a welcome message as a banner
        /// </summary>
        /// <param name="welcome">Message to display</param>
        public static void OutputHeader(List<string> welcome)
        {
            int maxLen = welcome.Select(s => s.Length).Max();
            List<string> contents = welcome.Select(s => $"* {s}{new String(' ', maxLen - s.Length)} *").ToList();
            Console.WriteLine(new String('*', maxLen + 4)); // + 4 to account for the leading *{space} and trailing {space}*
            Console.WriteLine(String.Join(Environment.NewLine, contents));
            Console.WriteLine(new String('*', maxLen + 4));
            Console.WriteLine();
        }
    }
}
