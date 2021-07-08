using Hangfire;
using Microsoft.AspNetCore.Identity;
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

namespace NetControl4BioMed.Pages.CreatedData.Networks.Details.Accounts.Users
{
    public class RemoveModel : PageModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IReCaptchaChecker _reCaptchaChecker;

        public RemoveModel(IServiceProvider serviceProvider, UserManager<User> userManager, ApplicationDbContext context, IReCaptchaChecker reCaptchaChecker)
        {
            _serviceProvider = serviceProvider;
            _userManager = userManager;
            _context = context;
            _reCaptchaChecker = reCaptchaChecker;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public string Id { get; set; }

            public IEnumerable<string> Emails { get; set; }

            public string ReCaptchaToken { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public Network Network { get; set; }

            public IEnumerable<NetworkUser> Items { get; set; }

            public bool IsCurrentUserSelected { get; set; }

            public bool AreAllUsersSelected { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id, IEnumerable<string> emails)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Networks/Index");
            }
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Get the items with the provided ID.
            var items = _context.Networks
                .Where(item => item.IsPublic || (user != null && item.NetworkUsers.Any(item1 => item1.Email == user.Email)))
                .Where(item => item.Id == id);
            // Check if there were no items found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Networks/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Network = items
                    .First()
            };
            // Check if the user is not an owner.
            if (user == null || !items.Any(item => item.NetworkUsers.Any(item1 => item1.Email == user.Email)))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: You need to be an owner of the network in order to remove a user.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Networks/Details/Accounts/Users/Index", new { id = View.Network.Id });
            }
            // Check if there aren't any emails provided.
            if (emails == null || !emails.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid emails have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Networks/Details/Accounts/Users/Index", new { id = View.Network.Id });
            }
            // Update the view.
            View.Items = items
                .Select(item => item.NetworkUsers)
                .SelectMany(item => item)
                .Where(item => emails.Contains(item.Email));
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No users have been found with the provided e-mails or you don't have access to remove them.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Networks/Details/Accounts/Users/Index", new { id = View.Network.Id });
            }
            // Define the view status of the selected items.
            View.IsCurrentUserSelected = View.Items
                .Any(item => item.Email == user.Email);
            View.AreAllUsersSelected = !items
                .Select(item => item.NetworkUsers)
                .SelectMany(item => item)
                .Select(item => item.Email)
                .AsEnumerable()
                .Except(View.Items.Select(item => item.Email))
                .Any();
            // Define the input.
            Input = new InputModel
            {
                Id = View.Network.Id
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(Input.Id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Networks/Index");
            }
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Get the items with the provided ID.
            var items = _context.Networks
                .Where(item => item.IsPublic || (user != null && item.NetworkUsers.Any(item1 => item1.Email == user.Email)))
                .Where(item => item.Id == Input.Id);
            // Check if there were no items found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Networks/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Network = items
                    .First()
            };
            // Check if the user is not an owner.
            if (user == null || !items.Any(item => item.NetworkUsers.Any(item1 => item1.Email == user.Email)))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: You need to be an owner of the network in order to remove a user.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Networks/Details/Accounts/Users/Index", new { id = View.Network.Id });
            }
            // Check if there aren't any emails provided.
            if (Input.Emails == null || !Input.Emails.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid emails have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Networks/Details/Accounts/Users/Index", new { id = View.Network.Id });
            }
            // Update the view.
            View.Items = items
                .Select(item => item.NetworkUsers)
                .SelectMany(item => item)
                .Where(item => Input.Emails.Contains(item.Email));
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No users have been found with the provided e-mails or you don't have access to remove them.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Networks/Details/Accounts/Users/Index", new { id = View.Network.Id });
            }
            // Define the view status of the selected items.
            View.IsCurrentUserSelected = View.Items
                .Any(item => item.Email == user.Email);
            View.AreAllUsersSelected = !items
                .Select(item => item.NetworkUsers)
                .SelectMany(item => item)
                .Select(item => item.Email)
                .AsEnumerable()
                .Except(View.Items.Select(item => item.Email))
                .Any();
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
            // Check if all of the users have been selected.
            if (View.AreAllUsersSelected && !View.Network.IsPublic)
            {
                // Define a new task.
                var task = new BackgroundTask
                {
                    DateTimeCreated = DateTime.UtcNow,
                    Name = $"{nameof(IContentTaskManager)}.{nameof(IContentTaskManager.DeleteNetworksAsync)}",
                    IsRecurring = false,
                    Data = JsonSerializer.Serialize(new NetworksTask
                    {
                        Items = new List<NetworkInputModel>
                        {
                            new NetworkInputModel
                            {
                                Id = View.Network.Id
                            }
                        }
                    })
                };
                // Mark the task for addition.
                _context.BackgroundTasks.Add(task);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
                // Create a new Hangfire background job.
                var jobId = BackgroundJob.Enqueue<IContentTaskManager>(item => item.DeleteNetworksAsync(task.Id, CancellationToken.None));
                // Display a message.
                TempData["StatusMessage"] = $"Success: A new background job was created to delete 1 network.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Networks/Index");
            }
            // Save the number of items found.
            var itemCount = View.Items.Count();
            // Define the new tasks.
            var networkUsersTask = new NetworkUsersTask
            {
                Items = View.Items.Select(item => new NetworkUserInputModel
                {
                    Network = new NetworkInputModel
                    {
                        Id = View.Network.Id
                    },
                    Email = item.Email
                })
            };
            // Try to run the tasks.
            try
            {
                // Run the tasks.
                await networkUsersTask.DeleteAsync(_serviceProvider, CancellationToken.None);
            }
            catch (Exception exception)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, exception.Message);
                // Redisplay the page.
                return Page();
            }
            // Display a message.
            TempData["StatusMessage"] = $"Success: {itemCount} user{(itemCount != 1 ? "s" : string.Empty)} removed successfully.";
            // Check if the current user was selected.
            if (View.IsCurrentUserSelected && !View.Network.IsPublic)
            {
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Networks/Index");
            }
            // Redirect to the index page.
            return RedirectToPage("/CreatedData/Networks/Details/Accounts/Users/Index", new { id = View.Network.Id });
        }
    }
}
