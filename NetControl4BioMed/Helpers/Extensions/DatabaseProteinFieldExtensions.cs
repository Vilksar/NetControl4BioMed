using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Extensions
{
    /// <summary>
    /// Provides extensions for the database protein fields.
    /// </summary>
    public static class DatabaseProteinFieldExtensions
    {
        /// <summary>
        /// Deletes the dependent proteins of the corresponding database protein fields.
        /// </summary>
        /// <param name="databaseProteinFieldIds">The database protein fields whose entities should be deleted.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public static async Task DeleteDependentProteinsAsync(IEnumerable<string> databaseProteinFieldIds, IServiceProvider serviceProvider, CancellationToken token)
        {
            // Define a variable to store the total number of entities.
            var entityCount = 0;
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items in the current batch.
                entityCount = context.DatabaseProteinFieldProteins
                    .Where(item => databaseProteinFieldIds.Contains(item.DatabaseProteinField.Id))
                    .Select(item => item.Protein)
                    .Distinct()
                    .Count();
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)entityCount / ApplicationDbContext.BatchSize);
            // Go over each batch.
            for (int index = 0; index < count; index++)
            {
                // Check if the cancellation was requested.
                if (token.IsCancellationRequested)
                {
                    // Break.
                    break;
                }
                // Define the batch items.
                var batchItemInputs = new List<ProteinInputModel>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items in the current batch.
                    batchItemInputs = context.DatabaseProteinFieldProteins
                        .Where(item => databaseProteinFieldIds.Contains(item.DatabaseProteinField.Id))
                        .Select(item => item.Protein)
                        .Distinct()
                        .Select(item => new ProteinInputModel
                        {
                            Id = item.Id
                        })
                        .Take(ApplicationDbContext.BatchSize)
                        .ToList();
                    // Check if there were no items found.
                    if (batchItemInputs == null || !batchItemInputs.Any())
                    {
                        // Continue.
                        continue;
                    }
                }
                // Delete the items.
                await new ProteinsTask { Items = batchItemInputs }.DeleteAsync(serviceProvider, token);
            }
        }
    }
}
