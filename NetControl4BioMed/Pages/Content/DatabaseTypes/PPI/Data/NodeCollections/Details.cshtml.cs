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

namespace NetControl4BioMed.Pages.Content.DatabaseTypes.PPI.Data.NodeCollections
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
            public NodeCollection NodeCollection { get; set; }

            public IEnumerable<NodeCollectionDatabase> NodeCollectionDatabases { get; set; }

            public IEnumerable<NodeCollectionNode> NodeCollectionNodes { get; set; }
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
                return RedirectToPage("/Content/DatabaseTypes/PPI/Data/NodeCollections/Index");
            }
            // Get the item with the provided ID.
            var items = _context.NodeCollections
                .Where(item => item.NodeCollectionDatabases.Any(item1 => item1.Database.DatabaseType.Name == "PPI"))
                .Where(item => item.NodeCollectionDatabases.Any(item1 => item1.Database.IsPublic || item1.Database.DatabaseUsers.Any(item2 => item2.User == user)))
                .Where(item => item.Id == id);
            // Check if there was no item found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/PPI/Data/NodeCollections/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                NodeCollection = items
                    .First(),
                NodeCollectionDatabases = items
                    .Select(item => item.NodeCollectionDatabases)
                    .SelectMany(item => item)
                    .Where(item => item.Database.IsPublic || item.Database.DatabaseUsers.Any(item1 => item1.User == user))
                    .Include(item => item.Database),
                NodeCollectionNodes = items
                    .Select(item => item.NodeCollectionNodes)
                    .SelectMany(item => item)
                    .Where(item => item.Node.DatabaseNodes.Any(item1 => item1.Database.IsPublic || item1.Database.DatabaseUsers.Any(item2 => item2.User == user)))
                    .Include(item => item.Node)
            };
            // Return the page.
            return Page();
        }
    }
}
