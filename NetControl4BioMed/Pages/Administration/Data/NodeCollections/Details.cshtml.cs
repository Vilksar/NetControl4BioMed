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

namespace NetControl4BioMed.Pages.Administration.Data.NodeCollections
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
            public NodeCollection NodeCollection { get; set; }

            public int TypeCount { get; set; }

            public int DatabaseCount { get; set; }

            public int NodeCount { get; set; }

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
                return RedirectToPage("/Administration/Data/NodeCollections/Index");
            }
            // Define the query.
            var query = _context.NodeCollections
                .Where(item => item.Id == id);
            // Define the view.
            View = new ViewModel
            {
                NodeCollection = query
                    .FirstOrDefault(),
                TypeCount = query
                    .Select(item => item.NodeCollectionTypes)
                    .Distinct()
                    .Count(),
                DatabaseCount = query
                    .Select(item => item.NodeCollectionDatabases)
                    .SelectMany(item => item)
                    .Select(item => item.Database)
                    .Distinct()
                    .Count(),
                NodeCount = query
                    .Select(item => item.NodeCollectionNodes)
                    .SelectMany(item => item)
                    .Select(item => item.Node)
                    .Distinct()
                    .Count(),
                NetworkCount = query
                    .Select(item => item.NetworkNodeCollections)
                    .SelectMany(item => item)
                    .Select(item => item.Network)
                    .Distinct()
                    .Count(),
                AnalysisCount = query
                    .Select(item => item.AnalysisNodeCollections)
                    .SelectMany(item => item)
                    .Select(item => item.Analysis)
                    .Distinct()
                    .Count()
            };
            // Check if there was no item found.
            if (View.NodeCollection == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Data/NodeCollections/Index");
            }
            // Return the page.
            return Page();
        }
    }
}
