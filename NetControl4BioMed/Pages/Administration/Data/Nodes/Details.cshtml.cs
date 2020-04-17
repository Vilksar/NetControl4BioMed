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

namespace NetControl4BioMed.Pages.Administration.Data.Nodes
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
            public Node Node { get; set; }

            public int DatabaseCount { get; set; }

            public int DatabaseNodeFieldCount { get; set; }

            public int EdgeCount { get; set; }

            public int NodeCollectionCount { get; set; }

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
                return RedirectToPage("/Administration/Data/Nodes/Index");
            }
            // Define the query.
            var query = _context.Nodes
                .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                .Where(item => item.Id == id);
            // Define the view.
            View = new ViewModel
            {
                Node = query
                    .FirstOrDefault(),
                DatabaseCount = query
                    .Select(item => item.DatabaseNodes)
                    .SelectMany(item => item)
                    .Select(item => item.Database)
                    .Distinct()
                    .Count(),
                DatabaseNodeFieldCount = query
                    .Select(item => item.DatabaseNodeFieldNodes)
                    .SelectMany(item => item)
                    .Select(item => item.DatabaseNodeField)
                    .Distinct()
                    .Count(),
                EdgeCount = query
                    .Select(item => item.EdgeNodes)
                    .SelectMany(item => item)
                    .Select(item => item.Edge)
                    .Distinct()
                    .Count(),
                NodeCollectionCount = query
                    .Select(item => item.NodeCollectionNodes)
                    .SelectMany(item => item)
                    .Select(item => item.NodeCollection)
                    .Distinct()
                    .Count(),
                NetworkCount = query
                    .Select(item => item.NetworkNodes)
                    .SelectMany(item => item)
                    .Select(item => item.Network)
                    .Distinct()
                    .Count(),
                AnalysisCount = query
                    .Select(item => item.AnalysisNodes)
                    .SelectMany(item => item)
                    .Select(item => item.Analysis)
                    .Distinct()
                    .Count(),
                PathCount = query
                    .Select(item => item.PathNodes)
                    .SelectMany(item => item)
                    .Select(item => item.Path)
                    .Distinct()
                    .Count()
            };
            // Check if there was no item found.
            if (View.Node == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Data/Nodes/Index");
            }
            // Return the page.
            return Page();
        }
    }
}
