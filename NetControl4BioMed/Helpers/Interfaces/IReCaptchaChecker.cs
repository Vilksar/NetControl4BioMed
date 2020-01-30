using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Interfaces
{
    /// <summary>
    /// Provides an abstraction for the reCaptcha checker.
    /// </summary>
    public interface IReCaptchaChecker
    {
        /// <summary>
        /// Checks if the provided token is valid.
        /// </summary>
        /// <param name="token">The reCaptcha token to be checked.</param>
        /// <returns>True if the reCaptcha succeeded, false otherwise.</returns>
        Task<bool> IsValid(string token);
    }
}
