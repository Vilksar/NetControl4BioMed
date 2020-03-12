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

namespace NetControl4BioMed.Pages.Administration.Relationships.EdgeNodes
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
            public SearchViewModel<EdgeNode> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "EdgeId", "Edge ID" },
                    { "EdgeName", "Edge name" },
                    { "NodeId", "Node ID" },
                    { "NodeName", "Node name" }
                },
                Filter = new Dictionary<string, string>
                {
                },
                SortBy = new Dictionary<string, string>
                {
                    { "EdgeId", "Edge ID" },
                    { "EdgeName", "Edge name" },
                    { "NodeId", "Node ID" },
                    { "NodeName", "NodeName" }
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
            // Start with all of the items in the non-generic databases.
            var query = _context.EdgeNodes
                .Where(item => !item.Node.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                .Where(item => !item.Edge.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"));
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("EdgeId") && item.Edge.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("EdgeName") && item.Edge.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("NodeId") && item.Node.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("NodeName") && item.Node.Name.Contains(input.SearchString));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => true);
            // Sort it according to the parameters.
            switch ((input.SortBy, input.SortDirection))
            {
                case var sort when sort == ("EdgeId", "Ascending"):
                    query = query.OrderBy(item => item.Edge.Id);
                    break;
                case var sort when sort == ("EdgeId", "Descending"):
                    query = query.OrderByDescending(item => item.Edge.Id);
                    break;
                case var sort when sort == ("EdgeName", "Ascending"):
                    query = query.OrderBy(item => item.Edge.Name);
                    break;
                case var sort when sort == ("EdgeName", "Descending"):
                    query = query.OrderByDescending(item => item.Edge.Name);
                    break;
                case var sort when sort == ("NodeId", "Ascending"):
                    query = query.OrderBy(item => item.Node.Id);
                    break;
                case var sort when sort == ("NodeId", "Descending"):
                    query = query.OrderByDescending(item => item.Node.Id);
                    break;
                case var sort when sort == ("NodeName", "Ascending"):
                    query = query.OrderBy(item => item.Node.Name);
                    break;
                case var sort when sort == ("NodeName", "Descending"):
                    query = query.OrderByDescending(item => item.Node.Name);
                    break;
                default:
                    break;
            }
            // Include the related entitites.
            query = query
                .Include(item => item.Node)
                .Include(item => item.Edge);
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<EdgeNode>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
