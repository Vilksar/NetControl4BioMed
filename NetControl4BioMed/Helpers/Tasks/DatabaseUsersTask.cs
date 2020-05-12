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
                // Get the items in the current batch.
                var batchItems = Items.Skip(index * ApplicationDbContext.BatchSize).Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the related entities that appear in the current batch.
                var databaseIds = batchItems.Select(item => item.DatabaseId);
                var userIds = batchItems.Select(item => item.UserId);
                // Get the related entities that appear in the current batch.
                var databases = context.Databases.Where(item => databaseIds.Contains(item.Id));
                var users = context.Users.Where(item => userIds.Contains(item.Id));
                // Define the items corresponding to the current batch.
                var databaseUsers = batchItems.Select(item => new DatabaseUser
                {
                    DatabaseId = item.DatabaseId,
                    Database = databases.First(item1 => item1.Id == item.DatabaseId),
                    UserId = item.UserId,
                    User = users.First(item1 => item1.Id == item.UserId),
                    DateTimeCreated = DateTime.Now
                });
                // Create the items.
                IEnumerableExtensions.Create(databaseUsers, context, token);
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
                // Get the items in the current batch.
                var batchItems = Items.Skip(index * ApplicationDbContext.BatchSize).Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems.Select(item => (item.DatabaseId, item.UserId));
                // Get the items with the provided IDs.
                var databaseUsers = context.DatabaseUsers
                    .Where(item => batchIds.Any(item1 => item1.DatabaseId == item.Database.Id && item1.UserId == item.User.Id));
                // Delete the items.
                IQueryableExtensions.Delete(databaseUsers, context, token);
            }
        }
    }
}
