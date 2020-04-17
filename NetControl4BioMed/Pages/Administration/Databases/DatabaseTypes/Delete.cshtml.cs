using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Administration.Databases.DatabaseTypes
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
            public IQueryable<DatabaseType> Items { get; set; }
        }

        public IActionResult OnGet(IEnumerable<string> ids)
        {
            // Check if there aren't any IDs provided.
            if (ids == null || !ids.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/DatabaseTypes/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Items = _context.DatabaseTypes
                    .Where(item => ids.Contains(item.Id))
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No items have been found with the provided IDs.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/DatabaseTypes/Index");
            }
            // Check if the generic database type is among the items to be deleted.
            if (View.Items.Any(item => item.Name == "Generic"))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The \"Generic\" database type can't be deleted.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/DatabaseTypes/Index");
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
                return RedirectToPage("/Administration/Databases/DatabaseTypes/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Items = _context.DatabaseTypes
                    .Where(item => Input.Ids.Contains(item.Id))
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No items have been found with the provided IDs.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/DatabaseTypes/Index");
            }
            // Check if the generic database type is among the items to be deleted.
            if (View.Items.Any(item => item.Name == "Generic"))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The \"Generic\" database type can't be deleted.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/DatabaseTypes/Index");
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
            var databaseTypeCount = View.Items.Count();
            // Get the related entities that use the items.
            var nodes = _context.Nodes
                .Where(item => item.DatabaseNodes.Any(item1 => View.Items.Contains(item1.Database.DatabaseType)));
            var edges = _context.Edges
                .Where(item => item.DatabaseEdges.Any(item1 => View.Items.Contains(item1.Database.DatabaseType)));
            var networks = _context.Networks
                .Where(item => item.NetworkDatabases.Any(item1 => View.Items.Contains(item1.Database.DatabaseType)));
            var analyses = _context.Analyses
                .Where(item => item.AnalysisDatabases.Any(item1 => View.Items.Contains(item1.Database.DatabaseType)));
            // Mark the items for deletion.
            _context.Analyses.RemoveRange(analyses);
            _context.Networks.RemoveRange(networks);
            _context.Edges.RemoveRange(edges);
            _context.Nodes.RemoveRange(nodes);
            _context.DatabaseTypes.RemoveRange(View.Items);
            // Save the changes to the database.
            await _context.SaveChangesAsync();
            // Display a message.
            TempData["StatusMessage"] = $"Success: {databaseTypeCount.ToString()} database type{(databaseTypeCount != 1 ? "s" : string.Empty)} deleted successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Databases/DatabaseTypes/Index");
        }
    }
}
