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

namespace NetControl4BioMed.Pages.Administration.Content.Networks
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
        }

        public IActionResult OnGet(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Content/Networks/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Network = _context.Networks
                    .Where(item => item.Id == id)
                    .Include(item => item.NetworkUsers)
                    .Include(item => item.NetworkUserInvitations)
                    .Include(item => item.NetworkNodes)
                    .Include(item => item.NetworkEdges)
                    .Include(item => item.AnalysisNetworks)
                    .Include(item => item.NetworkDatabases)
                    .Include(item => item.NetworkNodeCollections)
                    .FirstOrDefault()
            };
            // Check if there was no item found.
            if (View.Network == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Content/Networks/Index");
            }
            // Return the page.
            return Page();
        }
    }
}
