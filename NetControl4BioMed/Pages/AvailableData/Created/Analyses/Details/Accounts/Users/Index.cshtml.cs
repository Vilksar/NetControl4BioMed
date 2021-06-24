using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.AvailableData.Created.Analyses.Details.Accounts.Users
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
            public bool IsUserOwner { get; set; }

            public AnalysisUser CurrentAnalysisUser { get; set; }

            public Analysis Analysis { get; set; }

            public SearchViewModel<AnalysisUser> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "Email", "E-mail" }
                },
                Filter = new Dictionary<string, string>
                {
                    { "IsNone", "Is of type \"None\"" },
                    { "IsNotNone", "Is not of type \"None\"" },
                    { "IsOwner", "Is of type \"Owner\"" },
                    { "IsNotOwner", "Is not of type \"Owner\"" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "DateTimeCreated", "Date created" },
                    { "Email", "E-mail" },
                    { "Type", "Type" }
                }
            };
        }

        public async Task<IActionResult> OnGetAsync(string id, string searchString = null, IEnumerable<string> searchIn = null, IEnumerable<string> filter = null, string sortBy = null, string sortDirection = null, int? itemsPerPage = null, int? currentPage = 1)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/AvailableData/Created/Analyses/Index");
            }
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Get the items with the provided ID.
            var items = _context.Analyses
                .Where(item => item.IsPublic || (user != null && item.AnalysisUsers.Any(item1 => item1.Email == user.Email)))
                .Where(item => item.Id == id);
            // Check if there were no items found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/AvailableData/Created/Analyses/Index");
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
                .Select(item => item.AnalysisUsers)
                .SelectMany(item => item);
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Email") && item.Email.Contains(input.SearchString));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("IsNone") ? item.Type == AnalysisUserType.None : true)
                .Where(item => input.Filter.Contains("IsNotNone") ? item.Type != AnalysisUserType.None : true)
                .Where(item => input.Filter.Contains("IsOwner") ? item.Type == AnalysisUserType.Owner : true)
                .Where(item => input.Filter.Contains("IsNotOwner") ? item.Type != AnalysisUserType.Owner : true);
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
                case var sort when sort == ("Type", "Ascending"):
                    query = query.OrderBy(item => item.Type);
                    break;
                case var sort when sort == ("Type", "Descending"):
                    query = query.OrderByDescending(item => item.Type);
                    break;
                default:
                    break;
            }
            // Define the view.
            View = new ViewModel
            {
                IsUserOwner = items
                    .Any(item => user != null && item.AnalysisUsers.Any(item1 => item1.Type == AnalysisUserType.Owner && item1.Email == user.Email)),
                CurrentAnalysisUser = items
                    .Select(item => item.AnalysisUsers)
                    .SelectMany(item => item)
                    .FirstOrDefault(item => user != null && item.Type == AnalysisUserType.None && item.Email == user.Email),
                Analysis = items
                    .Include(item => item.AnalysisUsers)
                    .First(),
                Search = new SearchViewModel<AnalysisUser>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
