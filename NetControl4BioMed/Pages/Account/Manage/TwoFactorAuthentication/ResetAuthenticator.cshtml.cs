using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data.Models;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.Account.Manage.TwoFactorAuthentication
{
    [Authorize]
    public class ResetAuthenticatorModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public ResetAuthenticatorModel(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnGet()
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
            // Disable two-factor authentication for the user.
            await _userManager.SetTwoFactorEnabledAsync(user, false);
            // Reset the authenticator key of the user.
            await _userManager.ResetAuthenticatorKeyAsync(user);
            // Resign in the user to update the changes.
            await _signInManager.RefreshSignInAsync(user);
            // Display a message.
            TempData["StatusMessage"] = "Success: Your authenticator app key has been reset, you will need to configure your authenticator app using the new key.";
            // Redirect to the enable authenticator page.
            return RedirectToPage("/Account/Manage/TwoFactorAuthentication/EnableAuthenticator");
        }
    }
}
