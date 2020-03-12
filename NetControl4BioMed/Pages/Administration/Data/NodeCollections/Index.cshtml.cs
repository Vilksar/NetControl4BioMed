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

namespace NetControl4BioMed.Pages.Administration.Data.NodeCollections
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
            public SearchViewModel<NodeCollection> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "Name", "Name" },
                    { "Description", "Description" },
                    { "NetworkId", "Network ID" },
                    { "NetworkName", "Network name" },
                    { "AnalysisId", "Analysis ID" },
                    { "AnalysisName", "Analysis name" }
                },
                Filter = new Dictionary<string, string>
                {
                    { "HasNodeCollectionDatabases", "Has node collection databases" },
                    { "HasNoNodeCollectionDatabases", "Does not have node collection databases" },
                    { "HasNodeCollectionNodes", "Has node collection nodes" },
                    { "HasNoNodeCollectionNodes", "Does not have node collection nodes" },
                    { "HasNetworkNodeColections", "Has network node collections" },
                    { "HasNoNetworkNodeCollections", "Does not have network node collections" },
                    { "HasAnalysisNodeCollections", "Has analysis node collections" },
                    { "HasNoAnalysisNodeCollections", "Does not have analysis node collections" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "DateTimeCreated", "Date created" },
                    { "Name", "Name" },
                    { "NodeCollectionDatabaseCount", "Number of node collection databases" },
                    { "NodeCollectionNodeCount", "Number of node collection nodes" },
                    { "NetworkNodeCollectionCount", "Number of network node collections" },
                    { "AnalysisNodeCollectionCount", "Number of analysis node collections" }
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
            // Start with all of the items in the database.
            var query = _context.NodeCollections
                .Where(item => true);
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Name") && item.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Description") && item.Description.Contains(input.SearchString) ||
                    input.SearchIn.Contains("NetworkId") && item.NetworkNodeCollections.Any(item1 => item1.Network.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("NetworkName") && item.NetworkNodeCollections.Any(item1 => item1.Network.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("AnalysisId") && item.AnalysisNodeCollections.Any(item1 => item1.Analysis.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("AnalysisName") && item.AnalysisNodeCollections.Any(item1 => item1.Analysis.Name.Contains(input.SearchString)));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("HasNodeCollectionNodes") ? item.NodeCollectionNodes.Any() : true)
                .Where(item => input.Filter.Contains("HasNoNodeCollectionNodes") ? !item.NodeCollectionNodes.Any() : true)
                .Where(item => input.Filter.Contains("HasNodeCollectionDatabases") ? item.NodeCollectionDatabases.Any() : true)
                .Where(item => input.Filter.Contains("HasNoNodeCollectionDatabases") ? !item.NodeCollectionDatabases.Any() : true)
                .Where(item => input.Filter.Contains("HasNetworkNodeColections") ? item.NetworkNodeCollections.Any() : true)
                .Where(item => input.Filter.Contains("HasNoNetworkNodeColections") ? !item.NetworkNodeCollections.Any() : true)
                .Where(item => input.Filter.Contains("HasAnalysisNodeCollections") ? item.AnalysisNodeCollections.Any() : true)
                .Where(item => input.Filter.Contains("HasNoAnalysisNodeCollections") ? !item.AnalysisNodeCollections.Any() : true);
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
                case var sort when sort == ("NodeCollectionDatabaseCount", "Ascending"):
                    query = query.OrderBy(item => item.NodeCollectionDatabases.Count());
                    break;
                case var sort when sort == ("NodeCollectionDatabaseCount", "Descending"):
                    query = query.OrderByDescending(item => item.NodeCollectionDatabases.Count());
                    break;
                case var sort when sort == ("NodeCollectionNodeCount", "Ascending"):
                    query = query.OrderBy(item => item.NodeCollectionNodes.Count());
                    break;
                case var sort when sort == ("NodeCollectionNodeCount", "Descending"):
                    query = query.OrderByDescending(item => item.NodeCollectionNodes.Count());
                    break;
                case var sort when sort == ("NetworkNodeCollectionCount", "Ascending"):
                    query = query.OrderBy(item => item.NetworkNodeCollections.Count());
                    break;
                case var sort when sort == ("NetworkNodeCollectionCount", "Descending"):
                    query = query.OrderByDescending(item => item.NetworkNodeCollections.Count());
                    break;
                case var sort when sort == ("AnalysisNodeCollectionCount", "Ascending"):
                    query = query.OrderBy(item => item.AnalysisNodeCollections.Count());
                    break;
                case var sort when sort == ("AnalysisNodeCollectionCount", "Descending"):
                    query = query.OrderByDescending(item => item.AnalysisNodeCollections.Count());
                    break;
                default:
                    break;
            }
            // Include the related entitites.
            query = query
                .Include(item => item.NodeCollectionDatabases)
                .Include(item => item.NodeCollectionNodes)
                .Include(item => item.NetworkNodeCollections)
                .Include(item => item.AnalysisNodeCollections);
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
