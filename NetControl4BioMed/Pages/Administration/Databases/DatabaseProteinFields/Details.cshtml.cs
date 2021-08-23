using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace NetControl4BioMed.Pages.Administration.Databases.DatabaseProteinFields
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
            public DatabaseProteinField DatabaseProteinField { get; set; }

            public int DatabaseProteinFieldProteinCount { get; set; }

            public int ProteinCount { get; set; }
        }

        public IActionResult OnGet(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/DatabaseProteinFields/Index");
            }
            // Define the query.
            var query = _context.DatabaseProteinFields
                .Where(item => item.Id == id);
            // Define the view.
            View = new ViewModel
            {
                DatabaseProteinField = query
                    .Include(item => item.Database)
                    .FirstOrDefault(),
                DatabaseProteinFieldProteinCount = query
                    .Select(item => item.DatabaseProteinFieldProteins)
                    .SelectMany(item => item)
                    .Count(),
                ProteinCount = query
                    .Select(item => item.DatabaseProteinFieldProteins)
                    .SelectMany(item => item)
                    .Select(item => item.Protein)
                    .Distinct()
                    .Count()
            };
            // Check if there was no item found.
            if (View.DatabaseProteinField == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/DatabaseProteinFields/Index");
            }
            // Return the page.
            return Page();
        }
    }
}
