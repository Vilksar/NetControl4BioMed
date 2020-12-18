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

namespace NetControl4BioMed.Pages.Content.DatabaseTypes.PPI.Created.Analyses.Details.Created.ControlPaths.Details.Paths
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
            public Analysis Analysis { get; set; }

            public SearchViewModel<Path> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "NodeId", "Protein ID" },
                    { "NodeName", "Protein name" },
                    { "SourceNodeId", "Source protein ID" },
                    { "SourceNodeName", "Source protein name" },
                    { "TargetNodeId", "Target protein ID" },
                    { "TargetNodeName", "Target protein name" },
                    { "EdgeId", "Interaction ID" },
                    { "EdgeName", "Interaction name" }
                },
                Filter = new Dictionary<string, string>
                {
                },
                SortBy = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "SourceNodeId", "Source protein ID" },
                    { "SourceNodeName", "Source protein name" },
                    { "TargetNodeId", "Target protein ID" },
                    { "TargetNodeName", "Target protein name" },
                    { "Length", "Length" }
                }
            };
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
                return RedirectToPage("/Content/DatabaseTypes/PPI/Created/Analyses/Index");
            }
            // Get the items with the provided ID.
            var items = _context.Paths
                .Where(item => item.ControlPath.Analysis.AnalysisDatabases.Any(item1 => item1.Database.DatabaseType.Name == "PPI"))
                .Where(item => item.ControlPath.Analysis.IsPublic || item.ControlPath.Analysis.AnalysisUsers.Any(item1 => item1.User == user))
                .Where(item => item.ControlPath.Id == id);
            // Check if there were no items found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No control path has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/PPI/Created/Analyses/Index");
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
                .Where(item => true);
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("NodeId") && item.PathNodes.Any(item1 => item1.Node.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("NodeName") && item.PathNodes.Any(item1 => item1.Node.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("SourceNodeId") && item.PathNodes.Where(item1 => item1.Type == PathNodeType.Source).Any(item1 => item1.Node.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("SourceNodeName") && item.PathNodes.Where(item1 => item1.Type == PathNodeType.Source).Any(item1 => item1.Node.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("TargetNodeId") && item.PathNodes.Where(item1 => item1.Type == PathNodeType.Target).Any(item1 => item1.Node.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("TargetNodeName") && item.PathNodes.Where(item1 => item1.Type == PathNodeType.Target).Any(item1 => item1.Node.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("EdgeId") && item.PathEdges.Any(item1 => item1.Edge.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("EdgeName") && item.PathEdges.Any(item1 => item1.Edge.Name.Contains(input.SearchString)));
            // Sort it according to the parameters.
            switch ((input.SortBy, input.SortDirection))
            {
                case var sort when sort == ("Id", "Ascending"):
                    query = query.OrderBy(item => item.Id);
                    break;
                case var sort when sort == ("Id", "Descending"):
                    query = query.OrderByDescending(item => item.Id);
                    break;
                case var sort when sort == ("SourceNodeId", "Ascending"):
                    query = query.OrderBy(item => item.PathNodes.Where(item1 => item1.Type == PathNodeType.Source).First().Node.Id);
                    break;
                case var sort when sort == ("SourceNodeId", "Descending"):
                    query = query.OrderByDescending(item => item.PathNodes.Where(item1 => item1.Type == PathNodeType.Source).First().Node.Id);
                    break;
                case var sort when sort == ("SourceNodeName", "Ascending"):
                    query = query.OrderBy(item => item.PathNodes.Where(item1 => item1.Type == PathNodeType.Source).First().Node.Name);
                    break;
                case var sort when sort == ("SourceNodeName", "Descending"):
                    query = query.OrderByDescending(item => item.PathNodes.Where(item1 => item1.Type == PathNodeType.Source).First().Node.Name);
                    break;
                case var sort when sort == ("TargetNodeId", "Ascending"):
                    query = query.OrderBy(item => item.PathNodes.Where(item1 => item1.Type == PathNodeType.Target).First().Node.Id);
                    break;
                case var sort when sort == ("TargetNodeId", "Descending"):
                    query = query.OrderByDescending(item => item.PathNodes.Where(item1 => item1.Type == PathNodeType.Target).First().Node.Id);
                    break;
                case var sort when sort == ("TargetNodeName", "Ascending"):
                    query = query.OrderBy(item => item.PathNodes.Where(item1 => item1.Type == PathNodeType.Target).First().Node.Name);
                    break;
                case var sort when sort == ("TargetNodeName", "Descending"):
                    query = query.OrderByDescending(item => item.PathNodes.Where(item1 => item1.Type == PathNodeType.Target).First().Node.Name);
                    break;
                case var sort when sort == ("Length", "Ascending"):
                    query = query.OrderBy(item => item.PathEdges.Count());
                    break;
                case var sort when sort == ("Length", "Descending"):
                    query = query.OrderByDescending(item => item.PathEdges.Count());
                    break;
                default:
                    break;
            }
            // Include the related entities.
            query = query
                .Include(item => item.PathNodes)
                    .ThenInclude(item => item.Node);
            // Define the view.
            View = new ViewModel
            {
                Analysis = items
                    .Select(item => item.ControlPath.Analysis)
                    .First(),
                Search = new SearchViewModel<Path>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
