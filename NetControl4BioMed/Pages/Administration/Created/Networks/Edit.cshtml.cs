using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.Administration.Created.Networks
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

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public bool IsPublic { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public bool IsDemonstration { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public Network Network { get; set; }
        }

        public IActionResult OnGet(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Created/Networks/Index");
            }
            // Define the query.
            var query = _context.Networks
                .Where(item => item.Id == id);
            // Define the view.
            View = new ViewModel
            {
                Network = query
                    .FirstOrDefault()
            };
            // Check if the item hasn't been found.
            if (View.Network == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item could be found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Created/Networks/Index");
            }
            // Define the input.
            Input = new InputModel
            {
                Id = View.Network.Id,
                Name = View.Network.Name,
                Description = View.Network.Description,
                IsPublic = View.Network.IsPublic,
                IsDemonstration = View.Network.IsDemonstration
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
                return RedirectToPage("/Administration/Created/Networks/Index");
            }
            // Define the query.
            var query = _context.Networks
                .Where(item => item.Id == Input.Id);
            // Define the view.
            View = new ViewModel
            {
                Network = query
                    .FirstOrDefault()
            };
            // Check if the item hasn't been found.
            if (View.Network == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item could be found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Created/Networks/Index");
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
            var task = new NetworksTask
            {
                Items = new List<NetworkInputModel>
                {
                    new NetworkInputModel
                    {
                        Id = Input.Id,
                        Name = Input.Name,
                        Description = Input.Description,
                        IsPublic = Input.IsPublic,
                        IsDemonstration = Input.IsDemonstration
                    }
                }
            };
            // Try to run the tasks.
            try
            {
                // Run the tasks.
                await task.EditAsync(_serviceProvider, CancellationToken.None);
                await task.EditDemonstrationStatusAsync(_serviceProvider, CancellationToken.None);
            }
            catch (Exception exception)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, exception.Message);
                // Redisplay the page.
                return Page();
            }
            // Display a message.
            TempData["StatusMessage"] = "Success: 1 network updated successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Created/Networks/Index");
        }
    }
}
