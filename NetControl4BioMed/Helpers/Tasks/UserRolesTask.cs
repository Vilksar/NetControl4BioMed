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
        public void Create(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if there weren't any valid items found.
            if (Items == null || !Items.Any())
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
                var batchItems = Items.Skip(index * ApplicationDbContext.BatchSize).Take(ApplicationDbContext.BatchSize);
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding related entities.
                    var user = context.Users.FirstOrDefault(item => item.Id == batchItem.UserId);
                    // Check if there was no user found.
                    if (user == null)
                    {
                        // Throw an exception.
                        throw new ArgumentException("No user could be found matching the provided ID.");
                    }
                    // Get the corresponding related entities.
                    var role = context.Roles.FirstOrDefault(item => item.Id == batchItem.RoleId);
                    // Check if there was no role found.
                    if (role == null)
                    {
                        // Throw an exception.
                        throw new ArgumentException("No role could be found matching the provided ID.");
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
            if (Items == null || !Items.Any())
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
                var batchItems = Items.Skip(index * ApplicationDbContext.BatchSize).Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems.Select(item => (item.UserId, item.RoleId));
                // Get the items with the provided IDs.
                var userRoles = context.UserRoles
                    .Where(item => batchIds.Any(item1 => item1.UserId == item.User.Id && item1.RoleId == item.Role.Id));
                // Try to delete the items.
                try
                {
                    // Go over each of the item.
                    foreach (var userRole in userRoles.ToList())
                    {
                        // Delete it.
                        Task.Run(() => userManager.RemoveFromRoleAsync(userRole.User, userRole.Role.Name)).Wait();
                    }
                }
                catch (Exception exception)
                {
                    // Throw an exception.
                    throw exception;
                }
            }
        }
    }
}
