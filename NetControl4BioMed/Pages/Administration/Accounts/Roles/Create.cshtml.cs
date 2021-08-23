using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.Administration.Accounts.Roles
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
        }

        public IActionResult OnGet()
        {
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
            var task = new RolesTask
            {
                Items = new List<RoleInputModel>
                {
                    new RoleInputModel
                    {
                        Name = Input.Name
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
            TempData["StatusMessage"] = "Success: 1 role created successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Accounts/Roles/Index");
        }
    }
}
