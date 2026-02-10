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
    /// Represents an action that can be applied to one or more test steps in an adaptive test scenario.
    /// </summary>
    /// <author>Teradyne DIA</author>
    public class TestStepAction
    {
        /// <summary>
        /// The type of action to apply (e.g., ENABLE, DISABLE).
        /// </summary>
        [JsonRequired]
        public TestStepActionType ACTION { get; set; } = default!;

        /// <summary>
        /// A list of test step names to which the action applies.
        /// </summary>
        public List<string> TESTSTEP_NAMES { get; set; } = default!;

        /// <summary>
        /// A list of test step numbers to which the action applies.
        /// </summary>
        public List<int> TESTSTEP_NUMBERS { get; set; } = default!;

        /// <summary>
        /// The limit configuration to be used if the action is UPDATE_LIMITS.
        /// </summary>
        public Limits LIMITS { get; set; } = default!;
    }
}
