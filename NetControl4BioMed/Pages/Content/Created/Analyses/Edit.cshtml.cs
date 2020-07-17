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
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Tasks;

namespace NetControl4BioMed.Pages.Content.Created.Analyses
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public EditModel(IServiceProvider serviceProvider, UserManager<User> userManager, ApplicationDbContext context)
        {
            _serviceProvider = serviceProvider;
            _userManager = userManager;
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
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public Analysis Analysis { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if the user does not exist.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: An error occured while trying to load the user data. If you are already logged in, please log out and try again.";
                // Redirect to the home page.
                return RedirectToPage("/Index");
            }
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Define the query.
            var query = _context.Analyses
                .Where(item => item.AnalysisUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == id);
            // Define the view.
            View = new ViewModel
            {
                Analysis = query
                    .FirstOrDefault()
            };
            // Check if the item hasn't been found.
            if (View.Analysis == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No analysis has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Define the input.
            Input = new InputModel
            {
                Id = View.Analysis.Id,
                Name = View.Analysis.Name,
                Description = View.Analysis.Description
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if the user does not exist.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: An error occured while trying to load the user data. If you are already logged in, please log out and try again.";
                // Redirect to the home page.
                return RedirectToPage("/Index");
            }
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(Input.Id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Define the query.
            var query = _context.Analyses
                .Where(item => item.AnalysisUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == Input.Id);
            // Define the view.
            View = new ViewModel
            {
                Analysis = query
                    .FirstOrDefault()
            };
            // Check if the item hasn't been found.
            if (View.Analysis == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No analysis has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
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
            var task = new AnalysesTask
            {
                Items = new List<AnalysisInputModel>
                {
                    new AnalysisInputModel
                    {
                        Id = Input.Id,
                        Name = Input.Name,
                        Description = Input.Description
                    }
                }
            };
            // Try to run the task.
            try
            {
                // Run the task.
                _ = task.Edit(_serviceProvider, CancellationToken.None).ToList();
            }
            catch (Exception exception)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, exception.Message);
                // Redisplay the page.
                return Page();
            }
            // Display a message.
            TempData["StatusMessage"] = "Success: 1 analysis updated successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Content/Created/Analyses/Index");
        }
    }
}
