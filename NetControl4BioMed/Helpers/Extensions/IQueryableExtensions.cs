using NetControl4BioMed.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Extensions
{
    /// <summary>
    /// Provides extensions for IQueryables.
    /// </summary>
    public static class IQueryableExtensions
    {
        /// <summary>
        /// Deletes the provided items from the database.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="items">The items to be updated.</param>
        /// <param name="context">The application database context.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public static void Delete<T>(IQueryable<T> items, ApplicationDbContext context, CancellationToken token) where T : class
        {
            // Check if the items don't exist.
            if (items == null)
            {
                // Throw an exception.
                throw new ArgumentNullException("There provided items can't be null.");
            }
            // Get the corresponding database set.
            var set = context.Set<T>();
            // Check if the correpsonding set doesn't exist.
            if (set == null)
            {
                // Throw an exception.
                throw new ArgumentException("The provided type is not valid.");
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)items.Count() / ApplicationDbContext.BatchSize);
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
                var batchItems = items
                    .Take(ApplicationDbContext.BatchSize);
                // Mark the items for deletion.
                set.RemoveRange(batchItems);
                // Save the changes to the database.
                context.SaveChanges();
            }
        }
    }
}
