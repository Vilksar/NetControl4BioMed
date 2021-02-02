using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Tasks;

namespace NetControl4BioMed.Pages.Administration.Permissions.DatabaseUserInvitations
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
            public IEnumerable<string> Emails { get; set; }

            public IEnumerable<string> DatabaseIds { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public IEnumerable<DatabaseUserInvitation> Items { get; set; }
        }
        public IActionResult OnGet(IEnumerable<string> emails, IEnumerable<string> databaseIds)
        {
            // Check if there aren't any e-mails or IDs provided.
            if (emails == null || databaseIds == null || !emails.Any() || !databaseIds.Any() || emails.Count() != databaseIds.Count())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid e-mails or IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Permissions/DatabaseUserInvitations/Index");
            }
            // Get the IDs of all selected users and databases.
            var ids = emails.Zip(databaseIds);
            // Define the view.
            View = new ViewModel
            {
                Items = _context.DatabaseUserInvitations
                    .Where(item => emails.Contains(item.Email) && databaseIds.Contains(item.Database.Id))
                    .Include(item => item.Database)
                    .AsEnumerable()
                    .Where(item => ids.Contains((item.Email, item.Database.Id)))
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No items have been found with the provided e-mails and IDs.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Permissions/DatabaseUserInvitations/Index");
            }
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            // Check if there aren't any e-mails or IDs provided.
            if (Input.Emails == null || Input.DatabaseIds == null || !Input.Emails.Any() || !Input.DatabaseIds.Any() || Input.Emails.Count() != Input.DatabaseIds.Count())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid e-mails or IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Permissions/DatabaseUserInvitations/Index");
            }
            // Get the IDs of all selected users and databases.
            var ids = Input.Emails.Zip(Input.DatabaseIds);
            // Define the view.
            View = new ViewModel
            {
                Items = _context.DatabaseUserInvitations
                    .Where(item => Input.Emails.Contains(item.Email) && Input.DatabaseIds.Contains(item.Database.Id))
                    .Include(item => item.Database)
                    .AsEnumerable()
                    .Where(item => ids.Contains((item.Email, item.Database.Id)))
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No items have been found with the provided e-mails and IDs.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Permissions/DatabaseUserInvitations/Index");
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
                Name = $"{nameof(IAdministrationTaskManager)}.{nameof(IAdministrationTaskManager.DeleteDatabaseUserInvitationsAsync)}",
                IsRecurring = false,
                Data = JsonSerializer.Serialize(new DatabaseUserInvitationsTask
                {
                    Items = View.Items.Select(item => new DatabaseUserInvitationInputModel
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
            var jobId = BackgroundJob.Enqueue<IAdministrationTaskManager>(item => item.DeleteDatabaseUserInvitationsAsync(task.Id, CancellationToken.None));
            // Display a message.
            TempData["StatusMessage"] = $"Success: A new background job was created to delete {itemCount} database user invitation{(itemCount != 1 ? "s" : string.Empty)}.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Permissions/DatabaseUserInvitations/Index");
        }
    }
}
