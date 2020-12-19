using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Tasks;

namespace NetControl4BioMed.Pages.Content.DatabaseTypes.Generic.Created.Analyses
{
    public class StopModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IReCaptchaChecker _reCaptchaChecker;

        public StopModel(UserManager<User> userManager, ApplicationDbContext context, IReCaptchaChecker reCaptchaChecker)
        {
            _userManager = userManager;
            _context = context;
            _reCaptchaChecker = reCaptchaChecker;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public IEnumerable<string> Ids { get; set; }

            public string ReCaptchaToken { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public IEnumerable<Analysis> Items { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(IEnumerable<string> ids)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if there aren't any IDs provided.
            if (ids == null || !ids.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/Generic/Created/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Items = _context.Analyses
                    .Where(item => item.AnalysisDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => item.IsPublic || item.AnalysisUsers.Any(item1 => item1.User == user))
                    .Where(item => item.Status == AnalysisStatus.Ongoing)
                    .Where(item => ids.Contains(item.Id))
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No stoppable analyses have been found with the provided IDs, or you don't have access to them.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/Generic/Created/Analyses/Index");
            }
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if there aren't any IDs provided.
            if (Input.Ids == null || !Input.Ids.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/Generic/Created/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Items = _context.Analyses
                    .Where(item => item.AnalysisDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => item.IsPublic || item.AnalysisUsers.Any(item1 => item1.User == user))
                    .Where(item => item.Status == AnalysisStatus.Ongoing)
                    .Where(item => Input.Ids.Contains(item.Id))
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No stoppable analyses have been found with the provided IDs, or you don't have access to them.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/Generic/Created/Analyses/Index");
            }
            // Check if the reCaptcha is valid.
            if (!await _reCaptchaChecker.IsValid(Input.ReCaptchaToken))
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The reCaptcha verification failed.");
                // Return the page.
                return Page();
            }
            // Check if the provided model isn't valid.
            if (!ModelState.IsValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error has been encountered. Please check again the input fields.");
                // Redisplay the page.
                return Page();
            }
            // Save the number of items found.
            var itemCount = View.Items.Count();
            // Define a new task.
            var task = new BackgroundTask
            {
                DateTimeCreated = DateTime.UtcNow,
                Name = $"{nameof(IContentTaskManager)}.{nameof(IContentTaskManager.StopAnalysesAsync)}",
                IsRecurring = false,
                Data = JsonSerializer.Serialize(new AnalysesTask
                {
                    Items = View.Items.Select(item => new AnalysisInputModel
                    {
                        Id = item.Id
                    })
                })
            };
            // Mark the task for addition.
            _context.BackgroundTasks.Add(task);
            // Save the changes to the database.
            await _context.SaveChangesAsync();
            // Create a new Hangfire background job.
            var jobId = BackgroundJob.Enqueue<IContentTaskManager>(item => item.StopAnalysesAsync(task.Id, CancellationToken.None));
            // Display a message.
            TempData["StatusMessage"] = $"Success: A new background job was created to stop {itemCount} analys{(itemCount != 1 ? "e" : "i")}s.";
            // Redirect to the index page.
            return RedirectToPage("/Content/DatabaseTypes/Generic/Created/Analyses/Index");
        }
    }
}
