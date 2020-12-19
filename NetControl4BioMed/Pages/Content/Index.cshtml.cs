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
            [RegularExpression("Any|Networks|Analyses")]
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
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Define the view.
            View = new ViewModel
            {
                IsUserAuthenticated = user != null,
                ItemCount = new Dictionary<string, int?>()
            };
            // Check if the user is authenticated.
            if (View.IsUserAuthenticated)
            {
                // Update the view.
                View.ItemCount["GenericNetworks"] = _context.Networks
                    .Where(item => item.NetworkDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Count(item => item.NetworkUsers.Any(item1 => item1.User == user));
                View.ItemCount["GenericAnalyses"] = _context.Analyses
                    .Where(item => item.AnalysisDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Count(item => item.AnalysisUsers.Any(item1 => item1.User == user));
                View.ItemCount["PPINetworks"] = _context.Networks
                    .Where(item => item.NetworkDatabases.Any(item1 => item1.Database.DatabaseType.Name == "PPI"))
                    .Count(item => item.NetworkUsers.Any(item1 => item1.User == user));
                View.ItemCount["PPIAnalyses"] = _context.Analyses
                    .Where(item => item.AnalysisDatabases.Any(item1 => item1.Database.DatabaseType.Name == "PPI"))
                    .Count(item => item.AnalysisUsers.Any(item1 => item1.User == user));
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
                ItemCount = new Dictionary<string, int?>()
            };
            // Check if the user is authenticated.
            if (View.IsUserAuthenticated)
            {
                // Update the view.
                View.ItemCount["GenericNetworks"] = _context.Networks
                    .Where(item => item.NetworkDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Count(item => item.NetworkUsers.Any(item1 => item1.User == user));
                View.ItemCount["GenericAnalyses"] = _context.Analyses
                    .Where(item => item.AnalysisDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Count(item => item.AnalysisUsers.Any(item1 => item1.User == user));
                View.ItemCount["PPINetworks"] = _context.Networks
                    .Where(item => item.NetworkDatabases.Any(item1 => item1.Database.DatabaseType.Name == "PPI"))
                    .Count(item => item.NetworkUsers.Any(item1 => item1.User == user));
                View.ItemCount["PPIAnalyses"] = _context.Analyses
                    .Where(item => item.AnalysisDatabases.Any(item1 => item1.Database.DatabaseType.Name == "PPI"))
                    .Count(item => item.AnalysisUsers.Any(item1 => item1.User == user));
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
            // Check if a network should be searched for.
            if (Input.Type == "Any" || Input.Type == "Networks")
            {
                // Try to find a network with the given ID.
                var network = _context.Networks
                    .Where(item => item.IsPublic || item.NetworkUsers.Any(item1 => item1.User == user))
                    .Where(item => item.Id == Input.Id)
                    .FirstOrDefault();
                // Check if a network has been found.
                if (network != null)
                {
                    // Get the database type.
                    var databaseType = _context.Databases
                        .Where(item => item.NetworkDatabases.Any(item1 => item1.Network == network))
                        .Select(item => item.DatabaseType)
                        .FirstOrDefault();
                    // Check if there was no database type found.
                    if (databaseType == null)
                    {
                        // Display a message.
                        TempData["StatusMessage"] = "Error: The network with the provided ID does not have a valid database type.";
                        // Redirect to the index page.
                        return RedirectToPage();
                    }
                    // Redirect to page.
                    return RedirectToPage($"/Content/DatabaseTypes/Created/{databaseType.Name}/Created/Networks/Details/Index", new { id = Input.Id });
                }
                // Check if only a network should have been searched for.
                if (Input.Type == "Networks")
                {
                    // Display a message.
                    TempData["StatusMessage"] = "Error: No network has been found with the provided ID, or you don't have access to it.";
                    // Redirect to the index page.
                    return RedirectToPage();
                }
            }
            // Check if an analysis should be searched for.
            if (Input.Type == "Any" || Input.Type == "Analyses")
            {
                // Try to find an analysis with the given ID.
                var analysis = _context.Analyses
                    .Where(item => item.IsPublic || item.AnalysisUsers.Any(item1 => item1.User == user))
                    .Where(item => item.Id == Input.Id)
                    .FirstOrDefault();
                // Check if an analysis has been found.
                if (analysis != null)
                {
                    // Get the database type.
                    var databaseType = _context.Databases
                        .Where(item => item.AnalysisDatabases.Any(item1 => item1.Analysis == analysis))
                        .Select(item => item.DatabaseType)
                        .FirstOrDefault();
                    // Check if there was no database type found.
                    if (databaseType == null)
                    {
                        // Display a message.
                        TempData["StatusMessage"] = "Error: The analysis with the provided ID does not have a valid database type.";
                        // Redirect to the index page.
                        return RedirectToPage();
                    }
                    // Redirect to page.
                    return RedirectToPage($"/Content/DatabaseTypes/Created/{databaseType.Name}/Created/Analyses/Details/Index", new { id = Input.Id });
                }
                // Check if only an analysis should have been searched for.
                if (Input.Type == "Analyses")
                {
                    // Display a message.
                    TempData["StatusMessage"] = "Error: No analysis has been found with the provided ID, or you don't have access to it.";
                    // Redirect to the index page.
                    return RedirectToPage();
                }
            }
            // Display a message.
            TempData["StatusMessage"] = "Error: No network or analysis has been found with the provided ID, or you don't have access to it.";
            // Redirect to the index page.
            return RedirectToPage();
        }
    }
}
