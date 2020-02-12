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

namespace NetControl4BioMed.Pages.Administration.Content.Nodes
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
        }

        public IActionResult OnGet(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Content/Nodes/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Node = _context.Nodes
                    .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => item.Id == id)
                    .Include(item => item.DatabaseNodes)
                    .Include(item => item.DatabaseNodeFieldNodes)
                    .Include(item => item.EdgeNodes)
                    .Include(item => item.NetworkNodes)
                    .Include(item => item.AnalysisNodes)
                    .Include(item => item.PathNodes)
                    .Include(item => item.NodeCollectionNodes)
                    .FirstOrDefault()
            };
            // Check if there was no item found.
            if (View.Node == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Content/Nodes/Index");
            }
            // Return the page.
            return Page();
        }
    }
}
