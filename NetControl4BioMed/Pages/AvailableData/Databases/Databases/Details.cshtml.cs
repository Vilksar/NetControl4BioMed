using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.AvailableData.Databases.Databases
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
            public Database Database { get; set; }

            public IEnumerable<DatabaseProteinField> DatabaseProteinFields { get; set; }

            public IEnumerable<DatabaseInteractionField> DatabaseInteractionFields { get; set; }

            public int ProteinCount { get; set; }

            public int InteractionCount { get; set; }
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
                return RedirectToPage("/AvailableData/Databases/Databases/Index");
            }
            // Get the item with the provided ID.
            var items = _context.Databases
                .Where(item => item.IsPublic || item.DatabaseUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == id);
            // Check if there was no item found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/AvailableData/Databases/Databases/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Database = items
                    .First(),
                DatabaseProteinFields = items
                    .Select(item => item.DatabaseProteinFields)
                    .SelectMany(item => item),
                DatabaseInteractionFields = items
                    .Select(item => item.DatabaseInteractionFields)
                    .SelectMany(item => item),
                ProteinCount = items
                    .Select(item => item.DatabaseProteins)
                    .SelectMany(item => item)
                    .Select(item => item.Protein)
                    .Distinct()
                    .Count(),
                InteractionCount = items
                    .Select(item => item.DatabaseInteractions)
                    .SelectMany(item => item)
                    .Select(item => item.Interaction)
                    .Distinct()
                    .Count()
            };
            // Return the page.
            return Page();
        }
    }
}
