using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.ViewModels;

namespace NetControl4BioMed.Pages.Content.DatabaseTypes.PPI.Databases.Databases
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly LinkGenerator _linkGenerator;

        public IndexModel(UserManager<User> userManager, ApplicationDbContext context, LinkGenerator linkGenerator)
        {
            _userManager = userManager;
            _context = context;
            _linkGenerator = linkGenerator;
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public SearchViewModel<ItemModel> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "Name", "Name" },
                    { "Description", "Description" },
                    { "Url", "URL" }
                },
                Filter = new Dictionary<string, string>
                {
                    { "HasDatabaseNodeFields", "Has protein data" },
                    { "HasNoDatabaseNodeFields", "Does not have protein data" },
                    { "HasDatabaseEdgeFields", "Has interaction data" },
                    { "HasNoDatabaseEdgeFields", "Does not have interaction data" },
                    { "HasDatabaseNodes", "Has proteins" },
                    { "HasNoDatabaseNodes", "Does not have proteins" },
                    { "HasDatabaseEdges", "Has interactions" },
                    { "HasNoDatabaseEdges", "Does not have interactions" },
                    { "HasNodeCollectionDatabases", "Has protein collections" },
                    { "HasNoNodeCollectionDatabases", "Does not have protein collections" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "DateTimeCreated", "Date created" },
                    { "Name", "Name" },
                    { "DatabaseNodeFieldCount", "Number of protein values" },
                    { "DatabaseEdgeFieldCount", "Number of interaction values" },
                    { "DatabaseNodeCount", "Number of proteins" },
                    { "DatabaseEdgeCount", "Number of interactions" },
                    { "NodeCollectionDatabaseCount", "Number of protein collections" }
                }
            };
        }

        public class ItemModel
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public string Url { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string searchString = null, IEnumerable<string> searchIn = null, IEnumerable<string> filter = null, string sortBy = null, string sortDirection = null, int? itemsPerPage = null, int? currentPage = 1)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Define the search input.
            var input = new SearchInputViewModel(ViewModel.SearchOptions, null, searchString, searchIn, filter, sortBy, sortDirection, itemsPerPage, currentPage);
            // Check if any of the provided variables was null before the reassignment.
            if (input.NeedsRedirect)
            {
                // Redirect to the page where they are all explicitly defined.
                return RedirectToPage(new { searchString = input.SearchString, searchIn = input.SearchIn, filter = input.Filter, sortBy = input.SortBy, sortDirection = input.SortDirection, itemsPerPage = input.ItemsPerPage, currentPage = input.CurrentPage });
            }
            // Start with all of the items to which the user has access.
            var query = _context.Databases
                .Where(item => item.DatabaseType.Name == "PPI")
                .Where(item => item.IsPublic || item.DatabaseUsers.Any(item1 => item1.User == user));
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Name") && item.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Description") && item.Description.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Url") && item.Url.Contains(input.SearchString));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("HasDatabaseNodeFields") ? item.DatabaseNodeFields.Any() : true)
                .Where(item => input.Filter.Contains("HasNoDatabaseNodeFields") ? !item.DatabaseNodeFields.Any() : true)
                .Where(item => input.Filter.Contains("HasDatabaseEdgeFields") ? item.DatabaseEdgeFields.Any() : true)
                .Where(item => input.Filter.Contains("HasDatabaseNodes") ? item.DatabaseNodes.Any() : true)
                .Where(item => input.Filter.Contains("HasNoDatabaseNodes") ? !item.DatabaseNodes.Any() : true)
                .Where(item => input.Filter.Contains("HasDatabaseEdges") ? item.DatabaseEdges.Any() : true)
                .Where(item => input.Filter.Contains("HasNoDatabaseEdges") ? !item.DatabaseEdges.Any() : true)
                .Where(item => input.Filter.Contains("HasNodeCollectionDatabases") ? !item.NodeCollectionDatabases.Any() : true)
                .Where(item => input.Filter.Contains("HasNoNodeCollectionDatabases") ? !item.NodeCollectionDatabases.Any() : true);
            // Sort it according to the parameters.
            switch ((input.SortBy, input.SortDirection))
            {
                case var sort when sort == ("Id", "Ascending"):
                    query = query.OrderBy(item => item.Id);
                    break;
                case var sort when sort == ("Id", "Descending"):
                    query = query.OrderByDescending(item => item.Id);
                    break;
                case var sort when sort == ("DateTimeCreated", "Ascending"):
                    query = query.OrderBy(item => item.DateTimeCreated);
                    break;
                case var sort when sort == ("DateTimeCreated", "Descending"):
                    query = query.OrderByDescending(item => item.DateTimeCreated);
                    break;
                case var sort when sort == ("Name", "Ascending"):
                    query = query.OrderBy(item => item.Name);
                    break;
                case var sort when sort == ("Name", "Descending"):
                    query = query.OrderByDescending(item => item.Name);
                    break;
                case var sort when sort == ("DatabaseNodeFieldCount", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseNodeFields.Count());
                    break;
                case var sort when sort == ("DatabaseNodeFieldCount", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseNodeFields.Count());
                    break;
                case var sort when sort == ("DatabaseEdgeFieldCount", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseEdgeFields.Count());
                    break;
                case var sort when sort == ("DatabaseEdgeFieldCount", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseEdgeFields.Count());
                    break;
                case var sort when sort == ("DatabaseNodeCount", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseNodes.Count());
                    break;
                case var sort when sort == ("DatabaseNodeCount", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseNodes.Count());
                    break;
                case var sort when sort == ("DatabaseEdgeCount", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseEdges.Count());
                    break;
                case var sort when sort == ("DatabaseEdgeCount", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseEdges.Count());
                    break;
                case var sort when sort == ("NodeCollectionDatabaseCount", "Ascending"):
                    query = query.OrderBy(item => item.NodeCollectionDatabases.Count());
                    break;
                case var sort when sort == ("NodeCollectionDatabaseCount", "Descending"):
                    query = query.OrderByDescending(item => item.NodeCollectionDatabases.Count());
                    break;
                default:
                    break;
            }
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<ItemModel>(_linkGenerator, HttpContext, input, query.Select(item => new ItemModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    Url = item.Url
                }))
            };
            // Return the page.
            return Page();
        }
    }
}
