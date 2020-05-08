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
    /// Implements a background job to delete database edge fields in the database.
    /// </summary>
    public class DeleteDatabaseEdgeFieldsBackgroundJob : BaseBackgroundJob
    {
        /// <summary>
        /// Gets or sets the IDs of the items to be deleted.
        /// </summary>
        public IEnumerable<string> Ids { get; set; }

        /// <summary>
        /// Runs the current job.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public override void Run(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if the IDs don't exist.
            if (Ids == null)
            {
                // Throw an exception.
                throw new ArgumentNullException(nameof(Ids));
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)Ids.Count() / _batchSize);
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
                var batchIds = Ids.Skip(index * _batchSize).Take(_batchSize);
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
                    Delete(analyses, context, token);
                    Delete(networks, context, token);
                    Delete(edges, context, token);
                    Delete(databaseEdgeFields, context, token);
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
