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

namespace NetControl4BioMed.Pages.Content.Created.Networks
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
            public bool IsUserAuthenticated { get; set; }

            public IEnumerable<DatabaseType> DatabaseTypes { get; set; }

            public SearchViewModel<Network> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "Name", "Name" },
                    { "Description", "Description" },
                    { "NetworkDatabases", "Databases" },
                    { "NetworkNodes", "Nodes" },
                    { "NetworkEdges", "Edges" },
                    { "NetworkNodeCollections", "Node collections" },
                    { "AnalysisNetworks", "Analyses" }
                },
                Filter = new Dictionary<string, string>
                {
                    { "IsError", "Is error" },
                    { "IsNotError", "Is not error" },
                    { "IsDefined", "Is defined" },
                    { "IsNotDefined", "Is not defined" },
                    { "IsGenerating", "Is generating" },
                    { "IsNotGenerating", "Is not generating" },
                    { "IsCompleted", "Is completed" },
                    { "IsNotCompleted", "Is not completed" },
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
                    { "HasNetworkUserInvitations", "Has user invitations" },
                    { "HasNoNetworkUserInvitations", "Does not have user invitations" },
                    { "HasAnalysisNetworks", "Is used by analyses" },
                    { "HasNoAnalysisNetworks", "Is not used by any analyses" },
                    { "HasNetworkNodeCollections", "Uses node collections" },
                    { "HasNoNetworkNodeCollections", "Does not use node collections" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "DateTimeCreated", "Date created" },
                    { "Name", "Name" },
                    { "Status", "Status" },
                    { "NetworkUserCount", "Number of users" },
                    { "NetworkUserInvitationCount", "Number of user invitations" },
                    { "NetworkDatabaseCount", "Number of databases" },
                    { "NetworkNodeCount", "Number of nodes" },
                    { "NetworkEdgeCount", "Number of edges" },
                    { "NetworkNodeCollectionCount", "Number of node collections" },
                    { "AnalysisNetworkCount", "Number of analyses" }
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
            // Start with all of the items to which the user has access.
            var query = _context.Networks
                .Where(item => item.NetworkUsers.Any(item1 => item1.User == user));
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Name") && item.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Description") && item.Description.Contains(input.SearchString) ||
                    input.SearchIn.Contains("NetworkDatabases") && item.NetworkDatabases.Any(item1 => item1.Database.Id.Contains(input.SearchString) || item1.Database.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("NetworkNodes") && item.NetworkNodes.Where(item1 => item1.Node.DatabaseNodes.Any(item2 => item2.Database.IsPublic || item2.Database.DatabaseUsers.Any(item3 => item3.User == user))).Any(item1 => item1.Node.Id.Contains(input.SearchString) || item1.Node.Name.Contains(input.SearchString) || item1.Node.DatabaseNodeFieldNodes.Where(item2 => item2.DatabaseNodeField.Database.IsPublic || item2.DatabaseNodeField.Database.DatabaseUsers.Any(item3 => item3.User == user)).Any(item2 => item2.DatabaseNodeField.IsSearchable && item2.Value.Contains(input.SearchString))) ||
                    input.SearchIn.Contains("NetworkEdges") && item.NetworkEdges.Where(item1 => item1.Edge.DatabaseEdges.Any(item2 => item2.Database.IsPublic || item2.Database.DatabaseUsers.Any(item3 => item3.User == user))).Any(item1 => item1.Edge.Id.Contains(input.SearchString) || item1.Edge.Name.Contains(input.SearchString) || item1.Edge.EdgeNodes.Where(item2 => item2.Node.DatabaseNodeFieldNodes.Any(item3 => item3.DatabaseNodeField.Database.IsPublic || item3.DatabaseNodeField.Database.DatabaseUsers.Any(item4 => item4.User == user))).Any(item2 => item2.Node.Id.Contains(input.SearchString) || item2.Node.Name.Contains(input.SearchString) || item2.Node.DatabaseNodeFieldNodes.Where(item3 => item3.DatabaseNodeField.Database.IsPublic || item3.DatabaseNodeField.Database.DatabaseUsers.Any(item4 => item4.User == user)).Any(item3 => item3.DatabaseNodeField.IsSearchable && item3.Value.Contains(input.SearchString)))) ||
                    input.SearchIn.Contains("NetworkNodeCollections") && item.NetworkNodeCollections.Any(item1 => item1.NodeCollection.Id.Contains(input.SearchString) || item1.NodeCollection.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("AnalysisNetworks") && item.AnalysisNetworks.Any(item1 => item1.Analysis.Id.Contains(input.SearchString) || item1.Analysis.Name.Contains(input.SearchString)));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("IsError") ? item.Status == NetworkStatus.Error : true)
                .Where(item => input.Filter.Contains("IsNotError") ? item.Status != NetworkStatus.Error : true)
                .Where(item => input.Filter.Contains("IsDefined") ? item.Status == NetworkStatus.Defined : true)
                .Where(item => input.Filter.Contains("IsNotDefined") ? item.Status != NetworkStatus.Defined : true)
                .Where(item => input.Filter.Contains("IsGenerating") ? item.Status == NetworkStatus.Generating : true)
                .Where(item => input.Filter.Contains("IsNotGenerating") ? item.Status != NetworkStatus.Generating : true)
                .Where(item => input.Filter.Contains("IsCompleted") ? item.Status == NetworkStatus.Completed : true)
                .Where(item => input.Filter.Contains("IsNotCompleted") ? item.Status != NetworkStatus.Completed : true)
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
                .Where(item => input.Filter.Contains("HasNetworkUserInvitations") ? item.NetworkUserInvitations.Any() : true)
                .Where(item => input.Filter.Contains("HasNoNetworkUserInvitations") ? !item.NetworkUserInvitations.Any() : true)
                .Where(item => input.Filter.Contains("HasAnalysisNetworks") ? item.AnalysisNetworks.Any() : true)
                .Where(item => input.Filter.Contains("HasNoAnalysisNetworks") ? !item.AnalysisNetworks.Any() : true)
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
                case var sort when sort == ("Status", "Ascending"):
                    query = query.OrderBy(item => item.Status);
                    break;
                case var sort when sort == ("Status", "Descending"):
                    query = query.OrderByDescending(item => item.Status);
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
                case var sort when sort == ("NetworkDatabaseCount", "Ascending"):
                    query = query.OrderBy(item => item.NetworkDatabases.Count());
                    break;
                case var sort when sort == ("NetworkDatabaseCount", "Descending"):
                    query = query.OrderByDescending(item => item.NetworkDatabases.Count());
                    break;
                case var sort when sort == ("NetworkNodeCount", "Ascending"):
                    query = query.OrderBy(item => item.NetworkNodes.Where(item1 => item1.Type == NetworkNodeType.None).Count());
                    break;
                case var sort when sort == ("NetworkNodeCount", "Descending"):
                    query = query.OrderByDescending(item => item.NetworkNodes.Where(item1 => item1.Type == NetworkNodeType.None).Count());
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
                case var sort when sort == ("NetworkNodeCollectionCount", "Ascending"):
                    query = query.OrderBy(item => item.NetworkNodeCollections.Count());
                    break;
                case var sort when sort == ("NetworkNodeCollectionCount", "Descending"):
                    query = query.OrderByDescending(item => item.NetworkNodeCollections.Count());
                    break;
                default:
                    break;
            }
            // Define the view.
            View = new ViewModel
            {
                IsUserAuthenticated = user != null,
                DatabaseTypes = _context.DatabaseTypes.AsEnumerable(),
                Search = new SearchViewModel<Network>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnGetRefreshAsync(string id)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Return an empty result.
                return new JsonResult(new { });
            }
            // Get the item with the provided ID.
            var item = _context.Networks
                .Where(item => item.NetworkUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == id)
                .FirstOrDefault();
            // Return the analysis data.
            return new JsonResult(new
            {
                Status = item != null ? item.Status.ToString() : string.Empty
            });
        }
    }
}
