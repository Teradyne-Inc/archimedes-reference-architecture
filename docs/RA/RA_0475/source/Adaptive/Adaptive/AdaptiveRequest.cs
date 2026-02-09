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
    /// Represents an adaptive request sent to the ATE system, including test step actions,
    /// enable word changes, and target information.
    /// </summary>
    /// <author>Teradyne DIA</author>
    public class AdaptiveRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdaptiveRequest"/> class
        /// and sets the timestamp to the current UTC time.
        /// </summary>
        public AdaptiveRequest()
        {
            TIME_STAMP = DateTime.UtcNow;
            DAS_IDENTIFIER = default!;
            TARGET = default!;
            TESTSTEP_ACTIONS = default!;
            ENABLEWORD_ACTIONS = default!;
        }

        /// <summary>
        /// The timestamp when the request was created.
        /// </summary>
        [JsonRequired]
        public DateTime TIME_STAMP { get; set; }

        /// <summary>
        /// The identifier of the DAS instance generating the request.
        /// </summary>
        [JsonRequired]
        public string DAS_IDENTIFIER { get; set; }

        /// <summary>
        /// The target system or software intended to receive the request.
        /// </summary>
        [JsonRequired]
        public AdaptiveRequestTarget TARGET { get; set; }

        /// <summary>
        /// A list of test step actions to perform (e.g., ENABLE, DISABLE, UPDATE_LIMITS).
        /// </summary>
        public List<TestStepAction> TESTSTEP_ACTIONS { get; set; }

        /// <summary>
        /// A list of enable word actions to perform (e.g., ENABLE, DISABLE).
        /// </summary>
        public List<EnableWordAction> ENABLEWORD_ACTIONS { get; set; }
    }
}
