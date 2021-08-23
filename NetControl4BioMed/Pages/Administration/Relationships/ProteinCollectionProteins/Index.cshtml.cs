using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetControl4BioMed.Pages.Administration.Relationships.ProteinCollectionProteins
{
    [Authorize(Roles = "Administrator")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly LinkGenerator _linkGenerator;

        public IndexModel(ApplicationDbContext context, LinkGenerator linkGenerator)
        {
            _context = context;
            _linkGenerator = linkGenerator;
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public SearchViewModel<ProteinCollectionProtein> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "ProteinCollectionId", "Protein collection ID" },
                    { "ProteinCollectionName", "Protein collection name" },
                    { "ProteinId", "Protein ID" },
                    { "ProteinName", "Protein name" }
                },
                Filter = new Dictionary<string, string>
                {
                },
                SortBy = new Dictionary<string, string>
                {
                    { "ProteinCollectionId", "Protein collection ID" },
                    { "ProteinCollectionName", "Protein collection name" },
                    { "ProteinId", "Protein ID" },
                    { "ProteinName", "Protein name" }
                }
            };
        }

        public IActionResult OnGet(string searchString = null, IEnumerable<string> searchIn = null, IEnumerable<string> filter = null, string sortBy = null, string sortDirection = null, int? itemsPerPage = null, int? currentPage = 1)
        {
            // Define the search input.
            var input = new SearchInputViewModel(ViewModel.SearchOptions, null, searchString, searchIn, filter, sortBy, sortDirection, itemsPerPage, currentPage);
            // Check if any of the provided variables was null before the reassignment.
            if (input.NeedsRedirect)
            {
                // Redirect to the page where they are all explicitly defined.
                return RedirectToPage(new { searchString = input.SearchString, searchIn = input.SearchIn, filter = input.Filter, sortBy = input.SortBy, sortDirection = input.SortDirection, itemsPerPage = input.ItemsPerPage, currentPage = input.CurrentPage });
            }
            // Start with all of the items in the non-generic databases.
            var query = _context.ProteinCollectionProteins
                .Where(item => true);
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("ProteinCollectionId") && item.ProteinCollection.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("ProteinCollectionName") && item.ProteinCollection.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("ProteinId") && item.Protein.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("ProteinName") && item.Protein.Name.Contains(input.SearchString));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => true);
            // Sort it according to the parameters.
            switch ((input.SortBy, input.SortDirection))
            {
                case var sort when sort == ("ProteinCollectionId", "Ascending"):
                    query = query.OrderBy(item => item.ProteinCollection.Id);
                    break;
                case var sort when sort == ("ProteinCollectionId", "Descending"):
                    query = query.OrderByDescending(item => item.ProteinCollection.Id);
                    break;
                case var sort when sort == ("ProteinCollectionName", "Ascending"):
                    query = query.OrderBy(item => item.ProteinCollection.Name);
                    break;
                case var sort when sort == ("ProteinCollectionName", "Descending"):
                    query = query.OrderByDescending(item => item.ProteinCollection.Name);
                    break;
                case var sort when sort == ("ProteinId", "Ascending"):
                    query = query.OrderBy(item => item.Protein.Id);
                    break;
                case var sort when sort == ("ProteinId", "Descending"):
                    query = query.OrderByDescending(item => item.Protein.Id);
                    break;
                case var sort when sort == ("ProteinName", "Ascending"):
                    query = query.OrderBy(item => item.Protein.Name);
                    break;
                case var sort when sort == ("ProteinName", "Descending"):
                    query = query.OrderByDescending(item => item.Protein.Name);
                    break;
                default:
                    break;
            }
            // Include the related entitites.
            query = query
                .Include(item => item.ProteinCollection)
                .Include(item => item.Protein);
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<ProteinCollectionProtein>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
