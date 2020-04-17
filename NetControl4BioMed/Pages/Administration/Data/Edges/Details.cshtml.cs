using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office.CustomUI;
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

            public int DatabaseCount { get; set; }

            public int DatabaseEdgeFieldCount { get; set; }

            public int NodeCount { get; set; }

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
                return RedirectToPage("/Administration/Data/Edges/Index");
            }
            // Define the query.
            var query = _context.Edges
                .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                .Where(item => item.Id == id);
            // Define the view.
            View = new ViewModel
            {
                Edge = query
                    .FirstOrDefault(),
                DatabaseCount = query
                    .Select(item => item.DatabaseEdges)
                    .SelectMany(item => item)
                    .Select(item => item.Database)
                    .Distinct()
                    .Count(),
                DatabaseEdgeFieldCount = query
                    .Select(item => item.DatabaseEdgeFieldEdges)
                    .SelectMany(item => item)
                    .Select(item => item.DatabaseEdgeField)
                    .Distinct()
                    .Count(),
                NodeCount = query
                    .Select(item => item.EdgeNodes)
                    .SelectMany(item => item)
                    .Select(item => item.Node)
                    .Distinct()
                    .Count(),
                NetworkCount = query
                    .Select(item => item.NetworkEdges)
                    .SelectMany(item => item)
                    .Select(item => item.Network)
                    .Distinct()
                    .Count(),
                AnalysisCount = query
                    .Select(item => item.AnalysisEdges)
                    .SelectMany(item => item)
                    .Select(item => item.Analysis)
                    .Distinct()
                    .Count(),
                PathCount = query
                    .Select(item => item.PathEdges)
                    .SelectMany(item => item)
                    .Select(item => item.Path)
                    .Distinct()
                    .Count()
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
