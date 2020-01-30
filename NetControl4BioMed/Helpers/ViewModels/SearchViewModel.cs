using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents a search to be performed.
    /// </summary>
    /// <typeparam name="T">Represents the model of any one element returned by the search.</typeparam>
    public class SearchViewModel<T>
    {
        /// <summary>
        /// Gets or sets the list of items returned by the search.
        /// </summary>
        public IEnumerable<T> Items { get; set; }

        /// <summary>
        /// Gets or sets the input parameters for the search.
        /// </summary>
        public SearchInputViewModel Input { get; set; }

        /// <summary>
        /// Gets or sets the links for clearing the applied filters.
        /// </summary>
        public Dictionary<string, string> AppliedFilters { get; set; }

        /// <summary>
        /// Gets or sets the pagination parameters of the search.
        /// </summary>
        public SearchPaginationViewModel Pagination { get; set; }

        /// <summary>
        /// Initializes a new instance of the search.
        /// </summary>
        /// <param name="linkGenerator">Represents the link generator.</param>
        /// <param name="httpContext">Represents the current HTTP context.</param>
        /// <param name="input">Represents the input parameters.</param>
        /// <param name="query">Represents the query agains which to perform the search.</param>
        public SearchViewModel(LinkGenerator linkGenerator, HttpContext httpContext, SearchInputViewModel input, IQueryable<T> query, string id = null)
        {
            // Define the input.
            Input = input;
            // Get the pagination.
            Pagination = new SearchPaginationViewModel(linkGenerator, httpContext, Input, query.Count());
            // Define the current searches and filters.
            AppliedFilters = new Dictionary<string, string>();
            // Check if there is any search applied.
            if (Input.SearchIn.Any())
            {
                // Go over each of the search locations, get their names, and the links for clearing.
                foreach (var value in Input.SearchIn)
                {
                    var name = $"Search: \"{Input.SearchString}\" in {Input.Options.SearchIn[value]}";
                    var link = linkGenerator.GetPathByRouteValues(httpContext: httpContext, routeName: null, values: new { id = id ?? null, searchString = Input.SearchIn.Count() == 1 ? string.Empty : Input.SearchString, searchIn = Input.SearchIn.Where(item => item != value), filter = Input.Filter, sortBy = Input.SortBy, sortDirection = Input.SortDirection, itemsPerPage = Input.ItemsPerPage, currentPage = 1 });
                    AppliedFilters.Add(name, link);
                }
                AppliedFilters.Add("Search: Clear all", linkGenerator.GetPathByRouteValues(httpContext: httpContext, routeName: null, values: new { id = id ?? null, searchString = string.Empty, searchIn = Enumerable.Empty<string>(), filter = Input.Filter, sortBy = Input.SortBy, sortDirection = Input.SortDirection, itemsPerPage = Input.ItemsPerPage, currentPage = 1 }));
            }
            // Check if there are any filters applied.
            if (Input.Filter.Any())
            {
                // Go over each of the applied filters, get their names, and the links for clearing.
                foreach (var value in Input.Filter)
                {
                    var name = $"Filter: {Input.Options.Filter[value]}";
                    var link = linkGenerator.GetPathByRouteValues(httpContext: httpContext, routeName: null, values: new { id = id ?? null, searchString = Input.SearchString, searchIn = Input.SearchIn, filter = Input.Filter.Where(item => item != value), sortBy = Input.SortBy, sortDirection = Input.SortDirection, itemsPerPage = Input.ItemsPerPage, currentPage = 1 });
                    AppliedFilters.Add(name, link);
                }
                AppliedFilters.Add("Filter: Clear all", linkGenerator.GetPathByRouteValues(httpContext: httpContext, routeName: null, values: new { id = id ?? null, searchString = Input.SearchString, searchIn = Input.SearchIn, filter = Enumerable.Empty<string>(), sortBy = Input.SortBy, sortDirection = Input.SortDirection, itemsPerPage = Input.ItemsPerPage, currentPage = 1 }));
            }
            // Get only the items on the current page, as enumerable.
            Items = query.Skip((Input.CurrentPage - 1) * Input.ItemsPerPage).Take(Input.ItemsPerPage).AsEnumerable();
        }
    }
}
