// This code is provided 'as is' without any guarantee of its performance, suitability, or reliability.
// The code is not intended to be used for production purposes and has not been optimized. 
// It has also not been implemented following all state-of-the-art practices. 
// The purpose of this code is to demonstrate some capabilities of UltraEdge software. 
// It is intended for demonstration purposes only. The author shall not be held liable 
// for any damages arising from the use of this code. Use of this code is solely at the user's own risk.
// Teradyne company will not be responsible for any damages arising during the use of this code. 
// The code should be treated as confidential and is protected by intellectual property rights.

using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace Teradyne.FA.DIA.DAS.Adaptive
{
    /// <summary>
    /// Specifies the type of action to apply on enable words in adaptive control.
    /// </summary>
    /// <author>Teradyne DIA</author>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EnableWordActionType
    {
        /// <summary>
        /// Enables a feature or test based on the enable word.
        /// </summary>
        ENABLE = 0,

        /// <summary>
        /// Disables a feature or test based on the enable word.
        /// </summary>
        DISABLE = 1
    }
}
