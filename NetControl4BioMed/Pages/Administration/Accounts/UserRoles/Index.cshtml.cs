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

namespace NetControl4BioMed.Pages.Administration.Accounts.UserRoles
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
            public SearchViewModel<UserRole> Search { get; set; }
        }

        public IActionResult OnGet(string searchString = null, IEnumerable<string> searchIn = null, IEnumerable<string> filter = null, string sortBy = null, string sortDirection = null, int? itemsPerPage = null, int? currentPage = 1)
        {
            // Define the search options.
            var options = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "UserId", "User ID" },
                    { "UserEmail", "User e-mail" },
                    { "RoleId", "Role ID" },
                    { "RoleName", "Role name" }
                },
                Filter = new Dictionary<string, string>
                {
                },
                SortBy = new Dictionary<string, string>
                {
                    { "UserId", "User ID" },
                    { "UserEmail", "User e-mail" },
                    { "RoleId", "Role ID" },
                    { "RoleName", "Role name" }
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
            // Start with all of the items in the database.
            var query = _context.UserRoles
                .AsQueryable();
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("UserId") && item.User.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("UserEmail") && item.User.Email.Contains(input.SearchString) ||
                    input.SearchIn.Contains("RoleId") && item.Role.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("RoleName") && item.Role.Name.Contains(input.SearchString));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => true);
            // Sort it according to the parameters.
            switch ((input.SortBy, input.SortDirection))
            {
                case var sort when sort == ("UserId", "Ascending"):
                    query = query.OrderBy(item => item.User.Id);
                    break;
                case var sort when sort == ("UserId", "Descending"):
                    query = query.OrderByDescending(item => item.User.Id);
                    break;
                case var sort when sort == ("UserEmail", "Ascending"):
                    query = query.OrderBy(item => item.User.Email);
                    break;
                case var sort when sort == ("UserEmail", "Descending"):
                    query = query.OrderByDescending(item => item.User.Email);
                    break;
                case var sort when sort == ("RoleId", "Ascending"):
                    query = query.OrderBy(item => item.Role.Id);
                    break;
                case var sort when sort == ("RoleId", "Descending"):
                    query = query.OrderByDescending(item => item.Role.Id);
                    break;
                case var sort when sort == ("RoleName", "Ascending"):
                    query = query.OrderBy(item => item.Role.Name);
                    break;
                case var sort when sort == ("RoleName", "Descending"):
                    query = query.OrderByDescending(item => item.Role.Name);
                    break;
                default:
                    break;
            }
            // Include the related entitites.
            query = query
                .Include(item => item.User)
                .Include(item => item.Role);
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<UserRole>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
