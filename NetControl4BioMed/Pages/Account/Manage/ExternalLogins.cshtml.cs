using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Account.Manage
{
    [Authorize]
    public class ExternalLoginsModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public ExternalLoginsModel(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public IEnumerable<UserLoginInfo> CurrentLogins { get; set; }

            public IEnumerable<AuthenticationScheme> OtherLogins { get; set; }

            public bool ShowRemoveButton { get; set; }
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
                CurrentLogins = await _userManager.GetLoginsAsync(user)
            };
            // Get the other available external logins that are not assigned to the user.
            View.OtherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(auth => View.CurrentLogins.All(cl => !string.Equals(auth, cl)));
            // If there is only one external login and there is no local account, then hide the "Remove" button.
            View.ShowRemoveButton = await _userManager.HasPasswordAsync(user) || View.CurrentLogins.Count() > 1;
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostRemoveLoginAsync(string loginProvider, string providerKey)
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
                CurrentLogins = await _userManager.GetLoginsAsync(user)
            };
            // Get the other available external logins that are not assigned to the user.
            View.OtherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(auth => View.CurrentLogins.All(cl => !string.Equals(cl, auth)));
            // If there is only one external login and there is no local account, then hide the "Remove" button.
            View.ShowRemoveButton = await _userManager.HasPasswordAsync(user) || View.CurrentLogins.Count() > 1;
            // Try to remove the selected login from the user.
            var result = await _userManager.RemoveLoginAsync(user, loginProvider, providerKey);
            // Check if the removal was not successful.
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
            // Re-sign in the user to update the changes.
            await _signInManager.RefreshSignInAsync(user);
            // Display a message.
            TempData["StatusMessage"] = "Success: The external login was removed.";
            // Redirect to the page.
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostLinkLoginAsync(string provider)
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
            // Clear the existing external cookie to ensure a clean login process.
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.Page("/Account/Manage/ExternalLogins", pageHandler: "LinkLoginCallback");
            // Get the current properties of the external authentication configuration.
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, user.Id);
            // And apply the new authentication schema.
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetLinkLoginCallbackAsync()
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
                // The current external logins of the user.
                CurrentLogins = await _userManager.GetLoginsAsync(user)
            };
            // Other available external logins that are not assigned to the user.
            View.OtherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(auth => View.CurrentLogins.All(cl => !string.Equals(cl, auth)));
            // If there is only one external login and there is no local account, then hide the "Remove" button.
            View.ShowRemoveButton = await _userManager.HasPasswordAsync(user) || View.CurrentLogins.Count() > 1;
            // Get the information provided by the external authentication for the current user.
            var info = await _signInManager.GetExternalLoginInfoAsync(user.Id);
            // Check if there wasn't any information received.
            if (info == null)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "There was an error loading information from the external provider.");
                // Return the page.
                return Page();
            }
            // Try to add the external login to the current user.
            var result = await _userManager.AddLoginAsync(user, info);
            // Check if adding the external login failed.
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
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            // Display a message.
            TempData["StatusMessage"] = "Success: The external login was added.";
            // Redirect to page.
            return RedirectToPage();
        }
    }
}
