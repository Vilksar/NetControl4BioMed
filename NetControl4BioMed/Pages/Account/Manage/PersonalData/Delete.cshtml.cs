using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Tasks;

namespace NetControl4BioMed.Pages.Account.Manage.PersonalData
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IReCaptchaChecker _reCaptchaChecker;

        public DeleteModel(IServiceProvider serviceProvider, UserManager<User> userManager, SignInManager<User> signInManager, IReCaptchaChecker reCaptchaChecker)
        {
            _serviceProvider = serviceProvider;
            _userManager = userManager;
            _signInManager = signInManager;
            _reCaptchaChecker = reCaptchaChecker;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.Password)]
            [Required(ErrorMessage = "This field is required.")]
            public string Password { get; set; }

            public string ReCaptchaToken { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public bool HasPassword { get; set; }
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
            // Define the variables for the view.
            View = new ViewModel
            {
                HasPassword = await _userManager.HasPasswordAsync(user)
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if the user exists or not.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: An error occured while trying to load the user data. If you are already logged in, please log out and try again.";
                // Redirect to the login page.
                return RedirectToPage("/Index");
            }
            // Define the variables to return to the view.
            View = new ViewModel
            {
                HasPassword = await _userManager.HasPasswordAsync(user)
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
            // Check if the user is the last administrator user.
            if ((await _userManager.IsInRoleAsync(user, "Administrator")) && (await _userManager.GetUsersInRoleAsync("Administrator")).Count() == 1)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "If you delete your account, then there will be no administrator users left. Please add a new administrator, and then try again.");
                // Return the page.
                return Page();
            }
            // Check if the user has password and the provided password is incorrect.
            if (View.HasPassword && !await _userManager.CheckPasswordAsync(user, Input.Password))
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The provided password is not correct.");
                // Return the page.
                return Page();
            }
            // Define a new task.
            var task = new UsersTask
            {
                Items = new List<UserInputModel>
                {
                    new UserInputModel
                    {
                        Id = user.Id
                    }
                }
            };
            // Try to run the task.
            try
            {
                // Run the task.
                await task.DeleteAsync(_serviceProvider, CancellationToken.None);
            }
            catch (Exception)
            {
                // Log out the user.
                await _signInManager.SignOutAsync();
                // Display a message to the user.
                TempData["StatusMessage"] = $"Error: There was an error removing some of the data associated with your account.";
                // Redirect to the home page.
                return RedirectToPage("/Index");
            }
            // Sign out the user.
            await _signInManager.SignOutAsync();
            // Display a message to the user.
            TempData["StatusMessage"] = "Success: Your account has been successfully deleted, together with all of the associated data.";
            // Redirect to the home page.
            return RedirectToPage("/Index");
        }
    }
}
