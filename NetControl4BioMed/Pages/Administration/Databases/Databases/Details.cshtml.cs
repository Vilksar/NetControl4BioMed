using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Administration.Databases.Databases
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
            public Database Database { get; set; }

            public int DatabaseNodeCount { get; set; }

            public int DatabaseEdgeCount { get; set; }

            public int NodeCollectionDatabaseCount { get; set; }

            public int NetworkDatabaseCount { get; set; }

            public int AnalysisDatabaseCount { get; set; }
        }

        public IActionResult OnGet(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/Databases/Index");
            }
            // Define the query.
            var query = _context.Databases
                .Where(item => item.Id == id);
            // Define the view.
            View = new ViewModel
            {
                Database = query
                    .Include(item => item.DatabaseType)
                    .Include(item => item.DatabaseUsers)
                        .ThenInclude(item => item.User)
                    .Include(item => item.DatabaseUserInvitations)
                    .Include(item => item.DatabaseNodeFields)
                    .Include(item => item.DatabaseEdgeFields)
                    .FirstOrDefault(),
                DatabaseNodeCount = query
                    .Select(item => item.DatabaseNodes)
                    .SelectMany(item => item)
                    .Count(),
                DatabaseEdgeCount = query
                    .Select(item => item.DatabaseEdges)
                    .SelectMany(item => item)
                    .Count(),
                NodeCollectionDatabaseCount = query
                    .Select(item => item.NodeCollectionDatabases)
                    .SelectMany(item => item)
                    .Count(),
                NetworkDatabaseCount = query
                    .Select(item => item.NetworkDatabases)
                    .SelectMany(item => item)
                    .Count(),
                AnalysisDatabaseCount = query
                    .Select(item => item.AnalysisDatabases)
                    .SelectMany(item => item)
                    .Count(),
            };
            // Check if there was no item found.
            if (View.Database == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/Databases/Index");
            }
            // Return the page.
            return Page();
        }
    }
}
