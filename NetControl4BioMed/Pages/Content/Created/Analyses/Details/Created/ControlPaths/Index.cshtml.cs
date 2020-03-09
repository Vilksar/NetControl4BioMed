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

namespace NetControl4BioMed.Pages.Content.Created.Analyses.Details.Created.ControlPaths
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
            public Analysis Analysis { get; set; }

            public bool IsGeneric { get; set; }

            public SearchViewModel<ControlPath> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "NodeId", "Node ID" },
                    { "NodeName", "Node name" },
                    { "SourceNodeId", "Source node ID" },
                    { "SourceNodeName", "Source node name" },
                    { "TargetNodeId", "Target node ID" },
                    { "TargetNodeName", "Target node name" },
                    { "EdgeId", "Edge ID" },
                    { "EdgeName", "Edge name" }
                },
                Filter = new Dictionary<string, string>
                {
                },
                SortBy = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "ControlNodeCount", "Number of control nodes" }
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
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Get the items with the provided ID.
            var items = _context.Analyses
                .Where(item => item.AnalysisUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == id)
                .Include(item => item.AnalysisDatabases)
                    .ThenInclude(item => item.Database)
                        .ThenInclude(item => item.DatabaseType)
                .AsQueryable();
            // Check if there were no items found.
            if (items == null || items.Count() != 1)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Define the search input.
            var input = new SearchInputViewModel(ViewModel.SearchOptions, id, searchString, searchIn, filter, sortBy, sortDirection, itemsPerPage, currentPage);
            // Check if any of the provided variables was null before the reassignment.
            if (input.NeedsRedirect)
            {
                // Redirect to the page where they are all explicitly defined.
                return RedirectToPage(new { id = input.Id, searchString = input.SearchString, searchIn = input.SearchIn, filter = input.Filter, sortBy = input.SortBy, sortDirection = input.SortDirection, itemsPerPage = input.ItemsPerPage, currentPage = input.CurrentPage });
            }
            // Start with all of the items of the network.
            var query = items
                .Select(item => item.ControlPaths)
                .SelectMany(item => item)
                .AsQueryable();
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("NodeId") && item.Paths.Any(item1 => item1.PathNodes.Any(item2 => item2.Node.Id.Contains(input.SearchString))) ||
                    input.SearchIn.Contains("NodeName") && item.Paths.Any(item1 => item1.PathNodes.Any(item2 => item2.Node.Name.Contains(input.SearchString))) ||
                    input.SearchIn.Contains("SourceNodeId") && item.Paths.Any(item1 => item1.PathNodes.Where(item2 => item2.Type == PathNodeType.Source).Any(item2 => item2.Node.Id.Contains(input.SearchString))) ||
                    input.SearchIn.Contains("SourceNodeName") && item.Paths.Any(item1 => item1.PathNodes.Where(item2 => item2.Type == PathNodeType.Source).Any(item2 => item2.Node.Name.Contains(input.SearchString))) ||
                    input.SearchIn.Contains("TargetNodeId") && item.Paths.Any(item1 => item1.PathNodes.Where(item2 => item2.Type == PathNodeType.Target).Any(item2 => item2.Node.Id.Contains(input.SearchString))) ||
                    input.SearchIn.Contains("TargetNodeName") && item.Paths.Any(item1 => item1.PathNodes.Where(item2 => item2.Type == PathNodeType.Target).Any(item2 => item2.Node.Name.Contains(input.SearchString))) ||
                    input.SearchIn.Contains("EdgeId") && item.Paths.Any(item1 => item1.PathEdges.Any(item2 => item2.Edge.Id.Contains(input.SearchString))) ||
                    input.SearchIn.Contains("EdgeName") && item.Paths.Any(item1 => item1.PathEdges.Any(item2 => item2.Edge.Name.Contains(input.SearchString))));
            // Sort it according to the parameters.
            switch ((input.SortBy, input.SortDirection))
            {
                case var sort when sort == ("Id", "Ascending"):
                    query = query.OrderBy(item => item.Id);
                    break;
                case var sort when sort == ("Id", "Descending"):
                    query = query.OrderByDescending(item => item.Id);
                    break;
                case var sort when sort == ("ControlNodeCount", "Ascending"):
                    query = query.OrderBy(item => item.Paths.Select(item1 => item1.PathNodes.First()).Distinct().Count());
                    break;
                case var sort when sort == ("ControlNodeCount", "Descending"):
                    query = query.OrderByDescending(item => item.Paths.Select(item1 => item1.PathNodes.First()).Distinct().Count());
                    break;
                default:
                    break;
            }
            // Include the related entities.
            query = query
                .Include(item => item.Paths)
                    .ThenInclude(item => item.PathNodes)
                        .ThenInclude(item => item.Node)
                .Include(item => item.Paths)
                    .ThenInclude(item => item.PathEdges)
                        .ThenInclude(item => item.Edge);
            // Define the view.
            View = new ViewModel
            {
                Analysis = items.First(),
                IsGeneric = items.First().AnalysisDatabases
                    .Any(item => item.Database.DatabaseType.Name == "Generic"),
                Search = new SearchViewModel<ControlPath>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
