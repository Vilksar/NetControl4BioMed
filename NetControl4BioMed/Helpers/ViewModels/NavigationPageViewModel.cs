using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents a navigation page.
    /// </summary>
    public class NavigationPageViewModel
    {
        /// <summary>
        /// Gets or sets the ID of the page.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the title (displayed text) of the page.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the page.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the icon of the page.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets the color of the page.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets the link to the page.
        /// </summary>
        public string Link { get; set; }
    }
}
