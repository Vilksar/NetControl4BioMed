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

namespace NetControl4BioMed.Pages.Administration.Relationships.DatabaseProteinFieldProteins
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
            public SearchViewModel<DatabaseProteinFieldProtein> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "DatabaseProteinFieldId", "Database protein field ID" },
                    { "DatabaseProteinFieldName", "Database protein field name" },
                    { "ProteinId", "Protein ID" },
                    { "ProteinName", "Protein name" },
                    { "Value", "Value" }
                },
                Filter = new Dictionary<string, string>
                {
                },
                SortBy = new Dictionary<string, string>
                {
                    { "DatabaseProteinFieldId", "Database protein field ID" },
                    { "DatabaseProteinFieldName", "Database protein field name" },
                    { "ProteinId", "Protein ID" },
                    { "ProteinName", "ProteinName" },
                    { "Value", "Value" }
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
            var query = _context.DatabaseProteinFieldProteins
                .Where(item => item.Protein.DatabaseProteins.Any());
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("DatabaseProteinFieldId") && item.DatabaseProteinField.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("DatabaseProteinFieldName") && item.DatabaseProteinField.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("ProteinId") && item.Protein.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("ProteinName") && item.Protein.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Value") && item.Value.Contains(input.SearchString));
            // Sort it according to the parameters.
            switch ((input.SortBy, input.SortDirection))
            {
                case var sort when sort == ("DatabaseProteinFieldId", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseProteinField.Id);
                    break;
                case var sort when sort == ("DatabaseProteinFieldId", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseProteinField.Id);
                    break;
                case var sort when sort == ("DatabaseProteinFieldName", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseProteinField.Name);
                    break;
                case var sort when sort == ("DatabaseProteinFieldName", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseProteinField.Name);
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
                case var sort when sort == ("Value", "Ascending"):
                    query = query.OrderBy(item => item.Value);
                    break;
                case var sort when sort == ("Value", "Descending"):
                    query = query.OrderByDescending(item => item.Value);
                    break;
                default:
                    break;
            }
            // Include the related entitites.
            query = query
                .Include(item => item.DatabaseProteinField)
                .Include(item => item.Protein);
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<DatabaseProteinFieldProtein>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
