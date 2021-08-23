using Microsoft.AspNetCore.Identity;
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
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.AvailableData.Data.Proteins
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
                    { "Description", "Description" }
                },
                Filter = new Dictionary<string, string>
                {
                    { "HasInteractionProteins", "Has interactions" },
                    { "HasNoInteractionProteins", "Does not have interactions" },
                    { "HasProteinCollectionProteins", "Is contained by protein collections" },
                    { "HasNoProteinCollectionProteins", "Is not contained by protein collections" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "DateTimeCreated", "Date created" },
                    { "Name", "Name" },
                    { "DatabaseProteinCount", "Number of databases" },
                    { "DatabaseProteinFieldProteinCount", "Number of values" },
                    { "InteractionProteinCount", "Number of interactions" },
                    { "ProteinCollectionProteinCount", "Number of protein collections" }
                }
            };
        }

        public class ItemModel
        {
            public string Id { get; set; }

            public string Name { get; set; }
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
            // Start with all of the items to which the user has access.
            var query = _context.Proteins
                .Where(item => item.DatabaseProteins.Any())
                .Where(item => item.DatabaseProteins.Any(item1 => item1.Database.IsPublic || (user != null && item1.Database.DatabaseUsers.Any(item2 => item2.Email == user.Email))));
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Name") && item.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Description") && item.Description.Contains(input.SearchString));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("HasInteractionProteins") ? item.InteractionProteins.Any() : true)
                .Where(item => input.Filter.Contains("HasNoInteractionProteins") ? !item.InteractionProteins.Any() : true)
                .Where(item => input.Filter.Contains("HasProteinCollectionProteins") ? item.ProteinCollectionProteins.Any() : true)
                .Where(item => input.Filter.Contains("HasNoProteinCollectionProteins") ? !item.ProteinCollectionProteins.Any() : true);
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
                case var sort when sort == ("DatabaseProteinCount", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseProteins.Count());
                    break;
                case var sort when sort == ("DatabaseProteinCount", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseProteins.Count());
                    break;
                case var sort when sort == ("DatabaseProteinFieldProteinCount", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseProteinFieldProteins.Count());
                    break;
                case var sort when sort == ("DatabaseProteinFieldProteinCount", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseProteinFieldProteins.Count());
                    break;
                case var sort when sort == ("InteractionProteinCount", "Ascending"):
                    query = query.OrderBy(item => item.InteractionProteins.Count());
                    break;
                case var sort when sort == ("InteractionProteinCount", "Descending"):
                    query = query.OrderByDescending(item => item.InteractionProteins.Count());
                    break;
                case var sort when sort == ("ProteinCollectionProteinCount", "Ascending"):
                    query = query.OrderBy(item => item.ProteinCollectionProteins.Count());
                    break;
                case var sort when sort == ("ProteinCollectionProteinCount", "Descending"):
                    query = query.OrderByDescending(item => item.ProteinCollectionProteins.Count());
                    break;
                default:
                    break;
            }
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<ItemModel>(_linkGenerator, HttpContext, input, query.Select(item => new ItemModel
                {
                    Id = item.Id,
                    Name = item.Name
                }))
            };
            // Return the page.
            return Page();
        }
    }
}
