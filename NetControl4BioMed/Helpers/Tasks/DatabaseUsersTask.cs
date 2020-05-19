using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
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
        /// <returns>The created items.</returns>
        public IEnumerable<DatabaseUser> Create(IServiceProvider serviceProvider, CancellationToken token)
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
                var batchUserIds = batchItems
                    .Where(item => item.User != null)
                    .Select(item => item.User)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Get the related entities that appear in the current batch.
                var batchDatabases = context.Databases
                    .Where(item => batchDatabaseIds.Contains(item.Id));
                var batchUsers = context.Users
                    .Where(item => batchUserIds.Contains(item.Id));
                // Save the items to add.
                var databaseUsersToAdd = new List<DatabaseUser>();
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Check if there was no database provided.
                    if (batchItem.Database == null || string.IsNullOrEmpty(batchItem.Database.Id))
                    {
                        // Throw an exception.
                        throw new ArgumentException("There was no database provided for the database user.");
                    }
                    // Get the database.
                    var database = batchDatabases
                        .FirstOrDefault(item => item.Id == batchItem.Database.Id);
                    // Check if there was no database found.
                    if (database == null)
                    {
                        // Throw an exception.
                        throw new ArgumentException($"There was no database found for the database user.");
                    }
                    // Check if there was no user provided.
                    if (batchItem.User == null || string.IsNullOrEmpty(batchItem.User.Id))
                    {
                        // Throw an exception.
                        throw new ArgumentException("There was no user provided for the database user.");
                    }
                    // Get the user.
                    var user = batchUsers
                        .FirstOrDefault(item => item.Id == batchItem.User.Id);
                    // Check if there was no user found.
                    if (user == null)
                    {
                        // Throw an exception.
                        throw new ArgumentException($"There was no user found for the database user.");
                    }
                    // Define the new item.
                    var databaseUser = new DatabaseUser
                    {
                        DateTimeCreated = DateTime.Now,
                        DatabaseId = database.Id,
                        Database = database,
                        UserId = user.Id,
                        User = user
                    };
                    // Add the item to the list.
                    databaseUsersToAdd.Add(databaseUser);
                }
                // Create the items.
                IEnumerableExtensions.Create(databaseUsersToAdd, context, token);
                // Go over each item.
                foreach (var databaseUser in databaseUsersToAdd)
                {
                    // Yield return it.
                    yield return databaseUser;
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
                // Get the items in the current batch.
                var batchItems = Items
                    .Skip(index * ApplicationDbContext.BatchSize)
                    .Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems
                    .Where(item => item.Database != null && !string.IsNullOrEmpty(item.Database.Id))
                    .Where(item => item.User != null && !string.IsNullOrEmpty(item.User.Id))
                    .Select(item => (item.Database.Id, item.User.Id));
                // Get the items with the provided IDs.
                var databaseUsers = context.DatabaseUsers
                    .Where(item => batchIds.Any(item1 => item1.Item1 == item.Database.Id && item1.Item2 == item.User.Id));
                // Delete the items.
                IQueryableExtensions.Delete(databaseUsers, context, token);
            }
        }
    }
}
