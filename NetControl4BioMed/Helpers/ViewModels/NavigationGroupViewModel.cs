using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents a group of navigation pages.
    /// </summary>
    public class NavigationGroupViewModel
    {
        /// <summary>
        /// Gets or sets the ID of the group.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the title (displayed text) of the group.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the group.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the icon of the group.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets the color of the group.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets the link to the group.
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Gets or sets the navigation pages in the group.
        /// </summary>
        public IEnumerable<NavigationPageViewModel> NavigationPages { get; set; }
    }
}
