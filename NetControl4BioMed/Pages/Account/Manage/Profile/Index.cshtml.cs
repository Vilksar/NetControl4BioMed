using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.Account.Manage.Profile
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ISendGridEmailSender _emailSender;
        private readonly LinkGenerator _linkGenerator;
        private readonly IReCaptchaChecker _reCaptchaChecker;

        public IndexModel(IServiceProvider serviceProvider, UserManager<User> userManager, SignInManager<User> signInManager, ISendGridEmailSender emailSender, LinkGenerator linkGenerator, IReCaptchaChecker reCaptchaChecker)
        {
            _serviceProvider = serviceProvider;
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
                Email = user.Email
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
            // Check if the e-mail is different than the current one.
            if (Input.Email != user.Email)
            {
                // Check if an account with the new e-mail address already exists.
                if (await _userManager.FindByEmailAsync(Input.Email) != null)
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "An account with the new e-mail address already exists.");
                    // Return the page.
                    return Page();
                }
                // Generate an e-mail change code.
                var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.Email);
                // Create the callback URL to be encoded in the change email.
                var callbackUrl = _linkGenerator.GetUriByPage(HttpContext, "/Identity/ChangeEmail", handler: null, values: new { userId = user.Id, email = Input.Email, code = code });
                // Define a new view model for the e-mail.
                var emailChangeEmailViewModel = new EmailEmailChangeViewModel
                {
                    OldEmail = user.Email,
                    NewEmail = Input.Email,
                    Url = callbackUrl,
                    ApplicationUrl = _linkGenerator.GetUriByPage(HttpContext, "/Index", handler: null, values: null)
                };
                // Send the e-mail change e-mail to the user.
                await _emailSender.SendEmailChangeEmailAsync(emailChangeEmailViewModel);
                // Display a message.
                TempData["StatusMessage"] = $"Success: An e-mail has been sent to the specified e-mail address. Please follow the instructions there in order to change the e-mail address associated with the account.";
                // Redirect to page.
                return RedirectToPage();
            }
            // Display a message.
            TempData["StatusMessage"] = "Success: All details were already up to date.";
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
            // Define a new view model for the e-mail.
            var emailViewModel = new EmailEmailConfirmationViewModel
            {
                Email = user.Email,
                Url = callbackUrl,
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
