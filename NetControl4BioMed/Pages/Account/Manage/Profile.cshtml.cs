using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.ViewModels;

namespace NetControl4BioMed.Pages.Account.Manage
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ISendGridEmailSender _emailSender;
        private readonly LinkGenerator _linkGenerator;
        private readonly IReCaptchaChecker _reCaptchaChecker;

        public ProfileModel(UserManager<User> userManager, SignInManager<User> signInManager, ISendGridEmailSender emailSender, LinkGenerator linkGenerator, IReCaptchaChecker reCaptchaChecker)
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

            [DataType(DataType.PhoneNumber)]
            public string PhoneNumber { get; set; }

            public string ReCaptchaToken { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public bool IsEmailConfirmed { get; set; }
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
            // Define the variables for the view.
            View = new ViewModel
            {
                IsEmailConfirmed = user.EmailConfirmed
            };
            // Define the input.
            Input = new InputModel
            {
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
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
            // Define the variables to return to the view.
            View = new ViewModel
            {
                IsEmailConfirmed = user.EmailConfirmed
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
            // Store the current user e-mail.
            var oldEmail = user.Email;
            // Store the status message to be displayed to the user.
            var statusMessage = "Success:";
            // Check if the e-mail is different than the current one.
            if (Input.Email != oldEmail)
            {
                // Try to update the username.
                var result = await _userManager.SetUserNameAsync(user, Input.Email);
                // Check if the update was not successful.
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
                // Try to update the e-mail.
                result = await _userManager.SetEmailAsync(user, Input.Email);
                // Check if the update was not successful.
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
                // Check if the update was not successful.
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
                // Define a new view model for the e-mail.
                var emailChangedEmailViewModel = new EmailEmailChangedViewModel
                {
                    OldEmail = oldEmail,
                    NewEmail = user.Email,
                    ApplicationUrl = _linkGenerator.GetUriByPage(HttpContext, "/Index", handler: null, values: null)
                };
                // Send the e-mail changed e-mail to the user.
                await _emailSender.SendEmailChangedEmailAsync(emailChangedEmailViewModel);
                // Generate an e-mail confirmation code.
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                // Create the callback URL to be encoded in the confirmation email.
                var callbackUrl = _linkGenerator.GetUriByPage(HttpContext, "/Identity/ConfirmEmail", handler: null, values: new { userId = user.Id, code = code });
                var encodedUrl = HtmlEncoder.Default.Encode(callbackUrl);
                // Define a new view model for the e-mail.
                var emailConfirmationEmailViewModel = new EmailEmailConfirmationViewModel
                {
                    Email = user.Email,
                    Url = encodedUrl,
                    ApplicationUrl = _linkGenerator.GetUriByPage(HttpContext, "/Index", handler: null, values: null)
                };
                // Send the confirmation e-mail for the user.
                await _emailSender.SendEmailConfirmationEmailAsync(emailConfirmationEmailViewModel);
                // Display a message to the user.
                statusMessage = $"{statusMessage} The e-mail has been successfully updated. A confirmation e-mail was sent to the new address. Please follow the instructions there in order to confirm it. If you log out, you might not be able to log in before you confirm it.";
            }
            // Check if the phone number is different than the current one.
            if (Input.PhoneNumber != user.PhoneNumber)
            {
                // Try to update the phone number.
                var result = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                // Check if the update was not successful.
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
                // Display a message to the user.
                statusMessage = $"{statusMessage} The phone number has been successfully updated.";
            }
            // Re-sign in the user to update the changes.
            await _signInManager.RefreshSignInAsync(user);
            // Display a message.
            TempData["StatusMessage"] = statusMessage == "Success:" ? "Success: All details were already up to date." : statusMessage;
            // Redirect to page.
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
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
            // Define the variables to return to the view.
            View = new ViewModel
            {
                IsEmailConfirmed = user.EmailConfirmed
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
            // Generate an e-mail confirmation code.
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            // Create the callback URL to be encoded in the confirmation email.
            var callbackUrl = _linkGenerator.GetUriByPage(HttpContext, "/Identity/ConfirmEmail", handler: null, values: new { userId = user.Id, code = code });
            var encodedUrl = HtmlEncoder.Default.Encode(callbackUrl);
            // Define a new view model for the e-mail.
            var emailViewModel = new EmailEmailConfirmationViewModel
            {
                Email = user.Email,
                Url = encodedUrl,
                ApplicationUrl = _linkGenerator.GetUriByPage(HttpContext, "/Index", handler: null, values: null)
            };
            // Send the confirmation e-mail for the user.
            await _emailSender.SendEmailConfirmationEmailAsync(emailViewModel);
            // Display a message to the user.
            TempData["StatusMessage"] = "Success: Verification e-mail sent. Please check the provided e-mail address for instructions on confirming your e-mail.";
            // Redirect to page.
            return RedirectToPage();
        }
    }
}
