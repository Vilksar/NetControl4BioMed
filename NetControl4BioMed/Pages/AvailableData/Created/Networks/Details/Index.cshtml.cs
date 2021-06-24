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

namespace NetControl4BioMed.Pages.AvailableData.Created.Networks.Details
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
            public bool IsUserOwner { get; set; }

            public Network Network { get; set; }

            public bool ShowVisualization { get; set; }

            public Dictionary<string, int?> ItemCount { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/AvailableData/Created/Networks/Index");
            }
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Get the item with the provided ID.
            var items = _context.Networks
                .Where(item => item.IsPublic || (user != null && item.NetworkUsers.Any(item1 => item1.Email == user.Email)))
                .Where(item => item.Id == id);
            // Check if there was no item found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No network has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/AvailableData/Created/Networks/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                IsUserOwner = items
                    .Any(item => user != null && item.NetworkUsers.Any(item1 => item1.Type == NetworkUserType.Owner && item1.Email == user.Email)),
                Network = items
                    .First(),
                ShowVisualization = items
                    .All(item => item.Status == NetworkStatus.Completed && item.NetworkProteins
                        .Where(item1 => item1.Type == NetworkProteinType.None)
                        .Count() < NetworkExtensions.MaximumSizeForVisualization),
                ItemCount = new Dictionary<string, int?>
                {
                    { "Proteins", items.Select(item => item.NetworkProteins).SelectMany(item => item).Count(item => item.Type == NetworkProteinType.None) },
                    { "Interactions", items.Select(item => item.NetworkInteractions).SelectMany(item => item).Count() },
                    { "Databases", items.Select(item => item.NetworkDatabases).SelectMany(item => item).Count() },
                    { "ProteinCollections", items.Select(item => item.NetworkProteinCollections).SelectMany(item => item).Count() }
                }
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnGetRefreshAsync(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Return an empty result.
                return new JsonResult(new { });
            }
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Get the item with the provided ID.
            var item = _context.Networks
                .Where(item => item.IsPublic || (user != null && item.NetworkUsers.Any(item1 => item1.Email == user.Email)))
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
