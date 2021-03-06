﻿using Microsoft.EntityFrameworkCore;
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
                var batchDatabaseNodeFieldIds = batchItems
                    .Where(item => item.DatabaseNodeFieldNodes != null)
                    .Select(item => item.DatabaseNodeFieldNodes)
                    .SelectMany(item => item)
                    .Where(item => item.DatabaseNodeField != null)
                    .Select(item => item.DatabaseNodeField)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Define the list of items to get.
                var validBatchIds = new List<string>();
                var databaseNodeFields = new List<DatabaseNodeField>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the related entities that appear in the current batch.
                    databaseNodeFields = context.DatabaseNodeFields
                        .Include(item => item.Database)
                        .Where(item => item.Database.DatabaseType.Name != "Generic")
                        .Where(item => batchDatabaseNodeFieldIds.Contains(item.Id))
                        .ToList();
                    // Get the valid IDs, that do not appear in the database.
                    validBatchIds = batchIds
                        .Except(context.Nodes
                            .Where(item => batchIds.Contains(item.Id))
                            .Select(item => item.Id))
                        .ToList();
                }
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
                        .Where(item => databaseNodeFields.Any(item1 => item1.Id == item.Item1))
                        .Select(item => new DatabaseNodeFieldNode
                        {
                            DatabaseNodeFieldId = item.Item1,
                            Value = item.Item2
                        });
                    // Check if there were no database node fields found.
                    if (databaseNodeFieldNodes == null)
                    {
                        // Throw an exception.
                        throw new TaskException("There were no database node fields found.", showExceptionItem, batchItem);
                    }
                    // Check if there were no searchable database node fields found.
                    var databaseNodeFieldIds = databaseNodeFieldNodes
                        .Select(item => item.DatabaseNodeFieldId)
                        .Distinct();
                    var currentDatabaseNodeFields = databaseNodeFields
                        .Where(item => databaseNodeFieldIds.Contains(item.Id));
                    if (!currentDatabaseNodeFields.Any(item => item.IsSearchable))
                    {
                        // Throw an exception.
                        throw new TaskException("There were no searchable database node fields found.", showExceptionItem, batchItem);
                    }
                    // Define the new node.
                    var node = new Node
                    {
                        DateTimeCreated = DateTime.UtcNow,
                        Name = !string.IsNullOrEmpty(batchItem.Name) ? batchItem.Name :
                            (databaseNodeFieldNodes
                                .FirstOrDefault(item => item.DatabaseNodeFieldId == currentDatabaseNodeFields
                                    .FirstOrDefault(item => item.IsSearchable)?.Id)?.Value ??
                            "Unnamed node"),
                        Description = batchItem.Description,
                        DatabaseNodeFieldNodes = databaseNodeFieldNodes.ToList(),
                        DatabaseNodes = currentDatabaseNodeFields
                            .Select(item => item.Database)
                            .Distinct()
                            .Select(item => new DatabaseNode
                            {
                                DatabaseId = item.Id
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
                await IEnumerableExtensions.CreateAsync(nodesToAdd, serviceProvider, token);
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
                // Define the list of items to get.
                var nodes = new List<Node>();
                var databaseNodeFields = new List<DatabaseNodeField>();
                // Define the dependent list of items to get.
                var analysisInputs = new List<AnalysisInputModel>();
                var networkInputs = new List<NetworkInputModel>();
                var edgeInputs = new List<EdgeInputModel>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    var items = context.Nodes
                        .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                        .Where(item => batchIds.Contains(item.Id));
                    // Check if there were no items found.
                    if (items == null || !items.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Get the items found.
                    nodes = items
                        .ToList();
                    // Get the related entities that appear in the current batch.
                    databaseNodeFields = context.DatabaseNodeFields
                        .Include(item => item.Database)
                        .Where(item => item.Database.DatabaseType.Name != "Generic")
                        .Where(item => batchDatabaseNodeFieldIds.Contains(item.Id))
                        .ToList();
                }
                // Get the IDs of the items.
                var nodeIds = nodes
                    .Select(item => item.Id);
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
                        .Where(item => databaseNodeFields.Any(item1 => item1.Id == item.Item1))
                        .Select(item => new DatabaseNodeFieldNode
                        {
                            DatabaseNodeFieldId = item.Item1,
                            Value = item.Item2
                        });
                    // Check if there were no database node fields found.
                    if (databaseNodeFieldNodes == null)
                    {
                        // Throw an exception.
                        throw new TaskException("There were no database node fields found.", showExceptionItem, batchItem);
                    }
                    // Check if there were no searchable database node fields found.
                    var databaseNodeFieldIds = databaseNodeFieldNodes
                        .Select(item => item.DatabaseNodeFieldId)
                        .Distinct();
                    var currentDatabaseNodeFields = databaseNodeFields
                        .Where(item => databaseNodeFieldIds.Contains(item.Id));
                    if (!currentDatabaseNodeFields.Any(item => item.IsSearchable))
                    {
                        // Throw an exception.
                        throw new TaskException("There were no searchable database node fields found.", showExceptionItem, batchItem);
                    }
                    // Update the node.
                    node.Name = !string.IsNullOrEmpty(batchItem.Name) ? batchItem.Name :
                        (databaseNodeFieldNodes
                            .FirstOrDefault(item => item.DatabaseNodeFieldId == currentDatabaseNodeFields
                                .FirstOrDefault(item => item.IsSearchable)?.Id)?.Value ??
                        "Unnamed node");
                    node.Description = batchItem.Description;
                    node.DatabaseNodeFieldNodes = databaseNodeFieldNodes.ToList();
                    node.DatabaseNodes = currentDatabaseNodeFields
                        .Select(item => item.Database)
                        .Distinct()
                        .Select(item => new DatabaseNode
                        {
                            DatabaseId = item.Id
                        })
                        .ToList();
                    // Add the node to the list.
                    nodesToEdit.Add(node);
                }
                // Delete the dependent entities.
                await NodeExtensions.DeleteDependentAnalysesAsync(nodeIds, serviceProvider, token);
                await NodeExtensions.DeleteDependentNetworksAsync(nodeIds, serviceProvider, token);
                await NodeExtensions.DeleteDependentNodeCollectionsAsync(nodeIds, serviceProvider, token);
                await NodeExtensions.DeleteDependentEdgesAsync(nodeIds, serviceProvider, token);
                // Delete the related entities.
                await NodeExtensions.DeleteRelatedEntitiesAsync<DatabaseNodeFieldNode>(nodeIds, serviceProvider, token);
                await NodeExtensions.DeleteRelatedEntitiesAsync<DatabaseNode>(nodeIds, serviceProvider, token);
                // Edit the items.
                await IEnumerableExtensions.EditAsync(nodesToEdit, serviceProvider, token);
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
                var nodes = new List<Node>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    var items = context.Nodes
                        .Where(item => batchIds.Contains(item.Id));
                    // Check if there were no items found.
                    if (items == null || !items.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Get the items found.
                    nodes = items
                        .ToList();
                }
                // Get the IDs of the items.
                var nodeIds = nodes
                    .Select(item => item.Id);
                // Delete the dependent entities.
                await NodeExtensions.DeleteDependentAnalysesAsync(nodeIds, serviceProvider, token);
                await NodeExtensions.DeleteDependentNetworksAsync(nodeIds, serviceProvider, token);
                await NodeExtensions.DeleteDependentNodeCollectionsAsync(nodeIds, serviceProvider, token);
                await NodeExtensions.DeleteDependentEdgesAsync(nodeIds, serviceProvider, token);
                // Delete the related entities.
                await NodeExtensions.DeleteRelatedEntitiesAsync<DatabaseNodeFieldNode>(nodeIds, serviceProvider, token);
                await NodeExtensions.DeleteRelatedEntitiesAsync<DatabaseNode>(nodeIds, serviceProvider, token);
                // Delete the items.
                await IEnumerableExtensions.DeleteAsync(nodes, serviceProvider, token);
            }
        }
    }
}
