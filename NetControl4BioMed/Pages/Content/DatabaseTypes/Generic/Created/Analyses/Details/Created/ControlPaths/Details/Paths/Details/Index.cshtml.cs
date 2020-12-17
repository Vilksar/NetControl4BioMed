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

namespace NetControl4BioMed.Pages.Content.DatabaseTypes.Generic.Created.Analyses.Details.Created.ControlPaths.Details.Paths.Details
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

            public bool IsGeneric { get; set; }

            public bool ShowVisualization { get; set; }

            public Path Path { get; set; }

            public IEnumerable<PathNode> PathNodes { get; set; }

            public IEnumerable<PathEdge> PathEdges { get; set; }
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
            var items = _context.Paths
                .Where(item => item.ControlPath.Analysis.IsPublic || item.ControlPath.Analysis.AnalysisUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == id);
            // Check if there was no item found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No path has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/Generic/Created/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Analysis = items
                    .Select(item => item.ControlPath.Analysis)
                    .First(),
                IsGeneric = items
                    .Select(item => item.ControlPath.Analysis.AnalysisDatabases)
                    .SelectMany(item => item)
                    .Any(item => item.Database.DatabaseType.Name == "Generic"),
                ShowVisualization = items
                    .All(item => item.PathNodes
                        .Where(item1 => item1.Type == PathNodeType.None)
                        .Count() < PathExtensions.MaximumSizeForVisualization),
                Path = items
                    .First(),
                PathNodes = items
                    .Select(item => item.PathNodes)
                    .SelectMany(item => item)
                    .Include(item => item.Node)
                    .OrderBy(item => item.Index),
                PathEdges = items
                    .Select(item => item.PathEdges)
                    .SelectMany(item => item)
                    .Include(item => item.Edge)
                    .OrderBy(item => item.Index)
            };
            // Return the page.
            return Page();
        }
    }
}
