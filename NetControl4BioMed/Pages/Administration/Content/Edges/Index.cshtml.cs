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

namespace NetControl4BioMed.Pages.Administration.Content.Edges
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
            public SearchViewModel<Edge> Search { get; set; }
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
                    { "Description", "Description" }
                },
                Filter = new Dictionary<string, string>
                {
                    { "HasDatabaseEdges", "Has database edges" },
                    { "HasNoDatabaseEdges", "Does not have database edges" },
                    { "HasDatabaseEdgeFieldEdges", "Has database edge field edges" },
                    { "HasNoDatabaseEdgeFieldEdges", "Does not have database edge field edges" },
                    { "HasEdgeNodes", "Has node edges" },
                    { "HasNoEdgeNodes", "Does not have node edges" },
                    { "HasNetworkEdges", "Has network edges" },
                    { "HasNoNetworkEdges", "Does not have network edges" },
                    { "HasAnalysisEdges", "Has analysis edges" },
                    { "HasNoAnalysisEdges", "Does not have analysis edges" },
                    { "HasPathEdges", "Has path edges" },
                    { "HasNoPathEdges", "Does not have path edges" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "DateTimeCreated", "Date created" },
                    { "Name", "Name" },
                    { "DatabaseEdgeCount", "Number of database edges" },
                    { "DatabaseEdgeFieldEdgeCount", "Number of database edge field edges" },
                    { "EdgeNodeCount", "Number of edge nodes" },
                    { "NetworkEdgeCount", "Number of network edges" },
                    { "AnalysisEdgeCount", "Number of analysis edges" },
                    { "PathEdgeCount", "Number of path edges" }
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
            // Start with all of the items in the non-generic databases.
            var query = _context.Edges
                .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                .AsQueryable();
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Name") && item.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Description") && item.Description.Contains(input.SearchString));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("HasDatabaseEdges") ? item.DatabaseEdges.Any() : true)
                .Where(item => input.Filter.Contains("HasNoDatabaseEdges") ? !item.DatabaseEdges.Any() : true)
                .Where(item => input.Filter.Contains("HasDatabaseEdgeFieldEdges") ? item.DatabaseEdgeFieldEdges.Any() : true)
                .Where(item => input.Filter.Contains("HasNoDatabaseEdgeFieldEdges") ? !item.DatabaseEdgeFieldEdges.Any() : true)
                .Where(item => input.Filter.Contains("HasEdgeNodes") ? item.EdgeNodes.Any() : true)
                .Where(item => input.Filter.Contains("HasNoEdgeNodes") ? !item.EdgeNodes.Any() : true)
                .Where(item => input.Filter.Contains("HasNetworkEdges") ? item.NetworkEdges.Any() : true)
                .Where(item => input.Filter.Contains("HasNoNetworkEdges") ? !item.NetworkEdges.Any() : true)
                .Where(item => input.Filter.Contains("HasAnalysisEdges") ? item.AnalysisEdges.Any() : true)
                .Where(item => input.Filter.Contains("HasNoAnalysisEdges") ? !item.AnalysisEdges.Any() : true)
                .Where(item => input.Filter.Contains("HasPathEdges") ? item.PathEdges.Any() : true)
                .Where(item => input.Filter.Contains("HasNoPathEdges") ? !item.PathEdges.Any() : true);
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
                case var sort when sort == ("DatabaseEdgeCount", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseEdges.Count());
                    break;
                case var sort when sort == ("DatabaseEdgeCount", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseEdges.Count());
                    break;
                case var sort when sort == ("DatabaseEdgeFieldEdgeCount", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseEdgeFieldEdges.Count());
                    break;
                case var sort when sort == ("DatabaseEdgeFieldEdgeCount", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseEdgeFieldEdges.Count());
                    break;
                case var sort when sort == ("EdgeNodeCount", "Ascending"):
                    query = query.OrderBy(item => item.EdgeNodes.Count());
                    break;
                case var sort when sort == ("EdgeNodeCount", "Descending"):
                    query = query.OrderByDescending(item => item.EdgeNodes.Count());
                    break;
                case var sort when sort == ("NetworkEdgeCount", "Ascending"):
                    query = query.OrderBy(item => item.NetworkEdges.Count());
                    break;
                case var sort when sort == ("NetworkEdgeCount", "Descending"):
                    query = query.OrderByDescending(item => item.NetworkEdges.Count());
                    break;
                case var sort when sort == ("AnalysisEdgeCount", "Ascending"):
                    query = query.OrderBy(item => item.AnalysisEdges.Count());
                    break;
                case var sort when sort == ("AnalysisEdgeCount", "Descending"):
                    query = query.OrderByDescending(item => item.AnalysisEdges.Count());
                    break;
                case var sort when sort == ("PathEdgeCount", "Ascending"):
                    query = query.OrderBy(item => item.PathEdges.Count());
                    break;
                case var sort when sort == ("PathEdgeCount", "Descending"):
                    query = query.OrderByDescending(item => item.PathEdges.Count());
                    break;
                default:
                    break;
            }
            // Include the related entitites.
            query = query
                .Include(item => item.DatabaseEdges)
                .Include(item => item.DatabaseEdgeFieldEdges)
                .Include(item => item.EdgeNodes)
                .Include(item => item.NetworkEdges)
                .Include(item => item.AnalysisEdges)
                .Include(item => item.PathEdges);
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<Edge>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
