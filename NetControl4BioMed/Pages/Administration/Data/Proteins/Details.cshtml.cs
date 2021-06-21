using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace NetControl4BioMed.Pages.Administration.Data.Proteins
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
            public Protein Protein { get; set; }

            public int DatabaseCount { get; set; }

            public int DatabaseProteinFieldCount { get; set; }

            public int InteractionCount { get; set; }

            public int ProteinCollectionCount { get; set; }

            public int NetworkCount { get; set; }

            public int AnalysisCount { get; set; }

            public int PathCount { get; set; }
        }

        public IActionResult OnGet(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Data/Proteins/Index");
            }
            // Define the query.
            var query = _context.Proteins
                .Where(item => item.DatabaseProteins.Any())
                .Where(item => item.Id == id);
            // Define the view.
            View = new ViewModel
            {
                Protein = query
                    .FirstOrDefault(),
                DatabaseCount = query
                    .Select(item => item.DatabaseProteins)
                    .SelectMany(item => item)
                    .Select(item => item.Database)
                    .Distinct()
                    .Count(),
                DatabaseProteinFieldCount = query
                    .Select(item => item.DatabaseProteinFieldProteins)
                    .SelectMany(item => item)
                    .Select(item => item.DatabaseProteinField)
                    .Distinct()
                    .Count(),
                InteractionCount = query
                    .Select(item => item.InteractionProteins)
                    .SelectMany(item => item)
                    .Select(item => item.Interaction)
                    .Distinct()
                    .Count(),
                ProteinCollectionCount = query
                    .Select(item => item.ProteinCollectionProteins)
                    .SelectMany(item => item)
                    .Select(item => item.ProteinCollection)
                    .Distinct()
                    .Count(),
                NetworkCount = query
                    .Select(item => item.NetworkProteins)
                    .SelectMany(item => item)
                    .Select(item => item.Network)
                    .Distinct()
                    .Count(),
                AnalysisCount = query
                    .Select(item => item.AnalysisProteins)
                    .SelectMany(item => item)
                    .Select(item => item.Analysis)
                    .Distinct()
                    .Count(),
                PathCount = query
                    .Select(item => item.PathProteins)
                    .SelectMany(item => item)
                    .Select(item => item.Path)
                    .Distinct()
                    .Count()
            };
            // Check if there was no item found.
            if (View.Protein == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Data/Proteins/Index");
            }
            // Return the page.
            return Page();
        }
    }
}
