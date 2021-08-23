using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace NetControl4BioMed.Pages.Administration.Data.Interactions
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
            public Interaction Interaction { get; set; }

            public int DatabaseCount { get; set; }

            public int DatabaseInteractionFieldCount { get; set; }

            public int ProteinCount { get; set; }

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
                return RedirectToPage("/Administration/Data/Interactions/Index");
            }
            // Define the query.
            var query = _context.Interactions
                .Where(item => item.DatabaseInteractions.Any())
                .Where(item => item.Id == id);
            // Define the view.
            View = new ViewModel
            {
                Interaction = query
                    .FirstOrDefault(),
                DatabaseCount = query
                    .Select(item => item.DatabaseInteractions)
                    .SelectMany(item => item)
                    .Select(item => item.Database)
                    .Distinct()
                    .Count(),
                DatabaseInteractionFieldCount = query
                    .Select(item => item.DatabaseInteractionFieldInteractions)
                    .SelectMany(item => item)
                    .Select(item => item.DatabaseInteractionField)
                    .Distinct()
                    .Count(),
                ProteinCount = query
                    .Select(item => item.InteractionProteins)
                    .SelectMany(item => item)
                    .Select(item => item.Protein)
                    .Distinct()
                    .Count(),
                NetworkCount = query
                    .Select(item => item.NetworkInteractions)
                    .SelectMany(item => item)
                    .Select(item => item.Network)
                    .Distinct()
                    .Count(),
                AnalysisCount = query
                    .Select(item => item.AnalysisInteractions)
                    .SelectMany(item => item)
                    .Select(item => item.Analysis)
                    .Distinct()
                    .Count(),
                PathCount = query
                    .Select(item => item.PathInteractions)
                    .SelectMany(item => item)
                    .Select(item => item.Path)
                    .Distinct()
                    .Count()
            };
            // Check if there was no item found.
            if (View.Interaction == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Data/Interactions/Index");
            }
            // Return the page.
            return Page();
        }
    }
}
