using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teradyne.ARCM.RA.FailedTests.Models
{
    /// <summary>
    /// Failed functional test representation.    
    /// </summary>
    internal class FailedFunctionalTestModel : FailedTestModel
    {
        /// <summary>
        /// Function to "serialize" the functional test information to be recorded in the datafile.
        /// In this example, we only care about the status Pass/Fail of functional tests.
        /// We only list the tests reported 'fail'.
        /// </summary>
        /// <returns>String representation of the failed test data</returns>
        public override string ToDataFile()
        {            
            string entry = $"S:{Site}|TNum:{TestNumber}|TName:{TestName}";
            if (Sector.HasValue) entry = $"SCT:{Sector.Value}|{entry}";
            entry = $"FUNCTIONAL|{entry}";
            return entry;
        }
    }
}
