using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace NetControl4BioMed.Pages.Administration.Databases.Databases
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
            public Database Database { get; set; }

            public int UserCount { get; set; }

            public int DatabaseProteinFieldCount { get; set; }

            public int DatabaseInteractionFieldCount { get; set; }

            public int ProteinCount { get; set; }

            public int InteractionCount { get; set; }

            public int NetworkCount { get; set; }

            public int AnalysisCount { get; set; }
        }

        public IActionResult OnGet(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/Databases/Index");
            }
            // Define the query.
            var query = _context.Databases
                .Where(item => item.Id == id);
            // Define the view.
            View = new ViewModel
            {
                Database = query
                    .FirstOrDefault(),
                UserCount = query
                    .Select(item => item.DatabaseUsers)
                    .SelectMany(item => item)
                    .Select(item => item.User)
                    .Distinct()
                    .Count(),
                DatabaseProteinFieldCount = query
                    .Select(item => item.DatabaseProteinFields)
                    .SelectMany(item => item)
                    .Distinct()
                    .Count(),
                DatabaseInteractionFieldCount = query
                    .Select(item => item.DatabaseInteractionFields)
                    .SelectMany(item => item)
                    .Distinct()
                    .Count(),
                ProteinCount = query
                    .Select(item => item.DatabaseProteins)
                    .SelectMany(item => item)
                    .Select(item => item.Protein)
                    .Distinct()
                    .Count(),
                InteractionCount = query
                    .Select(item => item.DatabaseInteractions)
                    .SelectMany(item => item)
                    .Select(item => item.Interaction)
                    .Distinct()
                    .Count(),
                NetworkCount = query
                    .Select(item => item.NetworkDatabases)
                    .SelectMany(item => item)
                    .Select(item => item.Network)
                    .Distinct()
                    .Count(),
                AnalysisCount = query
                    .Select(item => item.AnalysisDatabases)
                    .SelectMany(item => item)
                    .Select(item => item.Analysis)
                    .Distinct()
                    .Count()
            };
            // Check if there was no item found.
            if (View.Database == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/Databases/Index");
            }
            // Return the page.
            return Page();
        }
    }
}
