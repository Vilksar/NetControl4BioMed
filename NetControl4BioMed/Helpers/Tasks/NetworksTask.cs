using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Exceptions;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Tasks
{
    /// <summary>
    /// Implements a task to update networks in the database.
    /// </summary>
    public class NetworksTask
    {
        /// <summary>
        /// Gets the maximum number of retries for a task.
        /// </summary>
        private static int NumberOfRetries { get; } = 2;

        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<NetworkInputModel> Items { get; set; }

        /// <summary>
        /// Gets or sets the HTTP context scheme.
        /// </summary>
        public string Scheme { get; set; }

        /// <summary>
        /// Gets or sets the HTTP context host value.
        /// </summary>
        public string HostValue { get; set; }

        /// <summary>
        /// Creates the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task<IEnumerable<string>> CreateAsync(IServiceProvider serviceProvider, CancellationToken token)
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
            // Save the IDs of the created items.
            var ids = Enumerable.Empty<string>();
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
                    throw new TaskException("One or more of the manually provided IDs are duplicated.");
                }
                // Get the IDs of the related entities that appear in the current batch.
                var batchUserIds = batchItems
                    .Where(item => item.NetworkUsers != null)
                    .Select(item => item.NetworkUsers)
                    .SelectMany(item => item)
                    .Where(item => item.User != null)
                    .Select(item => item.User)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                var batchDatabaseIds = batchItems
                    .Where(item => item.NetworkDatabases != null)
                    .Select(item => item.NetworkDatabases)
                    .SelectMany(item => item)
                    .Where(item => item.Database != null)
                    .Select(item => item.Database)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                var batchProteinCollectionIds = batchItems
                    .Where(item => item.NetworkProteinCollections != null)
                    .Select(item => item.NetworkProteinCollections)
                    .SelectMany(item => item)
                    .Where(item => item.ProteinCollection != null)
                    .Select(item => item.ProteinCollection)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Define the list of items to get.
                var users = new List<User>();
                var databases = new List<Database>();
                var proteinCollections = new List<ProteinCollection>();
                var validBatchIds = new List<string>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the related entities that appear in the current batch.
                    users = context.Users
                        .Where(item => batchUserIds.Contains(item.Id))
                        .ToList();
                    databases = context.Databases
                        .Where(item => batchDatabaseIds.Contains(item.Id))
                        .ToList();
                    proteinCollections = context.ProteinCollections
                        .Where(item => batchProteinCollectionIds.Contains(item.Id))
                        .ToList();
                    // Get the valid IDs, that do not appear in the database.
                    validBatchIds = batchIds
                        .Except(context.Networks
                            .Where(item => batchIds.Contains(item.Id))
                            .Select(item => item.Id))
                        .ToList();
                }
                // Save the items to add.
                var networksToAdd = new List<Network>();
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Check if the ID of the item is not valid.
                    if (!string.IsNullOrEmpty(batchItem.Id) && !validBatchIds.Contains(batchItem.Id))
                    {
                        // Continue.
                        continue;
                    }
                    // Check if there are no network users provided.
                    if (!batchItem.IsPublic && (batchItem.NetworkUsers == null || !batchItem.NetworkUsers.Any()))
                    {
                        // Throw an exception.
                        throw new TaskException("There were no network users provided and the network is not public.", showExceptionItem, batchItem);
                    }
                    // Get the network users.
                    var networkUsers = batchItem.NetworkUsers
                        .Where(item => item.User != null && !string.IsNullOrEmpty(item.Email))
                        .Where(item => !string.IsNullOrEmpty(item.User.Id))
                        .Select(item => (item.User.Id, item.Email))
                        .Distinct()
                        .Where(item => users.Any(item1 => item1.Id == item.Item1))
                        .Select(item => new NetworkUser
                        {
                            DateTimeCreated = DateTime.UtcNow,
                            UserId = item.Item1,
                            Email = item.Item2
                        });
                    // Check if there were no network users found.
                    if (!batchItem.IsPublic && (networkUsers == null || !networkUsers.Any()))
                    {
                        // Throw an exception.
                        throw new TaskException("There were no network users found and the network is not public.", showExceptionItem, batchItem);
                    }
                    // Get the network databases.
                    var networkDatabases = batchItem.NetworkDatabases != null ?
                        batchItem.NetworkDatabases
                            .Where(item => item.Database != null)
                            .Where(item => !string.IsNullOrEmpty(item.Database.Id))
                            .Where(item => item.Type == "Protein" || item.Type == "Interaction")
                            .Select(item => (item.Database.Id, item.Type))
                            .Distinct()
                            .Where(item => databases.Any(item1 => item1.Id == item.Item1))
                            .Select(item => new NetworkDatabase
                            {
                                DatabaseId = item.Item1,
                                Type = EnumerationExtensions.GetEnumerationValue<NetworkDatabaseType>(item.Item2)
                            }) :
                        Enumerable.Empty<NetworkDatabase>();
                    // Get the network protein collections.
                    var networkProteinCollections = batchItem.NetworkProteinCollections != null ?
                        batchItem.NetworkProteinCollections
                            .Where(item => item.ProteinCollection != null)
                            .Where(item => !string.IsNullOrEmpty(item.ProteinCollection.Id))
                            .Where(item => item.Type == "Seed")
                            .Select(item => (item.ProteinCollection.Id, item.Type))
                            .Distinct()
                            .Where(item => proteinCollections.Any(item1 => item1.Id == item.Item1))
                            .Select(item => new NetworkProteinCollection
                            {
                                ProteinCollectionId = item.Item1,
                                Type = EnumerationExtensions.GetEnumerationValue<NetworkProteinCollectionType>(item.Item2)
                            }) :
                        Enumerable.Empty<NetworkProteinCollection>();
                    // Define the new item.
                    var network = new Network
                    {
                        DateTimeCreated = DateTime.UtcNow,
                        DateTimeToDelete = DateTime.Today + TimeSpan.FromDays(ApplicationDbContext.DaysBeforeDelete),
                        Name = batchItem.Name,
                        Description = batchItem.Description,
                        IsPublic = batchItem.IsPublic,
                        IsDemonstration = false,
                        Status = NetworkStatus.Defined,
                        Log = JsonSerializer.Serialize(Enumerable.Empty<string>()),
                        Data = batchItem.Data,
                        NetworkDatabases = networkDatabases.ToList(),
                        NetworkUsers = networkUsers.ToList(),
                        NetworkProteinCollections = networkProteinCollections.ToList()
                    };
                    // Try to get the algorithm.
                    try
                    {
                        // Get the algorithm.
                        network.Algorithm = EnumerationExtensions.GetEnumerationValue<NetworkAlgorithm>(batchItem.Algorithm);
                    }
                    catch (Exception exception)
                    {
                        // Throw a new exception.
                        throw new TaskException(exception.Message, showExceptionItem, batchItem);
                    }
                    // Append a message to the log.
                    network.Log = network.AppendToLog("The network has been defined and stored in the database.");
                    // Check if there is any ID provided.
                    if (!string.IsNullOrEmpty(batchItem.Id))
                    {
                        // Assign it to the item.
                        network.Id = batchItem.Id;
                    }
                    // Add the new item to the list.
                    networksToAdd.Add(network);
                }
                // Create the items.
                await IEnumerableExtensions.CreateAsync(networksToAdd, serviceProvider, token);
                // Add the IDs of the created items to the list.
                ids = ids.Concat(networksToAdd.Select(item => item.Id));
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Define the new background task.
                    var backgroundTask = new BackgroundTask
                    {
                        DateTimeCreated = DateTime.UtcNow,
                        Name = $"{nameof(IContentTaskManager)}.{nameof(IContentTaskManager.GenerateNetworksAsync)}",
                        IsRecurring = false,
                        Data = JsonSerializer.Serialize(new NetworksTask
                        {
                            Scheme = Scheme,
                            HostValue = HostValue,
                            Items = networksToAdd.Select(item => new NetworkInputModel
                            {
                                Id = item.Id
                            })
                        })
                    };
                    // Mark the task for addition.
                    context.BackgroundTasks.Add(backgroundTask);
                    // Save the changes to the database.
                    await context.SaveChangesAsync();
                    // Create a new Hangfire background job.
                    BackgroundJob.Enqueue<IContentTaskManager>(item => item.GenerateNetworksAsync(backgroundTask.Id, CancellationToken.None));
                }
            }
            // Return the IDs of the created items.
            return ids;
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
                var batchIds = batchItems.Select(item => item.Id);
                // Define the list of items to get.
                var networks = new List<Network>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    var items = context.Networks
                        .Include(item => item.NetworkUsers)
                        .Where(item => batchIds.Contains(item.Id));
                    // Check if there were no items found.
                    if (items == null || !items.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Get the items found.
                    networks = items
                        .ToList();
                }
                // Save the items to add.
                var networksToEdit = new List<Network>();
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding item.
                    var network = networks.FirstOrDefault(item => item.Id == batchItem.Id);
                    // Check if there was no item found.
                    if (network == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Check if there were no network users found.
                    if (!batchItem.IsPublic && (network.NetworkUsers == null || !network.NetworkUsers.Any()))
                    {
                        // Throw an exception.
                        throw new TaskException("There were no network users found, so the network must be public.", showExceptionItem, batchItem);
                    }
                    // Check if the network is not public.
                    if (batchItem.IsDemonstration && !batchItem.IsPublic)
                    {
                        // Throw an exception.
                        throw new TaskException("The network must be public in order to be a demonstration.", showExceptionItem, batchItem);
                    }
                    // Update the data.
                    network.Name = batchItem.Name;
                    network.Description = batchItem.Description;
                    network.IsPublic = batchItem.IsPublic;
                    network.IsDemonstration = batchItem.IsDemonstration;
                    // Append a message to the log.
                    network.Log = network.AppendToLog("The network details have been updated.");
                    // Add the item to the list.
                    networksToEdit.Add(network);
                }
                // Edit the items.
                await IEnumerableExtensions.EditAsync(networksToEdit, serviceProvider, token);
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
                var networks = new List<Network>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    var items = context.Networks
                        .Where(item => batchIds.Contains(item.Id));
                    // Check if there were no items found.
                    if (items == null || !items.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Get the items found.
                    networks = items
                        .ToList();
                }
                // Get the IDs of the items.
                var networkIds = networks
                    .Select(item => item.Id);
                // Delete the dependent entities.
                await NetworkExtensions.DeleteDependentAnalysesAsync(networkIds, serviceProvider, token);
                await NetworkExtensions.DeleteDependentInteractionsAsync(networkIds, serviceProvider, token);
                await NetworkExtensions.DeleteDependentProteinsAsync(networkIds, serviceProvider, token);
                // Delete the related entities.
                await NetworkExtensions.DeleteRelatedEntitiesAsync<NetworkProteinCollection>(networkIds, serviceProvider, token);
                await NetworkExtensions.DeleteRelatedEntitiesAsync<NetworkInteraction>(networkIds, serviceProvider, token);
                await NetworkExtensions.DeleteRelatedEntitiesAsync<NetworkProtein>(networkIds, serviceProvider, token);
                await NetworkExtensions.DeleteRelatedEntitiesAsync<NetworkDatabase>(networkIds, serviceProvider, token);
                await NetworkExtensions.DeleteRelatedEntitiesAsync<NetworkUser>(networkIds, serviceProvider, token);
                // Delete the items.
                await IEnumerableExtensions.DeleteAsync(networks, serviceProvider, token);
            }
        }

        /// <summary>
        /// Extends the time until the items are automatically deleted.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task ExtendTimeUntilDeleteAsync(IServiceProvider serviceProvider, CancellationToken token)
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
                var batchIds = batchItems.Select(item => item.Id);
                // Define the list of items to get.
                var networks = new List<Network>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    var items = context.Networks
                        .Where(item => batchIds.Contains(item.Id));
                    // Check if there were no items found.
                    if (items == null || !items.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Get the items found.
                    networks = items
                        .ToList();
                }
                // Save the items to add.
                var networksToEdit = new List<Network>();
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding item.
                    var network = networks.FirstOrDefault(item => item.Id == batchItem.Id);
                    // Check if there was no item found.
                    if (network == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Update the data.
                    network.DateTimeToDelete = DateTime.Today + TimeSpan.FromDays(ApplicationDbContext.DaysBeforeDelete);
                    // Add the item to the list.
                    networksToEdit.Add(network);
                }
                // Edit the items.
                await IEnumerableExtensions.EditAsync(networksToEdit, serviceProvider, token);
            }
        }

        /// <summary>
        /// Generates the items.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task GenerateAsync(IServiceProvider serviceProvider, CancellationToken token)
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
                    .Take(ApplicationDbContext.BatchSize)
                    .ToList();
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems.Select(item => item.Id);
                // Define the batch items.
                var batchNetworks = new List<Network>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    batchNetworks = context.Networks
                        .Where(item => batchIds.Contains(item.Id))
                        .ToList();
                }
                // Define the current retry.
                var currentRetry = 0;
                // Go over each item in the current batch.
                for (var batchItemIndex = 0; batchItemIndex < batchItems.Count(); batchItemIndex++)
                {
                    // Get the corresponding batch item.
                    var batchItem = batchItems[batchItemIndex];
                    // Get the corresponding item.
                    var batchNetwork = batchNetworks
                        .FirstOrDefault(item => item.Id == batchItem.Id);
                    // Check if there was no item found.
                    if (batchNetwork == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Check if the status is not valid.
                    if (batchNetwork.Status != NetworkStatus.Defined && batchNetwork.Status != NetworkStatus.Generating)
                    {
                        // Use a new scope.
                        using (var scope = serviceProvider.CreateScope())
                        {
                            // Use a new context instance.
                            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            // Reload the network.
                            var network = context.Networks
                                .FirstOrDefault(item => item.Id == batchNetwork.Id);
                            // Check if there was no item found.
                            if (network == null)
                            {
                                // Continue.
                                continue;
                            }
                            // Update the status of the item.
                            network.Status = NetworkStatus.Error;
                            // Add a message to the log.
                            network.Log = network.AppendToLog("The status of the network is not valid in order to be generated.");
                            // Edit the network.
                            await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
                        }
                        // Continue.
                        continue;
                    }
                    // Try to generate the network.
                    try
                    {
                        // Use a new scope.
                        using (var scope = serviceProvider.CreateScope())
                        {
                            // Use a new context instance.
                            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            // Reload the network.
                            var network = context.Networks
                                .FirstOrDefault(item => item.Id == batchNetwork.Id);
                            // Check if there was no item found.
                            if (network == null)
                            {
                                // Continue.
                                continue;
                            }
                            // Update the status of the item.
                            network.Status = NetworkStatus.Generating;
                            // Add a message to the log.
                            network.Log = network.AppendToLog("The network is now generating.");
                            // Edit the network.
                            await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
                        }
                        // Check the algorithm to generate the network.
                        switch (batchNetwork.Algorithm)
                        {
                            case NetworkAlgorithm.None:
                                // Run the algorithm on the network.
                                await Algorithms.Networks.None.Algorithm.Run(batchNetwork.Id, serviceProvider, token);
                                // End the switch.
                                break;
                            case NetworkAlgorithm.Neighbors:
                                // Run the algorithm on the network.
                                await Algorithms.Networks.Neighbors.Algorithm.Run(batchNetwork.Id, serviceProvider, token);
                                // End the switch.
                                break;
                            case NetworkAlgorithm.Gap0:
                                // Run the algorithm on the network.
                                await Algorithms.Networks.Gap.Algorithm.Run(batchNetwork.Id, 0, serviceProvider, token);
                                // End the switch.
                                break;
                            case NetworkAlgorithm.Gap1:
                                // Run the algorithm on the network.
                                await Algorithms.Networks.Gap.Algorithm.Run(batchNetwork.Id, 1, serviceProvider, token);
                                // End the switch.
                                break;
                            case NetworkAlgorithm.Gap2:
                                // Run the algorithm on the network.
                                await Algorithms.Networks.Gap.Algorithm.Run(batchNetwork.Id, 2, serviceProvider, token);
                                // End the switch.
                                break;
                            case NetworkAlgorithm.Gap3:
                                // Run the algorithm on the network.
                                await Algorithms.Networks.Gap.Algorithm.Run(batchNetwork.Id, 3, serviceProvider, token);
                                // End the switch.
                                break;
                            case NetworkAlgorithm.Gap4:
                                // Run the algorithm on the network.
                                await Algorithms.Networks.Gap.Algorithm.Run(batchNetwork.Id, 4, serviceProvider, token);
                                // End the switch.
                                break;
                            default:
                                // Use a new scope.
                                using (var scope = serviceProvider.CreateScope())
                                {
                                    // Use a new context instance.
                                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                                    // Reload the network.
                                    var network = context.Networks
                                        .FirstOrDefault(item => item.Id == batchNetwork.Id);
                                    // Check if there was no item found.
                                    if (network == null)
                                    {
                                        // Continue.
                                        continue;
                                    }
                                    // Update the status of the item.
                                    network.Status = NetworkStatus.Error;
                                    // Add a message to the log.
                                    network.Log = network.AppendToLog("The network algorithm is not valid.");
                                    // Edit the network.
                                    await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
                                }
                                // End the switch.
                                break;
                        }
                    }
                    catch (Exception exception)
                    {
                        // Use a new scope.
                        using (var scope = serviceProvider.CreateScope())
                        {
                            // Use a new context instance.
                            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            // Reload the network.
                            var network = context.Networks
                                .FirstOrDefault(item => item.Id == batchNetwork.Id);
                            // Check if there was no item found.
                            if (network == null)
                            {
                                // Continue.
                                continue;
                            }
                            // Update the status of the item.
                            network.Status = NetworkStatus.Defined;
                            // Add a message to the log.
                            network.Log = network.AppendToLog($"The try number {currentRetry + 1} ended with an error ({NumberOfRetries - currentRetry} tr{(NumberOfRetries - currentRetry != 1 ? "ies" : "y")} remaining). {(string.IsNullOrEmpty(exception.Message) ? "There was no error message returned." : exception.Message)}");
                            // Edit the network.
                            await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
                        }
                        // Check if the task should be executed again.
                        if (currentRetry < NumberOfRetries)
                        {
                            // Increase the current retry.
                            currentRetry += 1;
                            // Repeat the loop for the current batch item.
                            batchItemIndex += -1;
                            // Continue.
                            continue;
                        }
                        // Use a new scope.
                        using (var scope = serviceProvider.CreateScope())
                        {
                            // Use a new context instance.
                            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            // Reload the network.
                            var network = context.Networks
                                .FirstOrDefault(item => item.Id == batchNetwork.Id);
                            // Check if there was no item found.
                            if (network == null)
                            {
                                // Continue.
                                continue;
                            }
                            // Update the status of the item.
                            network.Status = NetworkStatus.Error;
                            // Add a message to the log.
                            network.Log = network.AppendToLog("One or more errors occured while generating the network.");
                            // Edit the network.
                            await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
                        }
                        // Continue.
                        continue;
                    }
                    // Reset the current retry.
                    currentRetry = 0;
                    // Use a new scope.
                    using (var scope = serviceProvider.CreateScope())
                    {
                        // Use a new context instance.
                        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        // Reload the network.
                        var network = context.Networks
                            .FirstOrDefault(item => item.Id == batchNetwork.Id);
                        // Check if there was no item found.
                        if (network == null)
                        {
                            // Continue.
                            continue;
                        }
                        // Check if an error has been encountered.
                        if (network.Status == NetworkStatus.Error)
                        {
                            // Continue.
                            continue;
                        }
                        // Update the status of the item.
                        network.Status = NetworkStatus.Completed;
                        // Add a message to the log.
                        network.Log = network.AppendToLog("The network has been successfully generated.");
                        // Remove the generation data.
                        network.Data = null;
                        // Edit the network.
                        await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
                    }
                    // Use a new scope.
                    using (var scope = serviceProvider.CreateScope())
                    {
                        // Use a new context instance.
                        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        // Define the new background task.
                        var backgroundTask = new BackgroundTask
                        {
                            DateTimeCreated = DateTime.UtcNow,
                            Name = $"{nameof(IContentTaskManager)}.{nameof(IContentTaskManager.SendNetworksEndedEmailsAsync)}",
                            IsRecurring = false,
                            Data = JsonSerializer.Serialize(new NetworksTask
                            {
                                Scheme = Scheme,
                                HostValue = HostValue,
                                Items = batchNetwork.Yield().Select(item => new NetworkInputModel
                                {
                                    Id = item.Id
                                })
                            })
                        };
                        // Mark the task for addition.
                        context.BackgroundTasks.Add(backgroundTask);
                        // Save the changes to the database.
                        await context.SaveChangesAsync();
                        // Create a new Hangfire background job.
                        BackgroundJob.Enqueue<IContentTaskManager>(item => item.SendNetworksEndedEmailsAsync(backgroundTask.Id, CancellationToken.None));
                    }
                }
            }
        }

        /// <summary>
        /// Sends the e-mails to the corresponding users once the items have ended.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task SendEndedEmailsAsync(IServiceProvider serviceProvider, CancellationToken token)
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
                // Use a new e-mail sender instance.
                var emailSender = scope.ServiceProvider.GetRequiredService<ISendGridEmailSender>();
                // Use a new link generator instance.
                var linkGenerator = scope.ServiceProvider.GetRequiredService<LinkGenerator>();
                // Get the items in the current batch.
                var batchItems = Items
                    .Skip(index * ApplicationDbContext.BatchSize)
                    .Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems.Select(item => item.Id);
                // Get the items with the provided IDs.
                var networks = context.Networks
                    .Include(item => item.NetworkUsers)
                        .ThenInclude(item => item.User)
                    .Where(item => batchIds.Contains(item.Id));
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding item.
                    var network = networks
                        .FirstOrDefault(item => item.Id == batchItem.Id);
                    // Check if there was no item found.
                    if (network == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Define the HTTP context host.
                    var host = new HostString(HostValue);
                    // Go over each registered user in the analysis.
                    foreach (var user in network.NetworkUsers.Select(item => item.User))
                    {
                        // Send an analysis ending e-mail.
                        await emailSender.SendNetworkEndedEmailAsync(new EmailNetworkEndedViewModel
                        {
                            Email = user.Email,
                            Id = network.Id,
                            Name = network.Name,
                            Status = network.Status.GetDisplayName(),
                            Url = linkGenerator.GetUriByPage($"/CreatedData/Networks/Details/Index", handler: null, values: new { id = network.Id }, scheme: Scheme, host: host),
                            ApplicationUrl = linkGenerator.GetUriByPage("/Index", handler: null, values: null, scheme: Scheme, host: host)
                        });
                    }
                }
            }
        }
    }
}
