using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.PrivateData
{
    [Authorize]
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
            public Dictionary<string, int?> ItemCount { get; set; }

            public IEnumerable<Networks.IndexModel.ItemModel> RecentNetworks { get; set; }

            public IEnumerable<Analyses.IndexModel.ItemModel> RecentAnalyses { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Get all of the networks and analyses.
            var networks = _context.Networks
                .Where(item => item.NetworkUsers.Any(item1 => item1.Email == user.Email));
            var analyses = _context.Analyses
                .Where(item => item.AnalysisUsers.Any(item1 => item1.Email == user.Email));
            // Define the view.
            View = new ViewModel
            {
                ItemCount = new Dictionary<string, int?>
                {
                    { "Networks", networks.Count() },
                    { "Analyses", analyses.Count() }
                },
                RecentNetworks = networks
                    .OrderByDescending(item => item.DateTimeCreated)
                    .Take(5)
                    .Select(item => new Networks.IndexModel.ItemModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Status = item.Status
                    }),
                RecentAnalyses = analyses
                    .OrderByDescending(item => item.DateTimeCreated)
                    .Take(5)
                    .Select(item => new Analyses.IndexModel.ItemModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Status = item.Status
                    })
            };
            // Return the page.
            return Page();
        }
    }
}
