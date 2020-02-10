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
using NetControl4BioMed.Helpers.Extensions;

namespace NetControl4BioMed.Pages.Administration.Accounts.UserRoles
{
    [Authorize(Roles = "Administrator")]
    public class DeleteModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;

        public DeleteModel(UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public IEnumerable<string> UserEmails { get; set; }

            public IEnumerable<string> RoleIds { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public IEnumerable<UserRole> Items { get; set; }

            public bool IsCurrentUserSelected { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(IEnumerable<string> userEmails, IEnumerable<string> roleIds)
        {
            // Check if there aren't any e-mails or IDs provided.
            if (userEmails == null || roleIds == null || !userEmails.Any() || !roleIds.Any() || userEmails.Count() != roleIds.Count())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid e-mails or IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/UserRoles/Index");
            }
            // Get the IDs of all selected users and roles.
            var ids = userEmails.Zip(roleIds);
            // Define the view.
            View = new ViewModel
            {
                Items = _context.UserRoles
                    .Where(item => userEmails.Contains(item.User.Email) && roleIds.Contains(item.Role.Id))
                    .Include(item => item.User)
                    .Include(item => item.Role)
                    .AsEnumerable()
                    .Where(item => ids.Contains((item.User.Email, item.Role.Id))),
                IsCurrentUserSelected = userEmails.Contains((await _userManager.GetUserAsync(User)).Email)
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No items have been found with the provided IDs.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/UserRoles/Index");
            }
            // Check if there would be no administrator users after removal.
            if (View.Items.Where(item => item.Role.Name == "Administrator").Count() == (await _userManager.GetUsersInRoleAsync("Administrator")).Count())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No administrator users would remain after deleting the selected user roles.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/UserRoles/Index");
            }
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Check if there aren't any e-mails or IDs provided.
            if (Input.UserEmails == null || Input.RoleIds == null || !Input.UserEmails.Any() || !Input.RoleIds.Any() || Input.UserEmails.Count() != Input.RoleIds.Count())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/UserRoles/Index");
            }
            // Get the IDs of all selected users and roles.
            var ids = Input.UserEmails.Zip(Input.RoleIds);
            // Define the view.
            View = new ViewModel
            {
                Items = _context.UserRoles
                    .Where(item => Input.UserEmails.Contains(item.User.Id) && Input.RoleIds.Contains(item.Role.Id))
                    .Include(item => item.User)
                    .Include(item => item.Role)
                    .AsEnumerable()
                    .Where(item => ids.Contains((item.User.Email, item.Role.Id))),
                IsCurrentUserSelected = Input.UserEmails.Contains((await _userManager.GetUserAsync(User)).Email)
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No items have been found with the provided IDs.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/UserRoles/Index");
            }
            // Check if there would be no administrator users after removal.
            if (View.Items.Where(item => item.Role.Name == "Administrator").Count() == (await _userManager.GetUsersInRoleAsync("Administrator")).Count())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No administrator users would remain after deleting the selected user roles.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/UserRoles/Index");
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
            var userRoleCount = View.Items.Count();
            // Go over each of the items.
            foreach (var userRole in View.Items.ToList())
            {
                // Try to delete the item.
                var result = await _userManager.RemoveFromRoleAsync(userRole.User, userRole.Role.Name);
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
            // Check if the current user is selected.
            if (View.IsCurrentUserSelected)
            {
                // Log out the user.
                await _signInManager.SignOutAsync();
                // Display a message.
                TempData["StatusMessage"] = $"Info: {userRoleCount.ToString()} user role{(userRoleCount != 1 ? "s" : string.Empty)} deleted successfully. The roles assigned to your account have changed, so you have been signed out.";
                // Redirect to the index page.
                return RedirectToPage("/Index");
            }
            // Display a message.
            TempData["StatusMessage"] = $"Success: {userRoleCount.ToString()} user role{(userRoleCount != 1 ? "s" : string.Empty)} deleted successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Accounts/UserRoles/Index");
        }
    }
}
