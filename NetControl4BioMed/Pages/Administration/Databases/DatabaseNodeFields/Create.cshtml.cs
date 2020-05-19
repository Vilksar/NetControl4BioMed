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
            public bool IsSearchable { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string DatabaseId { get; set; }
        }

        public IActionResult OnGet(string databaseId = null)
        {
            // Check if there aren't any non-generic databases.
            if (!_context.Databases.Any(item => item.DatabaseType.Name != "Generic"))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No non-generic databases could be found. Please create a database first.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/DatabaseNodeFields/Index");
            }
            // Define the input.
            Input = new InputModel
            {
                DatabaseId = databaseId
            };
            // Return the page.
            return Page();
        }

        public IActionResult OnPost()
        {
            // Check if there aren't any non-generic databases.
            if (!_context.Databases.Any(item => item.DatabaseType.Name != "Generic"))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No non-generic databases could be found. Please create a database first.";
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
            // Define a new task.
            var task = new DatabaseNodeFieldsTask
            {
                Items = new List<DatabaseNodeFieldInputModel>
                {
                    new DatabaseNodeFieldInputModel
                    {
                        Name = Input.Name,
                        Description = Input.Description,
                        Url = Input.Url,
                        IsSearchable = Input.IsSearchable,
                        Database = new DatabaseInputModel
                        {
                            Id = Input.DatabaseId
                        }
                    }
                }
            };
            // Try to run the task.
            try
            {
                // Run the task.
                var databaseNodeFields = task.Create(_serviceProvider, CancellationToken.None).ToList();
            }
            catch (Exception exception)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, exception.Message);
                // Redisplay the page.
                return Page();
            }
            // Display a message.
            TempData["StatusMessage"] = "Success: 1 database node field created successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Databases/DatabaseNodeFields/Index");
        }
    }
}
