using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the variables needed for an e-mail sent to a user after they were added to a network.
    /// </summary>
    public class EmailWasAddedToNetworkViewModel
    {
        /// <summary>
        /// Gets or sets the e-mail of the user that added a new user to the network.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the e-mail of the user added to the network.
        /// </summary>
        public string AddedByEmail { get; set; }

        /// <summary>
        /// Gets or sets the name of the network.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the URL to the network.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the URL to the home page of the application.
        /// </summary>
        public string ApplicationUrl { get; set; }
    }
}
