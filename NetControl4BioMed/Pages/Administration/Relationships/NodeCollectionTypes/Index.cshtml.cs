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
using EnumerationNodeCollectionType = NetControl4BioMed.Data.Enumerations.NodeCollectionType;

namespace NetControl4BioMed.Pages.Administration.Relationships.NodeCollectionTypes
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
            public SearchViewModel<NodeCollectionType> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "NodeCollectionId", "Node collection ID" },
                    { "NodeCollectionName", "Node collection name" }
                },
                Filter = new Dictionary<string, string>
                {
                    { "ContainsSeedNodes", "Contains seed nodes" },
                    { "ContainsNotSeedNodes", "Does not contain seed nodes" },
                    { "ContainsSourceNodes", "Contains source nodes" },
                    { "ContainsNotSourceNodes", "Does not contain source nodes" },
                    { "ContainsTargetNodes", "Contains target nodes" },
                    { "ContainsNotTargetNodes", "Does not contain target nodes" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "NodeCollectionId", "Node collection ID" },
                    { "NodeCollectionName", "Node collection name" },
                    { "Type", "Type" }
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
            var query = _context.NodeCollectionTypes
                .Where(item => !item.NodeCollection.NodeCollectionDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"));
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("NodeCollectionId") && item.NodeCollection.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("NodeCollectionName") && item.NodeCollection.Name.Contains(input.SearchString));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("ContainsSeedNodes") ? item.Type == EnumerationNodeCollectionType.Seed : true)
                .Where(item => input.Filter.Contains("ContainsNotSeedNodes") ? item.Type != EnumerationNodeCollectionType.Seed : true)
                .Where(item => input.Filter.Contains("ContainsIsSourceNodes") ? item.Type == EnumerationNodeCollectionType.Source : true)
                .Where(item => input.Filter.Contains("ContainsIsNotSourceNodes") ? item.Type != EnumerationNodeCollectionType.Source : true)
                .Where(item => input.Filter.Contains("ContainsIsTargetNodes") ? item.Type == EnumerationNodeCollectionType.Target : true)
                .Where(item => input.Filter.Contains("ContainsNotTargetNodes") ? item.Type != EnumerationNodeCollectionType.Target : true);
            // Sort it according to the parameters.
            switch ((input.SortBy, input.SortDirection))
            {
                case var sort when sort == ("NodeCollectionId", "Ascending"):
                    query = query.OrderBy(item => item.NodeCollection.Id);
                    break;
                case var sort when sort == ("NodeCollectionId", "Descending"):
                    query = query.OrderByDescending(item => item.NodeCollection.Id);
                    break;
                case var sort when sort == ("NodeCollectionName", "Ascending"):
                    query = query.OrderBy(item => item.NodeCollection.Name);
                    break;
                case var sort when sort == ("NodeCollectionName", "Descending"):
                    query = query.OrderByDescending(item => item.NodeCollection.Name);
                    break;
                case var sort when sort == ("Type", "Ascending"):
                    query = query.OrderBy(item => item.Type);
                    break;
                case var sort when sort == ("Type", "Descending"):
                    query = query.OrderByDescending(item => item.Type);
                    break;
                default:
                    break;
            }
            // Include the related entitites.
            query = query
                .Include(item => item.NodeCollection);
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<NodeCollectionType>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
