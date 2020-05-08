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
    /// Implements a background job to delete database types in the database.
    /// </summary>
    public class DeleteDatabaseTypesBackgroundJob : BaseBackgroundJob
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
                // Try to delete the items.
                try
                {
                    // Delete the items.
                    Delete(analyses, context, token);
                    Delete(networks, context, token);
                    Delete(nodeCollections, context, token);
                    Delete(edges, context, token);
                    Delete(nodes, context, token);
                    Delete(databaseEdgeFields, context, token);
                    Delete(databaseNodeFields, context, token);
                    Delete(databases, context, token);
                    Delete(databaseTypes, context, token);
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
