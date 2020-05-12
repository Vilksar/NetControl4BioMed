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
            public string UserString { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string DatabaseString { get; set; }
        }

        public IActionResult OnGet(string userString = null, string databaseString = null)
        {
            // Create a new scope.
            using var scope = _serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Check if there aren't any databases.
            if (!context.Databases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No databases could be found. Please create a database first.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Permissions/DatabaseUsers/Index");
            }
            // Define the input.
            Input = new InputModel
            {
                UserString = userString,
                DatabaseString = databaseString
            };
            // Return the page.
            return Page();
        }

        public IActionResult OnPost()
        {
            // Create a new scope.
            using var scope = _serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Check if there aren't any databases.
            if (!context.Databases.Any())
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
            // Get the user based on the provided string.
            var user = context.Users.FirstOrDefault(item => item.Id == Input.UserString || item.Email == Input.UserString);
            // Check if there was no user found.
            if (user == null)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "No user could be found with the given string.");
                // Redisplay the page.
                return Page();
            }
            // Get the database based on the provided string.
            var database = context.Databases.FirstOrDefault(item => item.Id == Input.DatabaseString || item.Name == Input.DatabaseString);
            // Check if there was no database found.
            if (database == null)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "No database could be found with the given string.");
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
                        DatabaseId = database.Id,
                        UserId = user.Id
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
            TempData["StatusMessage"] = "Success: 1 database user created successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Permissions/DatabaseUsers/Index");
        }
    }
}
