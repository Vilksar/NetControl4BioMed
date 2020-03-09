using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Content.Databases.DatabaseNodeFields
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public DetailsModel(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public DatabaseNodeField DatabaseNodeField { get; set; }

            public IEnumerable<DatabaseNodeFieldNode> DatabaseNodeFieldNodes { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if the user does not exist.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: An error occured while trying to load the user data. If you are already logged in, please log out and try again.";
                // Redirect to the home page.
                return RedirectToPage("/Index");
            }
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Databases/DatabaseNodeFields/Index");
            }
            // Get the item with the provided ID.
            var item = _context.DatabaseNodeFields
                .Where(item => item.Database.DatabaseType.Name != "Generic")
                .Where(item => item.Database.IsPublic || item.Database.DatabaseUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == id)
                .Include(item => item.Database)
                    .ThenInclude(item => item.DatabaseType)
                .Include(item => item.DatabaseNodeFieldNodes)
                .FirstOrDefault();
            // Check if there was no item found.
            if (item == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Databases/DatabaseNodeFields/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                DatabaseNodeField = item,
                DatabaseNodeFieldNodes = item.DatabaseNodeFieldNodes
            };
            // Return the page.
            return Page();
        }
    }
}