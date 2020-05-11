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
        public void Create(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Throw an exception.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Edits the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void Edit(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Throw an exception.
            throw new NotImplementedException();
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
                // Try to delete the items.
                try
                {
                    // Delete the items.
                    IQueryableExtensions.Delete(analyses, context, token);
                    IQueryableExtensions.Delete(networks, context, token);
                    IQueryableExtensions.Delete(edges, context, token);
                    IQueryableExtensions.Delete(databaseEdgeFields, context, token);
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
