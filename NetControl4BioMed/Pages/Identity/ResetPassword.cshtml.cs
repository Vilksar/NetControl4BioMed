using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.Identity
{
    [AllowAnonymous]
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<User> _userManager;

        public ResetPasswordModel(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.EmailAddress)]
            [Required(ErrorMessage = "This field is required")]
            public string Email { get; set; }

            [DataType(DataType.Password)]
            [Required(ErrorMessage = "This field is required")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Required(ErrorMessage = "This field is required.")]
            [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public string Code { get; set; }
        }

        public IActionResult OnGet(string email = null, string code = null)
        {
            // Check if there is no code provided.
            if (string.IsNullOrEmpty(code))
            {
                // Display an error.
                TempData["StatusMessage"] = "Error: A unique code is needed in order to reset the password.";
                // Redirect to the login page.
                return RedirectToPage("/Identity/Login");
            }
            // Define the variables to return to the view.
            View = new ViewModel
            {
                Code = code
            };
            // Define the input.
            Input = new InputModel
            {
                Email = email
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string code = null)
        {
            // Check if there is no code provided.
            if (string.IsNullOrEmpty(code))
            {
                // Display an error.
                TempData["StatusMessage"] = "Error: A unique code is needed in order to reset the password.";
                // Redirect to the login page.
                return RedirectToPage("/Identity/Login");
            }
            // Define the variables to return to the view.
            View = new ViewModel
            {
                Code = code
            };
            // Check if the provided model is not valid.
            if (!ModelState.IsValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error was encountered. Please check again the input fields.");
                // Return the page.
                return Page();
            }
            // Try to get the user with the provided e-mail.
            var user = await _userManager.FindByEmailAsync(Input.Email);
            // Check if there wasn't any user found.
            if (user == null)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The provided e-mail address is not associated with any user.");
                // Return the page.
                return Page();
            }
            // Try to reset the password.
            var result = await _userManager.ResetPasswordAsync(user, View.Code, Input.Password);
            // Check if the password reset was successful.
            if (!result.Succeeded)
            {
                // Go over the encountered errors
                foreach (var error in result.Errors)
                {
                    // and add them to the model
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                // Return the page.
                return Page();
            }
            // Display a message.
            TempData["StatusMessage"] = "Success: The password was reset successfully.";
            // Redirect to the login page.
            return RedirectToPage("/Identity/Login");
        }
    }
}
