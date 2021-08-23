using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data.Models;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.Account.Manage.TwoFactorAuthentication
{
    [Authorize]
    public class GenerateRecoveryCodesModel : PageModel
    {
        private readonly UserManager<User> _userManager;

        public GenerateRecoveryCodesModel(UserManager<User> userManager)
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
                TempData["StatusMessage"] = "Error: Recovery codes can't be generated, as you don't have two-factor authentication enabled.";
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
                TempData["StatusMessage"] = "Error: The recovery codes can't be generated, as two-factor authentication is not currently enabled.";
                // Redirect to the two-factor authentication page.
                return RedirectToPage("/Account/Manage/TwoFactorAuthentication/Index");
            }
            // Generate the new recovery codes.
            TempData["RecoveryCodes"] = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            // Display a message.
            TempData["StatusMessage"] = "Success: The recovery codes were successfully generated.";
            // Redirect to the two-factor authentication page.
            return RedirectToPage("/Account/Manage/TwoFactorAuthentication/Index");
        }
    }
}
