using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the page-to-page custom options for a search.
    /// </summary>
    public class SearchOptionsViewModel
    {
        /// <summary>
        /// Gets or sets the fields which can be searched for results.
        /// </summary>
        public Dictionary<string, string> SearchIn { get; set; }

        /// <summary>
        /// Gets or sets the filters which can be applied to the search for results.
        /// </summary>
        public Dictionary<string, string> Filter { get; set; }

        /// <summary>
        /// Gets or sets the fields by which the results can be sorted.
        /// </summary>
        public Dictionary<string, string> SortBy { get; set; }

        /// <summary>
        /// Gets or sets the directions in which the results can be sorted.
        /// </summary>
        public Dictionary<string, string> SortDirection { get; set; } = new Dictionary<string, string>
        {
             { "Ascending", "Ascending" },
             { "Descending", "Descending" }
        };

        /// <summary>
        /// Gets or sets the maximum number of items per page for results.
        /// </summary>
        public Dictionary<int, string> ItemsPerPage { get; set; } = new Dictionary<int, string>
        {
            { 10, "10" },
            { 20, "20" },
            { 50, "50" },
            { 100, "100" },
            { 200, "200" }
        };
    }
}
