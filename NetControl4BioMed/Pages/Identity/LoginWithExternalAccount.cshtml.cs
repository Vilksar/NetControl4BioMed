using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Identity
{
    [AllowAnonymous]
    public class LoginWithExternalAccountModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly LinkGenerator _linkGenerator;

        public LoginWithExternalAccountModel(UserManager<User> userManager, SignInManager<User> signInManager, LinkGenerator linkGenerator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _linkGenerator = linkGenerator;
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public IEnumerable<AuthenticationScheme> ExternalLogins { get; set; }

            public string ReturnUrl { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            // Define the variables for the view.
            View = new ViewModel
            {
                ExternalLogins = await _signInManager.GetExternalAuthenticationSchemesAsync(),
                ReturnUrl = returnUrl ?? _linkGenerator.GetPathByPage(HttpContext, "/Index", handler: null, values: null)
            };
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if any user has been found.
            if (user != null)
            {
                // Redirect to the return URL.
                return LocalRedirect(View.ReturnUrl);
            }
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            // Return the page.
            return Page();
        }
    }
}
