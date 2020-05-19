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

namespace NetControl4BioMed.Pages.Administration.Databases.DatabaseEdgeFields
{
    [Authorize(Roles = "Administrator")]
    public class EditModel : PageModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationDbContext _context;

        public EditModel(IServiceProvider serviceProvider, ApplicationDbContext context)
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
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public DatabaseEdgeField DatabaseEdgeField { get; set; }
        }

        public IActionResult OnGet(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/DatabaseEdgeFields/Index");
            }
            // Define the query.
            var query = _context.DatabaseEdgeFields
                .Where(item => item.Id == id);
            // Define the view.
            View = new ViewModel
            {
                DatabaseEdgeField = query
                    .Include(item => item.Database)
                        .ThenInclude(item => item.DatabaseType)
                    .FirstOrDefault()
            };
            // Check if the item hasn't been found.
            if (View.DatabaseEdgeField == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item could be found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/DatabaseEdgeFields/Index");
            }
            // Check if the database edge field is the generic database edge field.
            if (View.DatabaseEdgeField.Database.DatabaseType.Name == "Generic")
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The generic database edge field can't be edited.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/DatabaseEdgeFields/Index");
            }
            // Define the input.
            Input = new InputModel
            {
                Id = View.DatabaseEdgeField.Id,
                Name = View.DatabaseEdgeField.Name,
                Description = View.DatabaseEdgeField.Description,
                Url = View.DatabaseEdgeField.Url,
                IsSearchable = View.DatabaseEdgeField.IsSearchable
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
                return RedirectToPage("/Administration/Databases/DatabaseEdgeFields/Index");
            }
            // Define the query.
            var query = _context.DatabaseEdgeFields
                .Where(item => item.Id == Input.Id);
            // Define the view.
            View = new ViewModel
            {
                DatabaseEdgeField = query
                    .Include(item => item.Database)
                        .ThenInclude(item => item.DatabaseType)
                    .FirstOrDefault()
            };
            // Check if the item hasn't been found.
            if (View.DatabaseEdgeField == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item could be found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/DatabaseEdgeFields/Index");
            }
            // Check if the database edge field is the generic database edge field.
            if (View.DatabaseEdgeField.Database.DatabaseType.Name == "Generic")
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The generic database edge field can't be edited.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/DatabaseEdgeFields/Index");
            }
            // Check if the provided model isn't valid.
            if (!ModelState.IsValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error has been encountered. Please check again the input fields.");
                // Redisplay the page.
                return Page();
            }
            // Define a new task.
            var task = new DatabaseEdgeFieldsTask
            {
                Items = new List<DatabaseEdgeFieldInputModel>
                {
                    new DatabaseEdgeFieldInputModel
                    {
                        Id = Input.Id,
                        Name = Input.Name,
                        Description = Input.Description,
                        Url = Input.Url,
                        IsSearchable = Input.IsSearchable
                    }
                }
            };
            // Try to run the task.
            try
            {
                // Run the task.
                var databaseEdgeFields = task.Edit(_serviceProvider, CancellationToken.None).ToList();
            }
            catch (Exception exception)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, exception.Message);
                // Redisplay the page.
                return Page();
            }
            // Display a message.
            TempData["StatusMessage"] = "Success: 1 database edge field updated successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Databases/DatabaseEdgeFields/Index");
        }
    }
}
