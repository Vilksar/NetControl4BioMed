using DocumentFormat.OpenXml.InkML;
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
                // Get the IDs of the related entities that appear in the current batch.
                var databaseTypeIds = batchItems.Select(item => item.DatabaseTypeId);
                // Get the related entities that appear in the current batch.
                var databaseTypes = context.DatabaseTypes.Where(item => databaseTypeIds.Contains(item.Id));
                // Define the items corresponding to the current batch.
                var databases = batchItems.Select(item => new Database
                {
                    Name = item.Name,
                    Description = item.Description,
                    DateTimeCreated = DateTime.Now,
                    Url = item.Url,
                    IsPublic = item.IsPublic,
                    DatabaseTypeId = item.DatabaseTypeId,
                    DatabaseType = databaseTypes.First(item1 => item1.Id == item.DatabaseTypeId)
                });
                // Create the items.
                IEnumerableExtensions.Create(databases, context, token);
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
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems.Select(item => item.Id);
                // Get the IDs of the related entities that appear in the current batch.
                var databaseTypeIds = batchItems.Select(item => item.DatabaseTypeId);
                // Get the related entities that appear in the current batch.
                var databaseTypes = context.DatabaseTypes.Where(item => databaseTypeIds.Contains(item.Id));
                // Get the items corresponding to the current batch.
                var databases = context.Databases
                    .Where(item => batchIds.Contains(item.Id));
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding item.
                    var database = databases.First(item => item.Id == batchItem.Id);
                    // Get the related entities.
                    var databaseType = databaseTypes.First(item1 => item1.Id == batchItem.DatabaseTypeId);
                    // Update the item.
                    database.Name = batchItem.Name;
                    database.Description = batchItem.Description;
                    database.Url = batchItem.Url;
                    database.IsPublic = batchItem.IsPublic;
                    database.DatabaseTypeId = databaseType.Id;
                    database.DatabaseType = databaseType;
                }
                // Edit the items.
                IEnumerableExtensions.Edit(databases, context, token);
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
                // Get the IDs of the items in the current batch.
                var batchIds = Items.Skip(index * ApplicationDbContext.BatchSize).Take(ApplicationDbContext.BatchSize).Select(item => item.Id);
                // Create a new scope.
                using var scope = serviceProvider.CreateScope();
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
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
                IQueryableExtensions.Delete(analyses, context, token);
                IQueryableExtensions.Delete(networks, context, token);
                IQueryableExtensions.Delete(nodeCollections, context, token);
                IQueryableExtensions.Delete(edges, context, token);
                IQueryableExtensions.Delete(nodes, context, token);
                IQueryableExtensions.Delete(databaseEdgeFields, context, token);
                IQueryableExtensions.Delete(databaseNodeFields, context, token);
                IQueryableExtensions.Delete(databases, context, token);
            }
        }
    }
}
