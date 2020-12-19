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

namespace NetControl4BioMed.Pages.Content.DatabaseTypes.PPI.Created.Analyses
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

            public SearchViewModel<ItemModel> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "Name", "Name" },
                    { "Description", "Description" },
                    { "UserId", "User ID" },
                    { "UserEmail", "User e-mail" },
                    { "DatabaseId", "Database ID" },
                    { "DatabaseName", "Database name" },
                    { "NodeId", "Protein ID" },
                    { "NodeName", "Protein name" },
                    { "EdgeId", "Interaction ID" },
                    { "EdgeName", "Interaction name" },
                    { "NodeCollectionId", "Protein collection ID" },
                    { "NodeCollectionName", "Protein collection name" },
                    { "NetworkId", "Network ID" },
                    { "NetworkName", "Network name" }
                },
                Filter = new Dictionary<string, string>
                {
                    { "HasStatusError", "Has status \"Error\"" },
                    { "HasNotStatusError", "Does not have status \"Error\"" },
                    { "HasStatusDefined", "Has status \"Defined\"" },
                    { "HasNotStatusDefined", "Does not have status \"Defined\"" },
                    { "HasStatusGenerating", "Has status \"Generating\"" },
                    { "HasNotStatusGenerating", "Does not have status \"Generating\"" },
                    { "HasStatusScheduled", "Has status \"Scheduled\"" },
                    { "HasNotStatusScheduled", "Does not have status \"Scheduled\"" },
                    { "HasStatusOngoing", "Has status \"Ongoing\"" },
                    { "HasNotStatusOngoing", "Does not have status \"Ongoing\"" },
                    { "HasStatusStopping", "Has status \"Stopping\"" },
                    { "HasNotStatusStopping", "Does not have status \"Stopping\"" },
                    { "HasStatusStopped", "Has status \"Stopped\"" },
                    { "HasNotStatusStopped", "Does not have status \"Stopped\"" },
                    { "HasStatusCompleted", "Has status \"Completed\"" },
                    { "HasNotStatusCompleted", "Does not have status \"Completed\"" },
                    { "HasAnalysisNodeCollections", "Has protein collections" },
                    { "HasNoAnalysisNodeCollections", "Does not have protein collections" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "DateTimeCreated", "Date created" },
                    { "DateTimeStarted", "Date started" },
                    { "DateTimeEnded", "Date ended" },
                    { "Name", "Name" },
                    { "Status", "Status" },
                    { "CurrentIteration", "Current iteration" },
                    { "CurrentIterationWithoutImprovement", "Current iteration without improvement" },
                    { "AnalysisUserCount", "Number of users" },
                    { "AnalysisDatabaseCount", "Number of databases" },
                    { "AnalysisNodeCount", "Number of proteins" },
                    { "AnalysisEdgeCount", "Number of interactions" },
                    { "AnalysisNodeCollectionCount", "Number of protein collections" },
                    { "AnalysisNetworkCount", "Number of networks" }
                }
            };
        }

        public class ItemModel
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public AnalysisStatus Status { get; set; }
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
            var query = _context.Analyses
                .Where(item => item.AnalysisDatabases.Any(item1 => item1.Database.DatabaseType.Name == "PPI"))
                .Where(item => item.AnalysisUsers.Any(item1 => item1.User == user));
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Name") && item.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Description") && item.Description.Contains(input.SearchString) ||
                    input.SearchIn.Contains("UserId") && item.AnalysisUsers.Any(item1 => item1.User.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("UserEmail") && (item.AnalysisUsers.Any(item1 => item1.User.Email.Contains(input.SearchString)) || item.AnalysisUserInvitations.Any(item1 => item1.Email.Contains(input.SearchString))) ||
                    input.SearchIn.Contains("DatabaseId") && item.AnalysisDatabases.Any(item1 => item1.Database.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("DatabaseName") && item.AnalysisDatabases.Any(item1 => item1.Database.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("NodeId") && item.AnalysisNodes.Any(item1 => item1.Node.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("NodeName") && item.AnalysisNodes.Any(item1 => item1.Node.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("EdgeId") && item.AnalysisEdges.Any(item1 => item1.Edge.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("EdgeName") && item.AnalysisEdges.Any(item1 => item1.Edge.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("NodeCollectionId") && item.AnalysisNodeCollections.Any(item1 => item1.NodeCollection.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("NodeCollectionName") && item.AnalysisNodeCollections.Any(item1 => item1.NodeCollection.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("NetworkId") && item.AnalysisNetworks.Any(item1 => item1.Network.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("NetworkName") && item.AnalysisNetworks.Any(item1 => item1.Network.Name.Contains(input.SearchString)));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("HasStatusError") ? item.Status == AnalysisStatus.Error : true)
                .Where(item => input.Filter.Contains("HasNotStatusError") ? item.Status != AnalysisStatus.Error : true)
                .Where(item => input.Filter.Contains("HasStatusDefined") ? item.Status == AnalysisStatus.Defined : true)
                .Where(item => input.Filter.Contains("HasNotStatusDefined") ? item.Status != AnalysisStatus.Defined : true)
                .Where(item => input.Filter.Contains("HasStatusGenerating") ? item.Status == AnalysisStatus.Generating : true)
                .Where(item => input.Filter.Contains("HasNotStatusGenerating") ? item.Status != AnalysisStatus.Generating : true)
                .Where(item => input.Filter.Contains("HasStatusScheduled") ? item.Status == AnalysisStatus.Scheduled : true)
                .Where(item => input.Filter.Contains("HasNotStatusScheduled") ? item.Status != AnalysisStatus.Scheduled : true)
                .Where(item => input.Filter.Contains("HasStatusInitializing") ? item.Status == AnalysisStatus.Initializing : true)
                .Where(item => input.Filter.Contains("HasNotStatusInitializing") ? item.Status != AnalysisStatus.Initializing : true)
                .Where(item => input.Filter.Contains("HasStatusOngoing") ? item.Status == AnalysisStatus.Ongoing : true)
                .Where(item => input.Filter.Contains("HasNotStatusOngoing") ? item.Status != AnalysisStatus.Ongoing : true)
                .Where(item => input.Filter.Contains("HasStatusStopping") ? item.Status == AnalysisStatus.Stopping : true)
                .Where(item => input.Filter.Contains("HasNotStatusStopping") ? item.Status != AnalysisStatus.Stopping : true)
                .Where(item => input.Filter.Contains("HasStatusStopped") ? item.Status == AnalysisStatus.Stopped : true)
                .Where(item => input.Filter.Contains("HasNotStatusStopped") ? item.Status != AnalysisStatus.Stopped : true)
                .Where(item => input.Filter.Contains("HasStatusCompleted") ? item.Status == AnalysisStatus.Completed : true)
                .Where(item => input.Filter.Contains("HasNotStatusCompleted") ? item.Status != AnalysisStatus.Completed : true)
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
                case var sort when sort == ("DateTimeStarted", "Ascending"):
                    query = query.OrderBy(item => item.DateTimeStarted);
                    break;
                case var sort when sort == ("DateTimeStarted", "Descending"):
                    query = query.OrderByDescending(item => item.DateTimeStarted);
                    break;
                case var sort when sort == ("DateTimeEnded", "Ascending"):
                    query = query.OrderBy(item => item.DateTimeEnded);
                    break;
                case var sort when sort == ("DateTimeEnded", "Descending"):
                    query = query.OrderByDescending(item => item.DateTimeEnded);
                    break;
                case var sort when sort == ("Name", "Ascending"):
                    query = query.OrderBy(item => item.Name);
                    break;
                case var sort when sort == ("Name", "Descending"):
                    query = query.OrderByDescending(item => item.Name);
                    break;
                case var sort when sort == ("CurrentIteration", "Ascending"):
                    query = query.OrderBy(item => item.CurrentIteration);
                    break;
                case var sort when sort == ("CurrentIteration", "Descending"):
                    query = query.OrderByDescending(item => item.CurrentIteration);
                    break;
                case var sort when sort == ("CurrentIterationWithoutImprovement", "Ascending"):
                    query = query.OrderBy(item => item.CurrentIterationWithoutImprovement);
                    break;
                case var sort when sort == ("CurrentIterationWithoutImprovement", "Descending"):
                    query = query.OrderByDescending(item => item.CurrentIterationWithoutImprovement);
                    break;
                case var sort when sort == ("Status", "Ascending"):
                    query = query.OrderBy(item => item.Status);
                    break;
                case var sort when sort == ("Status", "Descending"):
                    query = query.OrderByDescending(item => item.Status);
                    break;
                case var sort when sort == ("AnalysisUserCount", "Ascending"):
                    query = query.OrderBy(item => item.AnalysisUsers.Count() + item.AnalysisUserInvitations.Count());
                    break;
                case var sort when sort == ("AnalysisUserCount", "Descending"):
                    query = query.OrderByDescending(item => item.AnalysisUsers.Count() + item.AnalysisUserInvitations.Count());
                    break;
                case var sort when sort == ("AnalysisDatabaseCount", "Ascending"):
                    query = query.OrderBy(item => item.AnalysisDatabases.Count());
                    break;
                case var sort when sort == ("AnalysisDatabaseCount", "Descending"):
                    query = query.OrderByDescending(item => item.AnalysisDatabases.Count());
                    break;
                case var sort when sort == ("AnalysisNodeCount", "Ascending"):
                    query = query.OrderBy(item => item.AnalysisNodes.Count());
                    break;
                case var sort when sort == ("AnalysisNodeCount", "Descending"):
                    query = query.OrderByDescending(item => item.AnalysisNodes.Count());
                    break;
                case var sort when sort == ("AnalysisEdgeCount", "Ascending"):
                    query = query.OrderBy(item => item.AnalysisEdges.Count());
                    break;
                case var sort when sort == ("AnalysisEdgeCount", "Descending"):
                    query = query.OrderByDescending(item => item.AnalysisEdges.Count());
                    break;
                case var sort when sort == ("AnalysisNetworkCount", "Ascending"):
                    query = query.OrderBy(item => item.AnalysisNetworks.Count());
                    break;
                case var sort when sort == ("AnalysisNetworkCount", "Descending"):
                    query = query.OrderByDescending(item => item.AnalysisNetworks.Count());
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
            // Define the view.
            View = new ViewModel
            {
                IsUserAuthenticated = user != null,
                Search = new SearchViewModel<ItemModel>(_linkGenerator, HttpContext, input, query.Select(item => new ItemModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    Status = item.Status
                }))
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
            var item = _context.Analyses
                .Where(item => item.AnalysisDatabases.Any(item1 => item1.Database.DatabaseType.Name == "PPI"))
                .Where(item => item.AnalysisUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == id)
                .FirstOrDefault();
            // Return the analysis data.
            return new JsonResult(new
            {
                Status = item.Status.ToString(),
                Progress = ((double)item.CurrentIteration * 100 / item.MaximumIterations).ToString("0.00"),
                ProgressWithoutImprovement = ((double)item.CurrentIterationWithoutImprovement * 100 / item.MaximumIterationsWithoutImprovement).ToString("0.00")
            });
        }
    }
}
