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

namespace NetControl4BioMed.Pages.Content.Databases.Databases
{
    [Authorize]
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
            public SearchViewModel<Database> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "Name", "Name" },
                    { "Description", "Description" },
                    { "DatabaseTypeId", "Type ID" },
                    { "DatabaseTypeName", "Type name" },
                    { "DatabaseNodeFields", "Node fields" },
                    { "DatabaseEdgeFields", "Edge fields" },
                    { "DatabaseNodes", "Nodes" },
                    { "DatabaseEdges", "Edges" },
                    { "NodeCollectionDatabases", "Node collections" }
                },
                Filter = new Dictionary<string, string>
                {
                },
                SortBy = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "Name", "Name" },
                    { "DatabaseTypeId", "Type ID" },
                    { "DatabaseTypeName", "Type name" },
                    { "DatabaseNodeFieldCount", "Number of node fields" },
                    { "DatabaseEdgeFieldCount", "Number of edge fields" },
                    { "DatabaseNodeCount", "Number of nodes" },
                    { "DatabaseEdgeCount", "Number of edges" },
                    { "NodeCollectionDatabaseCount", "Number of node collections" }
                }
            };
        }

        public async Task<IActionResult> OnGetAsync(string searchString = null, IEnumerable<string> searchIn = null, IEnumerable<string> filter = null, string sortBy = null, string sortDirection = null, int? itemsPerPage = null, int? currentPage = 1)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if the user does not exist.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: An error occured while trying to load the user data. If you are already logged in, please log out and try again.";
                // Redirect to the home page.
                return RedirectToPage("/Index");
            }
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
                .Where(item => item.IsPublic || item.DatabaseUsers.Any(item1 => item1.User == user))
                .AsQueryable();
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Name") && item.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Description") && item.Description.Contains(input.SearchString) ||
                    input.SearchIn.Contains("DatabaseTypeId") && item.DatabaseType.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("DatabaseTypeName") && item.DatabaseType.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("DatabaseNodeFields") && item.DatabaseNodeFields.Any(item1 => item1.Id.Contains(input.SearchString) || item1.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("DatabaseEdgeFields") && item.DatabaseEdgeFields.Any(item1 => item1.Id.Contains(input.SearchString) || item1.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("DatabaseNodes") && item.DatabaseNodes.Any(item1 => item1.Node.Id.Contains(input.SearchString) || item1.Node.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("DatabaseEdges") && item.DatabaseEdges.Any(item1 => item1.Edge.Id.Contains(input.SearchString) || item1.Edge.Name.Contains(input.SearchString) || item1.Edge.EdgeNodes.Any(item2 => item2.Node.Id.Contains(input.SearchString) || item2.Node.Name.Contains(input.SearchString))) ||
                    input.SearchIn.Contains("NodeCollectionDatabases") && item.NodeCollectionDatabases.Any(item1 => item1.NodeCollection.Id.Contains(input.SearchString) || item1.NodeCollection.Name.Contains(input.SearchString)));
            // Sort it according to the parameters.
            switch ((input.SortBy, input.SortDirection))
            {
                case var sort when sort == ("Id", "Ascending"):
                    query = query.OrderBy(item => item.Id);
                    break;
                case var sort when sort == ("Id", "Descending"):
                    query = query.OrderByDescending(item => item.Id);
                    break;
                case var sort when sort == ("Name", "Ascending"):
                    query = query.OrderBy(item => item.Name);
                    break;
                case var sort when sort == ("Name", "Descending"):
                    query = query.OrderByDescending(item => item.Name);
                    break;
                case var sort when sort == ("DatabaseTypeId", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseType.Id);
                    break;
                case var sort when sort == ("DatabaseTypeId", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseType.Id);
                    break;
                case var sort when sort == ("DatabaseTypeName", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseType.Name);
                    break;
                case var sort when sort == ("DatabaseTypeName", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseType.Name);
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
                    query = query.OrderBy(item => item.DatabaseNodeFields.Count());
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
                Search = new SearchViewModel<Database>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
