using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.CreatedData.Analyses.Details.Databases
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
            public Analysis Analysis { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Analyses/Index");
            }
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if there isn't any user found.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: An error occured while trying to load the user data. If you are already logged in, please log out and try again.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Analyses/Index");
            }
            // Get the item with the provided ID.
            var items = _context.Analyses
                .Where(item => item.IsPublic || (user != null && item.AnalysisUsers.Any(item1 => item1.Email == user.Email)))
                .Where(item => item.Id == id);
            // Check if there was no item found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID, or you don't have access to it..";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Analysis = items
                    .First()
            };
            // Redirect to the index page.
            return RedirectToPage("/CreatedData/Analyses/Details/Index", new { id = View.Analysis.Id });
        }
    }
}
