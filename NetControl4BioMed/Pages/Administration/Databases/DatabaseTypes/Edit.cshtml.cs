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
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Tasks;

namespace NetControl4BioMed.Pages.Administration.Databases.DatabaseTypes
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
            [Required(ErrorMessage = "This field is required.")]
            public string Description { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public DatabaseType DatabaseType { get; set; }
        }

        public IActionResult OnGet(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/DatabaseTypes/Index");
            }
            // Create a new scope.
            using var scope = _serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Define the query.
            var query = context.DatabaseTypes
                .Where(item => item.Id == id);
            // Define the view.
            View = new ViewModel
            {
                DatabaseType = query
                    .FirstOrDefault()
            };
            // Check if the item hasn't been found.
            if (View.DatabaseType == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item could be found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/DatabaseTypes/Index");
            }
            // Check if the database type is the generic database type.
            if (View.DatabaseType.Name == "Generic")
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The generic database type can't be edited.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/DatabaseTypes/Index");
            }
            // Define the input.
            Input = new InputModel
            {
                Id = View.DatabaseType.Id,
                Name = View.DatabaseType.Name,
                Description = View.DatabaseType.Description
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
                return RedirectToPage("/Administration/Databases/DatabaseTypes/Index");
            }
            // Create a new scope.
            using var scope = _serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Define the query.
            var query = context.DatabaseTypes
                .Where(item => item.Id == Input.Id);
            // Define the view.
            View = new ViewModel
            {
                DatabaseType = query
                    .FirstOrDefault()
            };
            // Check if the item hasn't been found.
            if (View.DatabaseType == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item could be found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/DatabaseTypes/Index");
            }
            // Check if the database type is the generic database type.
            if (View.DatabaseType.Name == "Generic")
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The generic database type can't be edited.";
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
            // Check if there is another database type with the same name.
            if (context.DatabaseTypes.Any(item => item.Id != View.DatabaseType.Id && item.Name == Input.Name))
            {
                // Add an error to the model
                ModelState.AddModelError(string.Empty, $"A database type with the name \"{Input.Name}\" already exists.");
                // Redisplay the page.
                return Page();
            }
            // Define a new task.
            var task = new DatabaseTypesTask
            {
                Items = new List<DatabaseTypeInputModel>
                {
                    new DatabaseTypeInputModel
                    {
                        Id = Input.Id,
                        Name = Input.Name,
                        Description = Input.Description
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
            TempData["StatusMessage"] = "Success: 1 database type updated successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Databases/DatabaseTypes/Index");
        }
    }
}
