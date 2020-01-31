using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the filters applied on a search.
    /// </summary>
    public class SearchFilterViewModel
    {
        /// <summary>
        /// Gets or sets the text to be displayed for the filter.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the link to cancel the filter.
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Gets or sets the type of the filter.
        /// </summary>
        public string Type { get; set; }
    }
}
