using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetControl4BioMed.Pages.PublicData
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public IndexModel(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public Dictionary<string, int?> ItemCount { get; set; }

            public Network DemonstrationNetwork { get; set; }

            public Analysis DemonstrationAnalysis { get; set; }

            public IEnumerable<Networks.IndexModel.ItemModel> RecentNetworks { get; set; }

            public IEnumerable<Analyses.IndexModel.ItemModel> RecentAnalyses { get; set; }
        }

        public IActionResult OnGet()
        {
            // Get the data from configuration.
            var publicItemCount = _configuration
                .GetSection("Data")
                .GetSection("ItemCount")
                .GetSection("Public")
                .GetChildren()
                .ToDictionary(item => item.Key, item => int.TryParse(item.Value, out var result) ? (int?)result : null);
            // Get a random demonstration analysis.
            var randomDemonstrationAnalysis = _context.Analyses
                .Include(item => item.Network)
                .Where(item => item.IsDemonstration)
                .OrderBy(item => Guid.NewGuid())
                .FirstOrDefault();
            // Define the view.
            View = new ViewModel
            {
                ItemCount = publicItemCount,
                DemonstrationNetwork = randomDemonstrationAnalysis?.Network,
                DemonstrationAnalysis = randomDemonstrationAnalysis,
                RecentNetworks = _context.Networks
                    .Where(item => item.IsPublic)
                    .OrderByDescending(item => item.DateTimeCreated)
                    .Take(5)
                    .Select(item => new Networks.IndexModel.ItemModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Status = item.Status
                    }),
                RecentAnalyses = _context.Analyses
                    .Where(item => item.IsPublic)
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
