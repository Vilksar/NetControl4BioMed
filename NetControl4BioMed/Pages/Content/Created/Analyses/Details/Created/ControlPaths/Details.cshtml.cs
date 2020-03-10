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

namespace NetControl4BioMed.Pages.Content.Created.Analyses.Details.Created.ControlPaths
{
    [Authorize]
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

            public bool IsGeneric { get; set; }

            public ControlPath ControlPath { get; set; }

            public IEnumerable<Node> UniqueControlNodes { get; set; }
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
            var item = _context.ControlPaths
                .Where(item => item.Analysis.AnalysisUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == id)
                .Include(item => item.Paths)
                    .ThenInclude(item => item.PathNodes)
                        .ThenInclude(item => item.Node)
                .Include(item => item.Paths)
                    .ThenInclude(item => item.PathEdges)
                        .ThenInclude(item => item.Edge)
                            .ThenInclude(item => item.EdgeNodes)
                                .ThenInclude(item => item.Node)
                .Include(item => item.Analysis)
                    .ThenInclude(item => item.AnalysisDatabases)
                        .ThenInclude(item => item.Database)
                            .ThenInclude(item => item.DatabaseType)
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
                Analysis = item.Analysis,
                IsGeneric = item.Analysis.AnalysisDatabases
                    .Any(item => item.Database.DatabaseType.Name == "Generic"),
                ControlPath = item,
                UniqueControlNodes = item.Paths.Select(item => item.PathNodes).SelectMany(item => item).Where(item => item.Type == PathNodeType.Source).Select(item => item.Node).Distinct()
            };
            // Return the page.
            return Page();
        }
    }
}
