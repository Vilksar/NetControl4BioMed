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
    /// Provides extensions for IEnumerables.
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Creates the provided items in the database.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="items">The items to be created.</param>
        /// <param name="context">The application database context.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public static void Create<T>(IEnumerable<T> items, ApplicationDbContext context, CancellationToken token) where T : class
        {
            // Check if the items don't exist.
            if (items == null)
            {
                // Throw an exception.
                throw new ArgumentNullException(nameof(items));
            }
            // Get the corresponding database set.
            var set = context.Set<T>();
            // Check if the correpsonding set doesn't exist.
            if (set == null || !set.Any())
            {
                // Throw an exception.
                throw new ArgumentException(nameof(T));
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
                var batchItems = items.Skip(index * ApplicationDbContext.BatchSize).Take(ApplicationDbContext.BatchSize);
                // Mark the items for addition.
                set.AddRange(batchItems);
                // Save the changes to the database.
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Updates the provided items in the database.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="items">The items to be updated.</param>
        /// <param name="context">The application database context.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public static void Edit<T>(IEnumerable<T> items, ApplicationDbContext context, CancellationToken token) where T : class
        {
            // Check if the items don't exist.
            if (items == null)
            {
                // Throw an exception.
                throw new ArgumentNullException(nameof(items));
            }
            // Get the corresponding database set.
            var set = context.Set<T>();
            // Check if the correpsonding set doesn't exist.
            if (set == null || !set.Any())
            {
                // Throw an exception.
                throw new ArgumentException(nameof(T));
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
                var batchItems = items.Skip(index * ApplicationDbContext.BatchSize).Take(ApplicationDbContext.BatchSize);
                // Mark the items for update.
                set.UpdateRange(batchItems);
                // Save the changes to the database.
                context.SaveChanges();
            }
        }
    }
}
