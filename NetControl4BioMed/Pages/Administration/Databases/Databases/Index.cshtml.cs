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
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.ViewModels;

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
                    { "Url", "URL" },
                    { "DatabaseTypeId", "Database type ID" },
                    { "DatabaseTypeName", "Database type name" },
                    { "NetworkId", "Network ID" },
                    { "NetworkName", "Network name" },
                    { "AnalysisId", "Analysis ID" },
                    { "AnalysisName", "Analysis name" }
                },
                Filter = new Dictionary<string, string>
                {
                    { "IsPublic", "Is public" },
                    { "IsNotPublic", "Is not public" },
                    { "HasDatabaseUsers", "Has database users" },
                    { "HasNoDatabaseUsers", "Does not have database users" },
                    { "HasDatabasesUserInvitations", "Has database user invitations" },
                    { "HasNoDatabaseUserInvitations", "Does not have database user invitations" },
                    { "HasDatabaseNodeFields", "Has database node fields" },
                    { "HasNoDatabaseNodeFields", "Does not have database node fields" },
                    { "HasDatabaseEdgeFields", "Has database edge fields" },
                    { "HasNoDatabaseEdgeFields", "Does not have database edge fields" },
                    { "HasDatabaseNodes", "Has database nodes" },
                    { "HasNoDatabaseNodes", "Does not have database nodes" },
                    { "HasDatabaseEdges", "Has database edges" },
                    { "HasNoDatabaseEdges", "Does not have database edges" },
                    { "HasNodeCollectionDatabases", "Has node collection databases" },
                    { "HasNoNodeCollectionDatabases", "Does not have node collection databases" },
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
                    { "DatabaseTypeId", "Database type ID" },
                    { "DatabaseTypeName", "Database type name" },
                    { "DatabaseUserCount", "Number of database users" },
                    { "DatabaseUserInvitationCount", "Number of database user invitations" },
                    { "DatabaseNodeFieldCount", "Number of database node fields" },
                    { "DatabaseEdgeFieldCount", "Number of database edge fields" },
                    { "DatabaseNodeCount", "Number of database nodes" },
                    { "DatabaseEdgeCount", "Number of database edges" },
                    { "NodeCollectionDatabaseCount", "Number of node collection databases" },
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
                    input.SearchIn.Contains("Url") && item.Url.Contains(input.SearchString) ||
                    input.SearchIn.Contains("DatabaseTypeId") && item.DatabaseType.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("DatabaseTypeName") && item.DatabaseType.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("NetworkId") && item.NetworkDatabases.Any(item1 => item1.Network.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("NetworkName") && item.NetworkDatabases.Any(item1 => item1.Network.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("AnalysisId") && item.AnalysisDatabases.Any(item1 => item1.Analysis.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("AnalysisName") && item.AnalysisDatabases.Any(item1 => item1.Analysis.Name.Contains(input.SearchString)));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("IsPublic") ? item.IsPublic : true)
                .Where(item => input.Filter.Contains("IsNotPublic") ? !item.IsPublic : true)
                .Where(item => input.Filter.Contains("HasDatabaseUsers") ? item.DatabaseUsers.Any() : true)
                .Where(item => input.Filter.Contains("HasNoDatabaseUsers") ? !item.DatabaseUsers.Any() : true)
                .Where(item => input.Filter.Contains("HasDatabaseUserInvitations") ? item.DatabaseUserInvitations.Any() : true)
                .Where(item => input.Filter.Contains("HasNoDatabaseUserInvitations") ? !item.DatabaseUserInvitations.Any() : true)
                .Where(item => input.Filter.Contains("HasDatabaseNodeFields") ? item.DatabaseNodeFields.Any() : true)
                .Where(item => input.Filter.Contains("HasNoDatabaseNodeFields") ? !item.DatabaseNodeFields.Any() : true)
                .Where(item => input.Filter.Contains("HasDatabaseEdgeFields") ? item.DatabaseEdgeFields.Any() : true)
                .Where(item => input.Filter.Contains("HasNoDatabaseEdgeFields") ? !item.DatabaseEdgeFields.Any() : true)
                .Where(item => input.Filter.Contains("HasDatabaseNodes") ? item.DatabaseNodes.Any() : true)
                .Where(item => input.Filter.Contains("HasNoDatabaseNodes") ? !item.DatabaseNodes.Any() : true)
                .Where(item => input.Filter.Contains("HasDatabaseEdges") ? item.DatabaseEdges.Any() : true)
                .Where(item => input.Filter.Contains("HasNoDatabaseEdges") ? !item.DatabaseEdges.Any() : true)
                .Where(item => input.Filter.Contains("HasNodeCollectionDatabases") ? !item.NodeCollectionDatabases.Any() : true)
                .Where(item => input.Filter.Contains("HasNoNodeCollectionDatabases") ? !item.NodeCollectionDatabases.Any() : true)
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
                case var sort when sort == ("DatabaseTypeId", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseType.Id);
                    break;
                case var sort when sort == ("DatabaseTypeId", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseType.Id);
                    break;
                case var sort when sort == ("DatabaseTypeName", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseType.Name);
                    break;
                case var sort when sort == ("DatabaseTypeName", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseType.Name);
                    break;
                case var sort when sort == ("DatabaseUserCount", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseUsers.Count());
                    break;
                case var sort when sort == ("DatabaseUserCount", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseUsers.Count());
                    break;
                case var sort when sort == ("DatabaseUserInvitationCount", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseUserInvitations.Count());
                    break;
                case var sort when sort == ("DatabaseUserInvitationCount", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseUserInvitations.Count());
                    break;
                case var sort when sort == ("DatabaseNodeFieldCount", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseNodeFields.Count());
                    break;
                case var sort when sort == ("DatabaseNodeFieldCount", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseNodeFields.Count());
                    break;
                case var sort when sort == ("DatabaseEdgeFieldCount", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseEdgeFields.Count());
                    break;
                case var sort when sort == ("DatabaseEdgeFieldCount", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseEdgeFields.Count());
                    break;
                case var sort when sort == ("DatabaseNodeCount", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseNodes.Count());
                    break;
                case var sort when sort == ("DatabaseNodeCount", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseNodes.Count());
                    break;
                case var sort when sort == ("DatabaseEdgeCount", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseEdges.Count());
                    break;
                case var sort when sort == ("DatabaseEdgeCount", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseEdges.Count());
                    break;
                case var sort when sort == ("NodeCollectionDatabaseCount", "Ascending"):
                    query = query.OrderBy(item => item.NodeCollectionDatabases.Count());
                    break;
                case var sort when sort == ("NodeCollectionDatabaseCount", "Descending"):
                    query = query.OrderByDescending(item => item.NodeCollectionDatabases.Count());
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
            // Include the related entitites.
            query = query
                .Include(item => item.DatabaseType)
                .Include(item => item.DatabaseNodeFields)
                .Include(item => item.DatabaseEdgeFields)
                .Include(item => item.DatabaseNodes)
                .Include(item => item.DatabaseEdges);
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
