using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.Administration.Permissions.DatabaseUsers
{
    [Authorize(Roles = "Administrator")]
    public class DeleteModel : PageModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationDbContext _context;

        public DeleteModel(IServiceProvider serviceProvider, ApplicationDbContext context)
        {
            _serviceProvider = serviceProvider;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public IEnumerable<string> DatabaseIds { get; set; }

            public IEnumerable<string> Emails { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public IEnumerable<DatabaseUser> Items { get; set; }
        }

        public IActionResult OnGet(IEnumerable<string> databaseIds, IEnumerable<string> emails)
        {
            // Check if there aren't any IDs or e-mails provided.
            if (databaseIds == null || emails == null || !databaseIds.Any() || !emails.Any() || databaseIds.Count() != emails.Count())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Permissions/DatabaseUsers/Index");
            }
            // Get the IDs of all selected databases and e-mails.
            var ids = databaseIds.Zip(emails);
            // Define the view.
            View = new ViewModel
            {
                Items = _context.DatabaseUsers
                    .Where(item => databaseIds.Contains(item.Database.Id) && emails.Contains(item.Email))
                    .Include(item => item.Database)
                    .Include(item => item.User)
                    .AsEnumerable()
                    .Where(item => ids.Contains((item.Database.Id, item.Email)))
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No items have been found with the provided IDs.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Permissions/DatabaseUsers/Index");
            }
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            // Check if there aren't any IDs or e-mails provided.
            if (Input.DatabaseIds == null || Input.Emails == null || !Input.DatabaseIds.Any() || !Input.Emails.Any() || Input.DatabaseIds.Count() != Input.Emails.Count())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Permissions/DatabaseUsers/Index");
            }
            // Get the IDs of all selected databases and e-mails.
            var ids = Input.DatabaseIds.Zip(Input.Emails);
            // Define the view.
            View = new ViewModel
            {
                Items = _context.DatabaseUsers
                    .Where(item => Input.DatabaseIds.Contains(item.Database.Id) && Input.Emails.Contains(item.Email))
                    .Include(item => item.Database)
                    .Include(item => item.User)
                    .AsEnumerable()
                    .Where(item => ids.Contains((item.Database.Id, item.Email)))
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No items have been found with the provided IDs.";
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
            // Save the number of items found.
            var itemCount = View.Items.Count();
            // Define a new task.
            var task = new BackgroundTask
            {
                DateTimeCreated = DateTime.UtcNow,
                Name = $"{nameof(IAdministrationTaskManager)}.{nameof(IAdministrationTaskManager.DeleteDatabaseUsersAsync)}",
                IsRecurring = false,
                Data = JsonSerializer.Serialize(new DatabaseUsersTask
                {
                    Items = View.Items.Select(item => new DatabaseUserInputModel
                    {
                        Database = new DatabaseInputModel
                        {
                            Id = item.Database.Id
                        },
                        Email = item.Email
                    })
                }, new JsonSerializerOptions { IgnoreNullValues = true })
            };
            // Mark the task for addition.
            _context.BackgroundTasks.Add(task);
            // Save the changes to the database.
            await _context.SaveChangesAsync();
            // Create a new Hangfire background job.
            var jobId = BackgroundJob.Enqueue<IAdministrationTaskManager>(item => item.DeleteDatabaseUsersAsync(task.Id, CancellationToken.None));
            // Display a message.
            TempData["StatusMessage"] = $"Success: A new background job was created to delete {itemCount} database user{(itemCount != 1 ? "s" : string.Empty)}.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Permissions/DatabaseUsers/Index");
        }
    }
}
