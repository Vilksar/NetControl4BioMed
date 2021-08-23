using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using System.Linq;

namespace NetControl4BioMed.Pages.Administration.Other.BackgroundTasks
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
            public BackgroundTask BackgroundTask { get; set; }
        }

        public IActionResult OnGet(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Other/BackgroundTasks/Index");
            }
            // Define the query.
            var query = _context.BackgroundTasks
                .Where(item => item.Id == id);
            // Define the view.
            View = new ViewModel
            {
                BackgroundTask = query
                    .FirstOrDefault()
            };
            // Check if there was no item found.
            if (View.BackgroundTask == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Other/BackgroundTasks/Index");
            }
            // Return the page.
            return Page();
        }
    }
}
