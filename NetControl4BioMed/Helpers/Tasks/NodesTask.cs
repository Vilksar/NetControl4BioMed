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
        /// <returns>The created items.</returns>
        public IEnumerable<Node> Create(IServiceProvider serviceProvider, CancellationToken token)
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
                    .Except(context.Nodes
                        .Where(item => batchIds.Contains(item.Id))
                        .Select(item => item.Id));
                // Get the IDs of the related entities that appear in the current batch.
                var batchDatabaseNodeFieldIds = batchItems
                    .Where(item => item.DatabaseNodeFieldNodes != null)
                    .Select(item => item.DatabaseNodeFieldNodes)
                    .SelectMany(item => item)
                    .Where(item => item.DatabaseNodeField != null)
                    .Select(item => item.DatabaseNodeField)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Get the related entities that appear in the current batch.
                var batchDatabaseNodeFields = context.DatabaseNodeFields
                    .Include(item => item.Database)
                    .Where(item => item.Database.DatabaseType.Name != "Generic")
                    .Where(item => batchDatabaseNodeFieldIds.Contains(item.Id));
                // Save the items to add.
                var nodesToAdd = new List<Node>();
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Check if the ID of the item is not valid.
                    if (!string.IsNullOrEmpty(batchItem.Id) && !validBatchIds.Contains(batchItem.Id))
                    {
                        // Continue.
                        continue;
                    }
                    // Check if there are no database node field nodes provided.
                    if (batchItem.DatabaseNodeFieldNodes == null || !batchItem.DatabaseNodeFieldNodes.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no database node field nodes provided.", showExceptionItem, batchItem);
                    }
                    // Get the database node field nodes.
                    var databaseNodeFieldNodes = batchItem.DatabaseNodeFieldNodes
                        .Where(item => item.DatabaseNodeField != null)
                        .Where(item => !string.IsNullOrEmpty(item.DatabaseNodeField.Id))
                        .Where(item => !string.IsNullOrEmpty(item.Value))
                        .Select(item => (item.DatabaseNodeField.Id, item.Value))
                        .Distinct()
                        .Where(item => batchDatabaseNodeFields.Any(item1 => item1.Id == item.Item1))
                        .Select(item => new DatabaseNodeFieldNode
                        {
                            DatabaseNodeFieldId = item.Item1,
                            DatabaseNodeField = batchDatabaseNodeFields
                                .FirstOrDefault(item1 => item1.Id == item.Item1),
                            Value = item.Item2
                        })
                        .Where(item => item.DatabaseNodeField != null);
                    // Check if there were no database node field nodes found.
                    if (databaseNodeFieldNodes == null || !databaseNodeFieldNodes.Any(item => item.DatabaseNodeField.IsSearchable))
                    {
                        // Throw an exception.
                        throw new TaskException("There were no database node field nodes found.", showExceptionItem, batchItem);
                    }
                    // Define the new node.
                    var node = new Node
                    {
                        DateTimeCreated = DateTime.Now,
                        Name = !string.IsNullOrEmpty(batchItem.Name) ? batchItem.Name :
                            (databaseNodeFieldNodes.FirstOrDefault(item => item.DatabaseNodeField.IsSearchable)?.Value ?? "Unnamed node"),
                        Description = batchItem.Description,
                        DatabaseNodeFieldNodes = databaseNodeFieldNodes.ToList(),
                        DatabaseNodes = databaseNodeFieldNodes
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
                        // Assign it to the item.
                        node.Id = batchItem.Id;
                    }
                    // Add the item to the list.
                    nodesToAdd.Add(node);
                }
                // Create the items.
                IEnumerableExtensions.Create(nodesToAdd, context, token);
                // Go over each item.
                foreach (var node in nodesToAdd)
                {
                    // Yield return it.
                    yield return node;
                }
            }
        }

        /// <summary>
        /// Edits the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns>The edited items.</returns>
        public IEnumerable<Node> Edit(IServiceProvider serviceProvider, CancellationToken token)
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
                    .Select(item => item.Id)
                    .Distinct();
                // Get the IDs of the related entities that appear in the current batch.
                var batchDatabaseNodeFieldIds = batchItems
                    .Where(item => item.DatabaseNodeFieldNodes != null)
                    .Select(item => item.DatabaseNodeFieldNodes)
                    .SelectMany(item => item)
                    .Where(item => item.DatabaseNodeField != null)
                    .Select(item => item.DatabaseNodeField)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Get the related entities that appear in the current batch.
                var batchDatabaseNodeFields = context.DatabaseNodeFields
                    .Include(item => item.Database)
                    .Where(item => item.Database.DatabaseType.Name != "Generic")
                    .Where(item => batchDatabaseNodeFieldIds.Contains(item.Id));
                // Get the items corresponding to the current batch.
                var nodes = context.Nodes
                    .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => batchIds.Contains(item.Id));
                // Get the related entities to delete.
                var batchDatabaseNodes = context.DatabaseNodes
                    .Where(item => nodes.Contains(item.Node));
                var batchDatabaseNodeFieldNodes = context.DatabaseNodeFieldNodes
                    .Where(item => nodes.Contains(item.Node));
                // Save the items to edit.
                var nodesToEdit = new List<Node>();
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding item.
                    var node = nodes
                        .FirstOrDefault(item => item.Id == batchItem.Id);
                    // Check if there was no item found.
                    if (node == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Check if there are no database node field nodes provided.
                    if (batchItem.DatabaseNodeFieldNodes == null || !batchItem.DatabaseNodeFieldNodes.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no database node field nodes provided.", showExceptionItem, batchItem);
                    }
                    // Get the database node field nodes.
                    var databaseNodeFieldNodes = batchItem.DatabaseNodeFieldNodes
                        .Where(item => item.DatabaseNodeField != null)
                        .Where(item => !string.IsNullOrEmpty(item.DatabaseNodeField.Id))
                        .Where(item => !string.IsNullOrEmpty(item.Value))
                        .Select(item => (item.DatabaseNodeField.Id, item.Value))
                        .Distinct()
                        .Where(item => batchDatabaseNodeFields.Any(item1 => item1.Id == item.Item1))
                        .Select(item => new DatabaseNodeFieldNode
                        {
                            DatabaseNodeFieldId = item.Item1,
                            DatabaseNodeField = batchDatabaseNodeFields
                                .FirstOrDefault(item1 => item1.Id == item.Item1),
                            Value = item.Item2
                        })
                        .Where(item => item.DatabaseNodeField != null);
                    // Check if there were no database node field nodes found.
                    if (databaseNodeFieldNodes == null || !databaseNodeFieldNodes.Any(item => item.DatabaseNodeField.IsSearchable))
                    {
                        // Throw an exception.
                        throw new TaskException("There were no database node field nodes found.", showExceptionItem, batchItem);
                    }
                    // Delete all related entities that appear in the current batch.
                    IQueryableExtensions.Delete(batchDatabaseNodes.Where(item => item.Node == node), context, token);
                    IQueryableExtensions.Delete(batchDatabaseNodeFieldNodes.Where(item => item.Node == node), context, token);
                    // Update the node.
                    node.Name = !string.IsNullOrEmpty(batchItem.Name) ? batchItem.Name :
                        (databaseNodeFieldNodes.FirstOrDefault(item => item.DatabaseNodeField.IsSearchable)?.Value ?? "Unnamed node");
                    node.Description = batchItem.Description;
                    node.DatabaseNodeFieldNodes = databaseNodeFieldNodes.ToList();
                    node.DatabaseNodes = databaseNodeFieldNodes
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
                    nodesToEdit.Add(node);
                }
                // Get the networks and analyses that contain the nodes.
                var networks = context.Networks
                    .Where(item => item.NetworkNodes.Any(item1 => nodesToEdit.Contains(item1.Node)));
                var analyses = context.Analyses
                    .Where(item => item.AnalysisNodes.Any(item1 => nodesToEdit.Contains(item1.Node)));
                // Delete the items.
                IQueryableExtensions.Delete(analyses, context, token);
                IQueryableExtensions.Delete(networks, context, token);
                // Edit the items.
                IEnumerableExtensions.Edit(nodesToEdit, context, token);
                // Get the edges that contain the nodes.
                var edges = context.Edges
                    .Include(item => item.EdgeNodes)
                        .ThenInclude(item => item.Node)
                    .Where(item => item.EdgeNodes.Any(item1 => nodesToEdit.Contains(item1.Node)));
                // Go over each edge.
                foreach (var edge in edges)
                {
                    // Update its name.
                    edge.Name = string.Concat(edge.EdgeNodes.First(item => item.Type == EdgeNodeType.Source).Node.Name, " - ", edge.EdgeNodes.First(item => item.Type == EdgeNodeType.Target).Node.Name);
                }
                // Update the items.
                IEnumerableExtensions.Edit(edges, context, token);
                // Go over each item.
                foreach (var node in nodesToEdit)
                {
                    // Yield return it.
                    yield return node;
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
