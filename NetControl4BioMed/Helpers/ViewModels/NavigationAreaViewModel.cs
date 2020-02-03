using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents a navigation area, formed by several navigation groups.
    /// </summary>
    public class NavigationAreaViewModel
    {
        /// <summary>
        /// Gets or sets the ID of the area.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the title (displayed text) of the area.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the area.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the icon of the area.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets the color of the area.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets the navigation groups in the area.
        /// </summary>
        public IEnumerable<NavigationGroupViewModel> Groups { get; set; }
    }
}
