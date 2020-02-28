using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents a navigation title card.
    /// </summary>
    public class NavigationTitleCardViewModel
    {
        /// <summary>
        /// Gets or sets the ID of the title card.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the title (displayed text) of the title card.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the subtitle (displayed text) of the title card.
        /// </summary>
        public string Subtitle { get; set; }

        /// <summary>
        /// Gets or sets the icon of the banner.
        /// </summary>
        public string Icon { get; set; }
    }
}
