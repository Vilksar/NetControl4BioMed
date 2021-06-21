using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetControl4BioMed.Pages.Administration.Databases.Databases
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
            public SearchViewModel<Database> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "Name", "Name" },
                    { "Description", "Description" },
                    { "Url", "URL" }
                },
                Filter = new Dictionary<string, string>
                {
                    { "IsPublic", "Is public" },
                    { "IsNotPublic", "Is not public" },
                    { "HasDatabaseUsers", "Has database users" },
                    { "HasNoDatabaseUsers", "Does not have database users" },
                    { "HasDatabasesUserInvitations", "Has database user invitations" },
                    { "HasNoDatabaseUserInvitations", "Does not have database user invitations" },
                    { "HasDatabaseProteinFields", "Has database protein fields" },
                    { "HasNoDatabaseProteinFields", "Does not have database protein fields" },
                    { "HasDatabaseInteractionFields", "Has database interaction fields" },
                    { "HasNoDatabaseInteractionFields", "Does not have database interaction fields" },
                    { "HasDatabaseProteins", "Has database proteins" },
                    { "HasNoDatabaseProteins", "Does not have database proteins" },
                    { "HasDatabaseInteractions", "Has database interactions" },
                    { "HasNoDatabaseInteractions", "Does not have database interactions" },
                    { "HasProteinCollectionDatabases", "Has protein collection databases" },
                    { "HasNoProteinCollectionDatabases", "Does not have protein collection databases" },
                    { "HasNetworkDatabases", "Has network databases" },
                    { "HasNoNetworkDatabases", "Does not have network databases" },
                    { "HasAnalysisDatabases", "Has analysis databases" },
                    { "HasNoAnalysisDatabases", "Does not have analysis databases" },
                },
                SortBy = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "DateTimeCreated", "Date created" },
                    { "Name", "Name" },
                    { "DatabaseUserCount", "Number of database users" },
                    { "DatabaseProteinFieldCount", "Number of database protein fields" },
                    { "DatabaseInteractionFieldCount", "Number of database interaction fields" },
                    { "DatabaseProteinCount", "Number of database proteins" },
                    { "DatabaseInteractionCount", "Number of database interactions" },
                    { "NetworkDatabaseCount", "Number of network databases" },
                    { "AnalysisDatabaseCount", "Number of analysis databases" }
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
            var query = _context.Databases
                .Where(item => true);
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Name") && item.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Description") && item.Description.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Url") && item.Url.Contains(input.SearchString));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("IsPublic") ? item.IsPublic : true)
                .Where(item => input.Filter.Contains("IsNotPublic") ? !item.IsPublic : true)
                .Where(item => input.Filter.Contains("HasDatabaseUsers") ? item.DatabaseUsers.Any() : true)
                .Where(item => input.Filter.Contains("HasNoDatabaseUsers") ? !item.DatabaseUsers.Any() : true)
                .Where(item => input.Filter.Contains("HasDatabaseProteinFields") ? item.DatabaseProteinFields.Any() : true)
                .Where(item => input.Filter.Contains("HasNoDatabaseProteinFields") ? !item.DatabaseProteinFields.Any() : true)
                .Where(item => input.Filter.Contains("HasDatabaseInteractionFields") ? item.DatabaseInteractionFields.Any() : true)
                .Where(item => input.Filter.Contains("HasNoDatabaseInteractionFields") ? !item.DatabaseInteractionFields.Any() : true)
                .Where(item => input.Filter.Contains("HasDatabaseProteins") ? item.DatabaseProteins.Any() : true)
                .Where(item => input.Filter.Contains("HasNoDatabaseProteins") ? !item.DatabaseProteins.Any() : true)
                .Where(item => input.Filter.Contains("HasDatabaseInteractions") ? item.DatabaseInteractions.Any() : true)
                .Where(item => input.Filter.Contains("HasNoDatabaseInteractions") ? !item.DatabaseInteractions.Any() : true)
                .Where(item => input.Filter.Contains("HasNetworkDatabases") ? !item.NetworkDatabases.Any() : true)
                .Where(item => input.Filter.Contains("HasNoNetworkDatabases") ? !item.NetworkDatabases.Any() : true)
                .Where(item => input.Filter.Contains("HasAnalysisDatabases") ? !item.AnalysisDatabases.Any() : true)
                .Where(item => input.Filter.Contains("HasNoAnalysisDatabases") ? !item.AnalysisDatabases.Any() : true);
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
                case var sort when sort == ("DatabaseUserCount", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseUsers.Count());
                    break;
                case var sort when sort == ("DatabaseUserCount", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseUsers.Count());
                    break;
                case var sort when sort == ("DatabaseProteinFieldCount", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseProteinFields.Count());
                    break;
                case var sort when sort == ("DatabaseProteinFieldCount", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseProteinFields.Count());
                    break;
                case var sort when sort == ("DatabaseInteractionFieldCount", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseInteractionFields.Count());
                    break;
                case var sort when sort == ("DatabaseInteractionFieldCount", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseInteractionFields.Count());
                    break;
                case var sort when sort == ("DatabaseProteinCount", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseProteins.Count());
                    break;
                case var sort when sort == ("DatabaseProteinCount", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseProteins.Count());
                    break;
                case var sort when sort == ("DatabaseInteractionCount", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseInteractions.Count());
                    break;
                case var sort when sort == ("DatabaseInteractionCount", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseInteractions.Count());
                    break;
                case var sort when sort == ("NetworkDatabaseCount", "Ascending"):
                    query = query.OrderBy(item => item.NetworkDatabases.Count());
                    break;
                case var sort when sort == ("NetworkDatabaseCount", "Descending"):
                    query = query.OrderByDescending(item => item.NetworkDatabases.Count());
                    break;
                case var sort when sort == ("AnalysisDatabaseCount", "Ascending"):
                    query = query.OrderBy(item => item.AnalysisDatabases.Count());
                    break;
                case var sort when sort == ("AnalysisDatabaseCount", "Descending"):
                    query = query.OrderByDescending(item => item.AnalysisDatabases.Count());
                    break;
                default:
                    break;
            }
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<Database>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
