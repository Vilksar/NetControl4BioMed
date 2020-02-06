using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Administration.Accounts.Roles
{
    [Authorize(Roles = "Administrator")]
    public class EditModel : PageModel
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly ApplicationDbContext _context;

        public EditModel(RoleManager<Role> roleManager, ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string Id { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string Name { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public Role Role { get; set; }
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
            // Define the view.
            View = new ViewModel
            {
                Role = _context.Roles
                    .Where(item => item.Id == id)
                    .Include(item => item.UserRoles)
                        .ThenInclude(item => item.User)
                    .FirstOrDefault()
            };
            // Check if the item hasn't been found.
            if (View.Role == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item could be found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Roles/Index");
            }
            // Check if the role is the administrator role.
            if (View.Role.Name == "Administrator")
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The \"Administrator\" role can't be edited.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Roles/Index");
            }
            // Define the input.
            Input = new InputModel
            {
                Id = View.Role.Id,
                Name = View.Role.Name
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Check if the provided model is not valid.
            if (!ModelState.IsValid)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Roles/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Role = _context.Roles
                    .Where(item => item.Id == Input.Id)
                    .Include(item => item.UserRoles)
                        .ThenInclude(item => item.User)
                    .FirstOrDefault()
            };
            // Check if the item hasn't been found.
            if (View.Role == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item could be found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Roles/Index");
            }
            // Check if the role is the administrator role.
            if (View.Role.Name == "Administrator")
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The \"Administrator\" role can't be edited.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Roles/Index");
            }
            // Check if the name is different from the current one.
            if (Input.Name != View.Role.Name)
            {
                // Try to set the new role name.
                var result = await _roleManager.SetRoleNameAsync(View.Role, Input.Name);
                // Try to update the role.
                result = result.Succeeded ? await _roleManager.UpdateAsync(View.Role) : result;
                // Check if the update was not successful.
                if (!result.Succeeded)
                {
                    // Go over the encountered errors
                    foreach (var error in result.Errors)
                    {
                        // and add them to the model
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    // Redisplay the page.
                    return Page();
                }
            }
            // Display a message.
            TempData["StatusMessage"] = "Success: 1 role updated successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Accounts/Roles/Index");
        }
    }
}
