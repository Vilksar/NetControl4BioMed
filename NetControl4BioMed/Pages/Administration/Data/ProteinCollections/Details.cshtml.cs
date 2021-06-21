using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace NetControl4BioMed.Pages.Administration.Data.ProteinCollections
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
            public ProteinCollection ProteinCollection { get; set; }

            public int TypeCount { get; set; }

            public int ProteinCount { get; set; }

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
                return RedirectToPage("/Administration/Data/ProteinCollections/Index");
            }
            // Define the query.
            var query = _context.ProteinCollections
                .Where(item => item.Id == id);
            // Define the view.
            View = new ViewModel
            {
                ProteinCollection = query
                    .FirstOrDefault(),
                TypeCount = query
                    .Select(item => item.ProteinCollectionTypes)
                    .SelectMany(item => item)
                    .Select(item => item.Type)
                    .Distinct()
                    .Count(),
                ProteinCount = query
                    .Select(item => item.ProteinCollectionProteins)
                    .SelectMany(item => item)
                    .Select(item => item.Protein)
                    .Distinct()
                    .Count(),
                NetworkCount = query
                    .Select(item => item.NetworkProteinCollections)
                    .SelectMany(item => item)
                    .Select(item => item.Network)
                    .Distinct()
                    .Count(),
                AnalysisCount = query
                    .Select(item => item.AnalysisProteinCollections)
                    .SelectMany(item => item)
                    .Select(item => item.Analysis)
                    .Distinct()
                    .Count()
            };
            // Check if there was no item found.
            if (View.ProteinCollection == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Data/ProteinCollections/Index");
            }
            // Return the page.
            return Page();
        }
    }
}
