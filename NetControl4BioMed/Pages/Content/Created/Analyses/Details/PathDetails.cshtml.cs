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

namespace NetControl4BioMed.Pages.Content.Created.Analyses.Details
{
    [Authorize]
    public class PathDetailsModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public PathDetailsModel(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public Analysis Analysis { get; set; }

            public bool IsGeneric { get; set; }

            public Path Path { get; set; }

            public int NodeCount { get; set; }

            public int EdgeCount { get; set; }
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
            var item = _context.Paths
                .Where(item => item.ControlPath.Analysis.AnalysisUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == id)
                .Include(item => item.PathNodes)
                    .ThenInclude(item => item.Node)
                .Include(item => item.PathEdges)
                    .ThenInclude(item => item.Edge)
                .Include(item => item.ControlPath)
                    .ThenInclude(item => item.Analysis)
                .FirstOrDefault();
            // Check if there was no item found.
            if (item == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Analysis = item.ControlPath.Analysis,
                IsGeneric = item.ControlPath.Analysis.AnalysisDatabases
                    .Any(item => item.Database.DatabaseType.Name == "Generic"),
                Path = item,
                NodeCount = item.PathNodes.Count(),
                EdgeCount = item.PathEdges.Count()
            };
            // Return the page.
            return Page();
        }
    }
}
