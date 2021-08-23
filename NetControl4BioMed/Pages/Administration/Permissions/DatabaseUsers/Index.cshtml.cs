using Microsoft.AspNetCore.Authorization;
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

namespace NetControl4BioMed.Pages.Administration.Permissions.DatabaseUsers
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
            public SearchViewModel<DatabaseUser> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "DatabaseId", "Database ID" },
                    { "DatabaseName", "Database name" },
                    { "UserId", "User ID" },
                    { "Email", "E-mail" },
                },
                Filter = new Dictionary<string, string>
                {
                    { "IsDatabasePublic", "Database is public" },
                    { "IsNotDatabasePublic", "Database is not public" },
                    { "IsUserRegistered", "User is registered" },
                    { "IsNotUserRegistered", "User is not registered" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "DateTimeCreated", "Date created" },
                    { "DatabaseId", "Database ID" },
                    { "DatabaseName", "Database name" },
                    { "UserId", "User ID" },
                    { "Email", "E-mail" }
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
            // Start with all of the items in the database.
            var query = _context.DatabaseUsers
                .Where(item => true);
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("DatabaseId") && item.Database.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("DatabaseName") && item.Database.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("UserId") && item.User.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Email") && item.Email.Contains(input.SearchString));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("IsDatabasePublic") ? item.Database.IsPublic : true)
                .Where(item => input.Filter.Contains("IsNotDatabasePublic") ? !item.Database.IsPublic : true)
                .Where(item => input.Filter.Contains("IsUserRegistered") ? item.User != null : true)
                .Where(item => input.Filter.Contains("IsNotUserRegistered") ? item.User == null : true);
            // Sort it according to the parameters.
            switch ((input.SortBy, input.SortDirection))
            {
                case var sort when sort == ("DateTimeCreated", "Ascending"):
                    query = query.OrderBy(item => item.DateTimeCreated);
                    break;
                case var sort when sort == ("DateTimeCreated", "Descending"):
                    query = query.OrderByDescending(item => item.DateTimeCreated);
                    break;
                case var sort when sort == ("DatabaseId", "Ascending"):
                    query = query.OrderBy(item => item.Database.Id);
                    break;
                case var sort when sort == ("DatabaseId", "Descending"):
                    query = query.OrderByDescending(item => item.Database.Id);
                    break;
                case var sort when sort == ("DatabaseName", "Ascending"):
                    query = query.OrderBy(item => item.Database.Name);
                    break;
                case var sort when sort == ("DatabaseName", "Descending"):
                    query = query.OrderByDescending(item => item.Database.Name);
                    break;
                case var sort when sort == ("UserId", "Ascending"):
                    query = query.OrderBy(item => item.User.Id);
                    break;
                case var sort when sort == ("UserId", "Descending"):
                    query = query.OrderByDescending(item => item.User.Id);
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
            // Include the related entitites.
            query = query
                .Include(item => item.Database)
                .Include(item => item.User);
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<DatabaseUser>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
