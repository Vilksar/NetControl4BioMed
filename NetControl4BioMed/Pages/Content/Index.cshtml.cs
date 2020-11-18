using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Interfaces;

namespace NetControl4BioMed.Pages.Content
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IReCaptchaChecker _reCaptchaChecker;

        public IndexModel(UserManager<User> userManager, ApplicationDbContext context, IReCaptchaChecker reCaptchaChecker)
        {
            _userManager = userManager;
            _context = context;
            _reCaptchaChecker = reCaptchaChecker;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            [RegularExpression("Networks|Analyses")]
            public string Type { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string Id { get; set; }

            public string ReCaptchaToken { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public bool IsUserAuthenticated { get; set; }

            public Dictionary<string, int?> ItemCount { get; set; }

            public IEnumerable<Network> RecentNetworks { get; set; }

            public IEnumerable<Analysis> RecentAnalyses { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Define the view.
            View = new ViewModel
            {
                IsUserAuthenticated = user != null,
                ItemCount = new Dictionary<string, int?>(),
                RecentNetworks = Enumerable.Empty<Network>(),
                RecentAnalyses = Enumerable.Empty<Analysis>()
            };
            // Check if the user is authenticated.
            if (View.IsUserAuthenticated)
            {
                // Update the view.
                View.ItemCount["Networks"] = _context.Networks
                    .Count(item => item.NetworkUsers.Any(item1 => item1.User == user));
                View.ItemCount["Analyses"] = _context.Analyses
                    .Count(item => item.AnalysisUsers.Any(item1 => item1.User == user));
                View.RecentNetworks = _context.Networks
                    .Where(item => item.NetworkUsers.Any(item1 => item1.User == user))
                    .OrderByDescending(item => item.DateTimeCreated)
                    .Take(5);
                View.RecentAnalyses = _context.Analyses
                    .Where(item => item.AnalysisUsers.Any(item1 => item1.User == user))
                    .OrderByDescending(item => item.DateTimeStarted)
                    .Take(5);
            }
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Define the view.
            View = new ViewModel
            {
                IsUserAuthenticated = user != null,
                ItemCount = new Dictionary<string, int?>(),
                RecentNetworks = Enumerable.Empty<Network>(),
                RecentAnalyses = Enumerable.Empty<Analysis>()
            };
            // Check if the user is authenticated.
            if (View.IsUserAuthenticated)
            {
                // Update the view.
                View.ItemCount["Networks"] = _context.Networks
                    .Count(item => item.NetworkUsers.Any(item1 => item1.User == user));
                View.ItemCount["Analyses"] = _context.Analyses
                    .Count(item => item.AnalysisUsers.Any(item1 => item1.User == user));
                View.RecentNetworks = _context.Networks
                    .Where(item => item.NetworkUsers.Any(item1 => item1.User == user))
                    .OrderByDescending(item => item.DateTimeCreated)
                    .Take(5);
                View.RecentAnalyses = _context.Analyses
                    .Where(item => item.AnalysisUsers.Any(item1 => item1.User == user))
                    .OrderByDescending(item => item.DateTimeStarted)
                    .Take(5);
            }
            // Check if the reCaptcha is valid.
            if (!await _reCaptchaChecker.IsValid(Input.ReCaptchaToken))
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The reCaptcha verification failed.");
                // Return the page.
                return Page();
            }
            // Check if the provided model is not valid.
            if (!ModelState.IsValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error was encountered. Please check again the input fields.");
                // Return the page.
                return Page();
            }
            // Redirect to page.
            return RedirectToPage($"/Content/Created/{Input.Type}/Details/Index", new { id = Input.Id });
        }
    }
}
