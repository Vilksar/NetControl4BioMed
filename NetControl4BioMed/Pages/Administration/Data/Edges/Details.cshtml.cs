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

namespace NetControl4BioMed.Pages.Administration.Data.Edges
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
            public Edge Edge { get; set; }
        }

        public IActionResult OnGet(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Data/Edges/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Edge = _context.Edges
                    .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => item.Id == id)
                    .Include(item => item.DatabaseEdges)
                        .ThenInclude(item => item.Database)
                    .Include(item => item.DatabaseEdgeFieldEdges)
                        .ThenInclude(item => item.DatabaseEdgeField)
                    .Include(item => item.EdgeNodes)
                        .ThenInclude(item => item.Node)
                    .Include(item => item.NetworkEdges)
                    .Include(item => item.AnalysisEdges)
                    .Include(item => item.PathEdges)
                    .FirstOrDefault()
            };
            // Check if there was no item found.
            if (View.Edge == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Data/Edges/Index");
            }
            // Return the page.
            return Page();
        }
    }
}
