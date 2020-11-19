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
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Tasks;

namespace NetControl4BioMed.Pages.Content.Created.Analyses.Details.Accounts.Users
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
            public Analysis Analysis { get; set; }

            public IEnumerable<ItemModel> Items { get; set; }

            public bool IsCurrentUserSelected { get; set; }

            public bool AreAllUsersSelected { get; set; }
        }

        public class ItemModel
        {
            public string Email { get; set; }

            public DateTime DateTimeCreated { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id, string emails)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Get the items with the provided ID.
            var items = _context.Analyses
                .Where(item => item.IsPublic || item.AnalysisUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == id);
            // Check if there were no items found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Analysis = items
                    .First()
            };
            // Check if the user does not exist.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: You need to be logged in to remove a user from the analysis.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Details/Accounts/Users/Index", new { id = View.Analysis.Id });
            }
            // Get all of the network users and network user invitations.
            var analysisUsers = _context.AnalysisUsers
                .Include(item => item.User)
                .Where(item => item.Analysis == View.Analysis)
                .Where(item => emails.Contains(item.User.Email))
                .AsEnumerable();
            var analysisUserInvitations = _context.AnalysisUserInvitations
                .Where(item => item.Analysis == View.Analysis)
                .Where(item => emails.Contains(item.Email))
                .AsEnumerable();
            // Define the view items.
            View.Items = analysisUsers
                .Select(item => new ItemModel
                {
                    Email = item.User.Email,
                    DateTimeCreated = item.DateTimeCreated
                })
                .Concat(analysisUserInvitations
                    .Select(item => new ItemModel
                    {
                        Email = item.Email,
                        DateTimeCreated = item.DateTimeCreated
                    }));
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No users have been found with the provided e-mails.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Details/Accounts/Users/Index", new { id = View.Analysis.Id });
            }
            // Define the view status of the selected items.
            View.IsCurrentUserSelected = View.Items
                .Any(item => item.Email == user.Email);
            View.AreAllUsersSelected = !_context.AnalysisUsers
                .Where(item => item.Analysis == View.Analysis)
                .Select(item => item.User.Email)
                .AsEnumerable()
                .Except(View.Items.Select(item => item.Email))
                .Any();
            // Check if there aren't any emails provided.
            if (emails == null || !emails.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid emails have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Details/Accounts/Users/Index", new { id = View.Analysis.Id });
            }
            // Define the input.
            Input = new InputModel
            {
                Id = View.Analysis.Id
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(Input.Id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Get the items with the provided ID.
            var items = _context.Analyses
                .Where(item => item.IsPublic || item.AnalysisUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == Input.Id);
            // Check if there were no items found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Analysis = items
                    .First()
            };
            // Check if the user does not exist.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: You need to be logged in to remove a user from the analysis.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Details/Accounts/Users/Index", new { id = View.Analysis.Id });
            }
            // Get all of the network users and network user invitations.
            var analysisUsers = _context.AnalysisUsers
                .Include(item => item.User)
                .Where(item => item.Analysis == View.Analysis)
                .Where(item => Input.Emails.Contains(item.User.Email))
                .AsEnumerable();
            var analysisUserInvitations = _context.AnalysisUserInvitations
                .Where(item => item.Analysis == View.Analysis)
                .Where(item => Input.Emails.Contains(item.Email))
                .AsEnumerable();
            // Define the view items.
            View.Items = analysisUsers
                .Select(item => new ItemModel
                {
                    Email = item.User.Email,
                    DateTimeCreated = item.DateTimeCreated
                })
                .Concat(analysisUserInvitations
                    .Select(item => new ItemModel
                    {
                        Email = item.Email,
                        DateTimeCreated = item.DateTimeCreated
                    }));
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No users have been found with the provided e-mails.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Details/Accounts/Users/Index", new { id = View.Analysis.Id });
            }
            // Define the status of the selected items.
            View.IsCurrentUserSelected = View.Items
                .Any(item => item.Email == user.Email);
            View.AreAllUsersSelected = !_context.AnalysisUsers
                .Where(item => item.Analysis == View.Analysis)
                .Select(item => item.User.Email)
                .AsEnumerable()
                .Except(View.Items.Select(item => item.Email))
                .Any();
            // Check if there aren't any emails provided.
            if (Input.Emails == null || !Input.Emails.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid emails have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Details/Accounts/Users/Index", new { id = View.Analysis.Id });
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
            var itemCount = analysisUsers.Count() + analysisUserInvitations.Count();
            // Define the new tasks.
            var analysisUsersTask = new AnalysisUsersTask
            {
                Items = analysisUsers.Select(item => new AnalysisUserInputModel
                {
                    Analysis = new AnalysisInputModel
                    {
                        Id = View.Analysis.Id
                    },
                    User = new UserInputModel
                    {
                        Id = item.User.Id
                    }
                })
            };
            var analysisUserInvitationsTask = new AnalysisUserInvitationsTask
            {
                Items = analysisUserInvitations.Select(item => new AnalysisUserInvitationInputModel
                {
                    Analysis = new AnalysisInputModel
                    {
                        Id = View.Analysis.Id
                    },
                    Email = item.Email
                })
            };
            // Try to run the tasks.
            try
            {
                // Run the tasks.
                await analysisUsersTask.DeleteAsync(_serviceProvider, CancellationToken.None);
                await analysisUserInvitationsTask.DeleteAsync(_serviceProvider, CancellationToken.None);
            }
            catch (Exception exception)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, exception.Message);
                // Redisplay the page.
                return Page();
            }
            // Check if all of the users have been selected.
            if (View.AreAllUsersSelected && !View.Analysis.IsPublic)
            {
                // Define a new task.
                var task = new BackgroundTask
                {
                    DateTimeCreated = DateTime.UtcNow,
                    Name = $"{nameof(IContentTaskManager)}.{nameof(IContentTaskManager.DeleteAnalysesAsync)}",
                    IsRecurring = false,
                    Data = JsonSerializer.Serialize(new AnalysesTask
                    {
                        Items = new List<AnalysisInputModel>
                        {
                            new AnalysisInputModel
                            {
                                Id = View.Analysis.Id
                            }
                        }
                    })
                };
                // Mark the task for addition.
                _context.BackgroundTasks.Add(task);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
                // Create a new Hangfire background job.
                var jobId = BackgroundJob.Enqueue<IContentTaskManager>(item => item.DeleteAnalysesAsync(task.Id, CancellationToken.None));
                // Display a message.
                TempData["StatusMessage"] = $"Success: A new background job was created to delete 1 analysis.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Display a message.
            TempData["StatusMessage"] = $"Success: {itemCount} user{(itemCount != 1 ? "s" : string.Empty)} removed successfully.";
            // Check if the current user was selected.
            if (View.IsCurrentUserSelected && !View.Analysis.IsPublic)
            {
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Redirect to the index page.
            return RedirectToPage("/Content/Created/Analyses/Details/Accounts/Users/Index", new { id = View.Analysis.Id });
        }
    }
}
