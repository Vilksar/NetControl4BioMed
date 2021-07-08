using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.CreatedData.Analyses.Details.Results.Paths
{
    public class DetailsModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public DetailsModel(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public Analysis Analysis { get; set; }

            public bool HasNetworkDatabases { get; set; }

            public bool ShowVisualization { get; set; }

            public Path Path { get; set; }

            public IEnumerable<PathProtein> PathProteins { get; set; }

            public IEnumerable<PathInteraction> PathInteractions { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Analyses/Index");
            }
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Get the item with the provided ID.
            var items = _context.Paths
                .Where(item => item.ControlPath.Analysis.IsPublic || (user != null && item.ControlPath.Analysis.AnalysisUsers.Any(item1 => item1.Email == user.Email)))
                .Where(item => item.Id == id);
            // Check if there was no item found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No path has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Analysis = items
                    .Select(item => item.ControlPath.Analysis)
                    .First(),
                ShowVisualization = items
                    .All(item => item.PathProteins
                        .Where(item1 => item1.Type == PathProteinType.None)
                        .Count() < PathExtensions.MaximumSizeForVisualization),
                Path = items
                    .First(),
                PathProteins = items
                    .Select(item => item.PathProteins)
                    .SelectMany(item => item)
                    .Where(item => item.Type == PathProteinType.None)
                    .Include(item => item.Protein)
                    .OrderBy(item => item.Index),
                PathInteractions = items
                    .Select(item => item.PathInteractions)
                    .SelectMany(item => item)
                    .Include(item => item.Interaction)
                    .OrderBy(item => item.Index)
            };
            // Update the view.
            View.HasNetworkDatabases = _context.NetworkDatabases
                .Any(item => item.Network.Id == View.Analysis.NetworkId);
            // Return the page.
            return Page();
        }
    }
}
