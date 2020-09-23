using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Identity
{
    [AllowAnonymous]
    public class LoginWithTwoFactorAuthenticationModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly LinkGenerator _linkGenerator;

        public LoginWithTwoFactorAuthenticationModel(SignInManager<User> signInManager, LinkGenerator linkGenerator)
        {
            _signInManager = signInManager;
            _linkGenerator = linkGenerator;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at mmost {1} characters long.", MinimumLength = 6)]
            public string TwoFactorCode { get; set; }

            public bool RememberMachine { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public bool RememberMe { get; set; }

            public string ReturnUrl { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null)
        {
            // Define the variables for the view.
            View = new ViewModel
            {
                RememberMe = rememberMe,
                ReturnUrl = returnUrl ?? _linkGenerator.GetPathByPage(HttpContext, "/Index", handler: null, values: null)
            };
            // Ensure that the user has gone through the username and password screen first.
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            // Check if there wasn't any user found.
            if (user == null)
            {
                // Display an error.
                TempData["StatusMessage"] = "Error: You need to log in with your e-mail and password first.";
                // Redirect to the login page.
                return RedirectToPage("/Identity/Login");
            }
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(bool rememberMe, string returnUrl = null)
        {
            // Define the variables for the view.
            View = new ViewModel
            {
                RememberMe = rememberMe,
                ReturnUrl = returnUrl ?? _linkGenerator.GetPathByPage(HttpContext, "/Index", handler: null, values: null)
            };
            // Ensure that the user has gone through the username and password screen first.
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            // Check if there wasn't any user found.
            if (user == null)
            {
                // Display an error.
                TempData["StatusMessage"] = "Error: You need to log in with your e-mail and password first.";
                // Redirect to the login page.
                return RedirectToPage("/Identity/Login");
            }
            // Read and parse the authenticator code.
            var authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
            // Try to log the user in using the code.
            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, View.RememberMe, Input.RememberMachine);
            // Check if the account is locked out.
            if (result.IsLockedOut)
            {
                // Display an error.
                TempData["StatusMessage"] = "Error: This account has been locked out. Please try again later.";
                // Redirect to the login page.
                return RedirectToPage("/Identity/Login");
            }
            // Check if the authenticator code was incorrect.
            if (!result.Succeeded)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The authenticator code was invalid.");
                // Return the page.
                return Page();
            }
            // Redirect to the return URL.
            return LocalRedirect(View.ReturnUrl);
        }
    }
}
