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

namespace NetControl4BioMed.Pages.Content.Created.Networks.Details.Data.Edges
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
            public bool IsGeneric { get; set; }

            public Network Network { get; set; }

            public SearchViewModel<NetworkEdge> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "Name", "Name" },
                    { "Description", "Description" },
                    { "SourceNode", "Source node" },
                    { "TargetNode", "Target node" },
                    { "Values", "Values" }
                },
                Filter = new Dictionary<string, string>
                {
                },
                SortBy = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "Name", "Name" },
                    { "SourceNodeId", "Source node ID" },
                    { "SourceNodeName", "Source node name" },
                    { "TargetNodeId", "Target node ID" },
                    { "TargetNodeName", "Target node name" }
                }
            };
        }

        public async Task<IActionResult> OnGetAsync(string id, string searchString = null, IEnumerable<string> searchIn = null, IEnumerable<string> filter = null, string sortBy = null, string sortDirection = null, int? itemsPerPage = null, int? currentPage = 1)
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
                .Where(item => item.NetworkUsers.Any(item1 => item1.User == user))
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
                .Select(item => item.NetworkEdges)
                .SelectMany(item => item);
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Edge.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Name") && item.Edge.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Description") && item.Edge.Description.Contains(input.SearchString) ||
                    input.SearchIn.Contains("SourceNode") && item.Edge.EdgeNodes.Any(item1 => item1.Type == EdgeNodeType.Source && (item1.Node.Id.Contains(input.SearchString) || item1.Node.Name.Contains(input.SearchString) || item1.Node.DatabaseNodeFieldNodes.Any(item2 => item2.DatabaseNodeField.IsSearchable && item2.Value.Contains(input.SearchString)))) ||
                    input.SearchIn.Contains("TargetNode") && item.Edge.EdgeNodes.Any(item1 => item1.Type == EdgeNodeType.Target && (item1.Node.Id.Contains(input.SearchString) || item1.Node.Name.Contains(input.SearchString) || item1.Node.DatabaseNodeFieldNodes.Any(item2 => item2.DatabaseNodeField.IsSearchable && item2.Value.Contains(input.SearchString)))) ||
                    input.SearchIn.Contains("Values") && item.Edge.DatabaseEdgeFieldEdges.Where(item1 => item1.DatabaseEdgeField.Database.IsPublic || item1.DatabaseEdgeField.Database.DatabaseUsers.Any(item2 => item2.User == user)).Any(item1 => item1.DatabaseEdgeField.IsSearchable && item1.Value.Contains(input.SearchString)));
            // Sort it according to the parameters.
            switch ((input.SortBy, input.SortDirection))
            {
                case var sort when sort == ("Id", "Ascending"):
                    query = query.OrderBy(item => item.Edge.Id);
                    break;
                case var sort when sort == ("Id", "Descending"):
                    query = query.OrderByDescending(item => item.Edge.Id);
                    break;
                case var sort when sort == ("Name", "Ascending"):
                    query = query.OrderBy(item => item.Edge.Name);
                    break;
                case var sort when sort == ("Name", "Descending"):
                    query = query.OrderByDescending(item => item.Edge.Name);
                    break;
                case var sort when sort == ("SourceNodeId", "Ascending"):
                    query = query.OrderBy(item => item.Edge.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Source).Node.Id);
                    break;
                case var sort when sort == ("SourceNodeId", "Descending"):
                    query = query.OrderByDescending(item => item.Edge.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Source).Node.Id);
                    break;
                case var sort when sort == ("SourceNodeName", "Ascending"):
                    query = query.OrderBy(item => item.Edge.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Source).Node.Name);
                    break;
                case var sort when sort == ("SourceNodeName", "Descending"):
                    query = query.OrderByDescending(item => item.Edge.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Source).Node.Name);
                    break;
                case var sort when sort == ("TargetNodeId", "Ascending"):
                    query = query.OrderBy(item => item.Edge.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Target).Node.Id);
                    break;
                case var sort when sort == ("TargetNodeId", "Descending"):
                    query = query.OrderByDescending(item => item.Edge.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Target).Node.Id);
                    break;
                case var sort when sort == ("TargetNodeName", "Ascending"):
                    query = query.OrderBy(item => item.Edge.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Target).Node.Name);
                    break;
                case var sort when sort == ("TargetNodeName", "Descending"):
                    query = query.OrderByDescending(item => item.Edge.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Target).Node.Name);
                    break;
                default:
                    break;
            }
            // Include the related entities.
            query = query
                .Include(item => item.Edge);
            // Define the view.
            View = new ViewModel
            {
                IsGeneric = items
                    .Select(item => item.NetworkDatabases)
                    .SelectMany(item => item)
                    .Any(item => item.Database.DatabaseType.Name == "Generic"),
                Network = items
                    .First(),
                Search = new SearchViewModel<NetworkEdge>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
