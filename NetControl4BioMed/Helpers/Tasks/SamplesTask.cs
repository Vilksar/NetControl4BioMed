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
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Tasks
{
    /// <summary>
    /// Implements a task to update samples in the database.
    /// </summary>
    public class SamplesTask
    {
        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<SampleInputModel> Items { get; set; }

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
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id);
                // Check if any of the IDs are repeating in the list.
                if (batchIds.Distinct().Count() != batchIds.Count())
                {
                    // Throw an exception.
                    throw new TaskException("Two or more of the manually provided IDs are duplicated.");
                }
                // Get the IDs of the related entities that appear in the current batch.
                var batchDatabaseIds = batchItems
                    .Where(item => item.SampleDatabases != null)
                    .Select(item => item.SampleDatabases)
                    .SelectMany(item => item)
                    .Where(item => item.Database != null)
                    .Select(item => item.Database)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Define the list of items to get.
                var validBatchIds = new List<string>();
                var databases = new List<Database>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the related entities that appear in the current batch.
                    databases = context.Databases
                        .Where(item => batchDatabaseIds.Contains(item.Id))
                        .Distinct()
                        .ToList();
                    // Get the valid IDs, that do not appear in the database.
                    validBatchIds = batchIds
                        .Except(context.Samples
                            .Where(item => batchIds.Contains(item.Id))
                            .Select(item => item.Id))
                        .ToList();
                }
                // Save the items to add.
                var samplesToAdd = new List<Sample>();
                // Go over each of the items.
                foreach (var batchItem in batchItems)
                {
                    // Check if the ID of the item is not valid.
                    if (!string.IsNullOrEmpty(batchItem.Id) && !validBatchIds.Contains(batchItem.Id))
                    {
                        // Continue.
                        continue;
                    }
                    // Check if there were no sample databases provided.
                    if (batchItem.SampleDatabases == null || !batchItem.SampleDatabases.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no sample databases provided.", showExceptionItem, batchItem);
                    }
                    // Get the sample databases.
                    var sampleDatabases = batchItem.SampleDatabases
                        .Where(item => item.Database != null)
                        .Where(item => !string.IsNullOrEmpty(item.Database.Id))
                        .Select(item => item.Database.Id)
                        .Distinct()
                        .Where(item => databases.Any(item1 => item1.Id == item))
                        .Select(item => new SampleDatabase
                        {
                            DatabaseId = item
                        });
                    // Check if there were no node collection databases found.
                    if (sampleDatabases == null || !sampleDatabases.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no sample databases found.", showExceptionItem, batchItem);
                    }
                    // Check if the network algorithm is valid.
                    if (!Enum.TryParse<NetworkAlgorithm>(batchItem.NetworkAlgorithm, out var networkAlgorithm))
                    {
                        // Throw an exception.
                        throw new TaskException("The network algorithm is not valid.", showExceptionItem, batchItem);
                    }
                    // Define the new node collection.
                    var sample = new Sample
                    {
                        DateTimeCreated = DateTime.UtcNow,
                        Name = batchItem.Name,
                        Description = batchItem.Description,
                        NetworkName = batchItem.NetworkName,
                        NetworkDescription = batchItem.NetworkDescription,
                        NetworkAlgorithm = networkAlgorithm,
                        NetworkNodeDatabaseData = batchItem.NetworkNodeDatabaseData,
                        NetworkEdgeDatabaseData = batchItem.NetworkEdgeDatabaseData,
                        NetworkSeedNodeData = batchItem.NetworkSeedNodeData,
                        NetworkSeedEdgeData = batchItem.NetworkSeedEdgeData,
                        NetworkSeedNodeCollectionData = batchItem.NetworkSeedNodeCollectionData,
                        AnalysisName = batchItem.AnalysisName,
                        AnalysisDescription = batchItem.AnalysisDescription,
                        AnalysisNetworkData = batchItem.AnalysisNetworkData,
                        AnalysisSourceNodeData = batchItem.AnalysisSourceNodeData,
                        AnalysisSourceNodeCollectionData = batchItem.AnalysisSourceNodeCollectionData,
                        AnalysisTargetNodeData = batchItem.AnalysisTargetNodeData,
                        AnalysisTargetNodeCollectionData = batchItem.AnalysisTargetNodeCollectionData,
                        SampleDatabases = sampleDatabases.ToList()
                    };
                    // Check if there is any ID provided.
                    if (!string.IsNullOrEmpty(batchItem.Id))
                    {
                        // Assign it to the node collection.
                        sample.Id = batchItem.Id;
                    }
                    // Add the new node collection to the list.
                    samplesToAdd.Add(sample);
                }
                // Create the items.
                await IEnumerableExtensions.CreateAsync(samplesToAdd, serviceProvider, token);
            }
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
                var batchIds = batchItems
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id);
                // Get the IDs of the related entities that appear in the current batch.
                var batchDatabaseIds = batchItems
                    .Where(item => item.SampleDatabases != null)
                    .Select(item => item.SampleDatabases)
                    .SelectMany(item => item)
                    .Where(item => item.Database != null)
                    .Select(item => item.Database)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Define the list of items to get.
                var samples = new List<Sample>();
                var databases = new List<Database>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    var items = context.Samples
                        .Where(item => batchIds.Contains(item.Id));
                    // Check if there were no items found.
                    if (items == null || !items.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Get the items found.
                    samples = items
                        .ToList();
                    // Get the related entities that appear in the current batch.
                    databases = context.Databases
                        .Where(item => batchDatabaseIds.Contains(item.Id))
                        .Distinct()
                        .ToList();
                }
                // Get the IDs of the items.
                var sampleIds = samples
                    .Select(item => item.Id);
                // Save the items to edit.
                var samplesToEdit = new List<Sample>();
                // Go over each of the valid items.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding item.
                    var sample = samples
                        .FirstOrDefault(item => batchItem.Id == item.Id);
                    // Check if there was no item found.
                    if (sample == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Check if there were no sample databases provided.
                    if (batchItem.SampleDatabases == null || !batchItem.SampleDatabases.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no sample databases provided.", showExceptionItem, batchItem);
                    }
                    // Get the sample databases.
                    var sampleDatabases = batchItem.SampleDatabases
                        .Where(item => item.Database != null)
                        .Where(item => !string.IsNullOrEmpty(item.Database.Id))
                        .Select(item => item.Database.Id)
                        .Distinct()
                        .Where(item => databases.Any(item1 => item1.Id == item))
                        .Select(item => new SampleDatabase
                        {
                            DatabaseId = item
                        });
                    // Check if there were no node collection databases found.
                    if (sampleDatabases == null || !sampleDatabases.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no sample databases found.", showExceptionItem, batchItem);
                    }
                    // Check if the network algorithm is valid.
                    if (!Enum.TryParse<NetworkAlgorithm>(batchItem.NetworkAlgorithm, out var networkAlgorithm))
                    {
                        // Throw an exception.
                        throw new TaskException("The network algorithm is not valid.", showExceptionItem, batchItem);
                    }
                    // Update the node collection.
                    sample.Name = batchItem.Name;
                    sample.Description = batchItem.Description;
                    sample.NetworkName = batchItem.NetworkName;
                    sample.NetworkDescription = batchItem.NetworkDescription;
                    sample.NetworkAlgorithm = networkAlgorithm;
                    sample.NetworkNodeDatabaseData = batchItem.NetworkNodeDatabaseData;
                    sample.NetworkEdgeDatabaseData = batchItem.NetworkEdgeDatabaseData;
                    sample.NetworkSeedNodeData = batchItem.NetworkSeedNodeData;
                    sample.NetworkSeedEdgeData = batchItem.NetworkSeedEdgeData;
                    sample.NetworkSeedNodeCollectionData = batchItem.NetworkSeedNodeCollectionData;
                    sample.AnalysisName = batchItem.AnalysisName;
                    sample.AnalysisDescription = batchItem.AnalysisDescription;
                    sample.AnalysisNetworkData = batchItem.AnalysisNetworkData;
                    sample.AnalysisSourceNodeData = batchItem.AnalysisSourceNodeData;
                    sample.AnalysisSourceNodeCollectionData = batchItem.AnalysisSourceNodeCollectionData;
                    sample.AnalysisTargetNodeData = batchItem.AnalysisTargetNodeData;
                    sample.AnalysisTargetNodeCollectionData = batchItem.AnalysisTargetNodeCollectionData;
                    sample.SampleDatabases = sampleDatabases.ToList();
                    // Add the node collection to the list.
                    samplesToEdit.Add(sample);
                }
                // Delete the related entities.
                await SampleExtensions.DeleteRelatedEntitiesAsync<SampleDatabase>(sampleIds, serviceProvider, token);
                // Update the items.
                await IEnumerableExtensions.EditAsync(samplesToEdit, serviceProvider, token);
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
                var samples = new List<Sample>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    var items = context.Samples
                        .Where(item => batchIds.Contains(item.Id));
                    // Check if there were no items found.
                    if (items == null || !items.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Get the items found.
                    samples = items
                        .ToList();
                }
                // Get the IDs of the items.
                var sampleIds = samples
                    .Select(item => item.Id);
                // Delete the related entities.
                await SampleExtensions.DeleteRelatedEntitiesAsync<SampleDatabase>(sampleIds, serviceProvider, token);
                // Delete the items.
                await IEnumerableExtensions.DeleteAsync(samples, serviceProvider, token);
            }
        }
    }
}
