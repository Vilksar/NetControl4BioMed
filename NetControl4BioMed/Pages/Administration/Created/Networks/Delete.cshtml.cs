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
            public IQueryable<Network> Items { get; set; }
        }

        public IActionResult OnGet(IEnumerable<string> ids)
        {
            // Check if there aren't any IDs provided.
            if (ids == null || !ids.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Created/Networks/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Items = _context.Networks
                    .Where(item => ids.Contains(item.Id))
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No items have been found with the provided IDs.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Created/Networks/Index");
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
                return RedirectToPage("/Administration/Created/Networks/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Items = _context.Networks
                    .Where(item => Input.Ids.Contains(item.Id))
                    .Include(item => item.NetworkDatabases)
                        .ThenInclude(item => item.Database)
                            .ThenInclude(item => item.DatabaseType)
                    .Include(item => item.NetworkNodes)
                        .ThenInclude(item => item.Node)
                    .Include(item => item.NetworkEdges)
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No items have been found with the provided IDs.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Created/Networks/Index");
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
            var networkCount = View.Items.Count();
            // Get the related entities that use the items.
            var analyses = _context.Analyses.Where(item => item.AnalysisNetworks.Any(item1 => View.Items.Contains(item1.Network)));
            // Get the generic entities among them.
            var genericNetworks = View.Items
                .Where(item => item.NetworkDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"));
            var genericNodes = _context.Nodes
                .Where(item => item.NetworkNodes.Any(item1 => genericNetworks.Contains(item1.Network)));
            var genericEdges = _context.Edges
                .Where(item => item.NetworkEdges.Any(item1 => genericNetworks.Contains(item1.Network)) || item.EdgeNodes.Any(item1 => genericNodes.Contains(item1.Node)));
            // Mark the items for deletion.
            _context.Analyses.RemoveRange(analyses);
            _context.Networks.RemoveRange(View.Items);
            _context.Edges.RemoveRange(genericEdges);
            _context.Nodes.RemoveRange(genericNodes);
            // Save the changes to the database.
            await _context.SaveChangesAsync();
            // Display a message.
            TempData["StatusMessage"] = $"Success: {networkCount.ToString()} network{(networkCount != 1 ? "s" : string.Empty)} deleted successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Created/Networks/Index");
        }
    }
}
