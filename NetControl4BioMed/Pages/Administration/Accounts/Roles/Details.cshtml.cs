using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace NetControl4BioMed.Pages.Administration.Accounts.Roles
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
            public Role Role { get; set; }

            public int UserCount { get; set; }
        }

        public IActionResult OnGet(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Roles/Index");
            }
            // Define the query.
            var query = _context.Roles
                .Where(item => item.Id == id);
            // Define the view.
            View = new ViewModel
            {
                Role = query
                    .FirstOrDefault(),
                UserCount = query
                    .Select(item => item.UserRoles)
                    .SelectMany(item => item)
                    .Select(item => item.User)
                    .Distinct()
                    .Count()
            };
            // Check if there was no item found.
            if (View.Role == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Roles/Index");
            }
            // Return the page.
            return Page();
        }
    }
}
