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

namespace NetControl4BioMed.Pages.PrivateData.Analyses
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
                    { "NetworkId", "Network ID" },
                    { "NetworkName", "Network name" }
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
                    { "HasStatusScheduled", "Has status \"Scheduled\"" },
                    { "HasNotStatusScheduled", "Does not have status \"Scheduled\"" },
                    { "HasStatusInitializing", "Has status \"Initializing\"" },
                    { "HasNotStatusInitializing", "Does not have status \"Initializing\"" },
                    { "HasStatusOngoing", "Has status \"Ongoing\"" },
                    { "HasNotStatusOngoing", "Does not have status \"Ongoing\"" },
                    { "HasStatusStopping", "Has status \"Stopping\"" },
                    { "HasNotStatusStopping", "Does not have status \"Stopping\"" },
                    { "HasStatusStopped", "Has status \"Stopped\"" },
                    { "HasNotStatusStopped", "Does not have status \"Stopped\"" },
                    { "HasStatusCompleted", "Has status \"Completed\"" },
                    { "HasNotStatusCompleted", "Does not have status \"Completed\"" },
                    { "HasAnalysisDatabases", "Has databases" },
                    { "HasNoAnalysisDatabases", "Does not have databases" },
                    { "HasAnalysisProteinCollections", "Has protein collections" },
                    { "HasNoAnalysisProteinCollections", "Does not have protein collections" }
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
                    { "AnalysisDatabaseCount", "Number of databases" },
                    { "AnalysisProteinCount", "Number of proteins" },
                    { "AnalysisInteractionCount", "Number of interactions" },
                    { "AnalysisProteinCollectionCount", "Number of protein collections" },
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
                return RedirectToPage("/PrivateData/Analyses/Index");
            }
            // Start with all of the items to which the user has access.
            var query = _context.Analyses
                .Where(item => item.AnalysisUsers.Any(item1 => item1.Email == user.Email));
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Name") && item.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Description") && item.Description.Contains(input.SearchString) ||
                    input.SearchIn.Contains("DatabaseId") && item.AnalysisDatabases.Any(item1 => item1.Database.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("DatabaseName") && item.AnalysisDatabases.Any(item1 => item1.Database.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("ProteinId") && item.AnalysisProteins.Any(item1 => item1.Protein.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("ProteinName") && item.AnalysisProteins.Any(item1 => item1.Protein.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("InteractionId") && item.AnalysisInteractions.Any(item1 => item1.Interaction.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("InteractionName") && item.AnalysisInteractions.Any(item1 => item1.Interaction.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("ProteinCollectionId") && item.AnalysisProteinCollections.Any(item1 => item1.ProteinCollection.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("ProteinCollectionName") && item.AnalysisProteinCollections.Any(item1 => item1.ProteinCollection.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("NetworkId") && item.Network.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("NetworkName") && item.Network.Name.Contains(input.SearchString));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("IsPublic") ? item.IsPublic : true)
                .Where(item => input.Filter.Contains("IsNotPublic") ? !item.IsPublic : true)
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
                .Where(item => input.Filter.Contains("HasAnalysisDatabases") ? item.AnalysisDatabases.Any() : true)
                .Where(item => input.Filter.Contains("HasNoAnalysisDatabases") ? !item.AnalysisDatabases.Any() : true)
                .Where(item => input.Filter.Contains("HasAnalysisProteinCollections") ? item.AnalysisProteinCollections.Any() : true)
                .Where(item => input.Filter.Contains("HasNoAnalysisProteinCollections") ? !item.AnalysisProteinCollections.Any() : true);
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
                case var sort when sort == ("AnalysisDatabaseCount", "Ascending"):
                    query = query.OrderBy(item => item.AnalysisDatabases.Count());
                    break;
                case var sort when sort == ("AnalysisDatabaseCount", "Descending"):
                    query = query.OrderByDescending(item => item.AnalysisDatabases.Count());
                    break;
                case var sort when sort == ("AnalysisProteinCount", "Ascending"):
                    query = query.OrderBy(item => item.AnalysisProteins.Count());
                    break;
                case var sort when sort == ("AnalysisProteinCount", "Descending"):
                    query = query.OrderByDescending(item => item.AnalysisProteins.Count());
                    break;
                case var sort when sort == ("AnalysisInteractionCount", "Ascending"):
                    query = query.OrderBy(item => item.AnalysisInteractions.Count());
                    break;
                case var sort when sort == ("AnalysisInteractionCount", "Descending"):
                    query = query.OrderByDescending(item => item.AnalysisInteractions.Count());
                    break;
                case var sort when sort == ("AnalysisProteinCollectionCount", "Ascending"):
                    query = query.OrderBy(item => item.AnalysisProteinCollections.Count());
                    break;
                case var sort when sort == ("AnalysisProteinCollectionCount", "Descending"):
                    query = query.OrderByDescending(item => item.AnalysisProteinCollections.Count());
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
            var item = _context.Analyses
                .Where(item => item.AnalysisUsers.Any(item1 => item1.Email == user.Email))
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
