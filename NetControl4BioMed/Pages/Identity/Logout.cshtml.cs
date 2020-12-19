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
        private readonly SignInManager<User> _signInManager;

        public LogoutModel(SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
        }

        public IActionResult OnGet()
        {
            // Redirect to the home page. All requests to log out should come as "post".
            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Log out the user.
            await _signInManager.SignOutAsync();
            // Display a message to the user.
            TempData["StatusMessage"] = "Success: You have been successfully logged out.";
            // Redirect to the home page.
            return RedirectToPage("/Index");
        }
    }
}
