using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teradyne.ARCM.RA.FailedTests.Models
{
    /// <summary>
    /// Failed parametric test representation
    /// </summary>
    internal class FailedParametricTestModel: FailedTestModel
    {
        /// <summary>
        /// Test measure
        /// </summary>
        public double Measure { get; set; }

        /// <summary>
        /// Low limit for this test as defined in the test program
        /// </summary>
        public double LoLimit { get; set; }

        /// <summary>
        /// Low limit for this test as defined in the test program
        /// </summary>
        public double HiLimit { get; set; }
        
        /// <summary>
        /// Low limit scaling as defined in the test program
        /// </summary>
        public int LoScale { get; set; }

        /// <summary>
        /// High limit scaling as defined in the test program
        /// </summary>
        public int HiScale { get; set; }

        /// <summary>
        /// Result/Measure scaling as defined in the test program
        /// </summary>
        public int ResScale { get; set; }

        /// <summary>
        /// Function to "serialize" the test information to be recorded in the datafile
        /// </summary>
        /// <returns>String representation of the failed test data</returns>
        public override string ToDataFile()
        {
            string entry = $"S:{Site}|TNum:{TestNumber}|TName:{TestName}|R:{Measure}|LLM:{LoLimit}|HLM:{HiLimit}|RS:{ResScale}|LLS:{LoScale}|HLS:{HiScale}";
            if (Sector.HasValue) entry = $"SCT:{Sector.Value}|{entry}";
            return entry;
        }
    }
}
