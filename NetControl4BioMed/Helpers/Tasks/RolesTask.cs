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
                // Use a new role manager instance.
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
                // Get the items in the current batch.
                var batchItems = Items.Skip(index * ApplicationDbContext.BatchSize).Take(ApplicationDbContext.BatchSize);
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Define the corresponding item.
                    var role = new Role
                    {
                        Name = batchItem.Name,
                        DateTimeCreated = DateTime.Now
                    };
                    // Try to create the new role.
                    var result = Task.Run(() => roleManager.CreateAsync(role)).Result;
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
        /// Edits the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void Edit(IServiceProvider serviceProvider, CancellationToken token)
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
                // Use a new role manager instance.
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
                // Get the items in the current batch.
                var batchItems = Items.Skip(index * ApplicationDbContext.BatchSize).Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems.Select(item => item.Id);
                // Get the items corresponding to the current batch.
                var roles = context.Roles
                    .Where(item => batchIds.Contains(item.Id));
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding item.
                    var role = roles.First(item => item.Id == batchItem.Id);
                    // Define a new identity result.
                    var result = IdentityResult.Success;
                    // Check if the name is different from the current one.
                    if (batchItem.Name != role.Name)
                    {
                        // Try to set the new role name.
                        result = Task.Run(() => roleManager.SetRoleNameAsync(role, batchItem.Name)).Result;
                    }
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
                // Use a new role manager instance.
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
                // Get the items in the current batch.
                var batchItems = Items.Skip(index * ApplicationDbContext.BatchSize).Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems.Select(item => item.Id);
                // Get the items with the provided IDs.
                var roles = context.Roles
                    .Where(item => batchIds.Contains(item.Id));
                // Go over each item.
                foreach (var role in roles.ToList())
                {
                    // Delete it.
                    var result = Task.Run(() => roleManager.DeleteAsync(role)).Result;
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
    }
}
