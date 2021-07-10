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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.PrivateData.Networks
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
            public SearchViewModel<ItemModel> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "Name", "Name" },
                    { "Description", "Description" },
                    { "DatabaseId", "Database ID" },
                    { "DatabaseName", "Database name" },
                    { "ProteinId", "Protein ID" },
                    { "ProteinName", "Protein name" },
                    { "InteractionId", "Interaction ID" },
                    { "InteractionName", "Interaction name" },
                    { "ProteinCollectionId", "Protein collection ID" },
                    { "ProteinCollectionName", "Protein collection name" },
                    { "AnalysisId", "Analysis ID" },
                    { "AnalysisName", "Analysis name" }
                },
                Filter = new Dictionary<string, string>
                {
                    { "IsPublic", "Is public" },
                    { "IsNotPublic", "Is not public" },
                    { "HasStatusError", "Has status \"Error\"" },
                    { "HasNotStatusError", "Does not have status \"Error\"" },
                    { "HasStatusDefined", "Has status \"Defined\"" },
                    { "HasNotStatusDefined", "Does not have status \"Defined\"" },
                    { "HasStatusGenerating", "Has status \"Generating\"" },
                    { "HasNotStatusGenerating", "Does not have status \"Generating\"" },
                    { "HasStatusCompleted", "Has status \"Completed\"" },
                    { "HasNotStatusCompleted", "Does not have status \"Completed\"" },
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
                    { "HasNetworkDatabases", "Has databases" },
                    { "HasNoNetworkDatabases", "Does not have databases" },
                    { "HasNetworkProteinCollections", "Has protein collections" },
                    { "HasNoNetworkProteinCollections", "Does not have protein collections" },
                    { "HasAnalyses", "Has analyses" },
                    { "HasNoAnalyses", "Does not have analyses" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "DateTimeCreated", "Date created" },
                    { "Name", "Name" },
                    { "Status", "Status" },
                    { "NetworkDatabaseCount", "Number of databases" },
                    { "NetworkProteinCount", "Number of proteins" },
                    { "NetworkInteractionCount", "Number of interactions" },
                    { "NetworkProteinCollectionCount", "Number of protein collections" },
                    { "AnalysisCount", "Number of analyses" }
                }
            };
        }

        public class ItemModel
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public NetworkStatus Status { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string searchString = null, IEnumerable<string> searchIn = null, IEnumerable<string> filter = null, string sortBy = null, string sortDirection = null, int? itemsPerPage = null, int? currentPage = 1)
        {
            // Define the search input.
            var input = new SearchInputViewModel(ViewModel.SearchOptions, null, searchString, searchIn, filter, sortBy, sortDirection, itemsPerPage, currentPage);
            // Check if any of the provided variables was null before the reassignment.
            if (input.NeedsRedirect)
            {
                // Redirect to the page where they are all explicitly defined.
                return RedirectToPage(new { searchString = input.SearchString, searchIn = input.SearchIn, filter = input.Filter, sortBy = input.SortBy, sortDirection = input.SortDirection, itemsPerPage = input.ItemsPerPage, currentPage = input.CurrentPage });
            }
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if there isn't any user found.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: An error occured while trying to load the user data. If you are already logged in, please log out and try again.";
                // Redirect to the index page.
                return RedirectToPage("/PrivateData/Networks/Index");
            }
            // Start with all of the items to which the user has access.
            var query = _context.Networks
                .Where(item => item.NetworkUsers.Any(item1 => item1.Email == user.Email));
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Name") && item.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Description") && item.Description.Contains(input.SearchString) ||
                    input.SearchIn.Contains("DatabaseId") && item.NetworkDatabases.Any(item1 => item1.Database.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("DatabaseName") && item.NetworkDatabases.Any(item1 => item1.Database.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("ProteinId") && item.NetworkProteins.Any(item1 => item1.Protein.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("ProteinName") && item.NetworkProteins.Any(item1 => item1.Protein.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("InteractionId") && item.NetworkInteractions.Any(item1 => item1.Interaction.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("InteractionName") && item.NetworkInteractions.Any(item1 => item1.Interaction.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("ProteinCollectionId") && item.NetworkProteinCollections.Any(item1 => item1.ProteinCollection.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("ProteinCollectionName") && item.NetworkProteinCollections.Any(item1 => item1.ProteinCollection.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("AnalysisId") && item.Analyses.Any(item1 => item1.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("AnalysisName") && item.Analyses.Any(item1 => item1.Name.Contains(input.SearchString)));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("IsPublic") ? item.IsPublic : true)
                .Where(item => input.Filter.Contains("IsNotPublic") ? !item.IsPublic : true)
                .Where(item => input.Filter.Contains("HasStatusError") ? item.Status == NetworkStatus.Error : true)
                .Where(item => input.Filter.Contains("HasNotStatusError") ? item.Status != NetworkStatus.Error : true)
                .Where(item => input.Filter.Contains("HasStatusDefined") ? item.Status == NetworkStatus.Defined : true)
                .Where(item => input.Filter.Contains("HasNotStatusDefined") ? item.Status != NetworkStatus.Defined : true)
                .Where(item => input.Filter.Contains("HasStatusGenerating") ? item.Status == NetworkStatus.Generating : true)
                .Where(item => input.Filter.Contains("HasNotStatusGenerating") ? item.Status != NetworkStatus.Generating : true)
                .Where(item => input.Filter.Contains("HasStatusCompleted") ? item.Status == NetworkStatus.Completed : true)
                .Where(item => input.Filter.Contains("HasNotStatusCompleted") ? item.Status != NetworkStatus.Completed : true)
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
                .Where(item => input.Filter.Contains("HasNetworkDatabases") ? item.NetworkDatabases.Any() : true)
                .Where(item => input.Filter.Contains("HasNoNetworkDatabases") ? !item.NetworkDatabases.Any() : true)
                .Where(item => input.Filter.Contains("HasNetworkProteinCollections") ? item.NetworkProteinCollections.Any() : true)
                .Where(item => input.Filter.Contains("HasNoNetworkProteinCollections") ? !item.NetworkProteinCollections.Any() : true)
                .Where(item => input.Filter.Contains("HasAnalysisNetworks") ? item.Analyses.Any() : true)
                .Where(item => input.Filter.Contains("HasNoAnalysisNetworks") ? !item.Analyses.Any() : true);
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
                case var sort when sort == ("NetworkDatabaseCount", "Ascending"):
                    query = query.OrderBy(item => item.NetworkDatabases.Count());
                    break;
                case var sort when sort == ("NetworkDatabaseCount", "Descending"):
                    query = query.OrderByDescending(item => item.NetworkDatabases.Count());
                    break;
                case var sort when sort == ("NetworkProteinCount", "Ascending"):
                    query = query.OrderBy(item => item.NetworkProteins.Where(item1 => item1.Type == NetworkProteinType.None).Count());
                    break;
                case var sort when sort == ("NetworkProteinCount", "Descending"):
                    query = query.OrderByDescending(item => item.NetworkProteins.Where(item1 => item1.Type == NetworkProteinType.None).Count());
                    break;
                case var sort when sort == ("NetworkInteractionCount", "Ascending"):
                    query = query.OrderBy(item => item.NetworkInteractions.Count());
                    break;
                case var sort when sort == ("NetworkInteractionCount", "Descending"):
                    query = query.OrderByDescending(item => item.NetworkInteractions.Count());
                    break;
                case var sort when sort == ("NetworkProteinCollectionCount", "Ascending"):
                    query = query.OrderBy(item => item.NetworkProteinCollections.Count());
                    break;
                case var sort when sort == ("NetworkProteinCollectionCount", "Descending"):
                    query = query.OrderByDescending(item => item.NetworkProteinCollections.Count());
                    break;
                case var sort when sort == ("AnalysisCount", "Ascending"):
                    query = query.OrderBy(item => item.Analyses.Count());
                    break;
                case var sort when sort == ("AnalysisCount", "Descending"):
                    query = query.OrderByDescending(item => item.Analyses.Count());
                    break;
                default:
                    break;
            }
            // Define the view.
            View = new ViewModel
            {
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
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Return an empty result.
                return new JsonResult(new { });
            }
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Get the item with the provided ID.
            var item = _context.Networks
                .Where(item => item.NetworkUsers.Any(item1 => item1.Email == user.Email))
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
