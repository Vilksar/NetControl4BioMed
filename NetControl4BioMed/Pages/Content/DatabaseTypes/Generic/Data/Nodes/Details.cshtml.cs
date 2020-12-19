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
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Content.DatabaseTypes.Generic.Data.Nodes
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
            public Node Node { get; set; }

            public IEnumerable<EdgeNode> EdgeNodes { get; set; }
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
                return RedirectToPage("/Content/DatabaseTypes/Generic/Index");
            }
            // Get the item with the provided ID.
            var items = _context.Nodes
                .Where(item => item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                .Where(item => item.NetworkNodes.Any(item1 => item1.Network.IsPublic || item1.Network.NetworkUsers.Any(item2 => item2.User == user)) || item.AnalysisNodes.Any(item1 => item1.Analysis.IsPublic || item1.Analysis.AnalysisUsers.Any(item2 => item2.User == user)))
                .Where(item => item.Id == id);
            // Check if there was no item found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/Generic/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Node = items
                    .First(),
                EdgeNodes = items
                    .Select(item => item.EdgeNodes)
                    .SelectMany(item => item)
                    .Include(item => item.Edge)
            };
            // Return the page.
            return Page();
        }
    }
}
