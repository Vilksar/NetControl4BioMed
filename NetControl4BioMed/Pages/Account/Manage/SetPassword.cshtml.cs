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
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.ViewModels;

namespace NetControl4BioMed.Pages.Account.Manage
{
    [Authorize]
    public class SetPasswordModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ISendGridEmailSender _emailSender;
        private readonly LinkGenerator _linkGenerator;
        private readonly IReCaptchaChecker _reCaptchaChecker;

        public SetPasswordModel(UserManager<User> userManager, SignInManager<User> signInManager, ISendGridEmailSender emailSender, LinkGenerator linkGenerator, IReCaptchaChecker reCaptchaChecker)
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
            [DataType(DataType.Password)]
            [Required(ErrorMessage = "This field is required.")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Required(ErrorMessage = "This field is required.")]
            [Compare(nameof(NewPassword), ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public string ReCaptchaToken { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if the user exists or not.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: An error occured while trying to load the user data. If you are already logged in, please log out and try again.";
                // Redirect to the home page.
                return RedirectToPage("/Index");
            }
            // Check if the user doesn't have a password set.
            if (await _userManager.HasPasswordAsync(user))
            {
                // Redirect to the change password page.
                return RedirectToPage("/Account/Manage/ChangePassword");
            }
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // We get the current user.
            var user = await _userManager.GetUserAsync(User);
            // We check if the user exists or not.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: An error occured while trying to load the user data. If you are already logged in, please log out and try again.";
                // Redirect to the home page.
                return RedirectToPage("/Index");
            }
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
            // Try to change the password with the new one.
            var result = await _userManager.AddPasswordAsync(user, Input.NewPassword);
            // Check if the password update wasn't successful.
            if (!result.Succeeded)
            {
                // Go over the encountered errors and add them to the model.
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                // Return the page.
                return Page();
            }
            // Define a new view model for the e-mail.
            var emailViewModel = new EmailPasswordChangedViewModel
            {
                Email = user.Email,
                Url = _linkGenerator.GetUriByPage(HttpContext, "/Account/Manage/ChangePassword", handler: null, values: null),
                ApplicationUrl = _linkGenerator.GetUriByPage(HttpContext, "/Index", handler: null, values: null)
            };
            // Send the e-mail changed e-mail to the user.
            await _emailSender.SendPasswordChangedEmailAsync(emailViewModel);
            // Re-sign in the user to update the changes.
            await _signInManager.RefreshSignInAsync(user);
            // Display a message.
            TempData["StatusMessage"] = "Success: The password was sucessfully set.";
            // Redirect to page.
            return RedirectToPage();
        }
    }
}
