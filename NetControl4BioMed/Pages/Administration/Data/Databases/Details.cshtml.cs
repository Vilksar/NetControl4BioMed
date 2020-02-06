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

namespace NetControl4BioMed.Pages.Administration.Data.Databases
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
        }

        public IActionResult OnGet(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Data/Databases/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Database = _context.Databases
                    .Where(item => item.Id == id)
                    .Include(item => item.DatabaseType)
                    .Include(item => item.DatabaseNodeFields)
                    .Include(item => item.DatabaseEdgeFields)
                    .Include(item => item.DatabaseNodes)
                    .Include(item => item.DatabaseEdges)
                    .Include(item => item.DatabaseUsers)
                    .FirstOrDefault()
            };
            // Check if there was no item found.
            if (View.Database == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Data/Databases/Index");
            }
            // Return the page.
            return Page();
        }
    }
}
