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

namespace NetControl4BioMed.Pages.Administration.Created.Networks
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
            public SearchViewModel<Network> Search { get; set; }
        }

        public IActionResult OnGet(string searchString = null, IEnumerable<string> searchIn = null, IEnumerable<string> filter = null, string sortBy = null, string sortDirection = null, int? itemsPerPage = null, int? currentPage = 1)
        {
            // Define the search options.
            var options = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "Name", "Name" },
                    { "Description", "Description" },
                    { "DatabaseId", "Database ID" },
                    { "DatabaseName", "Database name" },
                    { "AnalysisId", "Analysis ID" },
                    { "AnalysisName", "Analysis name" },
                    { "NodeCollectionId", "Node collection ID" },
                    { "NodeCollectionName", "Node collection name" }
                },
                Filter = new Dictionary<string, string>
                {
                    { "UsesAlgorithmNone", "Was provided by user" },
                    { "UsesNotAlgorithmNone", "Was not provided by user" },
                    { "UsesAlgorithmNeighbors", "Was generated using \"Neighbors\" algorithm" },
                    { "UsesNotAlgorithmNeighbors", "Was not generated using \"Neighbors\" algorithm" },
                    { "UsesAlgorithmGap0", "Was generated using \"Gap 0\" algorithm" },
                    { "UsesNotAlgorithmGap0", "Was not generated using \"Gap 0\" algorithm" },
                    { "UsesAlgorithmGap1", "Was generated using \"Gap 1\" algorithm" },
                    { "UsesNotAlgorithmGap1", "Was not generated using \"Gap 1\" algorithm" },
                    { "UsesAlgorithmGap2", "Was generated using \"Gap 2\" algorithm" },
                    { "UsesNotAlgorithmGap2", "Was not generated using \"Gap 2\" algorithm" },
                    { "UsesAlgorithmGap3", "Was generated using \"Gap 3\" algorithm" },
                    { "UsesNotAlgorithmGap3", "Was not generated using \"Gap 3\" algorithm" },
                    { "UsesAlgorithmGap4", "Was generated using \"Gap 4\" algorithm" },
                    { "UsesNotAlgorithmGap4", "Was not generated using \"Gap 4\" algorithm" },
                    { "HasNetworkUsers", "Has network users" },
                    { "HasNoNetworkUsers", "Does not have network users" },
                    { "HasNetworkUserInvitations", "Has network user invitations" },
                    { "HasNoNetworkUserInvitations", "Does not have network user invitations" },
                    { "HasNetworkNodes", "Has network nodes" },
                    { "HasNoNetworkNodes", "Does not have network nodes" },
                    { "HasNetworkEdges", "Has network edges" },
                    { "HasNoNetworkEdges", "Does not have network edges" },
                    { "HasAnalysisNetworks", "Has analysis networks" },
                    { "HasNoAnalysisNetworks", "Does not have analysis networks" },
                    { "HasNetworkDatabases", "Has network databases" },
                    { "HasNoNetworkDatabases", "Does not have network databases" },
                    { "HasNetworkNodeCollections", "Has network node collections" },
                    { "HasNoNetworkNodeCollections", "Does not have network node collections" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "DateTimeCreated", "Date created" },
                    { "Name", "Name" },
                    { "NetworkUserCount", "Number of network users" },
                    { "NetworkUserInvitationCount", "Number of network user invitations" },
                    { "NetworkNodeCount", "Number of network nodes" },
                    { "NetworkEdgeCount", "Number of network edges" },
                    { "AnalysisNetworkCount", "Number of analysis networks" },
                    { "NetworkDatabaseCount", "Number of network databases" },
                    { "NetworkNodeCollectionCount", "Number of network node collections" }
                }
            };
            // Define the search input.
            var input = new SearchInputViewModel(options, null, searchString, searchIn, filter, sortBy, sortDirection, itemsPerPage, currentPage);
            // Check if any of the provided variables was null before the reassignment.
            if (input.NeedsRedirect)
            {
                // Redirect to the page where they are all explicitly defined.
                return RedirectToPage(new { searchString = input.SearchString, searchIn = input.SearchIn, filter = input.Filter, sortBy = input.SortBy, sortDirection = input.SortDirection, itemsPerPage = input.ItemsPerPage, currentPage = input.CurrentPage });
            }
            // Start with all of the items in the database.
            var query = _context.Networks
                .AsQueryable();
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Name") && item.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Description") && item.Description.Contains(input.SearchString) ||
                    input.SearchIn.Contains("DatabaseId") && item.NetworkDatabases.Any(item1 => item1.Database.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("DatabaseName") && item.NetworkDatabases.Any(item1 => item1.Database.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("AnalysisId") && item.AnalysisNetworks.Any(item1 => item1.Analysis.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("AnalysisName") && item.AnalysisNetworks.Any(item1 => item1.Analysis.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("NodeCollectionId") && item.NetworkNodeCollections.Any(item1 => item1.NodeCollection.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("NodeCollectionName") && item.NetworkNodeCollections.Any(item1 => item1.NodeCollection.Name.Contains(input.SearchString)));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("UsesAlgorithmNone") ? item.Algorithm == NetworkAlgorithm.None : true)
                .Where(item => input.Filter.Contains("UsesNotAlgorithmNone") ? item.Algorithm != NetworkAlgorithm.None : true)
                .Where(item => input.Filter.Contains("UsesAlgorithmNeighbors") ? item.Algorithm == NetworkAlgorithm.Neighbors : true)
                .Where(item => input.Filter.Contains("UsesNotAlgorithmNeighbors") ? item.Algorithm != NetworkAlgorithm.Neighbors : true)
                .Where(item => input.Filter.Contains("UsesAlgorithmGap0") ? item.Algorithm == NetworkAlgorithm.Gap0 : true)
                .Where(item => input.Filter.Contains("UsesNotAlgorithmGap0") ? item.Algorithm != NetworkAlgorithm.Gap0 : true)
                .Where(item => input.Filter.Contains("UsesAlgorithmGap1") ? item.Algorithm == NetworkAlgorithm.Gap1 : true)
                .Where(item => input.Filter.Contains("UsesNotAlgorithmGap1") ? item.Algorithm != NetworkAlgorithm.Gap1 : true)
                .Where(item => input.Filter.Contains("UsesAlgorithmGap2") ? item.Algorithm == NetworkAlgorithm.Gap2 : true)
                .Where(item => input.Filter.Contains("UsesNotAlgorithmGap2") ? item.Algorithm != NetworkAlgorithm.Gap2 : true)
                .Where(item => input.Filter.Contains("UsesAlgorithmGap3") ? item.Algorithm == NetworkAlgorithm.Gap3 : true)
                .Where(item => input.Filter.Contains("UsesNotAlgorithmGap3") ? item.Algorithm != NetworkAlgorithm.Gap3 : true)
                .Where(item => input.Filter.Contains("UsesAlgorithmGap4") ? item.Algorithm == NetworkAlgorithm.Gap4 : true)
                .Where(item => input.Filter.Contains("UsesNotAlgorithmGap4") ? item.Algorithm != NetworkAlgorithm.Gap4 : true)
                .Where(item => input.Filter.Contains("HasNetworkUsers") ? item.NetworkUsers.Any() : true)
                .Where(item => input.Filter.Contains("HasNoNetworkUsers") ? !item.NetworkUsers.Any() : true)
                .Where(item => input.Filter.Contains("HasNetworkUserInvitations") ? item.NetworkUserInvitations.Any() : true)
                .Where(item => input.Filter.Contains("HasNoNetworkUserInvitations") ? !item.NetworkUserInvitations.Any() : true)
                .Where(item => input.Filter.Contains("HasNetworkNodes") ? item.NetworkNodes.Any() : true)
                .Where(item => input.Filter.Contains("HasNoNetworkNodes") ? !item.NetworkNodes.Any() : true)
                .Where(item => input.Filter.Contains("HasNetworkEdges") ? item.NetworkEdges.Any() : true)
                .Where(item => input.Filter.Contains("HasNoNetworkEdges") ? !item.NetworkEdges.Any() : true)
                .Where(item => input.Filter.Contains("HasAnalysisNetworks") ? item.AnalysisNetworks.Any() : true)
                .Where(item => input.Filter.Contains("HasNoAnalysisNetworks") ? !item.AnalysisNetworks.Any() : true)
                .Where(item => input.Filter.Contains("HasNetworkDatabases") ? item.NetworkDatabases.Any() : true)
                .Where(item => input.Filter.Contains("HasNoNetworkDatabases") ? !item.NetworkDatabases.Any() : true)
                .Where(item => input.Filter.Contains("HasNetworkNodeCollections") ? item.NetworkNodeCollections.Any() : true)
                .Where(item => input.Filter.Contains("HasNoNetworkNodeCollections") ? !item.NetworkNodeCollections.Any() : true);
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
                case var sort when sort == ("NetworkUserCount", "Ascending"):
                    query = query.OrderBy(item => item.NetworkUsers.Count());
                    break;
                case var sort when sort == ("NetworkUserCount", "Descending"):
                    query = query.OrderByDescending(item => item.NetworkUsers.Count());
                    break;
                case var sort when sort == ("NetworkUserInvitationCount", "Ascending"):
                    query = query.OrderBy(item => item.NetworkUserInvitations.Count());
                    break;
                case var sort when sort == ("NetworkUserInvitationCount", "Descending"):
                    query = query.OrderByDescending(item => item.NetworkUserInvitations.Count());
                    break;
                case var sort when sort == ("NetworkNodeCount", "Ascending"):
                    query = query.OrderBy(item => item.NetworkNodes.Count());
                    break;
                case var sort when sort == ("NetworkNodeCount", "Descending"):
                    query = query.OrderByDescending(item => item.NetworkNodes.Count());
                    break;
                case var sort when sort == ("NetworkEdgeCount", "Ascending"):
                    query = query.OrderBy(item => item.NetworkEdges.Count());
                    break;
                case var sort when sort == ("NetworkEdgeCount", "Descending"):
                    query = query.OrderByDescending(item => item.NetworkEdges.Count());
                    break;
                case var sort when sort == ("AnalysisNetworkCount", "Ascending"):
                    query = query.OrderBy(item => item.AnalysisNetworks.Count());
                    break;
                case var sort when sort == ("AnalysisNetworkCount", "Descending"):
                    query = query.OrderByDescending(item => item.AnalysisNetworks.Count());
                    break;
                case var sort when sort == ("NetworkDatabaseCount", "Ascending"):
                    query = query.OrderBy(item => item.NetworkDatabases.Count());
                    break;
                case var sort when sort == ("NetworkDatabaseCount", "Descending"):
                    query = query.OrderByDescending(item => item.NetworkDatabases.Count());
                    break;
                case var sort when sort == ("NetworkNodeCollectionCount", "Ascending"):
                    query = query.OrderBy(item => item.NetworkNodeCollections.Count());
                    break;
                case var sort when sort == ("NetworkNodeCollectionCount", "Descending"):
                    query = query.OrderByDescending(item => item.NetworkNodeCollections.Count());
                    break;
                default:
                    break;
            }
            // Include the related entitites.
            query = query
                .Include(item => item.NetworkUsers)
                .Include(item => item.NetworkUserInvitations)
                .Include(item => item.NetworkNodes)
                .Include(item => item.NetworkEdges)
                .Include(item => item.AnalysisNetworks)
                .Include(item => item.NetworkDatabases)
                .Include(item => item.NetworkNodeCollections);
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<Network>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
