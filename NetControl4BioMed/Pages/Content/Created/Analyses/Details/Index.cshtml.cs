using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;

namespace NetControl4BioMed.Pages.Content.Created.Analyses.Details
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
            public Analysis Analysis { get; set; }

            public bool ShowVisualization { get; set; }

            public Helpers.Algorithms.Algorithm1.Parameters Algorithm1Parameters { get; set; }

            public Helpers.Algorithms.Algorithm2.Parameters Algorithm2Parameters { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
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
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Get the item with the provided ID.
            var items = _context.Analyses
                .Where(item => item.AnalysisUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == id);
            // Check if there was no item found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Analysis = items
                    .Include(item => item.AnalysisNodes)
                    .Include(item => item.AnalysisEdges)
                    .Include(item => item.AnalysisDatabases)
                    .Include(item => item.AnalysisNodeCollections)
                    .Include(item => item.AnalysisUsers)
                    .Include(item => item.AnalysisUserInvitations)
                    .Include(item => item.AnalysisNetworks)
                    .First(),
                ShowVisualization = items
                    .Select(item => item.AnalysisNodes)
                    .SelectMany(item => item)
                    .Count(item => item.Type == AnalysisNodeType.None) < 500,
                Algorithm1Parameters = null,
                Algorithm2Parameters = null
            };
            // Check which algorithm is used and try to deserialize the parameters.
            if (View.Analysis.Algorithm == AnalysisAlgorithm.Algorithm1)
            {
                // Try to deserialize the parameters
                if (View.Analysis.Parameters.TryDeserializeJsonObject<Helpers.Algorithms.Algorithm1.Parameters>(out var parameters))
                {
                    // Assign the parameters.
                    View.Algorithm1Parameters = parameters;
                }
            }
            else if (View.Analysis.Algorithm == AnalysisAlgorithm.Algorithm2)
            {
                // Try to deserialize the parameters
                if (View.Analysis.Parameters.TryDeserializeJsonObject<Helpers.Algorithms.Algorithm2.Parameters>(out var parameters))
                {
                    // Assign the parameters.
                    View.Algorithm2Parameters = parameters;
                }
            }
            // Return the page.
            return Page();
        }
    }
}
