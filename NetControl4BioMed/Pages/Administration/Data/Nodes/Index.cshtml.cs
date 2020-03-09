using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.ViewModels;

namespace NetControl4BioMed.Pages.Administration.Data.Nodes
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
            public SearchViewModel<Node> Search { get; set; }

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
                    { "HasDatabaseNodes", "Has database nodes" },
                    { "HasNoDatabaseNodes", "Does not have database nodes" },
                    { "HasDatabaseNodeFieldNodes", "Has database node field nodes" },
                    { "HasNoDatabaseNodeFieldNodes", "Does not have database node field nodes" },
                    { "HasEdgeNodes", "Has edge nodes" },
                    { "HasNoEdgeNodes", "Does not have edge nodes" },
                    { "HasNetworkNodes", "Has network nodes" },
                    { "HasNoNetworkNodes", "Does not have network nodes" },
                    { "HasAnalysisNodes", "Has analysis nodes" },
                    { "HasNoAnalysisNodes", "Does not have analysis nodes" },
                    { "HasPathNodes", "Has path nodes" },
                    { "HasNoPathNodes", "Does not have path nodes" },
                    { "HasNodeCollectionNodes", "Has node collection nodes" },
                    { "HasNoNodeCollectionNodes", "Does not have node collection nodes" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "DateTimeCreated", "Date created" },
                    { "Name", "Name" },
                    { "DatabaseNodeCount", "Number of database nodes" },
                    { "DatabaseNodeFieldNodeCount", "Number of database node field nodes" },
                    { "EdgeNodeCount", "Number of edge nodes" },
                    { "NetworkNodeCount", "Number of network nodes" },
                    { "AnalysisNodeCount", "Number of analysis nodes" },
                    { "PathNodeCount", "Number of path nodes" },
                    { "NodeCollectionNodeCount", "Number of node collection nodes" }
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
            var query = _context.Nodes
                .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                .AsQueryable();
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Name") && item.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Description") && item.Description.Contains(input.SearchString));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("HasDatabaseNodes") ? item.DatabaseNodes.Any() : true)
                .Where(item => input.Filter.Contains("HasNoDatabaseNodes") ? !item.DatabaseNodes.Any() : true)
                .Where(item => input.Filter.Contains("HasDatabaseNodeFieldNodes") ? item.DatabaseNodeFieldNodes.Any() : true)
                .Where(item => input.Filter.Contains("HasNoDatabaseNodeFieldNodes") ? !item.DatabaseNodeFieldNodes.Any() : true)
                .Where(item => input.Filter.Contains("HasEdgeNodes") ? item.EdgeNodes.Any() : true)
                .Where(item => input.Filter.Contains("HasNoEdgeNodes") ? !item.EdgeNodes.Any() : true)
                .Where(item => input.Filter.Contains("HasNetworkNodes") ? item.NetworkNodes.Any() : true)
                .Where(item => input.Filter.Contains("HasNoNetworkNodes") ? !item.NetworkNodes.Any() : true)
                .Where(item => input.Filter.Contains("HasAnalysisNodes") ? item.AnalysisNodes.Any() : true)
                .Where(item => input.Filter.Contains("HasNoAnalysisNodes") ? !item.AnalysisNodes.Any() : true)
                .Where(item => input.Filter.Contains("HasPathNodes") ? item.PathNodes.Any() : true)
                .Where(item => input.Filter.Contains("HasNoPathNodes") ? !item.PathNodes.Any() : true)
                .Where(item => input.Filter.Contains("HasNodeCollectionNodes") ? item.NodeCollectionNodes.Any() : true)
                .Where(item => input.Filter.Contains("HasNoNodeCollectionNodes") ? !item.NodeCollectionNodes.Any() : true);
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
                case var sort when sort == ("DatabaseNodeCount", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseNodes.Count());
                    break;
                case var sort when sort == ("DatabaseNodeCount", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseNodes.Count());
                    break;
                case var sort when sort == ("DatabaseNodeFieldNodeCount", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseNodeFieldNodes.Count());
                    break;
                case var sort when sort == ("DatabaseNodeFieldNodeCount", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseNodeFieldNodes.Count());
                    break;
                case var sort when sort == ("EdgeNodeCount", "Ascending"):
                    query = query.OrderBy(item => item.EdgeNodes.Count());
                    break;
                case var sort when sort == ("EdgeNodeCount", "Descending"):
                    query = query.OrderByDescending(item => item.EdgeNodes.Count());
                    break;
                case var sort when sort == ("NetworkNodeCount", "Ascending"):
                    query = query.OrderBy(item => item.NetworkNodes.Count());
                    break;
                case var sort when sort == ("NetworkNodeCount", "Descending"):
                    query = query.OrderByDescending(item => item.NetworkNodes.Count());
                    break;
                case var sort when sort == ("AnalysisNodeCount", "Ascending"):
                    query = query.OrderBy(item => item.AnalysisNodes.Count());
                    break;
                case var sort when sort == ("AnalysisNodeCount", "Descending"):
                    query = query.OrderByDescending(item => item.AnalysisNodes.Count());
                    break;
                case var sort when sort == ("PathNodeCount", "Ascending"):
                    query = query.OrderBy(item => item.PathNodes.Count());
                    break;
                case var sort when sort == ("PathNodeCount", "Descending"):
                    query = query.OrderByDescending(item => item.PathNodes.Count());
                    break;
                case var sort when sort == ("NodeCollectionNodeCount", "Ascending"):
                    query = query.OrderBy(item => item.NodeCollectionNodes.Count());
                    break;
                case var sort when sort == ("NodeCollectionNodeCount", "Descending"):
                    query = query.OrderByDescending(item => item.NodeCollectionNodes.Count());
                    break;
                default:
                    break;
            }
            // Include the related entitites.
            query = query
                .Include(item => item.DatabaseNodes)
                .Include(item => item.DatabaseNodeFieldNodes)
                .Include(item => item.EdgeNodes)
                .Include(item => item.NetworkNodes)
                .Include(item => item.AnalysisNodes)
                .Include(item => item.PathNodes)
                .Include(item => item.NodeCollectionNodes);
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<Node>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
