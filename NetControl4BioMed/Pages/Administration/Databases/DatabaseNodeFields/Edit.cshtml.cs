using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Tasks;

namespace NetControl4BioMed.Pages.Administration.Databases.DatabaseNodeFields
{
    [Authorize(Roles = "Administrator")]
    public class EditModel : PageModel
    {
        private readonly IServiceProvider _serviceProvider;

        public EditModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string Id { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string Name { get; set; }

            [DataType(DataType.MultilineText)]
            public string Description { get; set; }

            [DataType(DataType.Url)]
            public string Url { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public bool IsSearchable { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string DatabaseString { get; set; }
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
                return RedirectToPage("/Administration/Databases/DatabaseNodeFields/Index");
            }
            // Create a new scope.
            using var scope = _serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Define the query.
            var query = context.DatabaseNodeFields
                .Where(item => item.Id == id);
            // Define the view.
            View = new ViewModel
            {
                DatabaseNodeField = query
                    .Include(item => item.Database)
                        .ThenInclude(item => item.DatabaseType)
                    .FirstOrDefault()
            };
            // Check if the item hasn't been found.
            if (View.DatabaseNodeField == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item could be found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/DatabaseNodeFields/Index");
            }
            // Check if the database node field is the generic database node field.
            if (View.DatabaseNodeField.Database.DatabaseType.Name == "Generic")
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The generic database node field can't be edited.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/DatabaseNodeFields/Index");
            }
            // Define the input.
            Input = new InputModel
            {
                Id = View.DatabaseNodeField.Id,
                Name = View.DatabaseNodeField.Name,
                Description = View.DatabaseNodeField.Description,
                Url = View.DatabaseNodeField.Url,
                IsSearchable = View.DatabaseNodeField.IsSearchable,
                DatabaseString = View.DatabaseNodeField.Database.Name
            };
            // Return the page.
            return Page();
        }

        public IActionResult OnPost()
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(Input.Id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/DatabaseNodeFields/Index");
            }
            // Create a new scope.
            using var scope = _serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Define the query.
            var query = context.DatabaseNodeFields
                .Where(item => item.Id == Input.Id);
            // Define the view.
            View = new ViewModel
            {
                DatabaseNodeField = query
                    .Include(item => item.Database)
                        .ThenInclude(item => item.DatabaseType)
                    .FirstOrDefault()
            };
            // Check if the item hasn't been found.
            if (View.DatabaseNodeField == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item could be found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/DatabaseNodeFields/Index");
            }
            // Check if the database node field is the generic database node field.
            if (View.DatabaseNodeField.Database.DatabaseType.Name == "Generic")
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The generic database node field can't be edited.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/DatabaseNodeFields/Index");
            }
            // Check if the provided model isn't valid.
            if (!ModelState.IsValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error has been encountered. Please check again the input fields.");
                // Redisplay the page.
                return Page();
            }
            // Check if the name has changed and there is another database node field with the same name.
            if (View.DatabaseNodeField.Name != Input.Name && context.DatabaseNodeFields.Any(item => item.Name == Input.Name))
            {
                // Add an error to the model
                ModelState.AddModelError(string.Empty, $"A database node field with the name \"{Input.Name}\" already exists.");
                // Redisplay the page.
                return Page();
            }
            // Get the corresponding database.
            var database = context.Databases
                .Where(item => item.DatabaseType.Name != "Generic")
                .FirstOrDefault(item => item.Id == Input.DatabaseString || item.Name == Input.DatabaseString);
            // Check if no database has been found.
            if (database == null)
            {
                // Add an error to the model
                ModelState.AddModelError(string.Empty, "No non-generic database could be found with the provided string.");
                // Redisplay the page.
                return Page();
            }
            // Check if the database is to be changed, and the new database is of a different type.
            if (database.Id != View.DatabaseNodeField.Database.Id && database.DatabaseType.Id != View.DatabaseNodeField.Database.DatabaseType.Id)
            {
                // Add an error to the model
                ModelState.AddModelError(string.Empty, "The new database is of a different type.");
                // Redisplay the page.
                return Page();
            }
            // Define a new task.
            var task = new DatabaseNodeFieldsTask
            {
                Items = new List<DatabaseNodeFieldInputModel>
                {
                    new DatabaseNodeFieldInputModel
                    {
                        Id = Input.Id,
                        Name = Input.Name,
                        Description = Input.Description,
                        Url = Input.Url,
                        IsSearchable = Input.IsSearchable,
                        DatabaseId = database.Id
                    }
                }
            };
            // Try to run the task.
            try
            {
                // Run the task.
                task.Edit(_serviceProvider, CancellationToken.None);
            }
            catch (Exception exception)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, exception.Message);
                // Redisplay the page.
                return Page();
            }
            // Display a message.
            TempData["StatusMessage"] = "Success: 1 database node field updated successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Databases/DatabaseNodeFields/Index");
        }
    }
}
