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
    /// Implements a task to update database types in the database.
    /// </summary>
    public class DatabaseTypesTask
    {
        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<DatabaseTypeInputModel> Items { get; set; }

        /// <summary>
        /// Creates the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns>The created items.</returns>
        public IEnumerable<DatabaseType> Create(IServiceProvider serviceProvider, CancellationToken token)
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
                    .Except(context.DatabaseTypes
                        .Where(item => batchIds.Contains(item.Id))
                        .Select(item => item.Id));
                // Save the items to add.
                var databaseTypesToAdd = new List<DatabaseType>();
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Check if the ID of the item is not valid.
                    if (!string.IsNullOrEmpty(batchItem.Id) && !validBatchIds.Contains(batchItem.Id))
                    {
                        // Continue.
                        continue;
                    }
                    // Check if there is another database type with the same name.
                    if (context.DatabaseTypes.Any(item => item.Name == batchItem.Name) || databaseTypesToAdd.Any(item => item.Name == batchItem.Name))
                    {
                        // Throw an exception.
                        throw new TaskException("A database type with the same name already exists.", showExceptionItem, batchItem);
                    }
                    // Define the new item.
                    var databaseType = new DatabaseType
                    {
                        DateTimeCreated = DateTime.UtcNow,
                        Name = batchItem.Name,
                        Description = batchItem.Description
                    };
                    // Check if there is any ID provided.
                    if (!string.IsNullOrEmpty(batchItem.Id))
                    {
                        // Assign it to the item.
                        databaseType.Id = batchItem.Id;
                    }
                    // Add the item to the list.
                    databaseTypesToAdd.Add(databaseType);
                }
                // Create the items.
                IEnumerableExtensions.Create(databaseTypesToAdd, context, token);
                // Go over each item.
                foreach (var databaseType in databaseTypesToAdd)
                {
                    // Yield return it.
                    yield return databaseType;
                }
            }
        }

        /// <summary>
        /// Edits the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns>The edited items.</returns>
        public IEnumerable<DatabaseType> Edit(IServiceProvider serviceProvider, CancellationToken token)
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
                // Get the items corresponding to the current batch.
                var databaseTypes = context.DatabaseTypes
                    .Where(item => batchIds.Contains(item.Id));
                // Save the items to edit.
                var databaseTypesToEdit = new List<DatabaseType>();
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding item.
                    var databaseType = databaseTypes
                        .FirstOrDefault(item => item.Id == batchItem.Id);
                    // Check if there was no item found.
                    if (databaseType == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Check if the database type is generic.
                    if (databaseType.Name == "Generic")
                    {
                        // Throw an exception.
                        throw new TaskException("The generic database type can't be edited.", showExceptionItem, batchItem);
                    }
                    // Check if there is another database type with the same name.
                    if (context.DatabaseTypes.Any(item => item.Id != databaseType.Id && item.Name == batchItem.Name) || databaseTypesToEdit.Any(item => item.Name == batchItem.Name))
                    {
                        // Throw an exception.
                        throw new TaskException("A database type with the same name already exists.", showExceptionItem, batchItem);
                    }
                    // Update the item.
                    databaseType.Name = batchItem.Name;
                    databaseType.Description = batchItem.Description;
                    // Add the item to the list.
                    databaseTypesToEdit.Add(databaseType);
                }
                // Edit the items.
                IEnumerableExtensions.Edit(databaseTypesToEdit, context, token);
                // Go over each item.
                foreach (var databaseType in databaseTypesToEdit)
                {
                    // Yield return it.
                    yield return databaseType;
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
                // Get the items with the provided IDs.
                var databaseTypes = context.DatabaseTypes
                    .Where(item => batchIds.Contains(item.Id));
                // Get the related entities that use the items.
                var databases = context.Databases
                    .Where(item => databaseTypes.Contains(item.DatabaseType));
                var databaseNodeFields = context.DatabaseNodeFields
                    .Where(item => databaseTypes.Contains(item.Database.DatabaseType));
                var databaseEdgeFields = context.DatabaseEdgeFields
                    .Where(item => databaseTypes.Contains(item.Database.DatabaseType));
                var nodes = context.Nodes
                    .Where(item => item.DatabaseNodes.Any(item1 => databaseTypes.Contains(item1.Database.DatabaseType)));
                var edges = context.Edges
                    .Where(item => item.DatabaseEdges.Any(item1 => databaseTypes.Contains(item1.Database.DatabaseType)));
                var nodeCollections = context.NodeCollections
                    .Where(item => item.NodeCollectionDatabases.Any(item1 => databaseTypes.Contains(item1.Database.DatabaseType)));
                var networks = context.Networks
                    .Where(item => item.NetworkDatabases.Any(item1 => databaseTypes.Contains(item1.Database.DatabaseType)));
                var analyses = context.Analyses
                    .Where(item => item.AnalysisDatabases.Any(item1 => databaseTypes.Contains(item1.Database.DatabaseType)));
                // Delete the items.
                IQueryableExtensions.Delete(analyses, context, token);
                IQueryableExtensions.Delete(networks, context, token);
                IQueryableExtensions.Delete(nodeCollections, context, token);
                IQueryableExtensions.Delete(edges, context, token);
                IQueryableExtensions.Delete(nodes, context, token);
                IQueryableExtensions.Delete(databaseEdgeFields, context, token);
                IQueryableExtensions.Delete(databaseNodeFields, context, token);
                IQueryableExtensions.Delete(databases, context, token);
                IQueryableExtensions.Delete(databaseTypes, context, token);
            }
        }
    }
}
