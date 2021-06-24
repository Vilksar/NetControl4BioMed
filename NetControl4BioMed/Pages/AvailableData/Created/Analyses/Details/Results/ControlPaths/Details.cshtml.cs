using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.AvailableData.Created.Analyses.Details.Results.ControlPaths
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

            public bool ShowVisualization { get; set; }

            public ControlPath ControlPath { get; set; }

            public HashSet<Protein> SourceProteins { get; set; }

            public Dictionary<Protein, int> UniqueControlProteins { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/AvailableData/Created/Analyses/Index");
            }
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Get the item with the provided ID.
            var items = _context.ControlPaths
                .Where(item => item.Analysis.IsPublic || (user != null && item.Analysis.AnalysisUsers.Any(item1 => item1.Email == user.Email)))
                .Where(item => item.Id == id);
            // Check if there was no item found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No control path has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/AvailableData/Created/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Analysis = items
                    .Select(item => item.Analysis)
                    .First(),
                ShowVisualization = items
                    .All(item => item.Paths
                        .Select(item1 => item1.PathProteins)
                        .SelectMany(item1 => item1)
                        .Where(item1 => item1.Type == PathProteinType.None)
                        .Count() < ControlPathExtensions.MaximumSizeForVisualization),
                ControlPath = items
                    .First(),
                SourceProteins = items
                    .Select(item => item.Analysis)
                    .Select(item => item.AnalysisProteins)
                    .SelectMany(item => item)
                    .Where(item => item.Type == AnalysisProteinType.Source)
                    .Select(item => item.Protein)
                    .Distinct()
                    .ToHashSet(),
                UniqueControlProteins = items
                    .Select(item => item.Paths)
                    .SelectMany(item => item)
                    .Select(item => item.PathProteins)
                    .SelectMany(item => item)
                    .Where(item => item.Type == PathProteinType.Source)
                    .Select(item => item.Protein)
                    .AsEnumerable()
                    .GroupBy(item => item)
                    .ToDictionary(item => item.Key, item => item.Count())
            };
            // Return the page.
            return Page();
        }
    }
}
