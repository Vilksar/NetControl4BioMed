using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the model of an analysis runner.
    /// </summary>
    public class AnalysisRunnerViewModel
    {
        /// <summary>
        /// Gets or sets the analysis ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the HTTP context.
        /// </summary>
        public HttpContext HttpContext { get; set; }
    }
}
