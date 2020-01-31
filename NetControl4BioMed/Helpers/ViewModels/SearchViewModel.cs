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
        public List<T> Items { get; set; }

        /// <summary>
        /// Gets or sets the input parameters for the search.
        /// </summary>
        public SearchInputViewModel Input { get; set; }

        /// <summary>
        /// Gets or sets the links for clearing the applied filters.
        /// </summary>
        public List<SearchFilterViewModel> Filters { get; set; }

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
        public SearchViewModel(LinkGenerator linkGenerator, HttpContext httpContext, SearchInputViewModel input, IQueryable<T> query)
        {
            // Define the input.
            Input = input;
            // Get the pagination.
            Pagination = new SearchPaginationViewModel(linkGenerator, httpContext, Input, query.Count());
            // Define the current searches and filters.
            Filters = new List<SearchFilterViewModel>();
            // Check if there is any search applied.
            if (Input.SearchIn.Any())
            {
                // Go over each of the search locations.
                foreach (var value in Input.SearchIn)
                {
                    // Add a corresponding new filter.
                    Filters.Add(new SearchFilterViewModel
                    {
                        Text = $"Search: \"{Input.SearchString}\" in {Input.Options.SearchIn[value]}",
                        Link = linkGenerator.GetPathByRouteValues(httpContext: httpContext, routeName: null, values: new { id = Input.Id, searchString = Input.SearchIn.Count() == 1 ? string.Empty : Input.SearchString, searchIn = Input.SearchIn.Where(item => item != value), filter = Input.Filter, sortBy = Input.SortBy, sortDirection = Input.SortDirection, itemsPerPage = Input.ItemsPerPage, currentPage = 1 }),
                        Type = "SearchIn"
                    });
                }
                // Add a new filter for all search locations.
                Filters.Add(new SearchFilterViewModel
                {
                    Text = "Search: Clear all",
                    Link = linkGenerator.GetPathByRouteValues(httpContext: httpContext, routeName: null, values: new { id = Input.Id, searchString = string.Empty, searchIn = Enumerable.Empty<string>(), filter = Input.Filter, sortBy = Input.SortBy, sortDirection = Input.SortDirection, itemsPerPage = Input.ItemsPerPage, currentPage = 1 }),
                    Type = "All"
                });
            }
            // Check if there are any filters applied.
            if (Input.Filter.Any())
            {
                // Go over each of the applied filters.
                foreach (var value in Input.Filter)
                {
                    // Add a corresponding new filter.
                    Filters.Add(new SearchFilterViewModel
                    {
                        Text = $"Filter: {Input.Options.Filter[value]}",
                        Link = linkGenerator.GetPathByRouteValues(httpContext: httpContext, routeName: null, values: new { id = Input.Id, searchString = Input.SearchString, searchIn = Input.SearchIn, filter = Input.Filter.Where(item => item != value), sortBy = Input.SortBy, sortDirection = Input.SortDirection, itemsPerPage = Input.ItemsPerPage, currentPage = 1 }),
                        Type = "Filter"
                    });
                }
                // Add a new filter for all applied filters.
                Filters.Add(new SearchFilterViewModel
                {
                    Text = "Filter: Clear all",
                    Link = linkGenerator.GetPathByRouteValues(httpContext: httpContext, routeName: null, values: new { id = Input.Id, searchString = Input.SearchString, searchIn = Input.SearchIn, filter = Enumerable.Empty<string>(), sortBy = Input.SortBy, sortDirection = Input.SortDirection, itemsPerPage = Input.ItemsPerPage, currentPage = 1 }),
                    Type = "All"
                });
            }
            // Get only the items on the current page, as enumerable.
            Items = query.Skip((Input.CurrentPage - 1) * Input.ItemsPerPage).Take(Input.ItemsPerPage).ToList();
        }
    }
}
