// This code is provided 'as is' without any guarantee of its performance, suitability, or reliability.
// The code is not intended to be used for production purposes and has not been optimized. 
// It has also not been implemented following all state-of-the-art practices. 
// The purpose of this code is to demonstrate some capabilities of UltraEdge software. 
// It is intended for demonstration purposes only. The author shall not be held liable 
// for any damages arising from the use of this code. Use of this code is solely at the user's own risk.
// Teradyne company will not be responsible for any damages arising during the use of this code. 
// The code should be treated as confidential and is protected by intellectual property rights.

using System.Text.Json.Serialization;

namespace Teradyne.FA.DIA.DAS.Adaptive
{
    /// <summary>
    /// Defines numerical limits and metadata associated with a test step or limit configuration.
    /// </summary>
    /// <author>Teradyne DIA</author>
    public class Limits
    {
        /// <summary>
        /// Names of the limits involved (e.g., measurement names or parameter labels).
        /// </summary>
        public List<string> LIMIT_NAMES { get; set; } = default!;

        /// <summary>
        /// Numeric IDs corresponding to the limits.
        /// </summary>
        public List<int> LIMIT_NUMBERS { get; set; } = default!;

        /// <summary>
        /// Lower limit value (can be null).
        /// </summary>
        [JsonRequired]
        public double? LO_LIMIT { get; set; }

        /// <summary>
        /// Upper limit value (can be null).
        /// </summary>
        [JsonRequired]
        public double? HI_LIMIT { get; set; }

        /// <summary>
        /// Scale factor applied to the lower limit (optional).
        /// </summary>
        public int? LLM_SCALE { get; set; }

        /// <summary>
        /// Scale factor applied to the upper limit (optional).
        /// </summary>
        public int? HLM_SCALE { get; set; }
    }
}
