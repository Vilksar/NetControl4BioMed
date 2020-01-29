using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Account.Manage
{
    [Authorize]
    public class EnableAuthenticatorModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly UrlEncoder _urlEncoder;

        public EnableAuthenticatorModel(UserManager<User> userManager, UrlEncoder urlEncoder)
        {
            _userManager = userManager;
            _urlEncoder = urlEncoder;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            public string Code { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public string SharedKey { get; set; }

            public string AuthenticatorUri { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if the user does not exist.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: An error occured while trying to load the user data. If you are already logged in, please log out and try again.";
                // Redirect to the home page.
                return RedirectToPage("/Index");
            }
            // Get the authenticator key of the current user.
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            // Check if the key doesn't exist.
            if (string.IsNullOrEmpty(unformattedKey))
            {
                // Reset it.
                await _userManager.ResetAuthenticatorKeyAsync(user);
                // Read it again.
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }
            // Load the key and URI to be encoded into the QR for the current user.
            View = new ViewModel
            {
                // Format the key into an easier-to-read form, by adding a space after every fourth character.
                SharedKey = Regex.Replace(unformattedKey, ".{4}(?!$)", "$0-"),
                // Generate the URI to be included into the QR code.
                AuthenticatorUri = $"otpauth://totp/{_urlEncoder.Encode("NetControl4BioMed")}:{_urlEncoder.Encode(user.Email)}?secret={unformattedKey}&issuer={_urlEncoder.Encode("NetControl4BioMed")}&digits=6"
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if the user does not exist.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: An error occured while trying to load the user data. If you are already logged in, please log out and try again.";
                // Redirect to the home page.
                return RedirectToPage("/Index");
            }
            // Get the authenticator key of the current user.
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            // Check if the key doesn't exist.
            if (string.IsNullOrEmpty(unformattedKey))
            {
                // Reset it.
                await _userManager.ResetAuthenticatorKeyAsync(user);
                // Read it again.
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }
            // Load the key and URI to be encoded into the QR for the current user.
            View = new ViewModel
            {
                // Format the key into an easier-to-read form, by adding a space after every fourth character.
                SharedKey = Regex.Replace(unformattedKey, ".{4}(?!$)", "$0-"),
                // Generate the URI to be included into the QR code.
                AuthenticatorUri = $"otpauth://totp/{_urlEncoder.Encode("NetControl4BioMed")}:{_urlEncoder.Encode(user.Email)}?secret={unformattedKey}&issuer={_urlEncoder.Encode("NetControl4BioMed")}&digits=6"
            };
            // Check if the provided model is not valid.
            if (!ModelState.IsValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error was encountered. Please check again the input fields.");
                // Return the page.
                return Page();
            }
            // Strip the eventual spaces and hypens from the provided verification code.
            var verificationCode = Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
            // Check if the provided verification code corresponds to the two-factor authentication or not.
            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);
            // Check if the verification code is not valid.
            if (!is2faTokenValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The verification code was invalid.");
                // Return the page.
                return Page();
            }
            // Set the two-factor authentication as enabled.
            await _userManager.SetTwoFactorEnabledAsync(user, true);
            // Check if the user doesn't have any recovery codes.
            if (await _userManager.CountRecoveryCodesAsync(user) == 0)
            {
                // Generate the new recovery codes.
                TempData["RecoveryCodes"] = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            }
            // Display a message.
            TempData["StatusMessage"] = "Success: The authenticator app was verified.";
            // Redirect to the two-factor-authentication" page.
            return RedirectToPage("/Account/Manage/TwoFactorAuthentication");
        }
    }
}
