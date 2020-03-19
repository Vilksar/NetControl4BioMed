using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the variables for an e-mail changed e-mail.
    /// </summary>
    public class EmailEmailChangedViewModel
    {
        /// <summary>
        /// Gets or sets the old user e-mail.
        /// </summary>
        public string OldEmail { get; set; }

        /// <summary>
        /// Gets or sets the old user e-mail.
        /// </summary>
        public string NewEmail { get; set; }

        /// <summary>
        /// Gets or sets the URL to the account page.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the URL to the home page of the application.
        /// </summary>
        public string ApplicationUrl { get; set; }
    }
}
