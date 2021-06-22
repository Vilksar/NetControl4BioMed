using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.AvailableData.Data.Proteins
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
            public Protein Protein { get; set; }

            public IEnumerable<DatabaseProtein> DatabaseProteins { get; set; }

            public IEnumerable<DatabaseProteinFieldProtein> DatabaseProteinFieldProteins { get; set; }

            public IEnumerable<InteractionProtein> InteractionProteins { get; set; }

            public IEnumerable<ProteinCollectionProtein> ProteinCollectionProteins { get; set; }
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
                return RedirectToPage("/AvailableData/Data/Proteins/Index");
            }
            // Get the item with the provided ID.
            var items = _context.Proteins
                .Where(item => item.DatabaseProteins.Any())
                .Where(item => item.DatabaseProteins.Any(item1 => item1.Database.IsPublic || item1.Database.DatabaseUsers.Any(item2 => item2.User == user)))
                .Where(item => item.Id == id);
            // Check if there was no item found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/AvailableData/Data/Proteins/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Protein = items
                    .First(),
                DatabaseProteins = items
                    .Select(item => item.DatabaseProteins)
                    .SelectMany(item => item)
                    .Where(item => item.Database.IsPublic || item.Database.DatabaseUsers.Any(item1 => item1.User == user))
                    .Include(item => item.Database),
                DatabaseProteinFieldProteins = items
                    .Select(item => item.DatabaseProteinFieldProteins)
                    .SelectMany(item => item)
                    .Where(item => item.DatabaseProteinField.Database.IsPublic || item.DatabaseProteinField.Database.DatabaseUsers.Any(item1 => item1.User == user))
                    .Include(item => item.DatabaseProteinField),
                InteractionProteins = items
                    .Select(item => item.InteractionProteins)
                    .SelectMany(item => item)
                    .Where(item => item.Interaction.DatabaseInteractions.Any(item1 => item1.Database.IsPublic || item1.Database.DatabaseUsers.Any(item2 => item2.User == user)))
                    .Where(item => item.Interaction.InteractionProteins.All(item1 => item1.Protein.DatabaseProteins.Any(item2 => item2.Database.IsPublic || item2.Database.DatabaseUsers.Any(item3 => item3.User == user))))
                    .Include(item => item.Interaction),
                ProteinCollectionProteins = items
                    .Select(item => item.ProteinCollectionProteins)
                    .SelectMany(item => item)
                    .Include(item => item.ProteinCollection)
            };
            // Return the page.
            return Page();
        }
    }
}
