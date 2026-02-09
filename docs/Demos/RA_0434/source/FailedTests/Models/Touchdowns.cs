using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teradyne.ARCM.RA.FailedTests.Models
{
    /// <summary>
    /// List of failed tests for one touchdown
    /// </summary>
    internal class Touchdown
    {
        /// <summary>
        /// List of failed tests (parametric and functional)
        /// </summary>
        public List<FailedTestModel> failedTests { get; private set; }

        /// <summary>
        /// Initialization
        /// </summary>
        public Touchdown()
        {
            failedTests = new List<FailedTestModel>();
        }
    }

    /// <summary>
    /// List of touchdowns (this corresponds to the data we are interested in for each sublot)
    /// </summary>
    internal class TouchdownsList
    {
        /// <summary>
        /// List of touchdowns
        /// </summary>
        public List<Touchdown> touchdowns { get; private set; }

        /// <summary>
        /// Initialization
        /// </summary>
        public TouchdownsList()
        {
            touchdowns = new List<Touchdown>();
        }
    }
}
