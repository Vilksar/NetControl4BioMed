using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Tasks;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.Identity
{
    [AllowAnonymous]
    public class ChangeEmailModel : PageModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly LinkGenerator _linkGenerator;

        public ChangeEmailModel(IServiceProvider serviceProvider, SignInManager<User> signInManager, UserManager<User> userManager, IEmailSender emailSender, LinkGenerator linkGenerator)
        {
            _serviceProvider = serviceProvider;
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
            _linkGenerator = linkGenerator;
        }

        public async Task<IActionResult> OnGetAsync(string userId, string email, string code)
        {
            // Check if the user ID, e-mail and code aren't provided.
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
            {
                // Display an error.
                TempData["StatusMessage"] = "Error: The confirmation link is not valid. Please try to paste the link manually into the browser address bar.";
                // Redirect to the home page.
                return RedirectToPage("/Index");
            }
            // Get the user with the provided user ID.
            var user = await _userManager.FindByIdAsync(userId);
            // Check if there wasn't any user found.
            if (user == null)
            {
                // Display an error.
                TempData["StatusMessage"] = "Error: The user ID in the confirmation link is not valid. Please try to paste the link manually into the browser address bar.";
                // Redirect to the home page.
                return RedirectToPage("/Index");
            }
            // Check if an account with the new e-mail address already exists.
            if (await _userManager.FindByEmailAsync(email) != null)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An account with the new e-mail address already exists.");
                // Return the page.
                return Page();
            }
            // Get the current e-mail of the user.
            var oldEmail = user.Email;
            // Try to change the e-mail using the provided code.
            var result = await _userManager.ChangeEmailAsync(user, email, code);
            // Check if the confirmation failed.
            if (!result.Succeeded)
            {
                // Display an error.
                TempData["StatusMessage"] = "Error: The e-mail or the confirmation code in the link are not valid. Please try to paste the link manually into the browser address bar.";
                // Redirect to the home page.
                return RedirectToPage("/Index");
            }
            // Define a new task.
            var usersTask = new UsersTask
            {
                Items = new List<UserInputModel>
                {
                    new UserInputModel
                    {
                        Id = user.Id,
                        Email = user.Email,
                        EmailConfirmed = true
                    }
                }
            };
            // Try to run the task.
            try
            {
                // Run the task.
                await usersTask.EditAsync(_serviceProvider, CancellationToken.None);
            }
            catch (Exception exception)
            {
                // Log out the user.
                await _signInManager.SignOutAsync();
                // Define the messages to return.
                var messages = new List<string> { "An error occurred while setting the new e-mail address." };
                // Build the exception message.
                while (exception != null)
                {
                    // Update the messages and the exception.
                    messages.Add(exception.Message);
                    exception = exception.InnerException;
                }
                // Display a message to the user.
                TempData["StatusMessage"] = $"Error: {string.Join(" ", messages.Where(item => !string.IsNullOrEmpty(item)))}";
                // Redirect to the home page.
                return RedirectToPage("/Index");
            }
            // Define a new view model for the e-mail.
            var emailChangedEmailViewModel = new EmailEmailChangedViewModel
            {
                OldEmail = oldEmail,
                NewEmail = user.Email,
                Url = _linkGenerator.GetUriByPage(HttpContext, "/Account/Index", handler: null, values: null),
                ApplicationUrl = _linkGenerator.GetUriByPage(HttpContext, "/Index", handler: null, values: null)
            };
            // Send the e-mail changed e-mail to the user.
            await _emailSender.SendEmailChangedEmailAsync(emailChangedEmailViewModel);
            // Re-sign in the user to update the changes.
            await _signInManager.RefreshSignInAsync(user);
            // Display a message to the user.
            TempData["StatusMessage"] = "Success: Thank you for confirming your e-mail. You can now log in and use the application.";
            // Redirect to the login page.
            return RedirectToPage("/Identity/Login");
        }
    }
}
