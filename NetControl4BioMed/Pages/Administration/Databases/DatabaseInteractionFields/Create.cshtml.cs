using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.Administration.Databases.DatabaseInteractionFields
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
            // Define the input.
            Input = new InputModel
            {
                DatabaseId = databaseId
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPost()
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
            var task = new DatabaseInteractionFieldsTask
            {
                Items = new List<DatabaseInteractionFieldInputModel>
                {
                    new DatabaseInteractionFieldInputModel
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
                await task.CreateAsync(_serviceProvider, CancellationToken.None);
            }
            catch (Exception exception)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, exception.Message);
                // Redisplay the page.
                return Page();
            }
            // Display a message.
            TempData["StatusMessage"] = "Success: 1 database interaction field created successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Databases/DatabaseInteractionFields/Index");
        }
    }
}
