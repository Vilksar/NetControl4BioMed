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
    /// Implements a task to update network user invitations in the database.
    /// </summary>
    public class NetworkUserInvitationsTask
    {
        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<NetworkUserInvitationInputModel> Items { get; set; }

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
                var batchNetworkIds = batchItems
                    .Where(item => item.Network != null)
                    .Select(item => item.Network)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                var batchUserEmails = batchItems
                    .Where(item => !string.IsNullOrEmpty(item.Email))
                    .Select(item => item.Email)
                    .Distinct();
                // Define the list of items to get.
                var networks = new List<Network>();
                var users = new List<User>();
                // Create a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the related entities that appear in the current batch.
                    networks = context.Networks
                        .Where(item => batchNetworkIds.Contains(item.Id))
                        .ToList();
                    users = context.Users
                        .Where(item => batchUserEmails.Contains(item.Email))
                        .ToList();
                }
                // Save the items to add.
                var networkUserInvitationsToAdd = new List<NetworkUserInvitation>();
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
                    var network = networks
                        .FirstOrDefault(item => item.Id == batchItem.Network.Id);
                    // Check if there was no network found.
                    if (network == null)
                    {
                        // Throw an exception.
                        throw new TaskException("There was no network found.", showExceptionItem, batchItem);
                    }
                    // Check if there was no e-mail provided.
                    if (string.IsNullOrEmpty(batchItem.Email))
                    {
                        // Throw an exception.
                        throw new TaskException("There was no e-mail provided.", showExceptionItem, batchItem);
                    }
                    // Try to get the user.
                    var user = users
                        .FirstOrDefault(item => item.Email == batchItem.Email);
                    // Check if there was a user found.
                    if (user != null)
                    {
                        // Throw an exception.
                        throw new TaskException("The user with the provided e-mail already exists.", showExceptionItem, batchItem);
                    }
                    // Define the new item.
                    var networkUserInvitation = new NetworkUserInvitation
                    {
                        DateTimeCreated = DateTime.UtcNow,
                        NetworkId = network.Id,
                        Network = network,
                        Email = batchItem.Email
                    };
                    // Add the item to the list.
                    networkUserInvitationsToAdd.Add(networkUserInvitation);
                }
                // Create the items.
                await IEnumerableExtensions.CreateAsync(networkUserInvitationsToAdd, serviceProvider, token);
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
                    .Where(item => item.Network != null && !string.IsNullOrEmpty(item.Network.Id))
                    .Where(item => !string.IsNullOrEmpty(item.Email))
                    .Select(item => (item.Network.Id, item.Email));
                // Get the IDs of all individual items.
                var batchNetworkIds = batchIds
                    .Select(item => item.Item1);
                var batchEmails = batchIds
                    .Select(item => item.Email);
                // Define the list of items to get.
                var networkUserInvitations = new List<NetworkUserInvitation>();
                // Create a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    var items = context.NetworkUserInvitations
                        .Include(item => item.Network)
                        .Where(item => batchNetworkIds.Contains(item.Network.Id))
                        .Where(item => batchEmails.Contains(item.Email))
                        .AsEnumerable()
                        .Where(item => batchIds.Any(item1 => item1.Item1 == item.Network.Id && item1.Item2 == item.Email))
                        .ToList();
                    // Check if there were no items found.
                    if (items == null || !items.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Get the items found.
                    networkUserInvitations = items
                        .ToList();
                }
                // Delete the items.
                await IEnumerableExtensions.DeleteAsync(networkUserInvitations, serviceProvider, token);
            }
        }
    }
}
