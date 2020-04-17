using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Administration.Accounts.Users
{
    [Authorize(Roles = "Administrator")]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public User User { get; set; }

            public int RoleCount { get; set; }

            public int DatabaseCount { get; set; }
        }

        public IActionResult OnGet(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Users/Index");
            }
            // Define the query.
            var query = _context.Users
                .Where(item => item.Id == id);
            // Define the view.
            View = new ViewModel
            {
                User = query
                    .FirstOrDefault(),
                RoleCount = query
                    .Select(item => item.UserRoles)
                    .SelectMany(item => item)
                    .Select(item => item.Role)
                    .Distinct()
                    .Count(),
                DatabaseCount = query
                    .Select(item => item.DatabaseUsers)
                    .SelectMany(item => item)
                    .Select(item => item.Database)
                    .Distinct()
                    .Count()
            };
            // Check if there was no item found.
            if (View.User == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Users/Index");
            }
            // Return the page.
            return Page();
        }
    }
}
