using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Identity
{
    [AllowAnonymous]
    public class AccessDeniedModel : PageModel
    {
        private readonly UserManager<User> _userManager;

        public AccessDeniedModel(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public bool IsUserAuthenticated { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Define the view.
            View = new ViewModel
            {
                IsUserAuthenticated = user != null
            };
            // Return the page.
            return Page();
        }
    }
}
