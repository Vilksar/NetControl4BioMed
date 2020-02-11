using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;

namespace NetControl4BioMed.Pages.Administration.Content.NodeCollections
{
    [Authorize(Roles = "Administrator")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string Name { get; set; }

            [DataType(DataType.MultilineText)]
            [Required(ErrorMessage = "This field is required.")]
            public string Description { get; set; }
        }

        public IActionResult OnGet()
        {
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Check if the provided model isn't valid.
            if (!ModelState.IsValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error has been encountered. Please check again the input fields.");
                // Redisplay the page.
                return Page();
            }
            // Check if there is another node collection with the same name.
            if (_context.NodeCollections.Any(item => item.Name == Input.Name))
            {
                // Add an error to the model
                ModelState.AddModelError(string.Empty, $"A node collection with the name \"{Input.Name}\" already exists.");
                // Redisplay the page.
                return Page();
            }
            // Define the new node collection.
            var nodeCollection = new NodeCollection
            {
                Name = Input.Name,
                Description = Input.Description,
                DateTimeCreated = DateTime.Now,
            };
            // Mark it for addition.
            _context.NodeCollections.Add(nodeCollection);
            // Save the changes to the database.
            await _context.SaveChangesAsync();
            // Display a message.
            TempData["StatusMessage"] = "Success: 1 node collection created successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Content/NodeCollections/Index");
        }
    }
}
