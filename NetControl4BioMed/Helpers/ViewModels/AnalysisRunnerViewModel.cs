using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Models
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
        /// Gets or sets the URL to the analysis page.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the URL to the home page.
        /// </summary>
        public string ApplicationUrl { get; set; }
    }
}
