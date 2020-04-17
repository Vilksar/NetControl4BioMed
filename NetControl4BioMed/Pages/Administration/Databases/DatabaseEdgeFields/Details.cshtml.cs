using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Administration.Databases.DatabaseEdgeFields
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
            public DatabaseEdgeField DatabaseEdgeField { get; set; }

            public int DatabaseEdgeFieldEdgeCount { get; set; }

            public int EdgeCount { get; set; }
        }

        public IActionResult OnGet(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/DatabaseEdgeFields/Index");
            }
            // Define the query.
            var query = _context.DatabaseEdgeFields
                .Where(item => item.Id == id);
            // Define the view.
            View = new ViewModel
            {
                DatabaseEdgeField = query
                    .Include(item => item.Database)
                        .ThenInclude(item => item.DatabaseType)
                    .FirstOrDefault(),
                DatabaseEdgeFieldEdgeCount = query
                    .Select(item => item.DatabaseEdgeFieldEdges)
                    .SelectMany(item => item)
                    .Count(),
                EdgeCount = query
                    .Select(item => item.DatabaseEdgeFieldEdges)
                    .SelectMany(item => item)
                    .Select(item => item.Edge)
                    .Distinct()
                    .Count()
            };
            // Check if there was no item found.
            if (View.DatabaseEdgeField == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/DatabaseEdgeFields/Index");
            }
            // Return the page.
            return Page();
        }
    }
}
