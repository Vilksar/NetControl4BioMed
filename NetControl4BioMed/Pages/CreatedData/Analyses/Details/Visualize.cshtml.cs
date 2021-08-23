using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.CreatedData.Analyses.Details
{
    public class VisualizeModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly LinkGenerator _linkGenerator;

        public VisualizeModel(UserManager<User> userManager, ApplicationDbContext context, LinkGenerator linkGenerator)
        {
            _userManager = userManager;
            _context = context;
            _linkGenerator = linkGenerator;
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public Analysis Analysis { get; set; }

            public string CytoscapeJson { get; set; }
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
            // Get the item with the provided ID.
            var items = _context.Analyses
                .Where(item => item.IsPublic || (user != null && item.AnalysisUsers.Any(item1 => item1.Email == user.Email)))
                .Where(item => item.Id == id);
            // Check if there was no item found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No analysis has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Analysis = items
                    .First(),
                CytoscapeJson = JsonSerializer.Serialize(items.First().GetCytoscapeViewModel(HttpContext, _linkGenerator, _context), new JsonSerializerOptions { IgnoreNullValues = true })
            };
            // Return the page.
            return Page();
        }
    }
}
