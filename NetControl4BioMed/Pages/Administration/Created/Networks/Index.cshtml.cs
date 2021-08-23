using Microsoft.AspNetCore.Authorization;
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

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
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
                    { "ProteinCollectionId", "Protein collection ID" },
                    { "ProteinCollectionName", "Protein collection name" }
                },
                Filter = new Dictionary<string, string>
                {
                    { "IsPublic", "Is public" },
                    { "IsNotPublic", "Is not public" },
                    { "IsDemonstration", "Is demonstration" },
                    { "IsNotDemonstration", "Is not demonstration" },
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
                    { "HasNetworkUsers", "Has network users" },
                    { "HasNoNetworkUsers", "Does not have network users" },
                    { "HasNetworkProteins", "Has network proteins" },
                    { "HasNoNetworkProteins", "Does not have network proteins" },
                    { "HasNetworkInteractions", "Has network interactions" },
                    { "HasNoNetworkInteractions", "Does not have network interactions" },
                    { "HasAnalyses", "Has analyses" },
                    { "HasNoAnalyses", "Does not have analyses" },
                    { "HasNetworkDatabases", "Has network databases" },
                    { "HasNoNetworkDatabases", "Does not have network databases" },
                    { "HasNetworkProteinCollections", "Has network protein collections" },
                    { "HasNoNetworkProteinCollections", "Does not have network protein collections" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "DateTimeCreated", "Date created" },
                    { "Name", "Name" },
                    { "NetworkUserCount", "Number of network users" },
                    { "NetworkProteinCount", "Number of network proteins" },
                    { "NetworkInteractionCount", "Number of network interactions" },
                    { "AnalysisCount", "Number of analyses" },
                    { "NetworkDatabaseCount", "Number of network databases" },
                    { "NetworkProteinCollectionCount", "Number of network protein collections" }
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
            var query = _context.Networks
                .Where(item => true);
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Name") && item.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Description") && item.Description.Contains(input.SearchString) ||
                    input.SearchIn.Contains("DatabaseId") && item.NetworkDatabases.Any(item1 => item1.Database.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("DatabaseName") && item.NetworkDatabases.Any(item1 => item1.Database.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("AnalysisId") && item.Analyses.Any(item1 => item1.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("AnalysisName") && item.Analyses.Any(item1 => item1.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("ProteinCollectionId") && item.NetworkProteinCollections.Any(item1 => item1.ProteinCollection.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("ProteinCollectionName") && item.NetworkProteinCollections.Any(item1 => item1.ProteinCollection.Name.Contains(input.SearchString)));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("IsPublic") ? item.IsPublic : true)
                .Where(item => input.Filter.Contains("IsNotPublic") ? !item.IsPublic : true)
                .Where(item => input.Filter.Contains("IsDemonstration") ? item.IsDemonstration : true)
                .Where(item => input.Filter.Contains("IsNotDemonstration") ? !item.IsDemonstration : true)
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
                .Where(item => input.Filter.Contains("HasNetworkUsers") ? item.NetworkUsers.Any() : true)
                .Where(item => input.Filter.Contains("HasNoNetworkUsers") ? !item.NetworkUsers.Any() : true)
                .Where(item => input.Filter.Contains("HasNetworkProteins") ? item.NetworkProteins.Any() : true)
                .Where(item => input.Filter.Contains("HasNoNetworkProteins") ? !item.NetworkProteins.Any() : true)
                .Where(item => input.Filter.Contains("HasNetworkInteractions") ? item.NetworkInteractions.Any() : true)
                .Where(item => input.Filter.Contains("HasNoNetworkInteractions") ? !item.NetworkInteractions.Any() : true)
                .Where(item => input.Filter.Contains("HasAnalyses") ? item.Analyses.Any() : true)
                .Where(item => input.Filter.Contains("HasNoAnalyses") ? !item.Analyses.Any() : true)
                .Where(item => input.Filter.Contains("HasNetworkDatabases") ? item.NetworkDatabases.Any() : true)
                .Where(item => input.Filter.Contains("HasNoNetworkDatabases") ? !item.NetworkDatabases.Any() : true)
                .Where(item => input.Filter.Contains("HasNetworkProteinCollections") ? item.NetworkProteinCollections.Any() : true)
                .Where(item => input.Filter.Contains("HasNoNetworkProteinCollections") ? !item.NetworkProteinCollections.Any() : true);
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
                case var sort when sort == ("NetworkProteinCount", "Ascending"):
                    query = query.OrderBy(item => item.NetworkProteins.Count());
                    break;
                case var sort when sort == ("NetworkProteinCount", "Descending"):
                    query = query.OrderByDescending(item => item.NetworkProteins.Count());
                    break;
                case var sort when sort == ("NetworkInteractionCount", "Ascending"):
                    query = query.OrderBy(item => item.NetworkInteractions.Count());
                    break;
                case var sort when sort == ("NetworkInteractionCount", "Descending"):
                    query = query.OrderByDescending(item => item.NetworkInteractions.Count());
                    break;
                case var sort when sort == ("AnalysisCount", "Ascending"):
                    query = query.OrderBy(item => item.Analyses.Count());
                    break;
                case var sort when sort == ("AnalysisCount", "Descending"):
                    query = query.OrderByDescending(item => item.Analyses.Count());
                    break;
                case var sort when sort == ("NetworkDatabaseCount", "Ascending"):
                    query = query.OrderBy(item => item.NetworkDatabases.Count());
                    break;
                case var sort when sort == ("NetworkDatabaseCount", "Descending"):
                    query = query.OrderByDescending(item => item.NetworkDatabases.Count());
                    break;
                case var sort when sort == ("NetworkProteinCollectionCount", "Ascending"):
                    query = query.OrderBy(item => item.NetworkProteinCollections.Count());
                    break;
                case var sort when sort == ("NetworkProteinCollectionCount", "Descending"):
                    query = query.OrderByDescending(item => item.NetworkProteinCollections.Count());
                    break;
                default:
                    break;
            }
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
