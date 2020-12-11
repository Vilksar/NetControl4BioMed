using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Helpers.Exceptions;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.InputModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Tasks
{
    /// <summary>
    /// Implements a task to update roles in the database.
    /// </summary>
    public class RolesTask
    {
        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<RoleInputModel> Items { get; set; }

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
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id);
                // Check if any of the IDs are repeating in the list.
                if (batchIds.Distinct().Count() != batchIds.Count())
                {
                    // Throw an exception.
                    throw new TaskException("Two or more of the manually provided IDs are duplicated.");
                }
                // Define the list of items to get.
                var validBatchIds = new List<string>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the valid IDs, that do not appear in the database.
                    validBatchIds = batchIds
                        .Except(context.Roles
                            .Where(item => batchIds.Contains(item.Id))
                            .Select(item => item.Id))
                        .ToList();
                }
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new role manager instance.
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
                    // Go over each item in the current batch.
                    foreach (var batchItem in batchItems)
                    {
                        // Check if the ID of the item is not valid.
                        if (!string.IsNullOrEmpty(batchItem.Id) && !validBatchIds.Contains(batchItem.Id))
                        {
                            // Continue.
                            continue;
                        }
                        // Define the corresponding item.
                        var role = new Role
                        {
                            Name = batchItem.Name,
                            DateTimeCreated = DateTime.UtcNow
                        };
                        // Check if there is any ID provided.
                        if (!string.IsNullOrEmpty(batchItem.Id))
                        {
                            // Assign it to the item.
                            role.Id = batchItem.Id;
                        }
                        // Try to create the new role.
                        var result = await roleManager.CreateAsync(role);
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
        /// Edits the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task EditAsync(IServiceProvider serviceProvider, CancellationToken token)
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
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Define the list of items to get.
                var roles = new List<Role>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    var items = context.Roles
                        .Where(item => batchIds.Contains(item.Id));
                    // Check if there were no items found.
                    if (items == null || !items.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Get the items found.
                    roles = items
                        .ToList();
                }
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new role manager instance.
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
                    // Go over each item in the current batch.
                    foreach (var batchItem in batchItems)
                    {
                        // Get the corresponding item.
                        var role = roles
                            .FirstOrDefault(item => item.Id == batchItem.Id);
                        // Check if there was no item found.
                        if (role == null)
                        {
                            // Continue.
                            continue;
                        }
                        // Define a new identity result.
                        var result = IdentityResult.Success;
                        // Check if the name is different from the current one.
                        if (batchItem.Name != role.Name)
                        {
                            // Try to set the new role name.
                            result = await roleManager.SetRoleNameAsync(role, batchItem.Name);
                        }
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
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems.Select(item => item.Id);
                // Define the list of items to get.
                var roles = new List<Role>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    var items = context.Roles
                        .Where(item => batchIds.Contains(item.Id));
                    // Check if there were no items found.
                    if (items == null || !items.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Get the items found.
                    roles = items
                        .ToList();
                }
                // Define a variable to store the error messages.
                var errorMessages = new List<string>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new role manager instance.
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
                    // Go over each item.
                    foreach (var role in roles)
                    {
                        // Delete it.
                        var result = await roleManager.DeleteAsync(role);
                        // Check if any of the operations has failed.
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
