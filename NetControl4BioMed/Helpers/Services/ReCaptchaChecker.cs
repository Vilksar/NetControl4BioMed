using Microsoft.Extensions.Configuration;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Services
{
    /// <summary>
    /// Implements a reCaptcha checker.
    /// </summary>
    public class ReCaptchaChecker : IReCaptchaChecker
    {
        /// <summary>
        /// Represents the configuration.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Represents the HTTP 
        /// </summary>
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="emailSender">The e-mail sender.</param>
        public ReCaptchaChecker(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Checks if the provided token is valid.
        /// </summary>
        /// <param name="token">The reCaptcha token to be checked.</param>
        /// <returns>True if the reCaptcha succeeded, false otherwise.</returns>
        public async Task<bool> IsValid(string token)
        {
            // Create a new http client.
            var client = _httpClientFactory.CreateClient();
            // Create the values for the request.
            var requestValues = new Dictionary<string, string> { { "secret", _configuration["Authentication:reCaptcha:SecretKey"] }, { "response", token } };
            // Create the content for the request.
            var requestContent = new FormUrlEncodedContent(requestValues);
            // Get the reCaptcha response.
            var response = await client.PostAsync(_configuration["Authentication:reCaptcha:Url"], requestContent);
            // Get the content of the response.
            var responseContent = await response.Content.ReadAsStringAsync();
            // Get the result of the response.
            var responseResult = JsonSerializer.Deserialize<ReCaptchaResponseViewModel>(responseContent);
            // Return the status of the response.
            return responseResult.Success && 0.5 < responseResult.Score;
        }
    }
}