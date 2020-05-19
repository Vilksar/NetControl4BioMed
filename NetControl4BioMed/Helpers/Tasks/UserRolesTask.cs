using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
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
        public IEnumerable<UserRole> Create(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if there weren't any valid items found.
            if (Items == null)
            {
                // Throw an exception.
                throw new ArgumentException("No valid items could be found with the provided data.");
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
                // Get the IDs of the related entities that appear in the current batch.
                var userIds = batchItems
                    .Where(item => item.User != null)
                    .Select(item => item.User)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                var roleIds = batchItems
                    .Where(item => item.Role != null)
                    .Select(item => item.Role)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Get the related entities that appear in the current batch.
                var users = context.Users
                    .Where(item => userIds.Contains(item.Id));
                var roles = context.Roles
                    .Where(item => roleIds.Contains(item.Id));
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Check if there was no user provided.
                    if (batchItem.User == null || string.IsNullOrEmpty(batchItem.User.Id))
                    {
                        // Throw an exception.
                        throw new ArgumentException("There was no user provided for the user role.");
                    }
                    // Get the user.
                    var user = users
                        .FirstOrDefault(item => item.Id == batchItem.User.Id);
                    // Check if there was no user found.
                    if (user == null)
                    {
                        // Throw an exception.
                        throw new ArgumentException($"There was no user found for the user role.");
                    }
                    // Check if there was no role provided.
                    if (batchItem.Role == null || string.IsNullOrEmpty(batchItem.Role.Id))
                    {
                        // Throw an exception.
                        throw new ArgumentException("There was no role provided for the user role.");
                    }
                    // Get the role.
                    var role = roles
                        .FirstOrDefault(item => item.Id == batchItem.Role.Id);
                    // Check if there was no role found.
                    if (role == null)
                    {
                        // Throw an exception.
                        throw new ArgumentException($"There was no role found for the user role.");
                    }
                    // Try to add the user to the role.
                    var result = Task.Run(() => userManager.AddToRoleAsync(user, role.Name)).Result;
                    // Check if any of the operations has failed.
                    if (!result.Succeeded)
                    {
                        // Define the exception message.
                        var message = string.Empty;
                        // Go over each of the encountered errors.
                        foreach (var error in result.Errors)
                        {
                            // Add the error to the message.
                            message += error.Description;
                        }
                        // Throw an exception.
                        throw new DbUpdateException(message);
                    }
                    // Get the item.
                    var userRole = context.UserRoles
                        .FirstOrDefault(item => item.User.Id == user.Id && item.Role.Id == role.Id);
                    // Check if there was no item found.
                    if (userRole == null)
                    {
                        // Throw an exception.
                        throw new ArgumentException($"There was an error in retrieving the user role from the database after it was added.");
                    }
                    // Yield return the item.
                    yield return userRole;
                }
            }
        }

        /// <summary>
        /// Deletes the items from the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void Delete(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if there weren't any valid items found.
            if (Items == null)
            {
                // Throw an exception.
                throw new ArgumentException("No valid items could be found with the provided data.");
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
                // Get the items with the provided IDs.
                var userRoles = context.UserRoles
                    .Where(item => batchIds.Any(item1 => item1.Item1 == item.User.Id && item1.Item2 == item.Role.Id));
                // Go over each item.
                foreach (var userRole in userRoles.ToList())
                {
                    // Delete it.
                    var result = Task.Run(() => userManager.RemoveFromRoleAsync(userRole.User, userRole.Role.Name)).Result;
                    // Check if the operation has failed.
                    if (!result.Succeeded)
                    {
                        // Define the exception message.
                        var message = string.Empty;
                        // Go over each of the encountered errors.
                        foreach (var error in result.Errors)
                        {
                            // Add the error to the message.
                            message += error.Description;
                        }
                        // Throw an exception.
                        throw new DbUpdateException(message);
                    }
                }
            }
        }
    }
}
