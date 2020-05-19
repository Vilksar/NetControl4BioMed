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

namespace NetControl4BioMed.Pages.Administration.Databases.Databases
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
            public bool IsPublic { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public Database Database { get; set; }
        }

        public IActionResult OnGet(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/Databases/Index");
            }
            // Define the query.
            var query = _context.Databases
                .Where(item => item.Id == id);
            // Define the view.
            View = new ViewModel
            {
                Database = query
                    .Include(item => item.DatabaseType)
                    .FirstOrDefault()
            };
            // Check if the item hasn't been found.
            if (View.Database == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item could be found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/Databases/Index");
            }
            // Check if the database is of the generic type.
            if (View.Database.DatabaseType.Name == "Generic")
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The generic database can't be edited.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/Databases/Index");
            }
            // Define the input.
            Input = new InputModel
            {
                Id = View.Database.Id,
                Name = View.Database.Name,
                Description = View.Database.Description,
                Url = View.Database.Url,
                IsPublic = View.Database.IsPublic
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
                return RedirectToPage("/Administration/Databases/Databases/Index");
            }
            // Define the query.
            var query = _context.Databases
                .Where(item => item.Id == Input.Id);
            // Define the view.
            View = new ViewModel
            {
                Database = query
                    .Include(item => item.DatabaseType)
                    .FirstOrDefault()
            };
            // Check if the item hasn't been found.
            if (View.Database == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item could be found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Databases/Databases/Index");
            }
            // Check if the database is the generic database.
            if (View.Database.DatabaseType.Name == "Generic")
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The generic database can't be edited.";
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
            // Define a new task.
            var task = new DatabasesTask
            {
                Items = new List<DatabaseInputModel>
                {
                    new DatabaseInputModel
                    {
                        Id = Input.Id,
                        Name = Input.Name,
                        Description = Input.Description,
                        Url = Input.Url,
                        IsPublic = Input.IsPublic
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
            TempData["StatusMessage"] = "Success: 1 database updated successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Databases/Databases/Index");
        }
    }
}
