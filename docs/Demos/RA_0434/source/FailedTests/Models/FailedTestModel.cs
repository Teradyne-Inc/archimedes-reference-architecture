using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teradyne.ARCM.RA.FailedTests.Models
{
    /// <summary>
    /// Parent class for different types of test (parametric or functional).
    /// This is focused on failed tests only.
    /// </summary>
    internal abstract class FailedTestModel
    {
        /// <summary>
        /// Sector if any (only valid on MST)
        /// </summary>
        public int? Sector { get; set; }

        /// <summary>
        /// Site number
        /// </summary>
        public int Site { get; set; }

        /// <summary>
        /// Test number as defined in the test program
        /// </summary>
        public int TestNumber { get; set; }

        /// <summary>
        /// Test name as defined in the  test program
        /// </summary>
        public string TestName { get; set; }
        
        /// <summary>
        /// Function to "serialize" the test information to be recorded in the datafile
        /// </summary>
        /// <returns>String representation of the failed test data</returns>
        public abstract string ToDataFile();
        
    }
}
