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

namespace NetControl4BioMed.Pages.Administration.Databases.DatabaseTypes
{
    [Authorize(Roles = "Administrator")]
    public class CreateModel : PageModel
    {
        private readonly IServiceProvider _serviceProvider;

        public CreateModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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

        public IActionResult OnPost()
        {
            // Check if the provided model isn't valid.
            if (!ModelState.IsValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error has been encountered. Please check again the input fields.");
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
                        Name = Input.Name,
                        Description = Input.Description
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
            TempData["StatusMessage"] = "Success: 1 database type created successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Databases/DatabaseTypes/Index");
        }
    }
}
