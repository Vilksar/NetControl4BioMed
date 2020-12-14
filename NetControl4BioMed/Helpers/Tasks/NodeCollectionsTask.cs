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
    /// Implements a task to update node collections in the database.
    /// </summary>
    public class NodeCollectionsTask
    {
        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<NodeCollectionInputModel> Items { get; set; }

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
                    .Where(item => item.NodeCollectionDatabases != null)
                    .Select(item => item.NodeCollectionDatabases)
                    .SelectMany(item => item)
                    .Where(item => item.Database != null)
                    .Select(item => item.Database)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                var batchNodeIds = batchItems
                    .Where(item => item.NodeCollectionNodes != null)
                    .Select(item => item.NodeCollectionNodes)
                    .SelectMany(item => item)
                    .Where(item => item.Node != null)
                    .Select(item => item.Node)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Define the list of items to get.
                var validBatchIds = new List<string>();
                var databases = new List<Database>();
                var nodes = new List<Node>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the related entities that appear in the current batch.
                    databases = context.Databases
                        .Where(item => item.DatabaseType.Name != "Generic")
                        .Where(item => batchDatabaseIds.Contains(item.Id))
                        .Distinct()
                        .ToList();
                    nodes = context.Nodes
                        .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                        .Where(item => batchNodeIds.Contains(item.Id))
                        .ToList();
                    // Get the valid IDs, that do not appear in the database.
                    validBatchIds = batchIds
                        .Except(context.Edges
                            .Where(item => batchIds.Contains(item.Id))
                            .Select(item => item.Id))
                        .ToList();
                }
                // Save the items to add.
                var nodeCollectionsToAdd = new List<NodeCollection>();
                // Go over each of the items.
                foreach (var batchItem in batchItems)
                {
                    // Check if the ID of the item is not valid.
                    if (!string.IsNullOrEmpty(batchItem.Id) && !validBatchIds.Contains(batchItem.Id))
                    {
                        // Continue.
                        continue;
                    }
                    // Check if there were no node collection databases provided.
                    if (batchItem.NodeCollectionDatabases == null || !batchItem.NodeCollectionDatabases.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no node collection databases provided.", showExceptionItem, batchItem);
                    }
                    // Get the node collection databases.
                    var nodeCollectionDatabases = batchItem.NodeCollectionDatabases
                        .Where(item => item.Database != null)
                        .Where(item => !string.IsNullOrEmpty(item.Database.Id))
                        .Select(item => item.Database.Id)
                        .Distinct()
                        .Where(item => databases.Any(item1 => item1.Id == item))
                        .Select(item => new NodeCollectionDatabase
                        {
                            DatabaseId = item
                        });
                    // Check if there were no node collection databases found.
                    if (nodeCollectionDatabases == null || !nodeCollectionDatabases.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no node collection databases found.", showExceptionItem, batchItem);
                    }
                    // Check if there were no node collection nodes provided.
                    if (batchItem.NodeCollectionNodes == null)
                    {
                        // Throw an exception.
                        throw new TaskException("There were no node collection nodes provided.", showExceptionItem, batchItem);
                    }
                    // Get the node collection nodes.
                    var nodeCollectionNodes = batchItem.NodeCollectionNodes
                        .Where(item => item.Node != null)
                        .Where(item => !string.IsNullOrEmpty(item.Node.Id))
                        .Where(item => nodes.Any(item1 => item1.Id == item.Node.Id))
                        .Select(item => new NodeCollectionNode
                        {
                            NodeId = item.Node.Id
                        });
                    // Check if there were no edge nodes found.
                    if (nodeCollectionNodes == null)
                    {
                        // Throw an exception.
                        throw new TaskException("There were no node collection nodes found.", showExceptionItem, batchItem);
                    }
                    // Define the new node collection.
                    var nodeCollection = new NodeCollection
                    {
                        DateTimeCreated = DateTime.UtcNow,
                        Name = batchItem.Name,
                        Description = batchItem.Description,
                        NodeCollectionDatabases = nodeCollectionDatabases.ToList(),
                        NodeCollectionNodes = nodeCollectionNodes.ToList()
                    };
                    // Check if there is any ID provided.
                    if (!string.IsNullOrEmpty(batchItem.Id))
                    {
                        // Assign it to the node collection.
                        nodeCollection.Id = batchItem.Id;
                    }
                    // Add the new node collection to the list.
                    nodeCollectionsToAdd.Add(nodeCollection);
                }
                // Create the items.
                await IEnumerableExtensions.CreateAsync(nodeCollectionsToAdd, serviceProvider, token);
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
                    .Where(item => item.NodeCollectionDatabases != null)
                    .Select(item => item.NodeCollectionDatabases)
                    .SelectMany(item => item)
                    .Where(item => item.Database != null)
                    .Select(item => item.Database)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                var batchNodeIds = batchItems
                    .Where(item => item.NodeCollectionNodes != null)
                    .Select(item => item.NodeCollectionNodes)
                    .SelectMany(item => item)
                    .Where(item => item.Node != null)
                    .Select(item => item.Node)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Define the list of items to get.
                var nodeCollections = new List<NodeCollection>();
                var databases = new List<Database>();
                var nodes = new List<Node>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    var items = context.NodeCollections
                        .Where(item => !item.NodeCollectionDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                        .Where(item => batchIds.Contains(item.Id));
                    // Check if there were no items found.
                    if (items == null || !items.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Get the items found.
                    nodeCollections = items
                        .ToList();
                    // Get the related entities that appear in the current batch.
                    databases = context.Databases
                        .Where(item => item.DatabaseType.Name != "Generic")
                        .Where(item => batchDatabaseIds.Contains(item.Id))
                        .Distinct()
                        .ToList();
                    nodes = context.Nodes
                        .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                        .Where(item => batchNodeIds.Contains(item.Id))
                        .ToList();
                }
                // Get the IDs of the items.
                var nodeCollectionIds = nodeCollections
                    .Select(item => item.Id);
                // Save the items to edit.
                var nodeCollectionsToEdit = new List<NodeCollection>();
                // Go over each of the valid items.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding item.
                    var nodeCollection = nodeCollections
                        .FirstOrDefault(item => batchItem.Id == item.Id);
                    // Check if there was no item found.
                    if (nodeCollection == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Check if there were no node collection databases provided.
                    if (batchItem.NodeCollectionDatabases == null || !batchItem.NodeCollectionDatabases.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no node collection databases provided.", showExceptionItem, batchItem);
                    }
                    // Get the node collection databases.
                    var nodeCollectionDatabases = batchItem.NodeCollectionDatabases
                        .Where(item => item.Database != null)
                        .Where(item => !string.IsNullOrEmpty(item.Database.Id))
                        .Select(item => item.Database.Id)
                        .Distinct()
                        .Where(item => databases.Any(item1 => item1.Id == item))
                        .Select(item => new NodeCollectionDatabase
                        {
                            DatabaseId = item
                        });
                    // Check if there were no node collection databases found.
                    if (nodeCollectionDatabases == null || !nodeCollectionDatabases.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no node collection databases found.", showExceptionItem, batchItem);
                    }
                    // Check if there were no node collection nodes provided.
                    if (batchItem.NodeCollectionNodes == null)
                    {
                        // Throw an exception.
                        throw new TaskException("There were no node collection nodes provided.", showExceptionItem, batchItem);
                    }
                    // Get the node collection nodes.
                    var nodeCollectionNodes = batchItem.NodeCollectionNodes
                        .Where(item => item.Node != null)
                        .Where(item => !string.IsNullOrEmpty(item.Node.Id))
                        .Where(item => nodes.Any(item1 => item1.Id == item.Node.Id))
                        .Select(item => new NodeCollectionNode
                        {
                            NodeId = item.Node.Id
                        });
                    // Check if there were no edge nodes found.
                    if (nodeCollectionNodes == null)
                    {
                        // Throw an exception.
                        throw new TaskException("There were no node collection nodes found.", showExceptionItem, batchItem);
                    }
                    // Update the node collection.
                    nodeCollection.Name = batchItem.Name;
                    nodeCollection.Description = batchItem.Description;
                    nodeCollection.NodeCollectionDatabases = nodeCollectionDatabases.ToList();
                    nodeCollection.NodeCollectionNodes = nodeCollectionNodes.ToList();
                    // Add the node collection to the list.
                    nodeCollectionsToEdit.Add(nodeCollection);
                }
                // Delete the dependent entities.
                await NodeCollectionExtensions.DeleteDependentAnalysesAsync(nodeCollectionIds, serviceProvider, token);
                await NodeCollectionExtensions.DeleteDependentNetworksAsync(nodeCollectionIds, serviceProvider, token);
                // Delete the related entities.
                await NodeCollectionExtensions.DeleteRelatedEntitiesAsync<NodeCollectionNode>(nodeCollectionIds, serviceProvider, token);
                await NodeCollectionExtensions.DeleteRelatedEntitiesAsync<NodeCollectionDatabase>(nodeCollectionIds, serviceProvider, token);
                // Update the items.
                await IEnumerableExtensions.EditAsync(nodeCollectionsToEdit, serviceProvider, token);
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
                var nodeCollections = new List<NodeCollection>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    var items = context.NodeCollections
                        .Where(item => batchIds.Contains(item.Id));
                    // Check if there were no items found.
                    if (items == null || !items.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Get the items found.
                    nodeCollections = items
                        .ToList();
                }
                // Get the IDs of the items.
                var nodeCollectionIds = nodeCollections
                    .Select(item => item.Id);
                // Delete the dependent entities.
                await NodeCollectionExtensions.DeleteDependentAnalysesAsync(nodeCollectionIds, serviceProvider, token);
                await NodeCollectionExtensions.DeleteDependentNetworksAsync(nodeCollectionIds, serviceProvider, token);
                // Delete the related entities.
                await NodeCollectionExtensions.DeleteRelatedEntitiesAsync<NodeCollectionNode>(nodeCollectionIds, serviceProvider, token);
                await NodeCollectionExtensions.DeleteRelatedEntitiesAsync<NodeCollectionDatabase>(nodeCollectionIds, serviceProvider, token);
                // Delete the items.
                await IEnumerableExtensions.DeleteAsync(nodeCollections, serviceProvider, token);
            }
        }
    }
}
