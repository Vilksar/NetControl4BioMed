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

namespace NetControl4BioMed.Pages.Content.Created.Networks.Details.Databases.Databases
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
            public Network Network { get; set; }

            public bool IsGeneric { get; set; }

            public SearchViewModel<ItemModel> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "Name", "Name" },
                    { "Description", "Description" }
                },
                Filter = new Dictionary<string, string>
                {
                    { "IsNode", "Is of type \"Node\"" },
                    { "IsNotNode", "Is not of type \"Node\"" },
                    { "IsEdge", "Is of type \"Edge\"" },
                    { "IsNotEdge", "Is not of type \"Edge\"" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "Name", "Name" }
                }
            };
        }

        public class ItemModel
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public string Url { get; set; }

            public NetworkDatabaseType Type { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id, string searchString = null, IEnumerable<string> searchIn = null, IEnumerable<string> filter = null, string sortBy = null, string sortDirection = null, int? itemsPerPage = null, int? currentPage = 1)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Get the items with the provided ID.
            var items = _context.Networks
                .Where(item => item.IsPublic || item.NetworkUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == id);
            // Check if there were no items found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Define the search input.
            var input = new SearchInputViewModel(ViewModel.SearchOptions, id, searchString, searchIn, filter, sortBy, sortDirection, itemsPerPage, currentPage);
            // Check if any of the provided variables was null before the reassignment.
            if (input.NeedsRedirect)
            {
                // Redirect to the page where they are all explicitly defined.
                return RedirectToPage(new { id = input.Id, searchString = input.SearchString, searchIn = input.SearchIn, filter = input.Filter, sortBy = input.SortBy, sortDirection = input.SortDirection, itemsPerPage = input.ItemsPerPage, currentPage = input.CurrentPage });
            }
            // Start with all of the items.
            var query = items
                .Select(item => item.NetworkDatabases)
                .SelectMany(item => item);
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Database.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Name") && item.Database.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Description") && item.Database.Description.Contains(input.SearchString));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("IsNode") ? item.Type == NetworkDatabaseType.Node : true)
                .Where(item => input.Filter.Contains("IsNotNode") ? item.Type != NetworkDatabaseType.Node : true)
                .Where(item => input.Filter.Contains("IsEdge") ? item.Type == NetworkDatabaseType.Edge : true)
                .Where(item => input.Filter.Contains("IsNotEdge") ? item.Type != NetworkDatabaseType.Edge : true);
            // Sort it according to the parameters.
            switch ((input.SortBy, input.SortDirection))
            {
                case var sort when sort == ("Id", "Ascending"):
                    query = query.OrderBy(item => item.Database.Id);
                    break;
                case var sort when sort == ("Id", "Descending"):
                    query = query.OrderByDescending(item => item.Database.Id);
                    break;
                case var sort when sort == ("Name", "Ascending"):
                    query = query.OrderBy(item => item.Database.Name);
                    break;
                case var sort when sort == ("Name", "Descending"):
                    query = query.OrderByDescending(item => item.Database.Name);
                    break;
                default:
                    break;
            }
            // Include the related entities.
            query = query
                .Include(item => item.Database);
            // Define the view.
            View = new ViewModel
            {
                Network = items
                    .First(),
                IsGeneric = items
                    .Select(item => item.NetworkDatabases)
                    .SelectMany(item => item)
                    .Any(item => item.Database.DatabaseType.Name == "Generic"),
                Search = new SearchViewModel<ItemModel>(_linkGenerator, HttpContext, input, query
                    .Select(item => new ItemModel
                    {
                        Id = item.Database.Id,
                        Name = item.Database.Name,
                        Url = item.Database.Url,
                        Type = item.Type
                    }))
            };
            // Return the page.
            return Page();
        }
    }
}
