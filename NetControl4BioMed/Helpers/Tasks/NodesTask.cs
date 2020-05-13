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
    /// Implements a task to update nodes in the database.
    /// </summary>
    public class NodesTask
    {
        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<NodeInputModel> Items { get; set; }

        /// <summary>
        /// Creates the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void Create(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if there weren't any valid items found.
            if (Items == null)
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
                // Create a new scope.
                using var scope = serviceProvider.CreateScope();
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items in the current batch.
                var batchItems = Items.Skip(index * ApplicationDbContext.BatchSize).Take(ApplicationDbContext.BatchSize);
                // Get the manually provided IDs of all the items that are to be created.
                var itemNodeIds = batchItems
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id);
                // Check if any of the manually provided IDs are repeating in the list.
                if (itemNodeIds.Distinct().Count() != itemNodeIds.Count())
                {
                    // Throw an exception.
                    throw new ArgumentException("One or more of the manually provided IDs are duplicated.");
                }
                // Get the valid manually provided IDs, that do not appear in the database.
                var validItemNodeIds = itemNodeIds
                    .Except(context.Nodes
                        .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                        .Where(item => itemNodeIds.Contains(item.Id))
                        .Select(item => item.Id));
                // Get the IDs of all of the node fields that are to be updated.
                var itemNodeFieldIds = batchItems
                    .Select(item => item.DatabaseNodeFieldNodes)
                    .SelectMany(item => item)
                    .Where(item => !string.IsNullOrEmpty(item.DatabaseNodeFieldId) && !string.IsNullOrEmpty(item.Value))
                    .Select(item => item.DatabaseNodeFieldId)
                    .Distinct();
                // Get the node fields that are to be updated.
                var nodeFields = context.DatabaseNodeFields
                    .Where(item => item.Database.DatabaseType.Name != "Generic")
                    .Where(item => itemNodeFieldIds.Contains(item.Id))
                    .Include(item => item.Database)
                    .AsEnumerable();
                // Check if there weren't any node fields found.
                if (nodeFields == null || !nodeFields.Any())
                {
                    // Throw an exception.
                    throw new ArgumentException("No database node fields could be found in the database with the provided IDs.");
                }
                // Get the valid database node field IDs.
                var validItemNodeFieldIds = nodeFields
                    .Select(item => item.Id);
                // Save the nodes to add.
                var nodes = new List<Node>();
                // Go over each of the items.
                foreach (var batchItem in batchItems)
                {
                    // Check if the ID of the current item is valid.
                    if (!string.IsNullOrEmpty(batchItem.Id) && !validItemNodeIds.Contains(batchItem.Id))
                    {
                        // Continue.
                        continue;
                    }
                    // Get the valid item fields and the node field nodes to add.
                    var nodeFieldNodes = batchItem.DatabaseNodeFieldNodes
                        .Select(item => (item.DatabaseNodeFieldId, item.Value))
                        .Distinct()
                        .Where(item => validItemNodeFieldIds.Contains(item.DatabaseNodeFieldId))
                        .Select(item => new DatabaseNodeFieldNode
                        {
                            DatabaseNodeFieldId = item.DatabaseNodeFieldId,
                            DatabaseNodeField = nodeFields.FirstOrDefault(item1 => item.DatabaseNodeFieldId == item1.Id),
                            Value = item.Value
                        })
                        .Where(item => item.DatabaseNodeField != null);
                    // Check if there weren't any node fields found.
                    if (nodeFieldNodes == null || !nodeFieldNodes.Any() || !nodeFieldNodes.Any(item => item.DatabaseNodeField.IsSearchable))
                    {
                        // Continue.
                        continue;
                    }
                    // Define the new node.
                    var node = new Node
                    {
                        Name = nodeFieldNodes.First(item => item.DatabaseNodeField.IsSearchable).Value,
                        Description = batchItem.Description,
                        DateTimeCreated = DateTime.Now,
                        DatabaseNodeFieldNodes = nodeFieldNodes.ToList(),
                        DatabaseNodes = nodeFieldNodes
                            .Select(item => item.DatabaseNodeField.Database)
                            .Distinct()
                            .Select(item => new DatabaseNode
                            {
                                DatabaseId = item.Id,
                                Database = item
                            })
                            .ToList()
                    };
                    // Check if there is any ID provided.
                    if (!string.IsNullOrEmpty(batchItem.Id))
                    {
                        // Assign it to the node.
                        node.Id = batchItem.Id;
                    }
                    // Add the new node to the list.
                    nodes.Add(node);
                }
                // Create the items.
                IEnumerableExtensions.Create(nodes, context, token);
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
            if (Items == null)
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
                // Create a new scope.
                using var scope = serviceProvider.CreateScope();
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items in the current batch.
                var batchItems = Items.Skip(index * ApplicationDbContext.BatchSize).Take(ApplicationDbContext.BatchSize);
                // Get the list of IDs from the provided items.
                var itemIds = batchItems.Select(item => item.Id);
                // Get the nodes from the database that have the given IDs.
                var nodes = context.Nodes
                    .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => itemIds.Contains(item.Id))
                    .Include(item => item.DatabaseNodeFieldNodes)
                    .Include(item => item.DatabaseNodes)
                    .AsEnumerable();
                // Check if there weren't any nodes found.
                if (nodes == null || !nodes.Any())
                {
                    // Throw an exception.
                    throw new ArgumentException("No nodes could be found in the database with the provided IDs.");
                }
                // Get the IDs of all of the node fields that are to be updated.
                var itemNodeFieldIds = batchItems
                    .Select(item => item.DatabaseNodeFieldNodes)
                    .SelectMany(item => item)
                    .Where(item => !string.IsNullOrEmpty(item.DatabaseNodeFieldId) && !string.IsNullOrEmpty(item.Value))
                    .Select(item => item.DatabaseNodeFieldId)
                    .Distinct();
                // Get the node fields that are to be updated.
                var nodeFields = context.DatabaseNodeFields
                    .Where(item => item.Database.DatabaseType.Name != "Generic")
                    .Where(item => itemNodeFieldIds.Contains(item.Id))
                    .Include(item => item.Database)
                    .AsEnumerable();
                // Check if there weren't any node fields found.
                if (nodeFields == null || !nodeFields.Any())
                {
                    // Throw an exception.
                    throw new ArgumentException("No database node fields could be found in the database with the provided IDs.");
                }
                // Get the valid database node field IDs.
                var validItemNodeFieldIds = nodeFields
                    .Select(item => item.Id);
                // Save the nodes to update.
                var nodesToUpdate = new List<Node>();
                // Go over each of the valid items.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding node.
                    var node = nodes.FirstOrDefault(item => batchItem.Id == item.Id);
                    // Check if there was no node found.
                    if (node == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Get the valid item fields and the node field nodes to add.
                    var nodeFieldNodes = batchItem.DatabaseNodeFieldNodes
                        .Select(item => (item.DatabaseNodeFieldId, item.Value))
                        .Distinct()
                        .Where(item => validItemNodeFieldIds.Contains(item.DatabaseNodeFieldId))
                        .Select(item => new DatabaseNodeFieldNode
                        {
                            DatabaseNodeFieldId = item.DatabaseNodeFieldId,
                            DatabaseNodeField = nodeFields.FirstOrDefault(item1 => item.DatabaseNodeFieldId == item1.Id),
                            NodeId = node.Id,
                            Node = node,
                            Value = item.Value
                        })
                        .Where(item => item.DatabaseNodeField != null && item.Node != null);
                    // Check if there weren't any node fields found.
                    if (nodeFieldNodes == null || !nodeFieldNodes.Any() || !nodeFieldNodes.Any(item => item.DatabaseNodeField.IsSearchable))
                    {
                        // Continue.
                        continue;
                    }
                    // Update the node.
                    node.Name = nodeFieldNodes.First(item => item.DatabaseNodeField.IsSearchable).Value;
                    node.Description = batchItem.Description;
                    node.DatabaseNodeFieldNodes = nodeFieldNodes.ToList();
                    node.DatabaseNodes = nodeFieldNodes
                        .Select(item => item.DatabaseNodeField.Database)
                        .Distinct()
                        .Select(item => new DatabaseNode
                        {
                            DatabaseId = item.Id,
                            Database = item,
                            NodeId = node.Id,
                            Node = node
                        })
                        .ToList();
                    // Add the node to the list.
                    nodesToUpdate.Add(node);
                }
                // Get the networks and analyses that contain the nodes.
                var networks = context.Networks
                    .Where(item => item.NetworkNodes.Any(item1 => nodesToUpdate.Contains(item1.Node)));
                var analyses = context.Analyses
                    .Where(item => item.AnalysisNodes.Any(item1 => nodesToUpdate.Contains(item1.Node)));
                // Delete the items.
                IQueryableExtensions.Delete(analyses, context, token);
                IQueryableExtensions.Delete(networks, context, token);
                // Update the items.
                IEnumerableExtensions.Edit(nodesToUpdate, context, token);
                // Get the edges that contain the nodes.
                var edges = context.Edges
                    .Where(item => item.EdgeNodes.Any(item1 => nodesToUpdate.Contains(item1.Node)))
                    .Include(item => item.EdgeNodes)
                        .ThenInclude(item => item.Node)
                    .AsEnumerable();
                // Go over each edge.
                foreach (var edge in edges)
                {
                    // Update its name.
                    edge.Name = string.Concat(edge.EdgeNodes.First(item => item.Type == EdgeNodeType.Source).Node.Name, " -> ", edge.EdgeNodes.First(item => item.Type == EdgeNodeType.Target).Node.Name);
                }
                // Update the items.
                IEnumerableExtensions.Edit(edges, context, token);
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
            if (Items == null)
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
                // Create a new scope.
                using var scope = serviceProvider.CreateScope();
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items in the current batch.
                var batchItems = Items.Skip(index * ApplicationDbContext.BatchSize).Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems.Select(item => item.Id);
                // Get the items with the current batch IDs.
                var nodes = context.Nodes
                    .Where(item => batchIds.Contains(item.Id));
                // Get the related entities that use the items.
                var edges = context.Edges
                    .Where(item => item.EdgeNodes.Any(item1 => nodes.Contains(item1.Node)));
                var networks = context.Networks
                    .Where(item => item.NetworkNodes.Any(item1 => nodes.Contains(item1.Node)));
                var analyses = context.Analyses
                    .Where(item => item.AnalysisNodes.Any(item1 => nodes.Contains(item1.Node)));
                // Delete the items.
                IQueryableExtensions.Delete(analyses, context, token);
                IQueryableExtensions.Delete(networks, context, token);
                IQueryableExtensions.Delete(edges, context, token);
                IQueryableExtensions.Delete(nodes, context, token);
            }
        }
    }
}
