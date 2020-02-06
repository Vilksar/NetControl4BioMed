using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Administration.Data.Databases
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
            public string Description { get; set; }

            [DataType(DataType.Url)]
            public string Url { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public bool IsPublic { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string DatabaseTypeId { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public IEnumerable<DatabaseType> DatabaseTypes { get; set; }
        }

        public IActionResult OnGet(string databaseTypeId = null)
        {
            // Define the view.
            View = new ViewModel
            {
                DatabaseTypes = _context.DatabaseTypes
                    .Where(item => item.Name != "Generic")
                    .AsEnumerable()
            };
            // Check if there aren't any database types.
            if (!View.DatabaseTypes.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No non-generic database types could be found. Please create a database type first.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Data/Databases/Index");
            }
            // Define the input.
            Input = new InputModel
            {
                DatabaseTypeId = databaseTypeId
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Define the view.
            View = new ViewModel
            {
                DatabaseTypes = _context.DatabaseTypes
                    .Where(item => item.Name != "Generic")
                    .AsEnumerable()
            };
            // Check if the provided model isn't valid.
            if (!ModelState.IsValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error has been encountered. Please check again the input fields.");
                // Redisplay the page.
                return Page();
            }
            // Check if there is another database type with the same name.
            if (_context.Databases.Any(item => item.Name == Input.Name))
            {
                // Add an error to the model
                ModelState.AddModelError(string.Empty, $"A database with the name \"{Input.Name}\" already exists.");
                // Redisplay the page.
                return Page();
            }
            // Get the database type.
            var databaseType = _context.DatabaseTypes.FirstOrDefault(item => item.Id == Input.DatabaseTypeId);
            // Check if no database type has been found or if the database type is generic.
            if (databaseType == null || databaseType.Name == "Generic")
            {
                // Add an error to the model
                ModelState.AddModelError(string.Empty, "No non-generic database type could be found with the provided ID.");
                // Redisplay the page.
                return Page();
            }
            // Define the new database type.
            var database = new Database
            {
                Name = Input.Name,
                Description = Input.Description,
                Url = Input.Url,
                IsPublic = Input.IsPublic,
                DatabaseTypeId = databaseType.Id,
                DatabaseType = databaseType,
                DateTimeCreated = DateTime.Now
            };
            // Mark it for addition.
            _context.Databases.Add(database);
            // Save the changes to the database.
            await _context.SaveChangesAsync();
            // Display a message.
            TempData["StatusMessage"] = "Success: 1 database created successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Data/Databases/Index");
        }
    }
}
