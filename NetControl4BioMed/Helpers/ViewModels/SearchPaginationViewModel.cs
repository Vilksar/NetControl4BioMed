using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the pagination returned by a search.
    /// </summary>
    public class SearchPaginationViewModel
    {
        /// <summary>
        /// Gets or sets the current page for the pagination.
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Gets or sets the index of the first item displayed on the current page.
        /// </summary>
        public int ItemsPerPageFirst { get; set; }

        /// <summary>
        /// Gets or sets the index of the last item displayed on the current page.
        /// </summary>
        public int ItemsPerPageLast { get; set; }

        /// <summary>
        /// Gets or sets the total number of items.
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Gets or sets the link to the previous page.
        /// </summary>
        public string PreviousPageLink { get; set; }

        /// <summary>
        /// Gets or sets the link to the next page.
        /// </summary>
        public string NextPageLink { get; set; }

        /// <summary>
        /// Initializes a new instance of pagination.
        /// </summary>
        /// <param name="linkGenerator"></param>
        /// <param name="httpContext"></param>
        /// <param name="input"></param>
        /// <param name="totalItems"></param>
        public SearchPaginationViewModel(LinkGenerator linkGenerator, HttpContext httpContext, SearchInputViewModel input, int totalItems)
        {
            // Get the total number of items and of pages, as well as the number of items on the current page.
            TotalItems = totalItems;
            CurrentPage = input.CurrentPage;
            TotalPages = (int)Math.Ceiling((double)totalItems / input.ItemsPerPage);
            ItemsPerPageFirst = 0 < TotalItems ? (CurrentPage - 1) * input.ItemsPerPage + 1 : 0;
            ItemsPerPageLast = 0 < TotalItems ? CurrentPage * Math.Min(input.ItemsPerPage, TotalItems) < TotalItems ? CurrentPage * Math.Min(input.ItemsPerPage, TotalItems) : TotalItems : 0;
            // Create the links for the next and the previous pages.
            PreviousPageLink = CurrentPage == 1 || TotalPages == 0 ? null : linkGenerator.GetPathByRouteValues(httpContext: httpContext, routeName: null, values: new { searchString = input.SearchString, searchIn = input.SearchIn, filter = input.Filter, sortBy = input.SortBy, sortDirection = input.SortDirection, itemsPerPage = input.ItemsPerPage, currentPage = CurrentPage - 1 });
            NextPageLink = CurrentPage == TotalPages || TotalPages == 0 ? null : linkGenerator.GetPathByRouteValues(httpContext: httpContext, routeName: null, values: new { searchString = input.SearchString, searchIn = input.SearchIn, filter = input.Filter, sortBy = input.SortBy, sortDirection = input.SortDirection, itemsPerPage = input.ItemsPerPage, currentPage = CurrentPage + 1 });
        }
    }
}
