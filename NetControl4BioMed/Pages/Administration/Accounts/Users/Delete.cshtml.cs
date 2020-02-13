using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Administration.Accounts.Users
{
    [Authorize(Roles = "Administrator")]
    public class DeleteModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public DeleteModel(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public IEnumerable<string> Ids { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public IEnumerable<User> Items { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(IEnumerable<string> ids)
        {
            // Check if there aren't any (valid) IDs provided.
            if (ids == null || !ids.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Users/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Items = _context.Users.Where(item => ids.Contains(item.Id))
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No items have been found with the provided IDs.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Users/Index");
            }
            // Get the current user.
            var currentUser = await _userManager.GetUserAsync(User);
            // Check if the current user is among the users to be deleted.
            if (View.Items.Contains(currentUser))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: You can't delete yourself. Please delete first all other users, then delete your own account from your profile settings.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Users/Index");
            }
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Check if there aren't any IDs provided.
            if (Input.Ids == null || !Input.Ids.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Users/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Items = _context.Users.Where(item => Input.Ids.Contains(item.Id))
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No items have been found with the provided IDs.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Users/Index");
            }
            // Get the current user.
            var currentUser = await _userManager.GetUserAsync(User);
            // Check if the current user is among the users to be deleted.
            if (View.Items.Contains(currentUser))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: You can't delete yourself. Please delete first all other users, then delete your own account from your profile settings.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Users/Index");
            }
            // Check if the provided model isn't valid.
            if (!ModelState.IsValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error has been encountered. Please check again the input fields.");
                // Redisplay the page.
                return Page();
            }
            // Save the number of items found.
            var userCount = View.Items.Count();
            // Go over each of the items and try to delete it.
            foreach (var user in View.Items.ToList())
            {
                // Try to delete the item.
                var result = await _userManager.DeleteAsync(user);
                // Check if the deletion was not successful.
                if (!result.Succeeded)
                {
                    // Go over the errors and add them to the model.
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    // Redisplay the page.
                    return Page();
                }
            }
            // Go over all of the networks and analyses and get the ones without any users.
            var networks = _context.Networks.Where(item => !item.NetworkUsers.Any());
            var analyses = _context.Analyses.Where(item => !item.AnalysisUsers.Any() || item.AnalysisNetworks.Any(item1 => networks.Contains(item1.Network)));
            // Mark the items for deletion.
            _context.Analyses.RemoveRange(analyses);
            _context.Networks.RemoveRange(networks);
            // Save the changes in the database.
            await _context.SaveChangesAsync();
            // Display a message.
            TempData["StatusMessage"] = $"Success: {userCount.ToString()} user{(userCount != 1 ? "s" : string.Empty)}  deleted successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Accounts/Users/Index");
        }
    }
}
