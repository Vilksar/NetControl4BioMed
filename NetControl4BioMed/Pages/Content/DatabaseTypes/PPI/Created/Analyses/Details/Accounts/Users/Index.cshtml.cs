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
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.ViewModels;

namespace NetControl4BioMed.Pages.Content.DatabaseTypes.PPI.Created.Analyses.Details.Accounts.Users
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
            public bool IsUserAuthenticated { get; set; }

            public Analysis Analysis { get; set; }

            public SearchViewModel<ItemModel> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "Email", "E-mail" }
                },
                Filter = new Dictionary<string, string>
                {
                },
                SortBy = new Dictionary<string, string>
                {
                    { "DateTimeCreated", "Date created" },
                    { "Email", "E-mail" }
                }
            };
        }

        public class ItemModel
        {
            public string Email { get; set; }

            public DateTime DateTimeCreated { get; set; }
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
                return RedirectToPage("/Content/DatabaseTypes/PPI/Created/Analyses/Index");
            }
            // Get the items with the provided ID.
            var items = _context.Analyses
                .Where(item => item.AnalysisDatabases.Any(item1 => item1.Database.DatabaseType.Name == "PPI"))
                .Where(item => item.IsPublic || item.AnalysisUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == id);
            // Check if there were no items found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/PPI/Created/Analyses/Index");
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
            var query1 = items
                .Select(item => item.AnalysisUsers)
                .SelectMany(item => item)
                .Select(item => new ItemModel
                {
                    Email = item.User.Email,
                    DateTimeCreated = item.DateTimeCreated
                })
                .AsEnumerable();
            var query2 = items
                .Select(item => item.AnalysisUserInvitations)
                .SelectMany(item => item)
                .Select(item => new ItemModel
                {
                    Email = item.Email,
                    DateTimeCreated = item.DateTimeCreated
                })
                .AsEnumerable();
            var query = query1
                .Concat(query2)
                .AsQueryable();
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Email") && item.Email.Contains(input.SearchString));
            // Sort it according to the parameters.
            switch ((input.SortBy, input.SortDirection))
            {
                case var sort when sort == ("DateTimeCreated", "Ascending"):
                    query = query.OrderBy(item => item.DateTimeCreated);
                    break;
                case var sort when sort == ("DateTimeCreated", "Descending"):
                    query = query.OrderByDescending(item => item.DateTimeCreated);
                    break;
                case var sort when sort == ("Email", "Ascending"):
                    query = query.OrderBy(item => item.Email);
                    break;
                case var sort when sort == ("Email", "Descending"):
                    query = query.OrderByDescending(item => item.Email);
                    break;
                default:
                    break;
            }
            // Define the view.
            View = new ViewModel
            {
                IsUserAuthenticated = user != null,
                Analysis = items
                    .Include(item => item.AnalysisUsers)
                        .ThenInclude(item => item.User)
                    .Include(item => item.AnalysisUserInvitations)
                    .First(),
                Search = new SearchViewModel<ItemModel>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
