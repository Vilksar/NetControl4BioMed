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

namespace NetControl4BioMed.Pages.Administration.Relationships.DatabaseNodeFieldNodes
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
            public SearchViewModel<DatabaseNodeFieldNode> Search { get; set; }
        }

        public IActionResult OnGet(string searchString = null, IEnumerable<string> searchIn = null, IEnumerable<string> filter = null, string sortBy = null, string sortDirection = null, int? itemsPerPage = null, int? currentPage = 1)
        {
            // Define the search options.
            var options = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "DatabaseId", "Database ID" },
                    { "DatabaseName", "Database name" },
                    { "DatabaseNodeFieldId", "Database node field ID" },
                    { "DatabaseNodeFieldName", "Database node field name" },
                    { "NodeId", "Node ID" },
                    { "NodeName", "Node name" },
                    { "Value", "Value" }
                },
                Filter = new Dictionary<string, string>
                {
                    { "IsDatabasePublic", "Database is public" },
                    { "IsNotDatabasePublic", "Database is not public" },
                    { "IsDatabaseNodeFieldSearchable", "Database node field is searchable" },
                    { "IsNotDatabaseNodeFieldSearchable", "Database node field is not searchable" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "DatabaseId", "Database ID" },
                    { "DatabaseName", "Database name" },
                    { "DatabaseNodeFieldId", "Database node field ID" },
                    { "DatabaseNodeFieldName", "Database node field name" },
                    { "NodeId", "Node ID" },
                    { "NodeName", "NodeName" },
                    { "Value", "Value" }
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
            var query = _context.DatabaseNodeFieldNodes
                .Where(item => !item.Node.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                .AsQueryable();
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("DatabaseId") && item.DatabaseNodeField.Database.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("DatabaseName") && item.DatabaseNodeField.Database.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("DatabaseNodeFieldId") && item.DatabaseNodeField.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("DatabaseNodeFieldName") && item.DatabaseNodeField.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("NodeId") && item.Node.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("NodeName") && item.Node.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Value") && item.Value.Contains(input.SearchString));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("IsDatabasePublic") ? item.DatabaseNodeField.Database.IsPublic : true)
                .Where(item => input.Filter.Contains("IsNotDatabasePublic") ? !item.DatabaseNodeField.Database.IsPublic : true)
                .Where(item => input.Filter.Contains("IsDatabaseNodeFieldSearchable") ? item.DatabaseNodeField.IsSearchable : true)
                .Where(item => input.Filter.Contains("IsNotDatabaseNodeFieldSearchable") ? !item.DatabaseNodeField.IsSearchable : true);
            // Sort it according to the parameters.
            switch ((input.SortBy, input.SortDirection))
            {
                case var sort when sort == ("DatabaseId", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseNodeField.Database.Id);
                    break;
                case var sort when sort == ("DatabaseId", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseNodeField.Database.Id);
                    break;
                case var sort when sort == ("DatabaseName", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseNodeField.Database.Name);
                    break;
                case var sort when sort == ("DatabaseName", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseNodeField.Database.Name);
                    break;
                case var sort when sort == ("DatabaseNodeFieldId", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseNodeField.Id);
                    break;
                case var sort when sort == ("DatabaseNodeFieldId", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseNodeField.Id);
                    break;
                case var sort when sort == ("DatabaseNodeFieldName", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseNodeField.Name);
                    break;
                case var sort when sort == ("DatabaseNodeFieldName", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseNodeField.Name);
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
                case var sort when sort == ("Value", "Ascending"):
                    query = query.OrderBy(item => item.Value);
                    break;
                case var sort when sort == ("Value", "Descending"):
                    query = query.OrderByDescending(item => item.Value);
                    break;
                default:
                    break;
            }
            // Include the related entitites.
            query = query
                .Include(item => item.Node)
                .Include(item => item.DatabaseNodeField)
                    .ThenInclude(item => item.Database);
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<DatabaseNodeFieldNode>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
