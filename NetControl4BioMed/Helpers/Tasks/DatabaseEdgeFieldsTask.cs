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
    /// Implements a task to update database edge fields in the database.
    /// </summary>
    public class DatabaseEdgeFieldsTask
    {
        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<DatabaseEdgeFieldInputModel> Items { get; set; }

        /// <summary>
        /// Creates the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns>The created items.</returns>
        public IEnumerable<DatabaseEdgeField> Create(IServiceProvider serviceProvider, CancellationToken token)
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
                    .Except(context.DatabaseEdgeFields
                        .Where(item => batchIds.Contains(item.Id))
                        .Select(item => item.Id));
                // Get the IDs of the related entities that appear in the current batch.
                var batchDatabaseIds = batchItems
                    .Where(item => item.Database != null)
                    .Select(item => item.Database)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Get the related entities that appear in the current batch.
                var batchDatabases = context.Databases
                    .Include(item => item.DatabaseType)
                    .Where(item => batchDatabaseIds.Contains(item.Id));
                // Save the items to add.
                var databaseEdgeFieldsToAdd = new List<DatabaseEdgeField>();
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Check if the ID of the item is not valid.
                    if (!string.IsNullOrEmpty(batchItem.Id) && !validBatchIds.Contains(batchItem.Id))
                    {
                        // Continue.
                        continue;
                    }
                    // Check if there is another database edge field with the same name.
                    if (context.DatabaseEdgeFields.Any(item => item.Name == batchItem.Name) || databaseEdgeFieldsToAdd.Any(item => item.Name == batchItem.Name))
                    {
                        // Throw an exception.
                        throw new TaskException("A database edge field with the same name already exists.", showExceptionItem, batchItem);
                    }
                    // Check if there was no database provided.
                    if (batchItem.Database == null || string.IsNullOrEmpty(batchItem.Database.Id))
                    {
                        // Throw an exception.
                        throw new TaskException("There was no database provided.", showExceptionItem, batchItem);
                    }
                    // Get the database.
                    var database = batchDatabases
                        .FirstOrDefault(item => item.Id == batchItem.Database.Id);
                    // Check if there was no database found.
                    if (database == null)
                    {
                        // Throw an exception.
                        throw new TaskException("There was no database found.", showExceptionItem, batchItem);
                    }
                    // Check if the database is generic.
                    if (database.DatabaseType.Name == "Generic")
                    {
                        // Throw an exception.
                        throw new TaskException("The database edge field can't be generic.", showExceptionItem, batchItem);
                    }
                    // Define the new item.
                    var databaseEdgeField = new DatabaseEdgeField
                    {
                        DateTimeCreated = DateTime.Now,
                        Name = batchItem.Name,
                        Description = batchItem.Description,
                        Url = batchItem.Url,
                        IsSearchable = batchItem.IsSearchable,
                        DatabaseId = database.Id,
                        Database = database
                    };
                    // Check if there is any ID provided.
                    if (!string.IsNullOrEmpty(batchItem.Id))
                    {
                        // Assign it to the item.
                        database.Id = batchItem.Id;
                    }
                    // Add the item to the list.
                    databaseEdgeFieldsToAdd.Add(databaseEdgeField);
                }
                // Create the items.
                IEnumerableExtensions.Create(databaseEdgeFieldsToAdd, context, token);
                // Go over each item.
                foreach (var databaseEdgeField in databaseEdgeFieldsToAdd)
                {
                    // Yield return it.
                    yield return databaseEdgeField;
                }
            }
        }

        /// <summary>
        /// Edits the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns>The edited items.</returns>
        public IEnumerable<DatabaseEdgeField> Edit(IServiceProvider serviceProvider, CancellationToken token)
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
                var databaseEdgeFields = context.DatabaseEdgeFields
                    .Include(item => item.Database)
                        .ThenInclude(item => item.DatabaseType)
                    .Where(item => batchIds.Contains(item.Id));
                // Save the items to edit.
                var databaseEdgeFieldsToEdit = new List<DatabaseEdgeField>();
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding items.
                    var databaseEdgeField = databaseEdgeFields
                        .FirstOrDefault(item => item.Id == batchItem.Id);
                    // Check if there was no item found.
                    if (databaseEdgeField == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Check if the database edge field is generic.
                    if (databaseEdgeField.Database.DatabaseType.Name == "Generic")
                    {
                        // Throw an exception.
                        throw new TaskException("The generic database edge field can't be edited.", showExceptionItem, batchItem);
                    }
                    // Check if there is another database edge field with the same name.
                    if (context.DatabaseEdgeFields.Any(item => item.Id != databaseEdgeField.Id && item.Name == batchItem.Name) || databaseEdgeFieldsToEdit.Any(item => item.Name == batchItem.Name))
                    {
                        // Throw an exception.
                        throw new TaskException("A database edge field with the name already exists.", showExceptionItem, batchItem);
                    }
                    // Update the item.
                    databaseEdgeField.Name = batchItem.Name;
                    databaseEdgeField.Description = batchItem.Description;
                    databaseEdgeField.Url = batchItem.Url;
                    databaseEdgeField.IsSearchable = batchItem.IsSearchable;
                    // Add the item to the list.
                    databaseEdgeFieldsToEdit.Add(databaseEdgeField);
                }
                // Edit the items.
                IEnumerableExtensions.Edit(databaseEdgeFieldsToEdit, context, token);
                // Go over each item.
                foreach (var databaseEdgeField in databaseEdgeFieldsToEdit)
                {
                    // Yield return it.
                    yield return databaseEdgeField;
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
                var databaseEdgeFields = context.DatabaseEdgeFields
                    .Where(item => batchIds.Contains(item.Id));
                // Get the related entities that use the items.
                var edges = context.Edges
                    .Where(item => item.DatabaseEdgeFieldEdges.Any(item1 => databaseEdgeFields.Contains(item1.DatabaseEdgeField)));
                var networks = context.Networks
                    .Where(item => item.NetworkEdges.Any(item1 => edges.Contains(item1.Edge)));
                var analyses = context.Analyses
                    .Where(item => item.AnalysisEdges.Any(item1 => edges.Contains(item1.Edge)));
                // Delete the items.
                IQueryableExtensions.Delete(analyses, context, token);
                IQueryableExtensions.Delete(networks, context, token);
                IQueryableExtensions.Delete(edges, context, token);
                IQueryableExtensions.Delete(databaseEdgeFields, context, token);
            }
        }
    }
}
