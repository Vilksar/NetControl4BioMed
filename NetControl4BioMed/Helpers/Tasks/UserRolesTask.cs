using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Exceptions;
using NetControl4BioMed.Helpers.InputModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Tasks
{
    /// <summary>
    /// Implements a task to update user roles in the database.
    /// </summary>
    public class UserRolesTask
    {
        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<UserRoleInputModel> Items { get; set; }

        /// <summary>
        /// Creates the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task CreateAsync(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if there weren't any valid items found.
            if (Items == null)
            {
                // Throw an exception.
                throw new TaskException("No valid items could be found with the provided data.");
            }
            // Check if the exception item should be shown.
            var showExceptionItem = Items.Count() > 1;
            // Get the total number of batches.
            var count = Math.Ceiling((double)Items.Count() / ApplicationDbContext.BatchSize);
            // Go over each batch.
            for (var index = 0; index < count; index++)
            {
                // Check if the cancellation was requested.
                if (token.IsCancellationRequested)
                {
                    // Break.
                    break;
                }
                // Get the items in the current batch.
                var batchItems = Items
                    .Skip(index * ApplicationDbContext.BatchSize)
                    .Take(ApplicationDbContext.BatchSize);
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use new user manager and role manager instances.
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
                    // Go over each item in the current batch.
                    foreach (var batchItem in batchItems)
                    {
                        // Check if there was no user provided.
                        if (batchItem.User == null || string.IsNullOrEmpty(batchItem.User.Id))
                        {
                            // Throw an exception.
                            throw new TaskException("There was no user provided.", showExceptionItem, batchItem);
                        }
                        // Get the user.
                        var user = await userManager.FindByIdAsync(batchItem.User.Id);
                        // Check if there was no user found.
                        if (user == null)
                        {
                            // Throw an exception.
                            throw new TaskException("There was no user found.", showExceptionItem, batchItem);
                        }
                        // Check if there was no role provided.
                        if (batchItem.Role == null || string.IsNullOrEmpty(batchItem.Role.Id))
                        {
                            // Throw an exception.
                            throw new TaskException("There was no role provided.", showExceptionItem, batchItem);
                        }
                        // Get the role.
                        var role = await roleManager.FindByIdAsync(batchItem.Role.Id);
                        // Check if there was no role found.
                        if (role == null)
                        {
                            // Throw an exception.
                            throw new TaskException("There was no role found.", showExceptionItem, batchItem);
                        }
                        // Try to add the user to the role.
                        var result = await userManager.AddToRoleAsync(user, role.Name);
                        // Check if any of the operations has failed.
                        if (!result.Succeeded)
                        {
                            // Define the exception messages.
                            var messages = result.Errors
                                .Select(item => item.Description);
                            // Throw an exception.
                            throw new TaskException(string.Join(" ", messages), showExceptionItem, batchItem);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Deletes the items from the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteAsync(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if there weren't any valid items found.
            if (Items == null)
            {
                // Throw an exception.
                throw new TaskException("No valid items could be found with the provided data.");
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)Items.Count() / ApplicationDbContext.BatchSize);
            // Go over each batch.
            for (var index = 0; index < count; index++)
            {
                // Check if the cancellation was requested.
                if (token.IsCancellationRequested)
                {
                    // Break.
                    break;
                }
                // Get the items in the current batch.
                var batchItems = Items
                    .Skip(index * ApplicationDbContext.BatchSize)
                    .Take(ApplicationDbContext.BatchSize);
                // Define a variable to store the error messages.
                var errorMessages = new List<string>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use new user manager and role manager instances.
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
                    // Go over each item.
                    foreach (var batchItem in batchItems)
                    {
                        // Check if there was no user provided.
                        if (batchItem.User == null || string.IsNullOrEmpty(batchItem.User.Id))
                        {
                            // Continue.
                            continue;
                        }
                        // Get the user.
                        var user = await userManager.FindByIdAsync(batchItem.User.Id);
                        // Check if there was no user found.
                        if (user == null)
                        {
                            // Continue.
                            continue;
                        }
                        // Check if there was no role provided.
                        if (batchItem.Role == null || string.IsNullOrEmpty(batchItem.Role.Id))
                        {
                            // Continue.
                            continue;
                        }
                        // Get the role.
                        var role = await roleManager.FindByIdAsync(batchItem.Role.Id);
                        // Check if there was no role found.
                        if (role == null)
                        {
                            // Continue.
                            continue;
                        }
                        // Delete it.
                        var result = await userManager.RemoveFromRoleAsync(user, role.Name);
                        // Check if the operation has failed.
                        if (!result.Succeeded)
                        {
                            // Define the exception messages.
                            var messages = result.Errors
                                .Select(item => item.Description);
                            // Add the exception messages to the error messages.
                            errorMessages.AddRange(messages);
                        }
                    }
                }
                // Check if there have been any error messages.
                if (errorMessages.Any())
                {
                    // Throw an exception.
                    throw new TaskException(string.Join(" ", errorMessages));
                }
            }
        }
    }
}
