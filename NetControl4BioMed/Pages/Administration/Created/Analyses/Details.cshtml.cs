using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace NetControl4BioMed.Pages.Administration.Created.Analyses
{
    [Authorize(Roles = "Administrator")]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public Analysis Analysis { get; set; }

            public int UserCount { get; set; }

            public int DatabaseCount { get; set; }

            public int ProteinCount { get; set; }

            public int InteractionCount { get; set; }

            public int ProteinCollectionCount { get; set; }
        }

        public IActionResult OnGet(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Created/Analyses/Index");
            }
            // Define the query.
            var query = _context.Analyses
                .Where(item => item.Id == id);
            // Define the view.
            View = new ViewModel
            {
                Analysis = query
                    .FirstOrDefault(),
                UserCount = query
                    .Select(item => item.AnalysisUsers)
                    .SelectMany(item => item)
                    .Select(item => item.User)
                    .Distinct()
                    .Count(),
                DatabaseCount = query
                    .Select(item => item.AnalysisDatabases)
                    .SelectMany(item => item)
                    .Select(item => item.Database)
                    .Distinct()
                    .Count(),
                ProteinCount = query
                    .Select(item => item.AnalysisProteins)
                    .SelectMany(item => item)
                    .Select(item => item.Protein)
                    .Distinct()
                    .Count(),
                InteractionCount = query
                    .Select(item => item.AnalysisInteractions)
                    .SelectMany(item => item)
                    .Select(item => item.Interaction)
                    .Distinct()
                    .Count(),
                ProteinCollectionCount = query
                    .Select(item => item.AnalysisProteinCollections)
                    .SelectMany(item => item)
                    .Select(item => item.ProteinCollection)
                    .Distinct()
                    .Count(),
            };
            // Check if there was no item found.
            if (View.Analysis == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Created/Analyses/Index");
            }
            // Return the page.
            return Page();
        }
    }
}
