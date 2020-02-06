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

namespace NetControl4BioMed.Pages.Administration.Accounts.UserRoles
{
    [Authorize(Roles = "Administrator")]
    public class CreateModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;

        public CreateModel(UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string UserEmail { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string RoleId { get; set; }
        }

        public IActionResult OnGet(string userEmail = null, string roleId = null)
        {
            // Define the input.
            Input = new InputModel
            {
                UserEmail = userEmail,
                RoleId = roleId
            };
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
            // Get the user based on the provided string.
            var user = _context.Users.FirstOrDefault(item => item.Email == Input.UserEmail);
            // Check if there was no user found.
            if (user == null)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "No user could be found matching the provided string.");
                // Redisplay the page.
                return Page();
            }
            // Get the role based on the provided string.
            var role = _context.Roles.FirstOrDefault(item => item.Id == Input.RoleId);
            // Check if there was no role found.
            if (role == null)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "No role could be found matching the provided string.");
                // Redisplay the page.
                return Page();
            }
            // Try to add the user to the role.
            var result = await _userManager.AddToRoleAsync(user, role.Name);
            // Check if the operations has failed.
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
            // Check if the found user is the current one.
            if (await _userManager.GetUserAsync(User) == user)
            {
                // Log out the user.
                await _signInManager.SignOutAsync();
                // Display a message.
                TempData["StatusMessage"] = $"Info:1 user role created successfully. The roles assigned to your account have changed, so you have been signed out.";
                // Redirect to the index page.
                return RedirectToPage("/Index");
            }
            // Display a message.
            TempData["StatusMessage"] = "Success: 1 user role created successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Accounts/UserRoles/Index");
        }
    }
}
