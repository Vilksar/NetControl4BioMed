using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Account
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public IndexModel(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public User User { get; set; }

            public int NetworkCount { get; set; }

            public int AnalysisCount { get; set; }

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
            // Get the current date and time.
            var dateTimeNow = DateTime.Now;
            // Define the view.
            View = new ViewModel
            {
                User = user,
                NetworkCount = _context.Networks.Count(item => item.NetworkUsers.Any(item1 => item1.User == user)),
                AnalysisCount = _context.Analyses.Count(item => item.AnalysisUsers.Any(item1 => item1.User == user))
            };
            // Check for any announcements.
            View.AnnouncementMessage = null;
            // Return the page.
            return Page();
        }
    }
}
