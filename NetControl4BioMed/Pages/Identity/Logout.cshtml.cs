using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Identity
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public LogoutModel(SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public IActionResult OnGet()
        {
            // Redirect to the home page. All requests to log out should come as "post".
            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if there isn't any logged in user.
            if (user == null)
            {
                // Redirect to the home page.
                return RedirectToPage("/Index");
            }
            // Log out the user.
            await _signInManager.SignOutAsync();
            // Redirect to the home page.
            return RedirectToPage("/Index");
        }
    }
}
