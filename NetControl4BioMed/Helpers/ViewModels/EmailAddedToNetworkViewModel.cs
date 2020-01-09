using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the variables needed for an e-mail sent to a user after they added a new user to a generic network.
    /// </summary>
    public class EmailAddedToNetworkViewModel
    {
        /// <summary>
        /// Gets or sets the e-mail of the user that added a new user to the network.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the e-mail of the user added to the network.
        /// </summary>
        public string AddedEmail { get; set; }

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
