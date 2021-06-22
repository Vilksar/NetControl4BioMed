using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.AvailableData.Data.Interactions
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
            public Interaction Interaction { get; set; }

            public IEnumerable<DatabaseInteraction> DatabaseInteractions { get; set; }

            public IEnumerable<DatabaseInteractionFieldInteraction> DatabaseInteractionFieldInteractions { get; set; }

            public IEnumerable<InteractionProtein> InteractionProteins { get; set; }
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
                return RedirectToPage("/AvailableData/Data/Interactions/Index");
            }
            // Get the item with the provided ID.
            var items = _context.Interactions
                .Where(item => item.DatabaseInteractions.Any())
                .Where(item => item.DatabaseInteractions.Any(item1 => item1.Database.IsPublic || item1.Database.DatabaseUsers.Any(item2 => item2.User == user)))
                .Where(item => item.InteractionProteins.All(item1 => item1.Protein.DatabaseProteins.Any(item2 => item2.Database.IsPublic || item2.Database.DatabaseUsers.Any(item3 => item3.User == user))))
                .Where(item => item.Id == id);
            // Check if there was no item found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/AvailableData/Data/Interactions/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Interaction = items
                    .First(),
                DatabaseInteractions = items
                    .Select(item => item.DatabaseInteractions)
                    .SelectMany(item => item)
                    .Where(item => item.Database.IsPublic || item.Database.DatabaseUsers.Any(item1 => item1.User == user))
                    .Include(item => item.Database),
                DatabaseInteractionFieldInteractions = items
                    .Select(item => item.DatabaseInteractionFieldInteractions)
                    .SelectMany(item => item)
                    .Where(item => item.DatabaseInteractionField.Database.IsPublic || item.DatabaseInteractionField.Database.DatabaseUsers.Any(item1 => item1.User == user))
                    .Include(item => item.DatabaseInteractionField),
                InteractionProteins = items
                    .Select(item => item.InteractionProteins)
                    .SelectMany(item => item)
                    .Include(item => item.Protein)
            };
            // Return the page.
            return Page();
        }
    }
}
