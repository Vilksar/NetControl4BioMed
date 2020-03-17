using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the model of a Hangfire recurring cleaner.
    /// </summary>
    public class HangfireRecurringCleanerViewModel
    {
        /// <summary>
        /// Gets or sets the HTTP context.
        /// </summary>
        public HttpContext HttpContext { get; set; }
    }
}
