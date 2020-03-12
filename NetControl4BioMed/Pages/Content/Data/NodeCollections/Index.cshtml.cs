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

namespace NetControl4BioMed.Pages.Content.Data.NodeCollections
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
            public SearchViewModel<NodeCollection> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "Name", "Name" },
                    { "Description", "Description" },
                    { "Databases", "Databases" },
                    { "Nodes", "Nodes" }
                },
                Filter = new Dictionary<string, string>
                {
                },
                SortBy = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "Name", "Name" },
                    { "NodeCollectionDatabaseCount", "Number of databases" },
                    { "NodeCollectionNodeCount", "Number of nodes" }
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
            var query = _context.NodeCollections
                .Where(item => !item.NodeCollectionDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                .Where(item => item.NodeCollectionDatabases.Any(item1 => item1.Database.IsPublic || item1.Database.DatabaseUsers.Any(item2 => item2.User == user)))
                .Where(item => item.NodeCollectionNodes.Any(item1 => !item1.Node.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic") && item1.Node.DatabaseNodes.Any(item2 => item2.Database.IsPublic || item2.Database.DatabaseUsers.Any(item3 => item3.User == user))));
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Name") && item.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Description") && item.Description.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Databases") && item.NodeCollectionDatabases.Where(item1 => item1.Database.IsPublic || item1.Database.DatabaseUsers.Any(item2 => item2.User == user)).Any(item1 => item1.Database.Id.Contains(input.SearchString) || item1.Database.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("Nodes") && item.NodeCollectionNodes.Where(item1 => !item1.Node.DatabaseNodes.Any(item2 => item2.Database.DatabaseType.Name == "Generic") && item1.Node.DatabaseNodes.Any(item2 => item2.Database.IsPublic || item2.Database.DatabaseUsers.Any(item3 => item3.User == user))).Any(item1 => item1.Node.Id.Contains(input.SearchString) || item1.Node.Name.Contains(input.SearchString) || item1.Node.DatabaseNodeFieldNodes.Where(item2 => item2.DatabaseNodeField.Database.IsPublic || item2.DatabaseNodeField.Database.DatabaseUsers.Any(item3 => item3.User == user)).Any(item2 => item2.DatabaseNodeField.IsSearchable && item2.Value.Contains(input.SearchString))));
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
                case var sort when sort == ("NodeCollectionDatabaseCount", "Ascending"):
                    query = query.OrderBy(item => item.NodeCollectionDatabases.Where(item1 => item1.Database.IsPublic || item1.Database.DatabaseUsers.Any(item2 => item2.User == user)).Count(item1 => item1.Database.Id.Contains(input.SearchString) || item1.Database.Name.Contains(input.SearchString)));
                    break;
                case var sort when sort == ("NodeCollectionDatabaseCount", "Descending"):
                    query = query.OrderByDescending(item => item.NodeCollectionDatabases.Where(item1 => item1.Database.IsPublic || item1.Database.DatabaseUsers.Any(item2 => item2.User == user)).Count(item1 => item1.Database.Id.Contains(input.SearchString) || item1.Database.Name.Contains(input.SearchString)));
                    break;
                case var sort when sort == ("NodeCollectionNodeCount", "Ascending"):
                    query = query.OrderBy(item => item.NodeCollectionNodes.Where(item1 => !item1.Node.DatabaseNodes.Any(item2 => item2.Database.DatabaseType.Name == "Generic") && item1.Node.DatabaseNodes.Any(item2 => item2.Database.IsPublic || item2.Database.DatabaseUsers.Any(item3 => item3.User == user))).Count(item1 => item1.Node.Id.Contains(input.SearchString) || item1.Node.Name.Contains(input.SearchString) || item1.Node.DatabaseNodeFieldNodes.Where(item2 => item2.DatabaseNodeField.Database.IsPublic || item2.DatabaseNodeField.Database.DatabaseUsers.Any(item3 => item3.User == user)).Any(item2 => item2.DatabaseNodeField.IsSearchable && item2.Value.Contains(input.SearchString))));
                    break;
                case var sort when sort == ("NodeCollectionNodeCount", "Descending"):
                    query = query.OrderByDescending(item => item.NodeCollectionNodes.Where(item1 => !item1.Node.DatabaseNodes.Any(item2 => item2.Database.DatabaseType.Name == "Generic") && item1.Node.DatabaseNodes.Any(item2 => item2.Database.IsPublic || item2.Database.DatabaseUsers.Any(item3 => item3.User == user))).Count(item1 => item1.Node.Id.Contains(input.SearchString) || item1.Node.Name.Contains(input.SearchString) || item1.Node.DatabaseNodeFieldNodes.Where(item2 => item2.DatabaseNodeField.Database.IsPublic || item2.DatabaseNodeField.Database.DatabaseUsers.Any(item3 => item3.User == user)).Any(item2 => item2.DatabaseNodeField.IsSearchable && item2.Value.Contains(input.SearchString))));
                    break;
                default:
                    break;
            }
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<NodeCollection>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
