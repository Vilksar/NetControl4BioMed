using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace NetControl4BioMed.Pages.Administration.Databases.DatabaseInteractionFields
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
            public DatabaseInteractionField DatabaseInteractionField { get; set; }

            public int DatabaseInteractionFieldInteractionCount { get; set; }

            public int InteractionCount { get; set; }
        }

        public IActionResult OnGet(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/DatabaseInteractionFields/Index");
            }
            // Define the query.
            var query = _context.DatabaseInteractionFields
                .Where(item => item.Id == id);
            // Define the view.
            View = new ViewModel
            {
                DatabaseInteractionField = query
                    .Include(item => item.Database)
                    .FirstOrDefault(),
                DatabaseInteractionFieldInteractionCount = query
                    .Select(item => item.DatabaseInteractionFieldInteractions)
                    .SelectMany(item => item)
                    .Count(),
                InteractionCount = query
                    .Select(item => item.DatabaseInteractionFieldInteractions)
                    .SelectMany(item => item)
                    .Select(item => item.Interaction)
                    .Distinct()
                    .Count()
            };
            // Check if there was no item found.
            if (View.DatabaseInteractionField == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/DatabaseInteractionFields/Index");
            }
            // Return the page.
            return Page();
        }
    }
}
