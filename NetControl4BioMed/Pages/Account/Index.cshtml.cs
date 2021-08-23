using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using NetControl4BioMed.Data.Models;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.Account
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public IndexModel(UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public User User { get; set; }

            public string AnnouncementMessage { get; set; }
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
            // Define the view.
            View = new ViewModel
            {
                User = user,
                AnnouncementMessage = _configuration["Data:AnnouncementMessage"]
            };
            // Return the page.
            return Page();
        }
    }
}
