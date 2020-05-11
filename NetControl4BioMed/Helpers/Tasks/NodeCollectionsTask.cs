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
                // Get the items in the current batch.
                var batchItems = Items.Skip(index * ApplicationDbContext.BatchSize).Take(ApplicationDbContext.BatchSize);
                // Create a new scope.
                using var scope = serviceProvider.CreateScope();
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the manually provided IDs of all the items that are to be created.
                var itemNodeCollectionIds = batchItems
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id);
                // Check if any of the manually provided IDs are repeating in the list.
                if (itemNodeCollectionIds.Distinct().Count() != itemNodeCollectionIds.Count())
                {
                    // Throw an exception.
                    throw new ArgumentException("One or more of the manually provided IDs are duplicated.");
                }
                // Get the valid manually provided IDs, that do not appear in the database.
                var validItemNodeCollectionIds = itemNodeCollectionIds
                    .Except(context.NodeCollections
                        .Where(item => itemNodeCollectionIds.Contains(item.Id))
                        .Select(item => item.Id));
                // Get the IDs of all of the databases that are to be used by the collections.
                var itemDatabaseIds = batchItems
                    .Select(item => item.NodeCollectionDatabases)
                    .SelectMany(item => item)
                    .Select(item => item.DatabaseId)
                    .Where(item => !string.IsNullOrEmpty(item))
                    .Distinct();
                // Get the databases that are to be used by the collections.
                var databases = context.Databases
                    .Where(item => item.DatabaseType.Name != "Generic")
                    .Where(item => itemDatabaseIds.Contains(item.Id))
                    .Include(item => item.DatabaseNodes)
                        .ThenInclude(item => item.Node)
                    .AsEnumerable();
                // Get the IDs of all of the nodes that are to be added to the collections.
                var itemNodeIds = batchItems
                    .Select(item => item.NodeCollectionNodes)
                    .SelectMany(item => item)
                    .Select(item => item.NodeId)
                    .Where(item => !string.IsNullOrEmpty(item))
                    .Distinct();
                // Get the nodes that are to be added to the collections.
                var nodes = context.Nodes
                    .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => itemNodeIds.Contains(item.Id))
                    .Include(item => item.DatabaseNodes)
                        .ThenInclude(item => item.Database)
                    .AsEnumerable();
                // Get the valid database IDs.
                var validItemDatabaseIds = databases
                    .Select(item => item.Id);
                // Get the valid node IDs.
                var validItemNodeIds = nodes
                    .Select(item => item.Id);
                // Save the node collections to add.
                var nodeCollections = new List<NodeCollection>();
                // Go over each of the items.
                foreach (var batchItem in batchItems)
                {
                    // Check if the ID of the current item is valid.
                    if (!string.IsNullOrEmpty(batchItem.Id) && !validItemNodeCollectionIds.Contains(batchItem.Id))
                    {
                        // Continue.
                        continue;
                    }
                    // Get the valid databases and the node collection databases to add.
                    var nodeCollectionDatabases = batchItem.NodeCollectionDatabases
                        .Select(item => item.DatabaseId)
                        .Where(item => validItemDatabaseIds.Contains(item))
                        .Distinct()
                        .Select(item => new NodeCollectionDatabase
                        {
                            DatabaseId = item,
                            Database = databases.FirstOrDefault(item1 => item == item1.Id)
                        })
                        .Where(item => item.Database != null);
                    // Get the valid nodes and the node collection nodes to add.
                    var nodeCollectionNodes = batchItem.NodeCollectionNodes
                        .Select(item => item.NodeId)
                        .Where(item => validItemNodeIds.Contains(item))
                        .Distinct()
                        .Select(item => new NodeCollectionNode
                        {
                            NodeId = item,
                            Node = nodes.FirstOrDefault(item1 => item == item1.Id)
                        })
                        .Where(item => item.Node != null);
                    // Define the new node collection.
                    var nodeCollection = new NodeCollection
                    {
                        Name = batchItem.Name,
                        Description = batchItem.Description,
                        DateTimeCreated = DateTime.Now,
                        NodeCollectionDatabases = nodeCollectionDatabases
                            .Where(item => item.Database.DatabaseNodes.Any(item1 => validItemNodeIds.Contains(item1.Node.Id)))
                            .ToList(),
                        NodeCollectionNodes = nodeCollectionNodes
                            .Where(item => item.Node.DatabaseNodes.Any(item1 => validItemDatabaseIds.Contains(item1.Database.Id)))
                            .ToList()
                    };
                    // Check if there is any ID provided.
                    if (!string.IsNullOrEmpty(batchItem.Id))
                    {
                        // Assign it to the node.
                        nodeCollection.Id = batchItem.Id;
                    }
                    // Add the new node to the list.
                    nodeCollections.Add(nodeCollection);
                }
                // Try to create the items.
                try
                {
                    // Create the items.
                    IEnumerableExtensions.Create(nodeCollections, context, token);
                }
                catch (Exception exception)
                {
                    // Throw an exception.
                    throw exception;
                }
            }
        }

        /// <summary>
        /// Edits the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void Edit(IServiceProvider serviceProvider, CancellationToken token)
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
                // Get the items in the current batch.
                var batchItems = Items.Skip(index * ApplicationDbContext.BatchSize).Take(ApplicationDbContext.BatchSize);
                // Create a new scope.
                using var scope = serviceProvider.CreateScope();
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the list of IDs from the provided items.
                var itemIds = batchItems.Select(item => item.Id);
                // Get the node collections from the database that have the given IDs.
                var nodeCollections = context.NodeCollections
                    .Where(item => itemIds.Contains(item.Id))
                    .AsEnumerable();
                // Check if there weren't any node collections found.
                if (nodeCollections == null || !nodeCollections.Any())
                {
                    // Throw an exception.
                    throw new ArgumentException("No node collections could be found in the database with the provided IDs.");
                }
                // Get the IDs of all of the databases that are to be used by the collections.
                var itemDatabaseIds = batchItems
                    .Select(item => item.NodeCollectionDatabases)
                    .SelectMany(item => item)
                    .Select(item => item.DatabaseId)
                    .Where(item => !string.IsNullOrEmpty(item))
                    .Distinct();
                // Get the databases that are to be used by the collections.
                var databases = context.Databases
                    .Where(item => item.DatabaseType.Name != "Generic")
                    .Where(item => itemDatabaseIds.Contains(item.Id))
                    .Include(item => item.DatabaseNodes)
                        .ThenInclude(item => item.Node)
                    .AsEnumerable();
                // Get the IDs of all of the nodes that are to be added to the collections.
                var itemNodeIds = batchItems
                    .Select(item => item.NodeCollectionNodes)
                    .SelectMany(item => item)
                    .Select(item => item.NodeId)
                    .Where(item => !string.IsNullOrEmpty(item))
                    .Distinct();
                // Get the nodes that are to be added to the collections.
                var nodes = context.Nodes
                    .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => itemNodeIds.Contains(item.Id))
                    .Include(item => item.DatabaseNodes)
                        .ThenInclude(item => item.Database)
                    .AsEnumerable();
                // Get the valid database IDs.
                var validItemDatabaseIds = databases
                    .Select(item => item.Id);
                // Get the valid node IDs.
                var validItemNodeIds = nodes
                    .Select(item => item.Id);
                // Save the nodes to update.
                var nodeCollectionsToUpdate = new List<NodeCollection>();
                // Go over each of the valid items.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding node collection.
                    var nodeCollection = nodeCollections.FirstOrDefault(item => batchItem.Id == item.Id);
                    // Check if there was no node collection found.
                    if (nodeCollection == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Get the valid databases and the node collection databases to add.
                    var nodeCollectionDatabases = batchItem.NodeCollectionDatabases
                        .Select(item => item.DatabaseId)
                        .Where(item => validItemDatabaseIds.Contains(item))
                        .Distinct()
                        .Select(item => new NodeCollectionDatabase
                        {
                            NodeCollectionId = nodeCollection.Id,
                            NodeCollection = nodeCollection,
                            DatabaseId = item,
                            Database = databases.FirstOrDefault(item1 => item == item1.Id)
                        })
                        .Where(item => item.NodeCollection != null && item.Database != null);
                    // Get the valid nodes and the node collection nodes to add.
                    var nodeCollectionNodes = batchItem.NodeCollectionNodes
                        .Select(item => item.NodeId)
                        .Where(item => validItemNodeIds.Contains(item))
                        .Distinct()
                        .Select(item => new NodeCollectionNode
                        {
                            NodeCollectionId = nodeCollection.Id,
                            NodeCollection = nodeCollection,
                            NodeId = item,
                            Node = nodes.FirstOrDefault(item1 => item == item1.Id)
                        })
                        .Where(item => item.NodeCollection != null && item.Node != null);
                    // Update the node collection.
                    nodeCollection.Name = batchItem.Name;
                    nodeCollection.Description = batchItem.Description;
                    nodeCollection.NodeCollectionDatabases = nodeCollectionDatabases
                            .Where(item => item.Database.DatabaseNodes.Any(item1 => validItemNodeIds.Contains(item1.Node.Id)))
                            .ToList();
                    nodeCollection.NodeCollectionNodes = nodeCollectionNodes
                            .Where(item => item.Node.DatabaseNodes.Any(item1 => validItemDatabaseIds.Contains(item1.Database.Id)))
                            .ToList();
                    // Add the node collection to the list.
                    nodeCollectionsToUpdate.Add(nodeCollection);
                }
                // Get the networks and analyses that use the node collections.
                var networks = context.Networks
                    .Where(item => item.NetworkNodeCollections.Any(item1 => nodeCollectionsToUpdate.Contains(item1.NodeCollection)));
                var analyses = context.Analyses
                    .Where(item => item.AnalysisNodeCollections.Any(item1 => nodeCollectionsToUpdate.Contains(item1.NodeCollection)));
                // Try to update the items.
                try
                {
                    // Delete the items.
                    IQueryableExtensions.Delete(analyses, context, token);
                    IQueryableExtensions.Delete(networks, context, token);
                    // Update the items.
                    IEnumerableExtensions.Edit(nodeCollectionsToUpdate, context, token);
                }
                catch (Exception exception)
                {
                    // Throw an exception.
                    throw exception;
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
                // Get the IDs of the items in the current batch.
                var batchIds = Items.Skip(index * ApplicationDbContext.BatchSize).Take(ApplicationDbContext.BatchSize).Select(item => item.Id);
                // Create a new scope.
                using var scope = serviceProvider.CreateScope();
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the current batch IDs.
                var nodeCollections = context.NodeCollections
                    .Where(item => batchIds.Contains(item.Id));
                // Get the related entities that use the items.
                var networks = context.Networks.Where(item => item.NetworkNodeCollections.Any(item1 => nodeCollections.Contains(item1.NodeCollection)));
                var analyses = context.Analyses.Where(item => item.AnalysisNodeCollections.Any(item1 => nodeCollections.Contains(item1.NodeCollection)) || item.AnalysisNetworks.Any(item1 => networks.Contains(item1.Network)));
                // Try to delete the items.
                try
                {
                    // Delete the items.
                    IQueryableExtensions.Delete(analyses, context, token);
                    IQueryableExtensions.Delete(networks, context, token);
                    IQueryableExtensions.Delete(nodeCollections, context, token);
                }
                catch (Exception exception)
                {
                    // Throw an exception.
                    throw exception;
                }
            }
        }
    }
}
