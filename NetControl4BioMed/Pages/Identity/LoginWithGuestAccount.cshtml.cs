using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Tasks;

namespace NetControl4BioMed.Pages.Identity
{
    [AllowAnonymous]
    public class LoginWithGuestAccountModel : PageModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly LinkGenerator _linkGenerator;
        private readonly IReCaptchaChecker _reCaptchaChecker;

        public LoginWithGuestAccountModel(IServiceProvider serviceProvider, UserManager<User> userManager, SignInManager<User> signInManager, LinkGenerator linkGenerator, IReCaptchaChecker reCaptchaChecker)
        {
            _serviceProvider = serviceProvider;
            _userManager = userManager;
            _signInManager = signInManager;
            _linkGenerator = linkGenerator;
            _reCaptchaChecker = reCaptchaChecker;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public string ReCaptchaToken { get; set; }
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
            // Define a function which returns the guest user e-mail based on its index.
            Func<int, string> getGuestEmail = (int index) => $"guest{index}@{HttpContext.Request.Host.Host}";
            // Define a variable to store the index of the first unused guest user.
            var guestUserIndex = 1;
            // Repeat while an user exists with the current index.
            while(await _userManager.FindByEmailAsync(getGuestEmail(guestUserIndex)) != null)
            {
                // Increment the index.
                guestUserIndex++;
            }
            // Define a new task.
            var task = new UsersTask
            {
                Items = new List<UserInputModel>
                {
                    new UserInputModel
                    {
                        Email = getGuestEmail(guestUserIndex),
                        Type = "None",
                        EmailConfirmed = true,
                        UserRoles = new List<UserRoleInputModel>
                        {
                            new UserRoleInputModel
                            {
                                Role = new RoleInputModel
                                {
                                    Name = "Guest"
                                }
                            }
                        }
                    }
                }
            };
            // Try to run the task.
            try
            {
                // Run the task.
                await task.CreateAsync(_serviceProvider, CancellationToken.None);
            }
            catch (Exception exception)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, exception.Message);
                // Redisplay the page.
                return Page();
            }
            // Get the new user.
            var user = await _userManager.FindByNameAsync(getGuestEmail(guestUserIndex));
            // Check if there wasn't any user found.
            if (user == null)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error was encountered. Please try again.");
                // Redisplay the page.
                return Page();
            }
            // Try to add the user to the guest role.
            var result = await _userManager.AddToRoleAsync(user, "Guest");
            // Check if the operation has failed.
            if (!result.Succeeded)
            {
                // Get the error messages.
                var messages = result.Errors
                    .Select(item => item.Description);
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, string.Join(" ", messages));
                // Redisplay the page.
                return Page();
            }
            // Log in the guest user.
            await _signInManager.SignInAsync(user, new AuthenticationProperties { IsPersistent = false, ExpiresUtc = DateTime.UtcNow.AddDays(1) });
            // Display a message to the user.
            TempData["StatusMessage"] = "Success: You have successfully logged in with a guest account. Please note that the account and all of the associated data will be deleted automatically when you log out, close the browser, or within 24 to 48 hours.";
            // Redirect to the return URL.
            return LocalRedirect(View.ReturnUrl);
        }
    }
}
