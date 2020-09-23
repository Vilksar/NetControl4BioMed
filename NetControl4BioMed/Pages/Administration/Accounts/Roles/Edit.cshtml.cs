using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Tasks;

namespace NetControl4BioMed.Pages.Administration.Accounts.Roles
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
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public Role Role { get; set; }
        }

        public IActionResult OnGet(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Roles/Index");
            }
            // Define the query.
            var query = _context.Roles
                .Where(item => item.Id == id);
            // Define the view.
            View = new ViewModel
            {
                Role = query
                    .FirstOrDefault()
            };
            // Check if the item hasn't been found.
            if (View.Role == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item could be found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Roles/Index");
            }
            // Check if the role is the administrator role.
            if (View.Role.Name == "Administrator")
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The \"Administrator\" role can't be edited.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Roles/Index");
            }
            // Check if the role is the guest role.
            if (View.Role.Name == "Guest")
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The \"Guest\" role can't be edited.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Roles/Index");
            }
            // Define the input.
            Input = new InputModel
            {
                Id = View.Role.Id,
                Name = View.Role.Name
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(Input.Id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Roles/Index");
            }
            // Define the query.
            var query = _context.Roles
                .Where(item => item.Id == Input.Id);
            // Define the view.
            View = new ViewModel
            {
                Role = query
                    .FirstOrDefault()
            };
            // Check if the item hasn't been found.
            if (View.Role == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item could be found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Roles/Index");
            }
            // Check if the role is the administrator role.
            if (View.Role.Name == "Administrator")
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The \"Administrator\" role can't be edited.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Roles/Index");
            }
            // Check if the role is the guest role.
            if (View.Role.Name == "Guest")
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The \"Guest\" role can't be edited.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Roles/Index");
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
            var task = new RolesTask
            {
                Items = new List<RoleInputModel>
                {
                    new RoleInputModel
                    {
                        Id = Input.Id,
                        Name = Input.Name
                    }
                }
            };
            // Try to run the task.
            try
            {
                // Run the task.
                await task.EditAsync(_serviceProvider, CancellationToken.None);
            }
            catch (Exception exception)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, exception.Message);
                // Redisplay the page.
                return Page();
            }
            // Display a message.
            TempData["StatusMessage"] = "Success: 1 role updated successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Accounts/Roles/Index");
        }
    }
}
