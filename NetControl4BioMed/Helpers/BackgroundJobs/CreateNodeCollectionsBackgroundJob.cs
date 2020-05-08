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
    /// Implements a background job to create node collections in the database.
    /// </summary>
    public class CreateNodeCollectionsBackgroundJob : BaseBackgroundJob
    {
        /// <summary>
        /// Gets or sets the items to be created.
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
                // Save the node collections to add.
                var nodeCollections = new List<NodeCollection>();
                // Go over each of the items.
                foreach (var item in batchItems)
                {
                    // Check if the ID of the current item is valid.
                    if (!string.IsNullOrEmpty(item.Id) && !validItemNodeCollectionIds.Contains(item.Id))
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
                                DatabaseId = item1,
                                Database = databases.FirstOrDefault(item2 => item1 == item2.Id)
                            })
                        .Where(item1 => item1.Database != null);
                    // Get the valid nodes and the node collection nodes to add.
                    var nodeCollectionNodes = item.NodeIds
                        .Where(item1 => validItemNodeIds.Contains(item1))
                        .Distinct()
                        .Select(item1 =>
                            new NodeCollectionNode
                            {
                                NodeId = item1,
                                Node = nodes.FirstOrDefault(item2 => item1 == item2.Id)
                            })
                        .Where(item1 => item1.Node != null);
                    // Define the new node collection.
                    var nodeCollection = new NodeCollection
                    {
                        Name = item.Name,
                        Description = item.Description,
                        DateTimeCreated = DateTime.Now,
                        NodeCollectionDatabases = nodeCollectionDatabases
                            .Where(item1 => item1.Database.DatabaseNodes.Any(item2 => validItemNodeIds.Contains(item2.Node.Id)))
                            .ToList(),
                        NodeCollectionNodes = nodeCollectionNodes
                            .Where(item1 => item1.Node.DatabaseNodes.Any(item1 => validItemDatabaseIds.Contains(item1.Database.Id)))
                            .ToList()
                    };
                    // Check if there is any ID provided.
                    if (!string.IsNullOrEmpty(item.Id))
                    {
                        // Assign it to the node.
                        nodeCollection.Id = item.Id;
                    }
                    // Add the new node to the list.
                    nodeCollections.Add(nodeCollection);
                }
                // Try to create the items.
                try
                {
                    // Create the items.
                    Create(nodeCollections, context, token);
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
