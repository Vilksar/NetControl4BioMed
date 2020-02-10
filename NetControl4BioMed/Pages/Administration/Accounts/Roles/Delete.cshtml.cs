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

namespace NetControl4BioMed.Pages.Administration.Accounts.Roles
{
    [Authorize(Roles = "Administrator")]
    public class DeleteModel : PageModel
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly ApplicationDbContext _context;

        public DeleteModel(RoleManager<Role> roleManager, ApplicationDbContext context)
        {
            _roleManager = roleManager;
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
            public IEnumerable<Role> Items { get; set; }
        }

        public IActionResult OnGet(IEnumerable<string> ids)
        {
            // Check if there aren't any (valid) IDs provided.
            if (ids == null || !ids.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Roles/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Items = _context.Roles.Where(item => ids.Contains(item.Id))
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No items have been found with the provided IDs.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Roles/Index");
            }
            // Check if the administrator role is among the items to be deleted.
            if (View.Items.Any(item => item.Name == "Administrator"))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The \"Administrator\" role can't be deleted.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Roles/Index");
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
                return RedirectToPage("/Administration/Accounts/Roles/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Items = _context.Roles.Where(item => Input.Ids.Contains(item.Id))
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No items have been found with the provided IDs.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Roles/Index");
            }
            // Check if the administrator role is among the items to be deleted.
            if (View.Items.Any(item => item.Name == "Administrator"))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The \"Administrator\" role can't be deleted.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Roles/Index");
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
            var roleCount = View.Items.Count();
            // Go over each of the items and try to delete it.
            foreach (var role in View.Items.ToList())
            {
                // Try to delete the item.
                var result = await _roleManager.DeleteAsync(role);
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
            // Display a message.
            TempData["StatusMessage"] = $"Success: {roleCount.ToString()} role{(roleCount != 1 ? "s" : string.Empty)} deleted successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Accounts/Roles/Index");
        }
    }
}
