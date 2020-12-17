using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Interfaces;
using NetworkItemModel = NetControl4BioMed.Pages.Content.DatabaseTypes.Default.Created.Networks.IndexModel.ItemModel;
using AnalysisItemModel = NetControl4BioMed.Pages.Content.DatabaseTypes.Default.Created.Analyses.IndexModel.ItemModel;

namespace NetControl4BioMed.Pages.Content.DatabaseTypes.Default
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
            public bool IsUserAuthenticated { get; set; }

            public Dictionary<string, int?> ItemCount { get; set; }

            public IEnumerable<NetworkItemModel> RecentNetworks { get; set; }

            public IEnumerable<AnalysisItemModel> RecentAnalyses { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Define the view.
            View = new ViewModel
            {
                IsUserAuthenticated = user != null,
                ItemCount = new Dictionary<string, int?>(),
                RecentNetworks = Enumerable.Empty<NetworkItemModel>(),
                RecentAnalyses = Enumerable.Empty<AnalysisItemModel>()
            };
            // Check if the user is authenticated.
            if (View.IsUserAuthenticated)
            {
                // Update the view.
                View.ItemCount["Networks"] = _context.Networks
                    .Count(item => item.NetworkUsers.Any(item1 => item1.User == user));
                View.ItemCount["Analyses"] = _context.Analyses
                    .Count(item => item.AnalysisUsers.Any(item1 => item1.User == user));
                View.RecentNetworks = _context.Networks
                    .Where(item => item.NetworkUsers.Any(item1 => item1.User == user))
                    .OrderByDescending(item => item.DateTimeCreated)
                    .Take(5)
                    .Select(item => new NetworkItemModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Status = item.Status
                    });
                View.RecentAnalyses = _context.Analyses
                    .Where(item => item.AnalysisUsers.Any(item1 => item1.User == user))
                    .OrderByDescending(item => item.DateTimeStarted)
                    .Take(5)
                    .Select(item => new AnalysisItemModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Status = item.Status
                    });
            }
            // Return the page.
            return Page();
        }
    }
}
