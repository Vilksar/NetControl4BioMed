using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
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
                // Create a new scope.
                using var scope = serviceProvider.CreateScope();
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Use a new user manager instance.
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                // Get the items in the current batch.
                var batchItems = Items
                    .Skip(index * ApplicationDbContext.BatchSize)
                    .Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the related entities that appear in the current batch.
                var batchUserIds = batchItems
                    .Where(item => item.User != null)
                    .Select(item => item.User)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                var batchRoleIds = batchItems
                    .Where(item => item.Role != null)
                    .Select(item => item.Role)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Get the related entities that appear in the current batch.
                var batchUsers = context.Users
                    .Where(item => batchUserIds.Contains(item.Id));
                var batchRoles = context.Roles
                    .Where(item => batchRoleIds.Contains(item.Id));
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
                    var user = batchUsers
                        .FirstOrDefault(item => item.Id == batchItem.User.Id);
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
                    var role = batchRoles
                        .FirstOrDefault(item => item.Id == batchItem.Role.Id);
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
                    // Get the item.
                    var userRole = context.UserRoles
                        .FirstOrDefault(item => item.User.Id == user.Id && item.Role.Id == role.Id);
                    // Check if there was no item found.
                    if (userRole == null)
                    {
                        // Throw an exception.
                        throw new TaskException("There was an error in retrieving the user role from the database after it was added.");
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
                // Create a new scope.
                using var scope = serviceProvider.CreateScope();
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Use a new user manager instance.
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                // Get the items in the current batch.
                var batchItems = Items
                    .Skip(index * ApplicationDbContext.BatchSize)
                    .Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems
                    .Where(item => item.User != null && !string.IsNullOrEmpty(item.User.Id))
                    .Where(item => item.Role != null && !string.IsNullOrEmpty(item.Role.Id))
                    .Select(item => (item.User.Id, item.Role.Id));
                // Get the IDs of all individual items.
                var batchIdsUserIds = batchIds
                    .Select(item => item.Item1);
                var batchIdsRoleIds = batchIds
                    .Select(item => item.Item2);
                // Get the items with the provided IDs.
                var userRoles = context.UserRoles
                    .Include(item => item.User)
                    .Include(item => item.Role)
                    .Where(item => batchIdsUserIds.Contains(item.User.Id))
                    .Where(item => batchIdsRoleIds.Contains(item.Role.Id))
                    .AsEnumerable()
                    .Where(item => batchIds.Any(item1 => item1.Item1 == item.User.Id && item1.Item2 == item.Role.Id))
                    .AsQueryable();
                // Define a variable to store the error messages.
                var errorMessages = new List<string>();
                // Go over each item.
                foreach (var userRole in userRoles.ToList())
                {
                    // Delete it.
                    var result = await userManager.RemoveFromRoleAsync(userRole.User, userRole.Role.Name);
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
