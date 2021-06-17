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
    /// Implements a task to update protein collections in the database.
    /// </summary>
    public class ProteinCollectionsTask
    {
        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<ProteinCollectionInputModel> Items { get; set; }

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
                var batchProteinIds = batchItems
                    .Where(item => item.ProteinCollectionProteins != null)
                    .Select(item => item.ProteinCollectionProteins)
                    .SelectMany(item => item)
                    .Where(item => item.Protein != null)
                    .Select(item => item.Protein)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Define the list of items to get.
                var validBatchIds = new List<string>();
                var proteins = new List<Protein>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the related entities that appear in the current batch.
                    proteins = context.Proteins
                        .Where(item => batchProteinIds.Contains(item.Id))
                        .ToList();
                    // Get the valid IDs, that do not appear in the database.
                    validBatchIds = batchIds
                        .Except(context.ProteinCollections
                            .Where(item => batchIds.Contains(item.Id))
                            .Select(item => item.Id))
                        .ToList();
                }
                // Save the items to add.
                var proteinCollectionsToAdd = new List<ProteinCollection>();
                // Go over each of the items.
                foreach (var batchItem in batchItems)
                {
                    // Check if the ID of the item is not valid.
                    if (!string.IsNullOrEmpty(batchItem.Id) && !validBatchIds.Contains(batchItem.Id))
                    {
                        // Continue.
                        continue;
                    }
                    // Check if there were no protein collection proteins provided.
                    if (batchItem.ProteinCollectionProteins == null || !batchItem.ProteinCollectionProteins.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no protein collection proteins provided.", showExceptionItem, batchItem);
                    }
                    // Get the protein collection proteins.
                    var proteinCollectionProteins = batchItem.ProteinCollectionProteins
                        .Where(item => item.Protein != null)
                        .Where(item => !string.IsNullOrEmpty(item.Protein.Id))
                        .Where(item => proteins.Any(item1 => item1.Id == item.Protein.Id))
                        .Select(item => new ProteinCollectionProtein
                        {
                            ProteinId = item.Protein.Id
                        });
                    // Check if there were no protein collection proteins found.
                    if (proteinCollectionProteins == null || !proteinCollectionProteins.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no protein collection proteins found.", showExceptionItem, batchItem);
                    }
                    // Check if there were no protein collection types provided.
                    if (batchItem.ProteinCollectionTypes == null || !batchItem.ProteinCollectionTypes.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no protein collection types provided.", showExceptionItem, batchItem);
                    }
                    // Get the protein collection types.
                    var proteinCollectionTypes = batchItem.ProteinCollectionTypes
                        .Select(item => item.Type)
                        .Select(item => (Enum.TryParse<Data.Enumerations.ProteinCollectionType>(item, out var type), type))
                        .Where(item => item.Item1)
                        .Select(item => new ProteinCollectionType
                        {
                            Type = item.Item2
                        });
                    // Check if there were no protein collection types found.
                    if (proteinCollectionTypes == null || !proteinCollectionTypes.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no protein collection types found.", showExceptionItem, batchItem);
                    }
                    // Define the new protein collection.
                    var proteinCollection = new ProteinCollection
                    {
                        DateTimeCreated = DateTime.UtcNow,
                        Name = batchItem.Name,
                        Description = batchItem.Description,
                        ProteinCollectionTypes = proteinCollectionTypes.ToList(),
                        ProteinCollectionProteins = proteinCollectionProteins.ToList()
                    };
                    // Check if there is any ID provided.
                    if (!string.IsNullOrEmpty(batchItem.Id))
                    {
                        // Assign it to the protein collection.
                        proteinCollection.Id = batchItem.Id;
                    }
                    // Add the new protein collection to the list.
                    proteinCollectionsToAdd.Add(proteinCollection);
                }
                // Create the items.
                await IEnumerableExtensions.CreateAsync(proteinCollectionsToAdd, serviceProvider, token);
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
                var batchProteinIds = batchItems
                    .Where(item => item.ProteinCollectionProteins != null)
                    .Select(item => item.ProteinCollectionProteins)
                    .SelectMany(item => item)
                    .Where(item => item.Protein != null)
                    .Select(item => item.Protein)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Define the list of items to get.
                var proteinCollections = new List<ProteinCollection>();
                var proteins = new List<Protein>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    var items = context.ProteinCollections
                        .Where(item => batchIds.Contains(item.Id));
                    // Check if there were no items found.
                    if (items == null || !items.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Get the items found.
                    proteinCollections = items
                        .ToList();
                    // Get the related entities that appear in the current batch.
                    proteins = context.Proteins
                        .Where(item => batchProteinIds.Contains(item.Id))
                        .ToList();
                }
                // Get the IDs of the items.
                var proteinCollectionIds = proteinCollections
                    .Select(item => item.Id);
                // Save the items to edit.
                var proteinCollectionsToEdit = new List<ProteinCollection>();
                // Go over each of the valid items.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding item.
                    var proteinCollection = proteinCollections
                        .FirstOrDefault(item => batchItem.Id == item.Id);
                    // Check if there was no item found.
                    if (proteinCollection == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Check if there were no protein collection proteins provided.
                    if (batchItem.ProteinCollectionProteins == null || !batchItem.ProteinCollectionProteins.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no protein collection proteins provided.", showExceptionItem, batchItem);
                    }
                    // Get the protein collection proteins.
                    var proteinCollectionProteins = batchItem.ProteinCollectionProteins
                        .Where(item => item.Protein != null)
                        .Where(item => !string.IsNullOrEmpty(item.Protein.Id))
                        .Where(item => proteins.Any(item1 => item1.Id == item.Protein.Id))
                        .Select(item => new ProteinCollectionProtein
                        {
                            ProteinId = item.Protein.Id
                        });
                    // Check if there were no protein collection proteins found.
                    if (proteinCollectionProteins == null || !proteinCollectionProteins.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no protein collection proteins found.", showExceptionItem, batchItem);
                    }
                    // Check if there were no protein collection types provided.
                    if (batchItem.ProteinCollectionTypes == null || !batchItem.ProteinCollectionTypes.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no protein collection types provided.", showExceptionItem, batchItem);
                    }
                    // Get the protein collection types.
                    var proteinCollectionTypes = batchItem.ProteinCollectionTypes
                        .Select(item => item.Type)
                        .Select(item => (Enum.TryParse<Data.Enumerations.ProteinCollectionType>(item, out var type), type))
                        .Where(item => item.Item1)
                        .Select(item => new ProteinCollectionType
                        {
                            Type = item.Item2
                        });
                    // Check if there were no protein collection types found.
                    if (proteinCollectionTypes == null || !proteinCollectionTypes.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no protein collection types found.", showExceptionItem, batchItem);
                    }
                    // Update the protein collection.
                    proteinCollection.Name = batchItem.Name;
                    proteinCollection.Description = batchItem.Description;
                    proteinCollection.ProteinCollectionTypes = proteinCollectionTypes.ToList();
                    proteinCollection.ProteinCollectionProteins = proteinCollectionProteins.ToList();
                    // Add the protein collection to the list.
                    proteinCollectionsToEdit.Add(proteinCollection);
                }
                // Delete the dependent entities.
                await ProteinCollectionExtensions.DeleteDependentAnalysesAsync(proteinCollectionIds, serviceProvider, token);
                await ProteinCollectionExtensions.DeleteDependentNetworksAsync(proteinCollectionIds, serviceProvider, token);
                // Delete the related entities.
                await ProteinCollectionExtensions.DeleteRelatedEntitiesAsync<ProteinCollectionType>(proteinCollectionIds, serviceProvider, token);
                await ProteinCollectionExtensions.DeleteRelatedEntitiesAsync<ProteinCollectionProtein>(proteinCollectionIds, serviceProvider, token);
                // Update the items.
                await IEnumerableExtensions.EditAsync(proteinCollectionsToEdit, serviceProvider, token);
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
                var proteinCollections = new List<ProteinCollection>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    var items = context.ProteinCollections
                        .Where(item => batchIds.Contains(item.Id));
                    // Check if there were no items found.
                    if (items == null || !items.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Get the items found.
                    proteinCollections = items
                        .ToList();
                }
                // Get the IDs of the items.
                var proteinCollectionIds = proteinCollections
                    .Select(item => item.Id);
                // Delete the dependent entities.
                await ProteinCollectionExtensions.DeleteDependentAnalysesAsync(proteinCollectionIds, serviceProvider, token);
                await ProteinCollectionExtensions.DeleteDependentNetworksAsync(proteinCollectionIds, serviceProvider, token);
                // Delete the related entities.
                await ProteinCollectionExtensions.DeleteRelatedEntitiesAsync<ProteinCollectionType>(proteinCollectionIds, serviceProvider, token);
                await ProteinCollectionExtensions.DeleteRelatedEntitiesAsync<ProteinCollectionProtein>(proteinCollectionIds, serviceProvider, token);
                // Delete the items.
                await IEnumerableExtensions.DeleteAsync(proteinCollections, serviceProvider, token);
            }
        }
    }
}
