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

namespace NetControl4BioMed.Pages.Content.DatabaseTypes.PPI.Created.Networks.Details
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
            public bool IsUserAuthenticated { get; set; }

            public Network Network { get; set; }

            public string DatabaseTypeId { get; set; }

            public bool ShowVisualization { get; set; }

            public Dictionary<string, int?> ItemCount { get; set; }
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
                return RedirectToPage("/Content/DatabaseTypes/PPI/Created/Networks/Index");
            }
            // Get the item with the provided ID.
            var items = _context.Networks
                .Where(item => item.IsPublic || item.NetworkUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == id);
            // Check if there was no item found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No network has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/PPI/Created/Networks/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                IsUserAuthenticated = user != null,
                Network = items
                    .First(),
                DatabaseTypeId = items
                    .Select(item => item.NetworkDatabases)
                    .SelectMany(item => item)
                    .Select(item => item.Database.DatabaseType.Id)
                    .Distinct()
                    .FirstOrDefault(),
                ShowVisualization = items
                    .All(item => item.Status == NetworkStatus.Completed && item.NetworkNodes
                        .Where(item1 => item1.Type == NetworkNodeType.None)
                        .Count() < NetworkExtensions.MaximumSizeForVisualization),
                ItemCount = new Dictionary<string, int?>
                {
                    { "Nodes", items.Select(item => item.NetworkNodes).SelectMany(item => item).Count(item => item.Type == NetworkNodeType.None) },
                    { "Edges", items.Select(item => item.NetworkEdges).SelectMany(item => item).Count() },
                    { "Databases", items.Select(item => item.NetworkDatabases).SelectMany(item => item).Count() },
                    { "NodeCollections", items.Select(item => item.NetworkNodeCollections).SelectMany(item => item).Count() },
                    { "Users", items.Select(item => item.NetworkUsers).SelectMany(item => item).Count() + items.Select(item => item.NetworkUserInvitations).SelectMany(item => item).Count() },
                    { "Analyses", items.Select(item => item.AnalysisNetworks).SelectMany(item => item).Count() }
                }
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnGetRefreshAsync(string id)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Return an empty result.
                return new JsonResult(new { });
            }
            // Get the item with the provided ID.
            var item = _context.Networks
                .Where(item => item.IsPublic || item.NetworkUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == id)
                .FirstOrDefault();
            // Return the analysis data.
            return new JsonResult(new
            {
                Status = item != null ? item.Status.ToString() : string.Empty,
            });
        }
    }
}
