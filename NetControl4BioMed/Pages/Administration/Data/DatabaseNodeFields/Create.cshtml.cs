using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Administration.Data.DatabaseNodeFields
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
            public DatabaseNodeFieldType Type { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public bool IsSearchable { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string DatabaseString { get; set; }
        }

        public IActionResult OnGet(string databaseString = null)
        {
            // Check if there aren't any databases of non-generic type.
            if (!_context.Databases.Where(item => item.DatabaseType.Name != "Generic").Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No databases of non-generic type could be found. Please create a database first.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Data/DatabaseNodeFields/Index");
            }
            // Define the input.
            Input = new InputModel
            {
                DatabaseString = databaseString
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Check if there aren't any databases of non-generic type.
            if (!_context.Databases.Where(item => item.DatabaseType.Name != "Generic").Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No databases of non-generic type could be found. Please create a database first.";
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
            // Check if there is another database node field with the same name.
            if (_context.DatabaseNodeFields.Any(item => item.Name == Input.Name))
            {
                // Add an error to the model
                ModelState.AddModelError(string.Empty, $"A database node field with the name \"{Input.Name}\" already exists.");
                // Redisplay the page.
                return Page();
            }
            // Get the corresponding database.
            var database = _context.Databases
                .Where(item => item.DatabaseType.Name != "Generic")
                .FirstOrDefault(item => item.Id == Input.DatabaseString || item.Name == Input.DatabaseString);
            // Check if no database has been found.
            if (database == null)
            {
                // Add an error to the model
                ModelState.AddModelError(string.Empty, "No non-generic database could be found with the provided ID.");
                // Redisplay the page.
                return Page();
            }
            // Define the new database node field.
            var databaseNodeField = new DatabaseNodeField
            {
                Name = Input.Name,
                Description = Input.Description,
                Url = Input.Url,
                IsSearchable = Input.IsSearchable,
                Type = Input.Type,
                DatabaseId = database.Id,
                Database = database,
                DateTimeCreated = DateTime.Now
            };
            // Mark it for addition.
            _context.DatabaseNodeFields.Add(databaseNodeField);
            // Save the changes to the database.
            await _context.SaveChangesAsync();
            // Display a message.
            TempData["StatusMessage"] = "Success: 1 database node field created successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Data/DatabaseNodeFields/Index");
        }
    }
}
