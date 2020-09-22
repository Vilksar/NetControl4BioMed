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
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id);
                // Check if any of the IDs are repeating in the list.
                if (batchIds.Distinct().Count() != batchIds.Count())
                {
                    // Throw an exception.
                    throw new TaskException("Two or more of the manually provided IDs are duplicated.");
                }
                // Get the valid IDs, that do not appear in the database.
                var validBatchIds = batchIds
                    .Except(context.Edges
                        .Where(item => batchIds.Contains(item.Id))
                        .Select(item => item.Id));
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
                // Get the related entities that appear in the current batch.
                var batchDatabases = context.Databases
                    .Where(item => item.DatabaseType.Name != "Generic")
                    .Where(item => batchDatabaseIds.Contains(item.Id))
                    .Distinct();
                var batchNodes = context.Nodes
                    .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => batchNodeIds.Contains(item.Id));
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
                        .Where(item => batchDatabases.Any(item1 => item1.Id == item))
                        .Select(item => new NodeCollectionDatabase
                        {
                            DatabaseId = item,
                            Database = batchDatabases
                                .FirstOrDefault(item1 => item1.Id == item)
                        })
                        .Where(item => item.Database != null);
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
                        .Where(item => batchNodes.Any(item1 => item1.Id == item.Node.Id))
                        .Select(item => new NodeCollectionNode
                        {
                            NodeId = item.Node.Id,
                            Node = batchNodes
                                .FirstOrDefault(item1 => item1.Id == item.Node.Id)
                        })
                        .Where(item => item.Node != null);
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
                await IEnumerableExtensions.CreateAsync(nodeCollectionsToAdd, context, token);
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
                // Get the related entities that appear in the current batch.
                var batchDatabases = context.Databases
                    .Where(item => item.DatabaseType.Name != "Generic")
                    .Where(item => batchDatabaseIds.Contains(item.Id))
                    .Distinct();
                var batchNodes = context.Nodes
                    .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => batchNodeIds.Contains(item.Id));
                // Get the items corresponding to the current batch.
                var nodeCollections = context.NodeCollections
                    .Where(item => !item.NodeCollectionDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => batchIds.Contains(item.Id));
                // Get the related entities to delete.
                var batchNodeCollectionDatabases = context.NodeCollectionDatabases
                    .Where(item => nodeCollections.Contains(item.NodeCollection));
                var batchNodeCollectionNodes = context.NodeCollectionNodes
                    .Where(item => nodeCollections.Contains(item.NodeCollection));
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
                        .Where(item => batchDatabases.Any(item1 => item1.Id == item))
                        .Select(item => new NodeCollectionDatabase
                        {
                            DatabaseId = item,
                            Database = batchDatabases
                                .FirstOrDefault(item1 => item1.Id == item)
                        })
                        .Where(item => item.Database != null);
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
                        .Where(item => batchNodes.Any(item1 => item1.Id == item.Node.Id))
                        .Select(item => new NodeCollectionNode
                        {
                            NodeId = item.Node.Id,
                            Node = batchNodes
                                .FirstOrDefault(item1 => item1.Id == item.Node.Id)
                        })
                        .Where(item => item.Node != null);
                    // Check if there were no edge nodes found.
                    if (nodeCollectionNodes == null)
                    {
                        // Throw an exception.
                        throw new TaskException("There were no node collection nodes found.", showExceptionItem, batchItem);
                    }
                    // Delete all related entities that appear in the current batch.
                    await IQueryableExtensions.DeleteAsync(batchNodeCollectionDatabases.Where(item => item.NodeCollection == nodeCollection), context, token);
                    await IQueryableExtensions.DeleteAsync(batchNodeCollectionNodes.Where(item => item.NodeCollection == nodeCollection), context, token);
                    // Update the node collection.
                    nodeCollection.Name = batchItem.Name;
                    nodeCollection.Description = batchItem.Description;
                    nodeCollection.NodeCollectionDatabases = nodeCollectionDatabases.ToList();
                    nodeCollection.NodeCollectionNodes = nodeCollectionNodes.ToList();
                    // Add the node collection to the list.
                    nodeCollectionsToEdit.Add(nodeCollection);
                }
                // Get the networks and analyses that use the node collections.
                var networks = context.Networks
                    .Where(item => item.NetworkNodeCollections.Any(item1 => nodeCollectionsToEdit.Contains(item1.NodeCollection)));
                var analyses = context.Analyses
                    .Where(item => item.AnalysisNodeCollections.Any(item1 => nodeCollectionsToEdit.Contains(item1.NodeCollection)));
                // Delete the items.
                await IQueryableExtensions.DeleteAsync(analyses, context, token);
                await IQueryableExtensions.DeleteAsync(networks, context, token);
                // Update the items.
                await IEnumerableExtensions.EditAsync(nodeCollectionsToEdit, context, token);
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
                var batchIds = batchItems.Select(item => item.Id);
                // Get the items with the current batch IDs.
                var nodeCollections = context.NodeCollections
                    .Where(item => batchIds.Contains(item.Id));
                // Get the related entities that use the items.
                var networks = context.Networks.Where(item => item.NetworkNodeCollections.Any(item1 => nodeCollections.Contains(item1.NodeCollection)));
                var analyses = context.Analyses.Where(item => item.AnalysisNodeCollections.Any(item1 => nodeCollections.Contains(item1.NodeCollection)) || item.AnalysisNetworks.Any(item1 => networks.Contains(item1.Network)));
                // Delete the items.
                await IQueryableExtensions.DeleteAsync(analyses, context, token);
                await IQueryableExtensions.DeleteAsync(networks, context, token);
                await IQueryableExtensions.DeleteAsync(nodeCollections, context, token);
            }
        }
    }
}
