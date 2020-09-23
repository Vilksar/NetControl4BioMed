using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.ViewModels;

namespace NetControl4BioMed.Pages.Identity
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ISendGridEmailSender _emailSender;
        private readonly LinkGenerator _linkGenerator;
        private readonly IReCaptchaChecker _reCaptchaChecker;

        public LoginModel(UserManager<User> userManager, SignInManager<User> signInManager, ISendGridEmailSender emailSender, LinkGenerator linkGenerator, IReCaptchaChecker reCaptchaChecker)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _linkGenerator = linkGenerator;
            _reCaptchaChecker = reCaptchaChecker;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.EmailAddress)]
            [Required(ErrorMessage = "This field is required.")]
            public string Email { get; set; }

            [DataType(DataType.Password)]
            [Required(ErrorMessage = "This field is required.")]
            public string Password { get; set; }

            public bool RememberMe { get; set; }

            public string ReCaptchaToken { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public IEnumerable<AuthenticationScheme> ExternalLogins { get; set; }

            public string ReturnUrl { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            // Define the variables for the view.
            View = new ViewModel
            {
                ExternalLogins = await _signInManager.GetExternalAuthenticationSchemesAsync(),
                ReturnUrl = returnUrl ?? _linkGenerator.GetPathByPage(HttpContext, "/Index", handler: null, values: null)
            };
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if any user has been found.
            if (user != null)
            {
                // Redirect to the return URL.
                return LocalRedirect(View.ReturnUrl);
            }
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // Define the variables for the view.
            View = new ViewModel
            {
                ExternalLogins = await _signInManager.GetExternalAuthenticationSchemesAsync(),
                ReturnUrl = returnUrl ?? _linkGenerator.GetPathByPage(HttpContext, "/Index", handler: null, values: null)
            };
            // Check if the reCaptcha is valid.
            if (!await _reCaptchaChecker.IsValid(Input.ReCaptchaToken))
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The reCaptcha verification failed.");
                // Return the page.
                return Page();
            }
            // Check if the provided model is not valid.
            if (!ModelState.IsValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error was encountered. Please check again the input fields.");
                // Return the page.
                return Page();
            }
            // Get the user trying to log in.
            var user = await _userManager.FindByEmailAsync(Input.Email);
            // Check if any user has been found.
            if (user == null)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The e-mail and password combination was invalid.");
                // Return the page.
                return Page();
            }
            // Try to log in the user with the provided email and password. This doesn't count login failures towards account lockout. To enable password failures to trigger account lockout, set lockoutOnFailure: true.
            var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, false);
            // Check if the account is locked out.
            if (result.IsLockedOut)
            {
                // Display an error.
                TempData["StatusMessage"] = "Error: This account has been locked out. Please try again later.";
                // Redirect to the home page.
                return RedirectToPage();
            }
            // Check if the acount has two-factors authentication enabled.
            if (result.RequiresTwoFactor)
            {
                // Redirect to the corresponding page.
                return RedirectToPage("/Identity/LoginWithTwoFactorAuthentication", new { returnUrl = View.ReturnUrl, rememberMe = Input.RememberMe });
            }
            // Check if the user is not allowed to sign in because the e-mail is not confirmed.
            if (result.IsNotAllowed && !user.EmailConfirmed)
            {
                // Generate an e-mail confirmation code.
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                // Create the callback URL to be encoded in the confirmation email.
                var callbackUrl = _linkGenerator.GetUriByPage(HttpContext, "/Identity/ConfirmEmail", handler: null, values: new { userId = user.Id, code = code });
                // Define a new view model for the e-mail.
                var emailViewModel = new EmailEmailConfirmationViewModel
                {
                    Email = user.Email,
                    Url = callbackUrl,
                    ApplicationUrl = _linkGenerator.GetUriByPage(HttpContext, "/Index", handler: null, values: null)
                };
                // Send the confirmation e-mail for the user.
                await _emailSender.SendEmailConfirmationEmailAsync(emailViewModel);
                // Display an error.
                TempData["StatusMessage"] = "Error: You are not allowed to log in because your e-mail address is not yet confirmed. A new e-mail containing instructions on how to confirm it has been sent to the specified e-mail address.";
                // Redirect to page.
                return RedirectToPage();
            }
            // Check if the login has failed.
            if (!result.Succeeded)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The e-mail and password combination was invalid.");
                // Return the page.
                return Page();
            }
            // Redirect to the return URL.
            return LocalRedirect(View.ReturnUrl);
        }
    }
}
