using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.BackgroundJobs
{
    /// <summary>
    /// Represents an abstract implementation of a background job.
    /// </summary>
    public abstract class BaseBackgroundJob
    {
        /// <summary>
        /// Represents the batch size for a database operation.
        /// </summary>
        protected readonly int _batchSize = 200;

        /// <summary>
        /// Runs the current job.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token.</param>
        public abstract void Run(IServiceProvider serviceProvider, CancellationToken token);

        /// <summary>
        /// Creates the provided items in the database.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="items">The items to be created.</param>
        /// <param name="context">The application database context.</param>
        /// <param name="token">The cancellation token for the task.</param>
        protected void Create<T>(IEnumerable<T> items, ApplicationDbContext context, CancellationToken token) where T : class
        {
            // Check if the items don't exist.
            if (items == null)
            {
                // Throw an exception.
                throw new ArgumentNullException(nameof(items));
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)items.Count() / _batchSize);
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
                var batchItems = items.Skip(index * _batchSize).Take(_batchSize);
                // Mark the items for addition.
                context.Set<T>().AddRange(batchItems);
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
        protected void Update<T>(IEnumerable<T> items, ApplicationDbContext context, CancellationToken token) where T : class
        {
            // Check if the items don't exist.
            if (items == null)
            {
                // Throw an exception.
                throw new ArgumentNullException(nameof(items));
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)items.Count() / _batchSize);
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
                var batchItems = items.Skip(index * _batchSize).Take(_batchSize);
                // Mark the items for update.
                context.Set<T>().UpdateRange(batchItems);
                // Save the changes to the database.
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Deletes the provided items from the database.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="items">The items to be updated.</param>
        /// <param name="context">The application database context.</param>
        /// <param name="token">The cancellation token for the task.</param>
        protected void Delete<T>(IQueryable<T> items, ApplicationDbContext context, CancellationToken token) where T : class
        {
            // Check if the items don't exist.
            if (items == null)
            {
                // Throw an exception.
                throw new ArgumentNullException(nameof(items));
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)items.Count() / _batchSize);
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
                var batchItems = items.Take(_batchSize);
                // Mark the items for deletion.
                context.Set<T>().RemoveRange(batchItems);
                // Save the changes to the database.
                context.SaveChanges();
            }
        }
    }
}
