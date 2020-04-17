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

            public int UserCount { get; set; }

            public int UserInvitationCount { get; set; }

            public int DatabaseNodeFieldCount { get; set; }

            public int DatabaseEdgeFieldCount { get; set; }

            public int NodeCount { get; set; }

            public int EdgeCount { get; set; }

            public int NodeCollectionCount { get; set; }

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
                    .FirstOrDefault(),
                UserCount = query
                    .Select(item => item.DatabaseUsers)
                    .SelectMany(item => item)
                    .Select(item => item.User)
                    .Distinct()
                    .Count(),
                UserInvitationCount = query
                    .Select(item => item.DatabaseUserInvitations)
                    .SelectMany(item => item)
                    .Select(item => item.Email)
                    .Distinct()
                    .Count(),
                DatabaseNodeFieldCount = query
                    .Select(item => item.DatabaseNodeFields)
                    .SelectMany(item => item)
                    .Distinct()
                    .Count(),
                DatabaseEdgeFieldCount = query
                    .Select(item => item.DatabaseEdgeFields)
                    .SelectMany(item => item)
                    .Distinct()
                    .Count(),
                NodeCount = query
                    .Select(item => item.DatabaseNodes)
                    .SelectMany(item => item)
                    .Select(item => item.Node)
                    .Distinct()
                    .Count(),
                EdgeCount = query
                    .Select(item => item.DatabaseEdges)
                    .SelectMany(item => item)
                    .Select(item => item.Edge)
                    .Distinct()
                    .Count(),
                NodeCollectionCount = query
                    .Select(item => item.NodeCollectionDatabases)
                    .SelectMany(item => item)
                    .Select(item => item.NodeCollection)
                    .Distinct()
                    .Count(),
                NetworkCount = query
                    .Select(item => item.NetworkDatabases)
                    .SelectMany(item => item)
                    .Select(item => item.Network)
                    .Distinct()
                    .Count(),
                AnalysisCount = query
                    .Select(item => item.AnalysisDatabases)
                    .SelectMany(item => item)
                    .Select(item => item.Analysis)
                    .Distinct()
                    .Count()
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
