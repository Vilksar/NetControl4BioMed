using System;
using System.Collections.Generic;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the variables for an analyses delete alert e-mail.
    /// </summary>
    public class EmailAlertDeleteViewModel
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
        /// Gets or sets the network items.
        /// </summary>
        public IEnumerable<ItemModel> NetworkItems { get; set; }

        /// <summary>
        /// Gets or sets the analysis items.
        /// </summary>
        public IEnumerable<ItemModel> AnalysisItems { get; set; }

        /// <summary>
        /// Gets or sets the URL to the home page of the application.
        /// </summary>
        public string ApplicationUrl { get; set; }

        /// <summary>
        /// Represents the model of an item.
        /// </summary>
        public class ItemModel
        {
            /// <summary>
            /// Gets or sets the item ID.
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// Gets or sets the item name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the item URL.
            /// </summary>
            public string Url { get; set; }
        }
    }
}
