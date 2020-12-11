using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.InputModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NetControl4BioMed.Helpers.Extensions;
using System.Threading.Tasks;
using NetControl4BioMed.Helpers.Exceptions;

namespace NetControl4BioMed.Helpers.Tasks
{
    /// <summary>
    /// Implements a task to update paths in the database.
    /// </summary>
    public class PathsTask
    {
        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<PathInputModel> Items { get; set; }

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
                var paths = new List<Path>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    var items = context.Paths
                        .Where(item => batchIds.Contains(item.Id));
                    // Check if there were no items found.
                    if (items == null || !items.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Get the items found.
                    paths = items
                        .ToList();
                }
                // Get the IDs of the items.
                var pathIds = paths
                    .Select(item => item.Id);
                // Delete the related entities.
                await PathExtensions.DeleteRelatedEntitiesAsync<PathNode>(pathIds, serviceProvider, token);
                await PathExtensions.DeleteRelatedEntitiesAsync<PathEdge>(pathIds, serviceProvider, token);
                // Delete the items.
                await IEnumerableExtensions.DeleteAsync(paths, serviceProvider, token);
            }
        }
    }
}
