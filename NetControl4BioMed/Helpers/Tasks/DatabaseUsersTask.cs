using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Exceptions;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.InputModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Tasks
{
    /// <summary>
    /// Implements a task to update database users in the database.
    /// </summary>
    public class DatabaseUsersTask
    {
        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<DatabaseUserInputModel> Items { get; set; }

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
                // Get the IDs of the related entities that appear in the current batch.
                var batchDatabaseIds = batchItems
                    .Where(item => item.Database != null)
                    .Select(item => item.Database)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                var batchEmails = batchItems
                    .Where(item => !string.IsNullOrEmpty(item.Email))
                    .Select(item => item.Email)
                    .Distinct();
                // Define the list of items to get.
                var databases = new List<Database>();
                var users = new List<User>();
                // Create a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the related entities that appear in the current batch.
                    databases = context.Databases
                        .Where(item => batchDatabaseIds.Contains(item.Id))
                        .ToList();
                    users = context.Users
                        .Where(item => batchEmails.Contains(item.Email))
                        .ToList();
                }
                // Save the items to add.
                var databaseUsersToAdd = new List<DatabaseUser>();
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Check if there was no database provided.
                    if (batchItem.Database == null || string.IsNullOrEmpty(batchItem.Database.Id))
                    {
                        // Throw an exception.
                        throw new TaskException("There was no database provided.", showExceptionItem, batchItem);
                    }
                    // Get the database.
                    var database = databases
                        .FirstOrDefault(item => item.Id == batchItem.Database.Id);
                    // Check if there was no database found.
                    if (database == null)
                    {
                        // Throw an exception.
                        throw new TaskException("There was no database found.", showExceptionItem, batchItem);
                    }
                    // Check if there was no e-mail provided.
                    if (batchItem.Email == null || string.IsNullOrEmpty(batchItem.Email))
                    {
                        // Throw an exception.
                        throw new TaskException("There was no e-mail provided.", showExceptionItem, batchItem);
                    }
                    // Get the user.
                    var user = users
                        .FirstOrDefault(item => item.Email == batchItem.Email);
                    // Define the new item.
                    var databaseUser = new DatabaseUser
                    {
                        DateTimeCreated = DateTime.UtcNow,
                        DatabaseId = database.Id,
                        UserId = user?.Id,
                        Email = batchItem.Email
                    };
                    // Add the item to the list.
                    databaseUsersToAdd.Add(databaseUser);
                }
                // Create the items.
                await IEnumerableExtensions.CreateAsync(databaseUsersToAdd, serviceProvider, token);
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
                var batchIds = batchItems
                    .Where(item => item.Database != null && !string.IsNullOrEmpty(item.Database.Id))
                    .Where(item => !string.IsNullOrEmpty(item.Email))
                    .Select(item => (item.Database.Id, item.Email));
                // Get the IDs of all individual items.
                var batchDatabaseIds = batchIds
                    .Select(item => item.Item1);
                var batchEmails = batchIds
                    .Select(item => item.Item2);
                // Define the list of items to get.
                var databaseUsers = new List<DatabaseUser>();
                // Create a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    var items = context.DatabaseUsers
                        .Include(item => item.Database)
                        .Where(item => batchDatabaseIds.Contains(item.Database.Id))
                        .Where(item => batchEmails.Contains(item.Email))
                        .AsEnumerable()
                        .Where(item => batchIds.Any(item1 => item1.Item1 == item.Database.Id && item1.Item2 == item.Email))
                        .ToList();
                    // Check if there were no items found.
                    if (items == null || !items.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Get the items found.
                    databaseUsers = items
                        .ToList();
                }
                // Delete the items.
                await IEnumerableExtensions.DeleteAsync(databaseUsers, serviceProvider, token);
            }
        }

        /// <summary>
        /// Updates the corresponding user in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task UpdateUserAsync(IServiceProvider serviceProvider, CancellationToken token)
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
                    .Where(item => item.Database != null && !string.IsNullOrEmpty(item.Database.Id))
                    .Where(item => !string.IsNullOrEmpty(item.Email))
                    .Select(item => (item.Database.Id, item.Email));
                // Get the IDs of the related entities that appear in the current batch.
                var batchDatabaseIds = batchItems
                    .Where(item => item.Database != null)
                    .Select(item => item.Database)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                var batchEmails = batchItems
                    .Where(item => !string.IsNullOrEmpty(item.Email))
                    .Select(item => item.Email)
                    .Distinct();
                // Define the list of items to get.
                var databaseUsers = new List<DatabaseUser>();
                var users = new List<User>();
                // Create a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    var items = context.DatabaseUsers
                        .Where(item => batchDatabaseIds.Contains(item.Database.Id))
                        .Where(item => batchEmails.Contains(item.Email))
                        .AsEnumerable()
                        .Where(item => batchIds.Any(item1 => item1.Item1 == item.Database.Id && item1.Item2 == item.Email))
                        .ToList();
                    // Check if there were no items found.
                    if (items == null || !items.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Get the items found.
                    databaseUsers = items
                        .ToList();
                    // Get the related entities that appear in the current batch.
                    users = context.Users
                        .Where(item => batchEmails.Contains(item.Email))
                        .ToList();
                }
                // Save the items to edit.
                var databaseUsersToEdit = new List<DatabaseUser>();
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding item.
                    var databaseUser = databaseUsers
                        .FirstOrDefault(item => item.Database.Id == batchItem.Database.Id && item.Email == batchItem.Email);
                    // Check if there was no item found.
                    if (databaseUser == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Get the user.
                    var user = users
                        .FirstOrDefault(item => item.Email == batchItem.Email);
                    // Update the database user.
                    databaseUser.UserId = user?.Id;
                    // Add the item to the list.
                    databaseUsersToEdit.Add(databaseUser);
                }
                // Create the items.
                await IEnumerableExtensions.EditAsync(databaseUsersToEdit, serviceProvider, token);
            }
        }
    }
}
