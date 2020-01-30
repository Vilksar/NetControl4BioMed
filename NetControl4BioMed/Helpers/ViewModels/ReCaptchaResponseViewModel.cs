using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the model of a reCaptcha response.
    /// </summary>
    public class ReCaptchaResponseViewModel
    {
        /// <summary>
        /// Represents the validity of the token.
        /// </summary>
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        /// <summary>
        /// Represents the score of the request.
        /// </summary>
        [JsonPropertyName("score")]
        public double Score { get; set; }

        /// <summary>
        /// Represents the action name of the request.
        /// </summary>
        [JsonPropertyName("action")]
        public string Action { get; set; }

        /// <summary>
        /// Represents the date time of the challenge load.
        /// </summary>
        [JsonPropertyName("challenge_ts")]
        public DateTime ChallengeTs { get; set; }

        /// <summary>
        /// Represents the hostname of the website that forwarded the request.
        /// </summary>
        [JsonPropertyName("hostname")]
        public string HostName { get; set; }

        /// <summary>
        /// Represents the optional error codes.
        /// </summary>
        [JsonPropertyName("error-codes")]
        public IEnumerable<string> ErrorCodes { get; set; }
    }
}
