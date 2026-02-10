// This code is provided 'as is' without any guarantee of its performance, suitability, or reliability.
// The code is not intended to be used for production purposes and has not been optimized. 
// It has also not been implemented following all state-of-the-art practices. 
// The purpose of this code is to demonstrate some capabilities of UltraEdge software. 
// It is intended for demonstration purposes only. The author shall not be held liable 
// for any damages arising from the use of this code. Use of this code is solely at the user's own risk.
// Teradyne company will not be responsible for any damages arising during the use of this code. 
// The code should be treated as confidential and is protected by intellectual property rights.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Teradyne.FA.DIA.DAS.Adaptive
{
    /// <summary>
    /// Builds and serializes AdaptiveRequests to JSON for transmission.
    /// </summary>
    /// <author>Teradyne DIA</author>
    public class AdaptiveCommand 
    {
        private AdaptiveRequest _request;

 

     

        /// <summary>
        /// Initializes a new AdaptiveCommand object.
        /// </summary>
        /// <param name="DasId">DAS identifier string.</param>
        /// <param name="enableWords">Optional list of EnableWordAction objects.</param>
        /// <param name="steps">Optional list of TestStepAction objects.</param>
        public AdaptiveCommand(string DasId, List<EnableWordAction> enableWords = default!, List<TestStepAction> steps = default!)
        {
            _request = new AdaptiveRequest
            {
                TARGET = AdaptiveRequestTarget.ATE_SOFTWARE,
                TIME_STAMP = DateTime.UtcNow,
                DAS_IDENTIFIER = DasId,
                ENABLEWORD_ACTIONS = enableWords,
                TESTSTEP_ACTIONS = steps,
            };
        }

        /// <summary>
        /// Converts the AdaptiveRequest to a formatted JSON string.
        /// </summary>
        /// <returns>A JSON string, or "{}" in case of serialization error.</returns>
        public string toJSON()
        {
            try
            {
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseUpper) },
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                return JsonSerializer.Serialize(_request, options);
            }
            catch (Exception ex)
            {
            
                return "{}";
            }
        }

        /// <summary>
        /// Adds an EnableWordAction to the request.
        /// </summary>
        public bool addAction(EnableWordAction action)
        {
            try
            {
                if (action == null) return false;
                _request.ENABLEWORD_ACTIONS ??= new List<EnableWordAction>();
                _request.ENABLEWORD_ACTIONS.Add(action);
                return true;
            }
            catch (Exception ex) { return false; }
        }

        /// <summary>
        /// Adds a TestStepAction to the request.
        /// </summary>
        public bool addAction(TestStepAction action)
        {
            try
            {
                if (action == null) return false;
                _request.TESTSTEP_ACTIONS ??= new List<TestStepAction>();
                _request.TESTSTEP_ACTIONS.Add(action);
                return true;
            }
            catch (Exception ex) { return false; }
        }

        /// <summary>
        /// Creates a new EnableWordAction.
        /// </summary>
        public EnableWordAction createEnableWordAction(EnableWordActionType actionType, List<string> enableWords)
        {
            return new EnableWordAction { ACTION = actionType, ENABLEWORDS = enableWords };
        }

        /// <summary>
        /// Creates a TestStepAction based on test step names.
        /// </summary>
        public TestStepAction createTestStepAction(TestStepActionType actionType, List<string> names)
        {
            return new TestStepAction { ACTION = actionType, TESTSTEP_NAMES = names, LIMITS = default!, TESTSTEP_NUMBERS = default! };
        }

        /// <summary>
        /// Creates a TestStepAction that clears all existing configurations.
        /// </summary>
        public TestStepAction createClearAction()
        {
            return new TestStepAction { ACTION = TestStepActionType.CLEAR, TESTSTEP_NAMES = default!, TESTSTEP_NUMBERS = default!, LIMITS = default! };
        }

        /// <summary>
        /// Creates a TestStepAction based on test step numbers.
        /// </summary>
        public TestStepAction createTestStepAction(TestStepActionType actionType, List<int> numbers)
        {
            return new TestStepAction { ACTION = actionType, TESTSTEP_NUMBERS = numbers, LIMITS = default!, TESTSTEP_NAMES = default! };
        }

        /// <summary>
        /// Creates a TestStepAction with limits using step numbers.
        /// </summary>
        public TestStepAction createLimitsAction(TestStepActionType actionType, List<int> numbers, Limits limits)
        {
            return new TestStepAction { ACTION = actionType, TESTSTEP_NUMBERS = numbers, LIMITS = limits, TESTSTEP_NAMES = default! };
        }

        /// <summary>
        /// Creates a TestStepAction with limits using step names.
        /// </summary>
        public TestStepAction createLimitsAction(TestStepActionType actionType, List<string> names, Limits limits)
        {
            return new TestStepAction { ACTION = actionType, TESTSTEP_NAMES = names, LIMITS = limits, TESTSTEP_NUMBERS = default! };
        }
    }
}
