using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data.Models;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.Account.Manage.TwoFactorAuthentication
{
    [Authorize]
    public class DisableAuthenticatorModel : PageModel
    {
        private readonly UserManager<User> _userManager;

        public DisableAuthenticatorModel(UserManager<User> userManager)
        {
            _userManager = userManager;
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
            // Check if two-factor authentication is not enabled.
            if (!await _userManager.GetTwoFactorEnabledAsync(user))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: Two-factor authentication can't be disabled, as it is not currently enabled.";
                // Redirect to the two-factor authentication page.
                return RedirectToPage("/Account/Manage/TwoFactorAuthentication/Index");
            }
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
                // Redirect to the home page.
                return RedirectToPage("/Index");
            }
            // Check if two-factor authentication is not enabled.
            if (!await _userManager.GetTwoFactorEnabledAsync(user))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: Two-factor authentication can't be disabled, as it is not currently enabled.";
                // Redirect to the two-factor authentication page.
                return RedirectToPage("/Account/Manage/TwoFactorAuthentication/Index");
            }
            // Try to disable two-factor authentication.
            var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
            // Check if the disabling was not successful.
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
            // Display a message.
            TempData["StatusMessage"] = "Success: Two-factor authentication has been disabled. You can enable it again when you set up an authenticator app.";
            // Redirect to the two-factor authentication page.
            return RedirectToPage("/Account/Manage/TwoFactorAuthentication/Index");
        }
    }
}
