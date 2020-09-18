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

namespace NetControl4BioMed.Pages.Administration.Permissions.DatabaseUsers
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
            public string DatabaseId { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string UserId { get; set; }
        }

        public IActionResult OnGet(string databaseId = null, string userId = null)
        {
            // Check if there aren't any databases.
            if (!_context.Databases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No databases could be found. Please create a database first.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Permissions/DatabaseUsers/Index");
            }
            // Define the input.
            Input = new InputModel
            {
                DatabaseId = databaseId,
                UserId = userId
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            // Check if there aren't any databases.
            if (!_context.Databases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No databases could be found. Please create a database first.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Permissions/DatabaseUsers/Index");
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
            var task = new DatabaseUsersTask
            {
                Items = new List<DatabaseUserInputModel>
                {
                    new DatabaseUserInputModel
                    {
                        Database = new DatabaseInputModel
                        {
                            Id = Input.DatabaseId
                        },
                        User = new UserInputModel
                        {
                            Id = Input.UserId
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
            TempData["StatusMessage"] = "Success: 1 database user created successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Permissions/DatabaseUsers/Index");
        }
    }
}
