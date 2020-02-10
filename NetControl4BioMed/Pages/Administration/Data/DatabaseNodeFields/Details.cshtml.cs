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

namespace NetControl4BioMed.Pages.Administration.Data.DatabaseNodeFields
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
            public DatabaseNodeField DatabaseNodeField { get; set; }
        }

        public IActionResult OnGet(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Data/DatabaseNodeFields/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                DatabaseNodeField = _context.DatabaseNodeFields
                    .Where(item => item.Id == id)
                    .Include(item => item.Database)
                        .ThenInclude(item => item.DatabaseType)
                    .Include(item => item.DatabaseNodeFieldNodes)
                        .ThenInclude(item => item.Node)
                    .FirstOrDefault()
            };
            // Check if there was no item found.
            if (View.DatabaseNodeField == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Data/DatabaseNodeFields/Index");
            }
            // Return the page.
            return Page();
        }
    }
}
