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
            // Get the item.
            var user = _context.Users
                .Where(item => item.Id == id)
                .Include(item => item.UserRoles)
                    .ThenInclude(item => item.Role)
                .Include(item => item.DatabaseUsers)
                    .ThenInclude(item => item.Database)
                .FirstOrDefault();
            // Check if the item has not been found.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Users/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                User = user
            };
            // Return the page.
            return Page();
        }
    }
}
