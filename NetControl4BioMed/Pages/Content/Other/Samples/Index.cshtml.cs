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

namespace NetControl4BioMed.Pages.Content.Other.Samples
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
            public SearchViewModel<ItemModel> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "Name", "Name" },
                    { "Description", "Description" },
                    { "Data", "Data" }
                },
                Filter = new Dictionary<string, string>
                {
                    { "ContainsSeedNodes", "Contains seed nodes" },
                    { "ContainsNotSeedNodes", "Does not contain seed nodes" },
                    { "ContainsSeedEdges", "Contains seed edges" },
                    { "ContainsNotSeedEdges", "Does not contain seed edges" },
                    { "ContainsSourceNodes", "Contains source nodes" },
                    { "ContainsNotSourceNodes", "Does not contain source nodes" },
                    { "ContainsTargetNodes", "Contains target nodes" },
                    { "ContainsNotTargetNodes", "Does not contain target nodes" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "Name", "Name" },
                    { "Description", "Description" },
                    { "Type", "Type" },
                    { "DataLength", "Data length" }
                }
            };
        }

        public class ItemModel
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public SampleType Type { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string searchString = null, IEnumerable<string> searchIn = null, IEnumerable<string> filter = null, string sortBy = null, string sortDirection = null, int? itemsPerPage = null, int? currentPage = 1)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Define the search input.
            var input = new SearchInputViewModel(ViewModel.SearchOptions, null, searchString, searchIn, filter, sortBy, sortDirection, itemsPerPage, currentPage);
            // Check if any of the provided variables was null before the reassignment.
            if (input.NeedsRedirect)
            {
                // Redirect to the page where they are all explicitly defined.
                return RedirectToPage(new { searchString = input.SearchString, searchIn = input.SearchIn, filter = input.Filter, sortBy = input.SortBy, sortDirection = input.SortDirection, itemsPerPage = input.ItemsPerPage, currentPage = input.CurrentPage });
            }
            // Start with all of the items in the database.
            var query = _context.Samples
                .Where(item => true);
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Name") && item.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Description") && item.Description.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Data") && item.Data.Contains(input.SearchString));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("ContainsSeedNodes") ? item.Type == SampleType.SeedNodes : true)
                .Where(item => input.Filter.Contains("ContainsNotSeedNodes") ? item.Type != SampleType.SeedNodes : true)
                .Where(item => input.Filter.Contains("ContainsSeedEdges") ? item.Type == SampleType.SeedEdges : true)
                .Where(item => input.Filter.Contains("ContainsNotSeedEdges") ? item.Type != SampleType.SeedEdges : true)
                .Where(item => input.Filter.Contains("ContainsSourceNodes") ? item.Type == SampleType.SourceNodes : true)
                .Where(item => input.Filter.Contains("ContainsNotSourceNodes") ? item.Type != SampleType.SourceNodes : true)
                .Where(item => input.Filter.Contains("ContainsTargetNodes") ? item.Type == SampleType.TargetNodes : true)
                .Where(item => input.Filter.Contains("ContainsNotTargetNodes") ? item.Type != SampleType.TargetNodes : true);
            // Sort it according to the parameters.
            switch ((input.SortBy, input.SortDirection))
            {
                case var sort when sort == ("Id", "Ascending"):
                    query = query.OrderBy(item => item.Id);
                    break;
                case var sort when sort == ("Id", "Descending"):
                    query = query.OrderByDescending(item => item.Id);
                    break;
                case var sort when sort == ("Name", "Ascending"):
                    query = query.OrderBy(item => item.Name);
                    break;
                case var sort when sort == ("Name", "Descending"):
                    query = query.OrderByDescending(item => item.Name);
                    break;
                case var sort when sort == ("Description", "Ascending"):
                    query = query.OrderBy(item => item.Description);
                    break;
                case var sort when sort == ("Description", "Descending"):
                    query = query.OrderByDescending(item => item.Description);
                    break;
                case var sort when sort == ("Type", "Ascending"):
                    query = query.OrderBy(item => item.Type);
                    break;
                case var sort when sort == ("Type", "Descending"):
                    query = query.OrderByDescending(item => item.Type);
                    break;
                case var sort when sort == ("DataLength", "Ascending"):
                    query = query.OrderBy(item => item.Data.Length);
                    break;
                case var sort when sort == ("DataLength", "Descending"):
                    query = query.OrderByDescending(item => item.Data.Length);
                    break;
                default:
                    break;
            }
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<ItemModel>(_linkGenerator, HttpContext, input, query
                    .Select(item => new ItemModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Type = item.Type
                    }))
            };
            // Return the page.
            return Page();
        }
    }
}
