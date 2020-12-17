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

namespace NetControl4BioMed.Pages.Content.DatabaseTypes.PPI.Data.Edges
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
            public Edge Edge { get; set; }

            public IEnumerable<DatabaseEdge> DatabaseEdges { get; set; }

            public IEnumerable<DatabaseEdgeFieldEdge> DatabaseEdgeFieldEdges { get; set; }

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
                return RedirectToPage("/Content/DatabaseTypes/PPI/Data/Edges/Index");
            }
            // Get the item with the provided ID.
            var items = _context.Edges
                .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                .Where(item => item.DatabaseEdges.Any(item1 => item1.Database.IsPublic || item1.Database.DatabaseUsers.Any(item2 => item2.User == user)))
                .Where(item => item.EdgeNodes.All(item1 => !item1.Node.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic") && item1.Node.DatabaseNodes.Any(item2 => item2.Database.IsPublic || item2.Database.DatabaseUsers.Any(item3 => item3.User == user))))
                .Where(item => item.Id == id);
            // Check if there was no item found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/PPI/Data/Edges/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Edge = items
                    .First(),
                DatabaseEdges = items
                    .Select(item => item.DatabaseEdges)
                    .SelectMany(item => item)
                    .Where(item => item.Database.IsPublic || item.Database.DatabaseUsers.Any(item1 => item1.User == user))
                    .Include(item => item.Database),
                DatabaseEdgeFieldEdges = items
                    .Select(item => item.DatabaseEdgeFieldEdges)
                    .SelectMany(item => item)
                    .Where(item => item.DatabaseEdgeField.Database.IsPublic || item.DatabaseEdgeField.Database.DatabaseUsers.Any(item1 => item1.User == user))
                    .Include(item => item.DatabaseEdgeField),
                EdgeNodes = items
                    .Select(item => item.EdgeNodes)
                    .SelectMany(item => item)
                    .Include(item => item.Node)
            };
            // Return the page.
            return Page();
        }
    }
}
