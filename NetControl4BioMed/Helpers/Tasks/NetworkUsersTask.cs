using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
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
    /// Implements a task to update network users in the database.
    /// </summary>
    public class NetworkUsersTask
    {
        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<NetworkUserInputModel> Items { get; set; }

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
                // Get the items in the current batch.
                var batchItems = Items
                    .Skip(index * ApplicationDbContext.BatchSize)
                    .Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the related entities that appear in the current batch.
                var batchNetworkIds = batchItems
                    .Where(item => item.Network != null)
                    .Select(item => item.Network)
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
                var batchNetworks = context.Networks
                    .Where(item => batchNetworkIds.Contains(item.Id));
                var batchUsers = context.Users
                    .Where(item => batchUserIds.Contains(item.Id));
                // Save the items to add.
                var networkUsersToAdd = new List<NetworkUser>();
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Check if there was no network provided.
                    if (batchItem.Network == null || string.IsNullOrEmpty(batchItem.Network.Id))
                    {
                        // Throw an exception.
                        throw new TaskException("There was no network provided.", showExceptionItem, batchItem);
                    }
                    // Get the network.
                    var network = batchNetworks
                        .FirstOrDefault(item => item.Id == batchItem.Network.Id);
                    // Check if there was no network found.
                    if (network == null)
                    {
                        // Throw an exception.
                        throw new TaskException("There was no network found.", showExceptionItem, batchItem);
                    }
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
                    // Define the new item.
                    var networkUser = new NetworkUser
                    {
                        DateTimeCreated = DateTime.UtcNow,
                        NetworkId = network.Id,
                        Network = network,
                        UserId = user.Id,
                        User = user
                    };
                    // Add the item to the list.
                    networkUsersToAdd.Add(networkUser);
                }
                // Create the items.
                await IEnumerableExtensions.CreateAsync(networkUsersToAdd, serviceProvider, token);
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
                // Get the items in the current batch.
                var batchItems = Items
                    .Skip(index * ApplicationDbContext.BatchSize)
                    .Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems
                    .Where(item => item.Network != null && !string.IsNullOrEmpty(item.Network.Id))
                    .Where(item => item.User != null && !string.IsNullOrEmpty(item.User.Id))
                    .Select(item => (item.Network.Id, item.User.Id));
                // Get the items with the provided IDs.
                var networkUsers = context.NetworkUsers
                    .Where(item => batchIds.Any(item1 => item1.Item1 == item.Network.Id && item1.Item2 == item.User.Id));
                // Delete the items.
                await IQueryableExtensions.DeleteAsync(networkUsers, serviceProvider, token);
            }
        }
    }
}
