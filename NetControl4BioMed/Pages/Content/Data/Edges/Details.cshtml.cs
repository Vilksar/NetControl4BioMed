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

namespace NetControl4BioMed.Pages.Content.Data.Edges
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
            public Edge Edge { get; set; }

            public IEnumerable<DatabaseEdge> DatabaseEdges { get; set; }

            public IEnumerable<DatabaseEdgeFieldEdge> DatabaseEdgeFieldEdges { get; set; }

            public IEnumerable<EdgeNode> EdgeNodes { get; set; }
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
                return RedirectToPage("/Content/Data/Edges/Index");
            }
            // Get the item with the provided ID.
            var item = _context.Edges
                .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                .Where(item => item.Id == id)
                .Include(item => item.DatabaseEdges)
                    .ThenInclude(item => item.Database)
                        .ThenInclude(item => item.DatabaseUsers)
                .Include(item => item.DatabaseEdgeFieldEdges)
                    .ThenInclude(item => item.DatabaseEdgeField)
                        .ThenInclude(item => item.Database)
                            .ThenInclude(item => item.DatabaseUsers)
                .Include(item => item.EdgeNodes)
                    .ThenInclude(item => item.Node)
                        .ThenInclude(item => item.DatabaseNodes)
                            .ThenInclude(item => item.Database)
                                .ThenInclude(item => item.DatabaseType)
                .Include(item => item.EdgeNodes)
                    .ThenInclude(item => item.Node)
                        .ThenInclude(item => item.DatabaseNodes)
                            .ThenInclude(item => item.Database)
                                .ThenInclude(item => item.DatabaseUsers)
                .FirstOrDefault();
            // Check if there was no item found.
            if (item == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Data/Edges/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Edge = item,
                DatabaseEdges = item.DatabaseEdges
                    .Where(item => item.Database.IsPublic || item.Database.DatabaseUsers.Any(item1 => item1.User == user)),
                DatabaseEdgeFieldEdges = item.DatabaseEdgeFieldEdges
                    .Where(item => item.DatabaseEdgeField.Database.IsPublic || item.DatabaseEdgeField.Database.DatabaseUsers.Any(item1 => item1.User == user)),
                EdgeNodes = item.EdgeNodes
                    .Where(item => !item.Node.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => item.Node.DatabaseNodes.Any(item1 => item1.Database.IsPublic || item1.Database.DatabaseUsers.Any(item2 => item2.User == user)))
            };
            // Return the page.
            return Page();
        }
    }
}
