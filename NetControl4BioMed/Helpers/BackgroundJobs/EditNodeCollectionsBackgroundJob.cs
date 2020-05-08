using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.BackgroundJobs
{
    /// <summary>
    /// Implements a background job to update node collections in the database.
    /// </summary>
    public class EditNodeCollectionsBackgroundJob : BaseBackgroundJob
    {
        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<DataUpdateNodeCollectionViewModel> Items { get; set; }

        /// <summary>
        /// Runs the current job.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public override void Run(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if there weren't any valid items found.
            if (Items == null || !Items.Any())
            {
                // Throw an exception.
                throw new ArgumentException("No valid items could be found with the provided data.");
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)Items.Count() / _batchSize);
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
                var batchItems = Items.Skip(index * _batchSize).Take(_batchSize);
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
                    .Select(item => item.DatabaseIds)
                    .SelectMany(item => item)
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
                    .Select(item => item.NodeIds)
                    .SelectMany(item => item)
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
                foreach (var item in batchItems)
                {
                    // Get the corresponding node collection.
                    var nodeCollection = nodeCollections.FirstOrDefault(item1 => item.Id == item1.Id);
                    // Check if there was no node collection found.
                    if (nodeCollection == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Get the valid databases and the node collection databases to add.
                    var nodeCollectionDatabases = item.DatabaseIds
                        .Where(item1 => validItemDatabaseIds.Contains(item1))
                        .Distinct()
                        .Select(item1 =>
                            new NodeCollectionDatabase
                            {
                                NodeCollectionId = nodeCollection.Id,
                                NodeCollection = nodeCollection,
                                DatabaseId = item1,
                                Database = databases.FirstOrDefault(item2 => item1 == item2.Id)
                            })
                        .Where(item1 => item1.NodeCollection != null && item1.Database != null);
                    // Get the valid nodes and the node collection nodes to add.
                    var nodeCollectionNodes = item.NodeIds
                        .Where(item1 => validItemNodeIds.Contains(item1))
                        .Distinct()
                        .Select(item1 =>
                            new NodeCollectionNode
                            {
                                NodeCollectionId = nodeCollection.Id,
                                NodeCollection = nodeCollection,
                                NodeId = item1,
                                Node = nodes.FirstOrDefault(item2 => item1 == item2.Id)
                            })
                        .Where(item1 => item1.NodeCollection != null && item1.Node != null);
                    // Update the node collection.
                    nodeCollection.Name = item.Name;
                    nodeCollection.Description = item.Description;
                    nodeCollection.NodeCollectionDatabases = nodeCollectionDatabases
                            .Where(item1 => item1.Database.DatabaseNodes.Any(item1 => validItemNodeIds.Contains(item1.Node.Id)))
                            .ToList();
                    nodeCollection.NodeCollectionNodes = nodeCollectionNodes
                            .Where(item1 => item1.Node.DatabaseNodes.Any(item1 => validItemDatabaseIds.Contains(item1.Database.Id)))
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
                    Delete(analyses, context, token);
                    Delete(networks, context, token);
                    // Update the items.
                    Update(nodeCollectionsToUpdate, context, token);
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
