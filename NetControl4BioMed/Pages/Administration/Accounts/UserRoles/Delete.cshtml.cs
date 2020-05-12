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
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Tasks;

namespace NetControl4BioMed.Pages.Administration.Accounts.UserRoles
{
    [Authorize(Roles = "Administrator")]
    public class DeleteModel : PageModel
    {
        private readonly IServiceProvider _serviceProvider;

        public DeleteModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public IEnumerable<string> UserIds { get; set; }

            public IEnumerable<string> RoleIds { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public IEnumerable<UserRole> Items { get; set; }

            public bool IsCurrentUserSelected { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(IEnumerable<string> userIds, IEnumerable<string> roleIds)
        {
            // Check if there aren't any e-mails or IDs provided.
            if (userIds == null || roleIds == null || !userIds.Any() || !roleIds.Any() || userIds.Count() != roleIds.Count())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid e-mails or IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/UserRoles/Index");
            }
            // Create a new scope.
            using var scope = _serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Use a new user manager instance.
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            // Get the IDs of all selected users and roles.
            var ids = userIds.Zip(roleIds);
            // Define the view.
            View = new ViewModel
            {
                Items = context.UserRoles
                    .Where(item => userIds.Contains(item.User.Id) && roleIds.Contains(item.Role.Id))
                    .Include(item => item.User)
                    .Include(item => item.Role)
                    .AsEnumerable()
                    .Where(item => ids.Contains((item.User.Id, item.Role.Id))),
                IsCurrentUserSelected = userIds.Contains((await userManager.GetUserAsync(User)).Id)
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No items have been found with the provided IDs.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/UserRoles/Index");
            }
            // Check if there would be no administrator users after removal.
            if (View.Items.Where(item => item.Role.Name == "Administrator").Count() == (await userManager.GetUsersInRoleAsync("Administrator")).Count())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No administrator users would remain after deleting the selected user roles.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/UserRoles/Index");
            }
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Check if there aren't any e-mails or IDs provided.
            if (Input.UserIds == null || Input.RoleIds == null || !Input.UserIds.Any() || !Input.RoleIds.Any() || Input.UserIds.Count() != Input.RoleIds.Count())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/UserRoles/Index");
            }
            // Create a new scope.
            using var scope = _serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Use a new user manager instance.
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            // Use a new sign in manager instance.
            var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<User>>();
            // Get the IDs of all selected users and roles.
            var ids = Input.UserIds.Zip(Input.RoleIds);
            // Define the view.
            View = new ViewModel
            {
                Items = context.UserRoles
                    .Where(item => Input.UserIds.Contains(item.User.Id) && Input.RoleIds.Contains(item.Role.Id))
                    .Include(item => item.User)
                    .Include(item => item.Role)
                    .AsEnumerable()
                    .Where(item => ids.Contains((item.User.Id, item.Role.Id))),
                IsCurrentUserSelected = Input.UserIds.Contains((await userManager.GetUserAsync(User)).Id)
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No items have been found with the provided IDs.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/UserRoles/Index");
            }
            // Check if there would be no administrator users after removal.
            if (View.Items.Where(item => item.Role.Name == "Administrator").Count() == (await userManager.GetUsersInRoleAsync("Administrator")).Count())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No administrator users would remain after deleting the selected user roles.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/UserRoles/Index");
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
                DateTimeCreated = DateTime.Now,
                Name = $"{nameof(IAdministrationTaskManager)}.{nameof(IAdministrationTaskManager.DeleteUserRoles)}",
                IsRecurring = false,
                Data = JsonSerializer.Serialize(new UserRolesTask
                {
                    Items = View.Items.Select(item => new UserRoleInputModel
                    {
                        UserId = item.User.Id,
                        RoleId = item.Role.Id
                    })
                })
            };
            // Mark the task for addition.
            context.BackgroundTasks.Add(task);
            // Save the changes to the database.
            context.SaveChanges();
            // Create a new Hangfire background job.
            var jobId = BackgroundJob.Enqueue<IAdministrationTaskManager>(item => item.DeleteUserRoles(task.Id, CancellationToken.None));
            // Check if the current user is selected.
            if (View.IsCurrentUserSelected)
            {
                // Log out the user.
                await signInManager.SignOutAsync();
                // Display a message.
                TempData["StatusMessage"] = $"Info: A new background job was created to delete {itemCount} user role{(itemCount != 1 ? "s" : string.Empty)}. The roles assigned to your account will change, so you have been signed out.";
                // Redirect to the index page.
                return RedirectToPage("/Index");
            }
            // Display a message.
            TempData["StatusMessage"] = $"Success: A new background job was created to delete {itemCount} user role{(itemCount != 1 ? "s" : string.Empty)}.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Accounts/UserRoles/Index");
        }
    }
}
