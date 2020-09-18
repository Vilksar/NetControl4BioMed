using DocumentFormat.OpenXml.InkML;
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
    /// Implements a task to update databases in the database.
    /// </summary>
    public class DatabasesTask
    {
        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<DatabaseInputModel> Items { get; set; }

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
                    .Except(context.Databases
                        .Where(item => batchIds.Contains(item.Id))
                        .Select(item => item.Id));
                // Get the IDs of the related entities that appear in the current batch.
                var batchDatabaseTypeIds = batchItems
                    .Where(item => item.DatabaseType != null)
                    .Select(item => item.DatabaseType)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Get the related entities that appear in the current batch.
                var batchDatabaseTypes = context.DatabaseTypes
                    .Where(item => batchDatabaseTypeIds.Contains(item.Id));
                // Save the items to add.
                var databasesToAdd = new List<Database>();
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Check if the ID of the item is not valid.
                    if (!string.IsNullOrEmpty(batchItem.Id) && !validBatchIds.Contains(batchItem.Id))
                    {
                        // Continue.
                        continue;
                    }
                    // Check if there is another database with the same name.
                    if (context.Databases.Any(item => item.Name == batchItem.Name) || databasesToAdd.Any(item => item.Name == batchItem.Name))
                    {
                        // Throw an exception.
                        throw new TaskException("A database with the same name already exists.", showExceptionItem, batchItem);
                    }
                    // Check if there was no database type provided.
                    if (batchItem.DatabaseType == null || string.IsNullOrEmpty(batchItem.DatabaseType.Id))
                    {
                        // Throw an exception.
                        throw new TaskException("There was no database type provided.", showExceptionItem, batchItem);
                    }
                    // Get the database type.
                    var databaseType = batchDatabaseTypes
                        .FirstOrDefault(item => item.Id == batchItem.DatabaseType.Id);
                    // Check if there was no database type found.
                    if (databaseType == null)
                    {
                        // Throw an exception.
                        throw new TaskException("There was no database type found.", showExceptionItem, batchItem);
                    }
                    // Check if the database type is generic.
                    if (databaseType.Name == "Generic")
                    {
                        // Throw an exception.
                        throw new TaskException($"The database can't be generic.", showExceptionItem, batchItem);
                    }
                    // Define the new item.
                    var database = new Database
                    {
                        DateTimeCreated = DateTime.UtcNow,
                        Name = batchItem.Name,
                        Description = batchItem.Description,
                        Url = batchItem.Url,
                        IsPublic = batchItem.IsPublic,
                        DatabaseTypeId = databaseType.Id,
                        DatabaseType = databaseType
                    };
                    // Check if there is any ID provided.
                    if (!string.IsNullOrEmpty(batchItem.Id))
                    {
                        // Assign it to the item.
                        database.Id = batchItem.Id;
                    }
                    // Add the item to the list.
                    databasesToAdd.Add(database);
                }
                // Create the items.
                await IEnumerableExtensions.CreateAsync(databasesToAdd, serviceProvider, token);
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
                    .Select(item => item.Id)
                    .Distinct();
                // Get the items corresponding to the current batch.
                var databases = context.Databases
                    .Include(item => item.DatabaseType)
                    .Where(item => batchIds.Contains(item.Id));
                // Save the items to edit.
                var databasesToEdit = new List<Database>();
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding items.
                    var database = databases
                        .FirstOrDefault(item => item.Id == batchItem.Id);
                    // Check if there was no item found.
                    if (database == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Check if the database type of the database is generic.
                    if (database.DatabaseType.Name == "Generic")
                    {
                        // Throw an exception.
                        throw new TaskException("The generic database can't be edited.", showExceptionItem, batchItem);
                    }
                    // Check if there is another database with the same name.
                    if (context.Databases.Any(item => item.Id != database.Id && item.Name == batchItem.Name) || databasesToEdit.Any(item => item.Name == batchItem.Name))
                    {
                        // Throw an exception.
                        throw new TaskException("A database with the same name already exists.", showExceptionItem, batchItem);
                    }
                    // Update the item.
                    database.Name = batchItem.Name;
                    database.Description = batchItem.Description;
                    database.Url = batchItem.Url;
                    database.IsPublic = batchItem.IsPublic;
                    // Add the item to the list.
                    databasesToEdit.Add(database);
                }
                // Edit the items.
                await IEnumerableExtensions.EditAsync(databasesToEdit, serviceProvider, token);
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
                // Get the items with the provided IDs.
                var databases = context.Databases
                    .Where(item => batchIds.Contains(item.Id));
                // Get the related entities that use the items.
                var databaseNodeFields = context.DatabaseNodeFields
                    .Where(item => databases.Contains(item.Database));
                var databaseEdgeFields = context.DatabaseEdgeFields
                    .Where(item => databases.Contains(item.Database));
                var nodes = context.Nodes
                    .Where(item => item.DatabaseNodes.Any(item1 => databases.Contains(item1.Database)));
                var edges = context.Edges
                    .Where(item => item.DatabaseEdges.Any(item1 => databases.Contains(item1.Database)));
                var nodeCollections = context.NodeCollections
                    .Where(item => item.NodeCollectionDatabases.Any(item1 => databases.Contains(item1.Database)));
                var networks = context.Networks
                    .Where(item => item.NetworkDatabases.Any(item1 => databases.Contains(item1.Database)));
                var analyses = context.Analyses
                    .Where(item => item.AnalysisDatabases.Any(item1 => databases.Contains(item1.Database)));
                // Delete the items.
                await IQueryableExtensions.DeleteAsync(analyses, serviceProvider, token);
                await IQueryableExtensions.DeleteAsync(networks, serviceProvider, token);
                await IQueryableExtensions.DeleteAsync(nodeCollections, serviceProvider, token);
                await IQueryableExtensions.DeleteAsync(edges, serviceProvider, token);
                await IQueryableExtensions.DeleteAsync(nodes, serviceProvider, token);
                await IQueryableExtensions.DeleteAsync(databaseEdgeFields, serviceProvider, token);
                await IQueryableExtensions.DeleteAsync(databaseNodeFields, serviceProvider, token);
                await IQueryableExtensions.DeleteAsync(databases, serviceProvider, token);
            }
        }
    }
}
