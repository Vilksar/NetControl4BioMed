using System;
using System.Collections.Generic;
using System.Linq;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the input needed for performing a search.
    /// </summary>
    public class SearchInputViewModel
    {
        /// <summary>
        /// Gets or sets the search input options.
        /// </summary>
        public SearchOptionsViewModel Options { get; set; }

        /// <summary>
        /// Gets or sets the ID of the currently displayed item (if it exists).
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the text to search for.
        /// </summary>
        public string SearchString { get; set; }

        /// <summary>
        /// Gets or sets the JSON string containing the fields in which to search for the text.
        /// </summary>
        public IEnumerable<string> SearchIn { get; set; }

        /// <summary>
        /// Gets or sets the JSON string containing the filters to be applied to the results.
        /// </summary>
        public IEnumerable<string> Filter { get; set; }

        /// <summary>
        /// Gets or sets the field based which to sort the results.
        /// </summary>
        public string SortBy { get; set; }

        /// <summary>
        /// Gets or sets the direction in which to sort the results.
        /// </summary>
        public string SortDirection { get; set; }

        /// <summary>
        /// Gets or sets the number of items to be displayed at one time, on a single page.
        /// </summary>
        public int ItemsPerPage { get; set; }

        /// <summary>
        /// Gets or sets the current page of the search.
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Gets or sets a flag which indicates if any of the provided search parameters was null (or default).
        /// </summary>
        public bool NeedsRedirect { get; set; }

        /// <summary>
        /// Initializes a new instance of the search input.
        /// </summary>
        /// <param name="options">Represents the search input options.</param>
        /// <param name="searchString">Represents the text to search for.</param>
        /// <param name="searchIn">Represents the fields in which to search for the given text.</param>
        /// <param name="filter">Represents the filters to be applied to the results.</param>
        /// <param name="sortBy">Represents the field based which to sort the results.</param>
        /// <param name="sortDirection">Represents the direction in which to sort the results.</param>
        /// <param name="itemsPerPage">Represents the number of items to be displayed at one time, on a single page.</param>
        /// <param name="currentPage">Represents the current page of the search.</param>
        public SearchInputViewModel(SearchOptionsViewModel options, string id = null, string searchString = null, IEnumerable<string> searchIn = null, IEnumerable<string> filter = null, string sortBy = null, string sortDirection = null, int? itemsPerPage = null, int? currentPage = 1)
        {
            // Check the search options for possible errors.
            if (options.Filter == null || options.ItemsPerPage == null || options.SearchIn == null || options.SortBy == null || options.SortDirection == null)
            {
                // Throw an exception.
                throw new ArgumentException();
            }
            // Assign the search options.
            Options = options;
            // Check if the given parameters are the default ones.
            NeedsRedirect = searchIn == null || filter == null || string.IsNullOrEmpty(sortBy) || !Options.SortBy.ContainsKey(sortBy) || string.IsNullOrEmpty(sortDirection) || !Options.SortDirection.ContainsKey(sortDirection) || itemsPerPage == null || itemsPerPage.Value < 1;
            // Get the default values for those that are null or invalid.
            searchString = string.IsNullOrEmpty(searchString) ? string.Empty : searchString;
            searchIn = searchIn ?? Enumerable.Empty<string>();
            filter = filter ?? Enumerable.Empty<string>();
            sortBy = string.IsNullOrEmpty(sortBy) || !Options.SortBy.ContainsKey(sortBy) ? Options.SortBy.FirstOrDefault().Key : sortBy;
            sortDirection = string.IsNullOrEmpty(sortDirection) || !Options.SortDirection.ContainsKey(sortDirection) ? Options.SortDirection.FirstOrDefault().Key : sortDirection;
            itemsPerPage = itemsPerPage == null || itemsPerPage.Value < 1 ? Options.ItemsPerPage.FirstOrDefault().Key : itemsPerPage.Value;
            currentPage = currentPage == null || currentPage.Value < 1 ? 1 : currentPage.Value;
            // Define the properties.
            Id = id;
            SearchString = searchString;
            SortBy = sortBy;
            SortDirection = sortDirection;
            ItemsPerPage = itemsPerPage.Value;
            CurrentPage = currentPage.Value;
            SearchIn = searchIn.Intersect(Options.SearchIn.Keys);
            Filter = filter.Intersect(Options.Filter.Keys);
            // Check if there is a search string applied, but there are no values selected to search in.
            if (!string.IsNullOrEmpty(SearchString) && !SearchIn.Any())
            {
                // Select all of them.
                SearchIn = Options.SearchIn.Keys;
            }
        }
    }
}
