using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the variables for a network end e-mail.
    /// </summary>
    public class EmailNetworkEndedViewModel
    {
        /// <summary>
        /// Gets or sets the user e-mail.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the analysis ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the analysis name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the analysis status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the analysis URL.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the URL to the home page of the application.
        /// </summary>
        public string ApplicationUrl { get; set; }
    }
}
