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

namespace NetControl4BioMed.Pages.Administration.Accounts.DatabaseUsers
{
    [Authorize(Roles = "Administrator")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public IEnumerable<string> UserEmails { get; set; }

            public IEnumerable<string> DatabaseIds { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public IEnumerable<DatabaseUser> Items { get; set; }
        }
        public IActionResult OnGet(IEnumerable<string> userEmails, IEnumerable<string> databaseIds)
        {
            // Check if there aren't any e-mails or IDs provided.
            if (userEmails == null || databaseIds == null || !userEmails.Any() || !databaseIds.Any() || userEmails.Count() != databaseIds.Count())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid e-mails or IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/DatabaseUsers/Index");
            }
            // Get the IDs of all selected users and databases.
            var ids = userEmails.Zip(databaseIds);
            // Define the view.
            View = new ViewModel
            {
                Items = _context.DatabaseUsers
                    .Where(item => userEmails.Contains(item.User.Email) && databaseIds.Contains(item.Database.Id))
                    .Include(item => item.User)
                    .Include(item => item.Database)
                    .AsEnumerable()
                    .Where(item => ids.Contains((item.User.Email, item.Database.Id)))
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No items have been found with the provided IDs.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/DatabaseUsers/Index");
            }
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Check if the provided model is not valid.
            if (!ModelState.IsValid)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid e-mails or IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/DatabaseUsers/Index");
            }
            // Check if there aren't any e-mails or IDs provided.
            if (Input.UserEmails == null || Input.DatabaseIds == null || !Input.UserEmails.Any() || !Input.DatabaseIds.Any() || Input.UserEmails.Count() != Input.DatabaseIds.Count())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid e-mails or IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/DatabaseUsers/Index");
            }
            // Get the IDs of all selected users and databases.
            var ids = Input.UserEmails.Zip(Input.DatabaseIds);
            // Define the view.
            View = new ViewModel
            {
                Items = _context.DatabaseUsers
                    .Where(item => Input.UserEmails.Contains(item.User.Email) && Input.DatabaseIds.Contains(item.Database.Id))
                    .Include(item => item.User)
                    .Include(item => item.Database)
                    .AsEnumerable()
                    .Where(item => ids.Contains((item.User.Email, item.Database.Id)))
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No items have been found with the provided IDs.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/DatabaseUsers/Index");
            }
            // Save the number of items found.
            var databaseUserCount = View.Items.Count();
            // Mark them for removal.
            _context.DatabaseUsers.RemoveRange(View.Items);
            // Save the changes.
            await _context.SaveChangesAsync();
            // Display a message.
            TempData["StatusMessage"] = $"Success: {databaseUserCount.ToString()} database user{(databaseUserCount != 1 ? "s" : string.Empty)} deleted successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Accounts/DatabaseUsers/Index");
        }
    }
}
