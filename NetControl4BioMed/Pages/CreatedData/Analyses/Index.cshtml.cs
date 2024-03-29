using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.CreatedData.Analyses
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

        public IActionResult OnGet()
        {
            // Check if the user is not authenticated.
            if (!User.Identity.IsAuthenticated)
            {
                // Redirect to the public data.
                return RedirectToPage("/PublicData/Analyses/Index");
            }
            // Redirect to the private data.
            return RedirectToPage("/PrivateData/Analyses/Index");
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
            var item = _context.Analyses
                .Where(item => item.IsPublic || (user != null && item.AnalysisUsers.Any(item1 => item1.Email == user.Email)))
                .Where(item => item.Id == id)
                .FirstOrDefault();
            // Return the analysis data.
            return new JsonResult(new
            {
                Status = item.Status.ToString(),
                Progress = ((double)item.CurrentIteration * 100 / item.MaximumIterations).ToString("0.00"),
                ProgressWithoutImprovement = ((double)item.CurrentIterationWithoutImprovement * 100 / item.MaximumIterationsWithoutImprovement).ToString("0.00")
            });
        }
    }
}
