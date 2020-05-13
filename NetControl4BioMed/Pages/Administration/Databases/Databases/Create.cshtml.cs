using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Tasks;

namespace NetControl4BioMed.Pages.Administration.Databases.Databases
{
    [Authorize(Roles = "Administrator")]
    public class CreateModel : PageModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationDbContext _context;

        public CreateModel(IServiceProvider serviceProvider, ApplicationDbContext context)
        {
            _serviceProvider = serviceProvider;
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
            public string DatabaseTypeString { get; set; }
        }

        public IActionResult OnGet(string databaseTypeString = null)
        {
            // Check if there aren't any non-generic database types.
            if (!_context.DatabaseTypes.Any(item => item.Name != "Generic"))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No non-generic database types could be found. Please create a database type first.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/Databases/Index");
            }
            // Define the input.
            Input = new InputModel
            {
                DatabaseTypeString = databaseTypeString
            };
            // Return the page.
            return Page();
        }

        public IActionResult OnPost()
        {
            // Check if there aren't any non-generic database types.
            if (!_context.DatabaseTypes.Any(item => item.Name != "Generic"))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No non-generic database types could be found. Please create a database type first.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/Databases/Index");
            }
            // Check if the provided model isn't valid.
            if (!ModelState.IsValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error has been encountered. Please check again the input fields.");
                // Redisplay the page.
                return Page();
            }
            // Check if there is another database with the same name.
            if (_context.Databases.Any(item => item.Name == Input.Name))
            {
                // Add an error to the model
                ModelState.AddModelError(string.Empty, $"A database with the name \"{Input.Name}\" already exists.");
                // Redisplay the page.
                return Page();
            }
            // Get the corresponding database type.
            var databaseType = _context.DatabaseTypes
                .Where(item => item.Name != "Generic")
                .FirstOrDefault(item => item.Id == Input.DatabaseTypeString || item.Name == Input.DatabaseTypeString);
            // Check if no database type has been found.
            if (databaseType == null)
            {
                // Add an error to the model
                ModelState.AddModelError(string.Empty, "No non-generic database type could be found with the provided string.");
                // Redisplay the page.
                return Page();
            }
            // Define a new task.
            var task = new DatabasesTask
            {
                Items = new List<DatabaseInputModel>
                {
                    new DatabaseInputModel
                    {
                        Name = Input.Name,
                        Description = Input.Description,
                        Url = Input.Url,
                        IsPublic = Input.IsPublic,
                        DatabaseTypeId = databaseType.Id
                    }
                }
            };
            // Try to run the task.
            try
            {
                // Run the task.
                task.Create(_serviceProvider, CancellationToken.None);
            }
            catch (Exception exception)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, exception.Message);
                // Redisplay the page.
                return Page();
            }
            // Display a message.
            TempData["StatusMessage"] = "Success: 1 database created successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Databases/Databases/Index");
        }
    }
}
