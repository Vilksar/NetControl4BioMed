using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Administration.Data.DatabaseNodeFields
{
    [Authorize(Roles = "Administrator")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public IEnumerable<string> Ids { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public IEnumerable<DatabaseNodeField> Items { get; set; }
        }

        public IActionResult OnGet(IEnumerable<string> ids)
        {
            // Check if there aren't any IDs provided.
            if (ids == null || !ids.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Data/DatabaseNodeFields/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Items = _context.DatabaseNodeFields.Where(item => ids.Contains(item.Id))
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No items have been found with the provided IDs.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Data/DatabaseNodeFields/Index");
            }
            // Check if any database node fields of the generic database are among the items to be deleted.
            if (View.Items.Any(item => item.Database.Name == "Generic"))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The database node fields of the \"Generic\" database can't be deleted.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Data/DatabaseNodeFields/Index");
            }
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Check if there aren't any IDs provided.
            if (Input.Ids == null || !Input.Ids.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Data/DatabaseNodeFields/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Items = _context.DatabaseNodeFields.Where(item => Input.Ids.Contains(item.Id))
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No items have been found with the provided IDs.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Data/DatabaseNodeFields/Index");
            }
            // Check if any database node fields of the generic database are among the items to be deleted.
            if (View.Items.Any(item => item.Database.Name == "Generic"))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The database node fields of the \"Generic\" database can't be deleted.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Data/DatabaseNodeFields/Index");
            }
            // Check if the provided model isn't valid.
            if (!ModelState.IsValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error has been encountered. Please check again the input fields.");
                // Redisplay the page.
                return Page();
            }
            // Save the number of items found.
            var databaseNodeFieldCount = View.Items.Count();
            // Mark the items for deletion.
            _context.DatabaseNodeFields.RemoveRange(View.Items);
            // Save the changes to the database.
            await _context.SaveChangesAsync();
            // Display a message.
            TempData["StatusMessage"] = $"Success: {databaseNodeFieldCount.ToString()} database node field{(databaseNodeFieldCount != 1 ? "s" : string.Empty)} deleted successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Data/DatabaseNodeFields/Index");
        }
    }
}
