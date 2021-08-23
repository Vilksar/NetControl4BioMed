using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data.Models;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.Identity
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<User> _userManager;

        public ConfirmEmailModel(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            // Check if the user ID and code aren't provided.
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
            {
                // Display an error.
                TempData["StatusMessage"] = "Error: The confirmation link is not valid. Please try to paste it manually into the browser address bar.";
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
            // Try to confirm the e-mail using the provided code.
            var result = await _userManager.ConfirmEmailAsync(user, code);
            // Check if the confirmation failed.
            if (!result.Succeeded)
            {
                // Display an error.
                TempData["StatusMessage"] = "Error: The confirmation code in the link is not valid. Please try to paste the link manually into the browser address bar.";
                // Redirect to the home page.
                return RedirectToPage("/Index");
            }
            // Display a message to the user.
            TempData["StatusMessage"] = "Success: Thank you for confirming your e-mail. You can now log in and use the application.";
            // Redirect to the login page.
            return RedirectToPage("/Identity/Login");
        }
    }
}
