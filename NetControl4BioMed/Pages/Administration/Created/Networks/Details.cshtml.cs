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

namespace NetControl4BioMed.Pages.Administration.Created.Networks
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
            public Network Network { get; set; }

            public int UserCount { get; set; }

            public int UserInvitationCount { get; set; }

            public int DatabaseCount { get; set; }

            public int NodeCount { get; set; }

            public int EdgeCount { get; set; }

            public int NodeCollectionCount { get; set; }

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
                return RedirectToPage("/Administration/Created/Networks/Index");
            }
            // Define the query.
            var query = _context.Networks
                .Where(item => item.Id == id);
            // Define the view.
            View = new ViewModel
            {
                Network = query
                    .FirstOrDefault(),
                UserCount = query
                    .Select(item => item.NetworkUsers)
                    .SelectMany(item => item)
                    .Select(item => item.User)
                    .Distinct()
                    .Count(),
                UserInvitationCount = query
                    .Select(item => item.NetworkUserInvitations)
                    .SelectMany(item => item)
                    .Select(item => item.Email)
                    .Distinct()
                    .Count(),
                DatabaseCount = query
                    .Select(item => item.NetworkDatabases)
                    .SelectMany(item => item)
                    .Select(item => item.Database)
                    .Distinct()
                    .Count(),
                NodeCount = query
                    .Select(item => item.NetworkNodes)
                    .SelectMany(item => item)
                    .Select(item => item.Node)
                    .Distinct()
                    .Count(),
                EdgeCount = query
                    .Select(item => item.NetworkEdges)
                    .SelectMany(item => item)
                    .Select(item => item.Edge)
                    .Distinct()
                    .Count(),
                NodeCollectionCount = query
                    .Select(item => item.NetworkNodeCollections)
                    .SelectMany(item => item)
                    .Select(item => item.NodeCollection)
                    .Distinct()
                    .Count(),
                AnalysisCount = query
                    .Select(item => item.AnalysisNetworks)
                    .SelectMany(item => item)
                    .Select(item => item.Analysis)
                    .Distinct()
                    .Count()
            };
            // Check if there was no item found.
            if (View.Network == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Created/Networks/Index");
            }
            // Return the page.
            return Page();
        }
    }
}
