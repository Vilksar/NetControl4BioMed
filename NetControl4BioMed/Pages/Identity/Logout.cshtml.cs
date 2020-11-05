using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Tasks;

namespace NetControl4BioMed.Pages.Identity
{
    public class LogoutModel : PageModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public LogoutModel(IServiceProvider serviceProvider, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _serviceProvider = serviceProvider;
            _userManager = userManager;
            _signInManager = signInManager;
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
            // Log out the user.
            await _signInManager.SignOutAsync();
            // Display a message to the user.
            TempData["StatusMessage"] = "Success: You have been successfully logged out.";
            // Redirect to the home page.
            return RedirectToPage("/Index");
        }
    }
}
