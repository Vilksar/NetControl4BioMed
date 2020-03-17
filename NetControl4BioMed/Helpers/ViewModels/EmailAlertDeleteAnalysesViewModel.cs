using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the variables for an analyses delete alert e-mail.
    /// </summary>
    public class EmailAlertDeleteAnalysesViewModel
    {
        /// <summary>
        /// Gets or sets the user e-mail.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the date of the deletion.
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        public IEnumerable<ItemModel> Items { get; set; }

        /// <summary>
        /// Gets or sets the URL to the home page of the application.
        /// </summary>
        public string ApplicationUrl { get; set; }

        /// <summary>
        /// Represents the model of an analysis item.
        /// </summary>
        public class ItemModel
        {
            /// <summary>
            /// Gets or sets the analysis ID.
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// Gets or sets the analysis name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the analysis URL.
            /// </summary>
            public string Url { get; set; }
        }
    }
}
