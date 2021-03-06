using System;
using System.Collections.Generic;
using System.Linq;
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

namespace NetControl4BioMed.Pages.Content.DatabaseTypes.Generic.Created.Analyses.Details.Created.ControlPaths.Details
{
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

            public ControlPath ControlPath { get; set; }

            public HashSet<Node> SourceNodes { get; set; }

            public Dictionary<Node, int> UniqueControlNodes { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/Generic/Created/Analyses/Index");
            }
            // Get the item with the provided ID.
            var items = _context.ControlPaths
                .Where(item => item.Analysis.AnalysisDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                .Where(item => item.Analysis.IsPublic || item.Analysis.AnalysisUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == id);
            // Check if there was no item found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No control path has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/Generic/Created/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Analysis = items
                    .Select(item => item.Analysis)
                    .First(),
                ShowVisualization = items
                    .All(item => item.Paths
                        .Select(item1 => item1.PathNodes)
                        .SelectMany(item1 => item1)
                        .Where(item1 => item1.Type == PathNodeType.None)
                        .Count() < ControlPathExtensions.MaximumSizeForVisualization),
                ControlPath = items
                    .First(),
                SourceNodes = items
                    .Select(item => item.Analysis)
                    .Select(item => item.AnalysisNodes)
                    .SelectMany(item => item)
                    .Where(item => item.Type == AnalysisNodeType.Source)
                    .Select(item => item.Node)
                    .Distinct()
                    .ToHashSet(),
                UniqueControlNodes = items
                    .Select(item => item.Paths)
                    .SelectMany(item => item)
                    .Select(item => item.PathNodes)
                    .SelectMany(item => item)
                    .Where(item => item.Type == PathNodeType.Source)
                    .Select(item => item.Node)
                    .AsEnumerable()
                    .GroupBy(item => item)
                    .ToDictionary(item => item.Key, item => item.Count())
            };
            // Return the page.
            return Page();
        }
    }
}
