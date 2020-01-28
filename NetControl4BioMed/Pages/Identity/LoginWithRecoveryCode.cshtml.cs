using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Identity
{
    [AllowAnonymous]
    public class LoginWithRecoveryCodeModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;

        public LoginWithRecoveryCodeModel(SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string RecoveryCode { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public string ReturnUrl { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            // Define the variables for the view.
            View = new ViewModel
            {
                ReturnUrl = returnUrl ?? Url.Content("~/")
            };
            // Ensure that the user has gone through the e-mail and password screen first.
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            // Check if there wasn't any user found.
            if (user == null)
            {
                // Display an error.
                TempData["StatusMessage"] = "Error: You need to log in with your e-mail and password first.";
                // Redirect to the login the page.
                return RedirectToPage("/Identity/Login");
            }
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // Define the variables to return to the view.
            View = new ViewModel
            {
                ReturnUrl = returnUrl ?? Url.Content("~/")
            };
            // Ensure that the user has gone through the username and password screen first.
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            // Check if there wasn't any user found.
            if (user == null)
            {
                // Display an error.
                TempData["StatusMessage"] = "Error: You need to log in with your e-mail and password first.";
                // Redirect to the login the page.
                return RedirectToPage("/Identity/Login");
            }
            // Read and parse the recovery code.
            var recoveryCode = Input.RecoveryCode.Replace(" ", string.Empty).Replace("-", string.Empty);
            // Try to log the user in using the code.
            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);
            // Check if the account is locked out.
            if (result.IsLockedOut)
            {
                // Display an error.
                TempData["StatusMessage"] = "Error: This account has been locked out. Please try again later.";
                // Redirect to the login page.
                return RedirectToPage("/Identity/Login");
            }
            // Check if the recovery code was incorrect.
            if (!result.Succeeded)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The recovery code was invalid.");
                // Return the page.
                return Page();
            }
            // Redirect to the return URL.
            return LocalRedirect(View.ReturnUrl);
        }
    }
}
