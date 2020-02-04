using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    public class CreateModel : PageModel
    {
        private readonly RoleManager<Role> _roleManager;

        public CreateModel(RoleManager<Role> roleManager)
        {
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string Name { get; set; }
        }

        public IActionResult OnGet()
        {
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Check if the provided model isn't valid.
            if (!ModelState.IsValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error has been encountered. Please check again the input fields.");
                // Redisplay the page.
                return Page();
            }
            // Define the new role.
            var role = new Role
            {
                Name = Input.Name,
                DateTimeCreated = DateTime.Now
            };
            // Try to create the new role.
            var result = await _roleManager.CreateAsync(role);
            // Check if any of the operations has failed.
            if (!result.Succeeded)
            {
                // Go over each of the encountered errors.
                foreach (var error in result.Errors)
                {
                    // Add the error to the model.
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                // Redisplay the page.
                return Page();
            }
            // Display a message.
            TempData["StatusMessage"] = "Success: 1 role created successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Accounts/Roles/Index");
        }
    }
}
