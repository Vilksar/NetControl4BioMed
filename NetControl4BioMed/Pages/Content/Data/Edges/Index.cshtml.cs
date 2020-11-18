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

namespace NetControl4BioMed.Pages.Content.Data.Edges
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
            public SearchViewModel<Edge> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "Name", "Name" },
                    { "Description", "Description" },
                    { "SourceNode", "Source node" },
                    { "TargetNode", "Target node" },
                    { "Databases", "Databases" },
                    { "DatabasesEdgeFields", "DatabaseEdgeFields" },
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
            // Start with all of the items in the non-generic databases.
            var query = _context.Edges
                .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                .Where(item => item.DatabaseEdges.Any(item1 => item1.Database.IsPublic || item1.Database.DatabaseUsers.Any(item2 => item2.User == user)))
                .Where(item => item.EdgeNodes.All(item1 => !item1.Node.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic") && item1.Node.DatabaseNodes.Any(item2 => item2.Database.IsPublic || item2.Database.DatabaseUsers.Any(item3 => item3.User == user))));
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Name") && item.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Description") && item.Description.Contains(input.SearchString) ||
                    input.SearchIn.Contains("SourceNode") && item.EdgeNodes.Any(item1 => item1.Type == EdgeNodeType.Source && (item1.Node.Id.Contains(input.SearchString) || item1.Node.Name.Contains(input.SearchString) || item1.Node.DatabaseNodeFieldNodes.Any(item2 => item2.DatabaseNodeField.IsSearchable && item2.Value.Contains(input.SearchString)))) ||
                    input.SearchIn.Contains("TargetNode") && item.EdgeNodes.Any(item1 => item1.Type == EdgeNodeType.Target && (item1.Node.Id.Contains(input.SearchString) || item1.Node.Name.Contains(input.SearchString) || item1.Node.DatabaseNodeFieldNodes.Any(item2 => item2.DatabaseNodeField.IsSearchable && item2.Value.Contains(input.SearchString)))) ||
                    input.SearchIn.Contains("Databases") && item.DatabaseEdges.Where(item1 => item1.Database.IsPublic || item1.Database.DatabaseUsers.Any(item2 => item2.User == user)).Any(item1 => item1.Database.Id.Contains(input.SearchString) || item1.Database.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("DatabaseEdgeFields") && item.DatabaseEdges.Where(item1 => item1.Database.IsPublic || item1.Database.DatabaseUsers.Any(item2 => item2.User == user)).Any(item1 => item1.Database.DatabaseEdgeFields.Any(item2 => item2.Id.Contains(input.SearchString) || item2.Name.Contains(input.SearchString))) ||
                    input.SearchIn.Contains("Values") && item.DatabaseEdgeFieldEdges.Where(item1 => item1.DatabaseEdgeField.Database.IsPublic || item1.DatabaseEdgeField.Database.DatabaseUsers.Any(item2 => item2.User == user)).Any(item1 => item1.DatabaseEdgeField.IsSearchable && item1.Value.Contains(input.SearchString)));
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
                case var sort when sort == ("SourceNodeId", "Ascending"):
                    query = query.OrderBy(item => item.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Source).Node.Id);
                    break;
                case var sort when sort == ("SourceNodeId", "Descending"):
                    query = query.OrderByDescending(item => item.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Source).Node.Id);
                    break;
                case var sort when sort == ("SourceNodeName", "Ascending"):
                    query = query.OrderBy(item => item.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Source).Node.Name);
                    break;
                case var sort when sort == ("SourceNodeName", "Descending"):
                    query = query.OrderByDescending(item => item.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Source).Node.Name);
                    break;
                case var sort when sort == ("TargetNodeId", "Ascending"):
                    query = query.OrderBy(item => item.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Target).Node.Id);
                    break;
                case var sort when sort == ("TargetNodeId", "Descending"):
                    query = query.OrderByDescending(item => item.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Target).Node.Id);
                    break;
                case var sort when sort == ("TargetNodeName", "Ascending"):
                    query = query.OrderBy(item => item.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Target).Node.Name);
                    break;
                case var sort when sort == ("TargetNodeName", "Descending"):
                    query = query.OrderByDescending(item => item.EdgeNodes.First(item1 => item1.Type == EdgeNodeType.Target).Node.Name);
                    break;
                default:
                    break;
            }
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<Edge>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
