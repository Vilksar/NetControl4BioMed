using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

namespace NetControl4BioMed.Pages.Content.Created.Analyses.Details.Data.Nodes
{
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
            public Analysis Analysis { get; set; }

            public bool IsGeneric { get; set; }

            public SearchViewModel<ItemModel> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "Name", "Name" },
                    { "Description", "Description" },
                    { "Values", "Values" }
                },
                Filter = new Dictionary<string, string>
                {
                    { "IsNone", "Is of type \"None\"" },
                    { "IsNotNone", "Is not of type \"None\"" },
                    { "IsSource", "Is of type \"Source\"" },
                    { "IsNotSource", "Is not of type \"Source\"" },
                    { "IsTarget", "Is of type \"Target\"" },
                    { "IsNotTarget", "Is not of type \"Target\"" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "Name", "Name" },
                    { "Type", "Type" }
                }
            };
        }

        public class ItemModel
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public AnalysisNodeType Type { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id, string searchString = null, IEnumerable<string> searchIn = null, IEnumerable<string> filter = null, string sortBy = null, string sortDirection = null, int? itemsPerPage = null, int? currentPage = 1)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Get the items with the provided ID.
            var items = _context.Analyses
                .Where(item => item.IsPublic || item.AnalysisUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == id);
            // Check if there were no items found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Define the search input.
            var input = new SearchInputViewModel(ViewModel.SearchOptions, id, searchString, searchIn, filter, sortBy, sortDirection, itemsPerPage, currentPage);
            // Check if any of the provided variables was null before the reassignment.
            if (input.NeedsRedirect)
            {
                // Redirect to the page where they are all explicitly defined.
                return RedirectToPage(new { id = input.Id, searchString = input.SearchString, searchIn = input.SearchIn, filter = input.Filter, sortBy = input.SortBy, sortDirection = input.SortDirection, itemsPerPage = input.ItemsPerPage, currentPage = input.CurrentPage });
            }
            // Start with all of the items.
            var query = items
                .Select(item => item.AnalysisNodes)
                .SelectMany(item => item);
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Node.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Name") && item.Node.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Description") && item.Node.Description.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Values") && item.Node.DatabaseNodeFieldNodes.Where(item1 => item1.DatabaseNodeField.Database.IsPublic || item1.DatabaseNodeField.Database.DatabaseUsers.Any(item2 => item2.User == user)).Any(item1 => item1.DatabaseNodeField.IsSearchable && item1.Value.Contains(input.SearchString)));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("IsNone") ? item.Type == AnalysisNodeType.None : true)
                .Where(item => input.Filter.Contains("IsNotNone") ? item.Type != AnalysisNodeType.None : true)
                .Where(item => input.Filter.Contains("IsSource") ? item.Type == AnalysisNodeType.Source : true)
                .Where(item => input.Filter.Contains("IsNotSource") ? item.Type != AnalysisNodeType.Source : true)
                .Where(item => input.Filter.Contains("IsTarget") ? item.Type == AnalysisNodeType.Target : true)
                .Where(item => input.Filter.Contains("IsNotTarget") ? item.Type != AnalysisNodeType.Target : true);
            // Sort it according to the parameters.
            switch ((input.SortBy, input.SortDirection))
            {
                case var sort when sort == ("Id", "Ascending"):
                    query = query.OrderBy(item => item.Node.Id);
                    break;
                case var sort when sort == ("Id", "Descending"):
                    query = query.OrderByDescending(item => item.Node.Id);
                    break;
                case var sort when sort == ("Name", "Ascending"):
                    query = query.OrderBy(item => item.Node.Name);
                    break;
                case var sort when sort == ("Name", "Descending"):
                    query = query.OrderByDescending(item => item.Node.Name);
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
            // Include the related entities.
            query = query
                .Include(item => item.Node);
            // Define the view.
            View = new ViewModel
            {
                Analysis = items
                    .First(),
                IsGeneric = items
                    .Select(item => item.AnalysisDatabases)
                    .SelectMany(item => item)
                    .Any(item => item.Database.DatabaseType.Name == "Generic"),
                Search = new SearchViewModel<ItemModel>(_linkGenerator, HttpContext, input, query
                    .Select(item => new ItemModel
                    {
                        Id = item.Node.Id,
                        Name = item.Node.Name,
                        Type = item.Type
                    }))
            };
            // Return the page.
            return Page();
        }
    }
}
