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

namespace NetControl4BioMed.Pages.Administration.Created.Analyses
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
            public SearchViewModel<Analysis> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "Name", "Name" },
                    { "Description", "Description" },
                    { "DatabaseId", "Database ID" },
                    { "DatabaseName", "Database name" },
                    { "NetworkId", "Network ID" },
                    { "NetworkName", "Network name" },
                    { "ProteinCollectionId", "Protein collection ID" },
                    { "ProteinCollectionName", "Protein collection name" }
                },
                Filter = new Dictionary<string, string>
                {
                    { "IsError", "Is error" },
                    { "IsNotError", "Is not error" },
                    { "IsDefined", "Is defined" },
                    { "IsNotDefined", "Is not defined" },
                    { "IsGenerating", "Is generating" },
                    { "IsNotGenerating", "Is not generating" },
                    { "IsScheduled", "Is scheduled" },
                    { "IsNotScheduled", "Is not scheduled" },
                    { "IsInitializing", "Is initializing" },
                    { "IsNotInitializing", "Is not initializing" },
                    { "IsOngoing", "Is ongoing" },
                    { "IsNotOngoing", "Is not ongoing" },
                    { "IsStopping", "Is stopping" },
                    { "IsNotStopping", "Is not stopping" },
                    { "IsStopped", "Is stopped" },
                    { "IsNotStopped", "Is not stopped" },
                    { "IsCompleted", "Is completed" },
                    { "IsNotCompleted", "Is not completed" },
                    { "UsesGreedyAlgorithm", "Uses the greedy algorithm" },
                    { "UsesNotGreedyAlgorithm", "Doesn't use the greedy algorithm" },
                    { "UsesGeneticAlgorithm", "Uses the genetic algorithm" },
                    { "UsesNotGeneticAlgorithm", "Doesn't use the genetic algorithm" },
                    { "HasAnalysisUsers", "Has analysis users" },
                    { "HasNoAnalysisUsers", "Does not have analysis users" },
                    { "HasAnalysisProteins", "Has analysis proteins" },
                    { "HasNoAnalysisProteins", "Does not have analysis proteins" },
                    { "HasAnalysisInteractions", "Has analysis interactions" },
                    { "HasNoAnalysisInteractions", "Does not have analysis interactions" },
                    { "HasAnalysisDatabases", "Has analysis databases" },
                    { "HasNoAnalysisDatabases", "Does not have analysis databases" },
                    { "HasAnalysisProteinCollections", "Has analysis protein collections" },
                    { "HasNoAnalysisProteinCollections", "Does not have analysis protein collections" },
                    { "HasControlPaths", "Has control paths" },
                    { "HasNoControlPaths", "Does not have control paths" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "DateTimeCreated", "Date created" },
                    { "DateTimeStarted", "Date started" },
                    { "DateTimeEnded", "Date ended" },
                    { "Name", "Name" },
                    { "AnalysisUserCount", "Number of analysis users" },
                    { "AnalysisProteinCount", "Number of analysis proteins" },
                    { "AnalysisInteractionCount", "Number of analysis interactions" },
                    { "AnalysisDatabaseCount", "Number of analysis databases" },
                    { "AnalysisProteinCollectionCount", "Number of analysis protein collections" },
                    { "ControlPathCount", "Number of control paths" }
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
            var query = _context.Analyses
                .Where(item => true);
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Name") && item.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Description") && item.Description.Contains(input.SearchString) ||
                    input.SearchIn.Contains("DatabaseId") && item.AnalysisDatabases.Any(item1 => item1.Database.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("DatabaseName") && item.AnalysisDatabases.Any(item1 => item1.Database.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("NetworkId") && item.Network.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("NetworkName") && item.Network.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("ProteinCollectionId") && item.AnalysisProteinCollections.Any(item1 => item1.ProteinCollection.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("ProteinCollectionName") && item.AnalysisProteinCollections.Any(item1 => item1.ProteinCollection.Name.Contains(input.SearchString)));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("IsError") ? item.Status == AnalysisStatus.Error : true)
                .Where(item => input.Filter.Contains("IsNotError") ? item.Status != AnalysisStatus.Error : true)
                .Where(item => input.Filter.Contains("IsDefined") ? item.Status == AnalysisStatus.Defined : true)
                .Where(item => input.Filter.Contains("IsNotDefined") ? item.Status != AnalysisStatus.Defined : true)
                .Where(item => input.Filter.Contains("IsGenerating") ? item.Status == AnalysisStatus.Generating : true)
                .Where(item => input.Filter.Contains("IsNotGenerating") ? item.Status != AnalysisStatus.Generating : true)
                .Where(item => input.Filter.Contains("IsScheduled") ? item.Status == AnalysisStatus.Scheduled : true)
                .Where(item => input.Filter.Contains("IsNotScheduled") ? item.Status != AnalysisStatus.Scheduled : true)
                .Where(item => input.Filter.Contains("IsInitializing") ? item.Status == AnalysisStatus.Initializing : true)
                .Where(item => input.Filter.Contains("IsNotInitializing") ? item.Status != AnalysisStatus.Initializing : true)
                .Where(item => input.Filter.Contains("IsOngoing") ? item.Status == AnalysisStatus.Ongoing : true)
                .Where(item => input.Filter.Contains("IsNotOngoing") ? item.Status != AnalysisStatus.Ongoing : true)
                .Where(item => input.Filter.Contains("IsStopping") ? item.Status == AnalysisStatus.Stopping : true)
                .Where(item => input.Filter.Contains("IsNotStopping") ? item.Status != AnalysisStatus.Stopping : true)
                .Where(item => input.Filter.Contains("IsStopped") ? item.Status == AnalysisStatus.Stopped : true)
                .Where(item => input.Filter.Contains("IsNotStopped") ? item.Status != AnalysisStatus.Stopped : true)
                .Where(item => input.Filter.Contains("IsCompleted") ? item.Status == AnalysisStatus.Completed : true)
                .Where(item => input.Filter.Contains("IsNotCompleted") ? item.Status != AnalysisStatus.Completed : true)
                .Where(item => input.Filter.Contains("UsesGreedyAlgorithm") ? item.Algorithm == AnalysisAlgorithm.Greedy : true)
                .Where(item => input.Filter.Contains("UsesNotGreedyAlgorithm") ? item.Algorithm != AnalysisAlgorithm.Greedy : true)
                .Where(item => input.Filter.Contains("UsesGeneticAlgorithm") ? item.Algorithm == AnalysisAlgorithm.Genetic : true)
                .Where(item => input.Filter.Contains("UsesNotGeneticAlgorithm") ? item.Algorithm != AnalysisAlgorithm.Genetic : true)
                .Where(item => input.Filter.Contains("HasAnalysisUsers") ? item.AnalysisUsers.Any() : true)
                .Where(item => input.Filter.Contains("HasNoAnalysisUsers") ? !item.AnalysisUsers.Any() : true)
                .Where(item => input.Filter.Contains("HasAnalysisProteins") ? item.AnalysisProteins.Any() : true)
                .Where(item => input.Filter.Contains("HasNoAnalysisProteins") ? !item.AnalysisProteins.Any() : true)
                .Where(item => input.Filter.Contains("HasAnalysisInteractions") ? item.AnalysisInteractions.Any() : true)
                .Where(item => input.Filter.Contains("HasNoAnalysisInteractions") ? !item.AnalysisInteractions.Any() : true)
                .Where(item => input.Filter.Contains("HasAnalysisDatabases") ? item.AnalysisDatabases.Any() : true)
                .Where(item => input.Filter.Contains("HasNoAnalysisDatabases") ? !item.AnalysisDatabases.Any() : true)
                .Where(item => input.Filter.Contains("HasAnalysisProteinCollections") ? item.AnalysisProteinCollections.Any() : true)
                .Where(item => input.Filter.Contains("HasNoAnalysisProteinCollections") ? !item.AnalysisProteinCollections.Any() : true)
                .Where(item => input.Filter.Contains("HasControlPaths") ? item.ControlPaths.Any() : true)
                .Where(item => input.Filter.Contains("HasNoControlPaths") ? !item.ControlPaths.Any() : true);
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
                case var sort when sort == ("AnalysisUserCount", "Ascending"):
                    query = query.OrderBy(item => item.AnalysisUsers.Count());
                    break;
                case var sort when sort == ("AnalysisUserCount", "Descending"):
                    query = query.OrderByDescending(item => item.AnalysisUsers.Count());
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
                case var sort when sort == ("AnalysisDatabaseCount", "Ascending"):
                    query = query.OrderBy(item => item.AnalysisDatabases.Count());
                    break;
                case var sort when sort == ("AnalysisDatabaseCount", "Descending"):
                    query = query.OrderByDescending(item => item.AnalysisDatabases.Count());
                    break;
                case var sort when sort == ("AnalysisProteinCollectionCount", "Ascending"):
                    query = query.OrderBy(item => item.AnalysisProteinCollections.Count());
                    break;
                case var sort when sort == ("AnalysisProteinCollectionCount", "Descending"):
                    query = query.OrderByDescending(item => item.AnalysisProteinCollections.Count());
                    break;
                case var sort when sort == ("ControlPathCount", "Ascending"):
                    query = query.OrderBy(item => item.ControlPaths.Count());
                    break;
                case var sort when sort == ("ControlPathCount", "Descending"):
                    query = query.OrderByDescending(item => item.ControlPaths.Count());
                    break;
                default:
                    break;
            }
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<Analysis>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
