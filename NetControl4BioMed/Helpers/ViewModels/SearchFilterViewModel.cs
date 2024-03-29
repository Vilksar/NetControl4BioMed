﻿namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the filters applied on a search.
    /// </summary>
    public class SearchFilterViewModel
    {
        /// <summary>
        /// Gets or sets the title (displayed text) of the filter.
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
