using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.AvailableData.Created.Networks.Create
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public IndexModel(UserManager<User> userManager, ApplicationDbContext context, IConfiguration configuration)
        {
            _userManager = userManager;
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync(string networkId, bool loadDemonstration)
        {
            // Check if the demonstration should be loaded.
            if (loadDemonstration)
            {
                // Check if there are no demonstration items configured.
                if (string.IsNullOrEmpty(_configuration["Data:Demonstration:NetworkId"]))
                {
                    // Try to get a demonstration control path.
                    var controlPath = _context.ControlPaths
                        .Include(item => item.Analysis)
                            .ThenInclude(item => item.Network)
                        .Where(item => item.Analysis.IsPublic && item.Analysis.IsDemonstration && item.Analysis.Network.IsPublic && item.Analysis.Network.IsDemonstration)
                        .AsNoTracking()
                        .FirstOrDefault();
                    // Check if there was no demonstration control path found.
                    if (controlPath == null || controlPath.Analysis == null || controlPath.Analysis.Network == null)
                    {
                        // Display a message.
                        TempData["StatusMessage"] = "Error: There are no demonstration networks available.";
                        // Redirect to the index page.
                        return RedirectToPage("/AvailableData/Created/Networks/Index");
                    }
                    // Update the demonstration item IDs.
                    _configuration["Data:Demonstration:NetworkId"] = controlPath.Analysis.Network.Id;
                    _configuration["Data:Demonstration:AnalysisId"] = controlPath.Analysis.Id;
                    _configuration["Data:Demonstration:ControlPathId"] = controlPath.Id;
                }
                // Get the ID of the configured demonstration item.
                networkId = _configuration["Data:Demonstration:NetworkId"];
            }
            // Check if there was a network provided.
            if (!string.IsNullOrEmpty(networkId))
            {
                // Get the current user.
                var user = await _userManager.GetUserAsync(User);
                // Try to get the network with the provided ID.
                var networks = _context.Networks
                    .Where(item => item.IsPublic || item.NetworkUsers.Any(item1 => item1.Email == user.Email))
                    .Where(item => item.Id == networkId);
                // Check if there was an ID provided, but there was no network found.
                if (networks == null || !networks.Any())
                {
                    // Display a message.
                    TempData["StatusMessage"] = "Error: No network could be found with the provided ID, or you don't have access to it.";
                    // Redirect to the index page.
                    return RedirectToPage("/AvailableData/Created/Networks/Index");
                }
                // Check if the network found has any databases.
                if (networks.Select(item => item.NetworkDatabases).SelectMany(item => item).Any())
                {
                    // Redirect to the build page.
                    return RedirectToPage("/AvailableData/Created/Networks/Create/Build", new { networkId = networkId });
                }
                // Check if the network found does not have any databases.
                else
                {
                    // Redirect to the define page.
                    return RedirectToPage("/AvailableData/Created/Networks/Create/Define", new { networkId = networkId });
                }
            }
            // Return the page.
            return Page();
        }
    }
}
