using Hangfire;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Interfaces
{
    /// <summary>
    /// Provides an abstraction for the administration task manager.
    /// </summary>
    [AutomaticRetry(Attempts = 0)]
    [DisableConcurrentExecution(86400)]
    public interface IRecurringTaskManager
    {
        /// <summary>
        /// Counts the items in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        Task CountAllItemsAsync(string id, CancellationToken token);

        /// <summary>
        /// Counts the duplicate items in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        Task CountDuplicateItemsAsync(string id, CancellationToken token);

        /// <summary>
        /// Counts the orphaned items in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        Task CountOrphanedItemsAsync(string id, CancellationToken token);

        /// <summary>
        /// Counts the inconsistent items in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        Task CountInconsistentItemsAsync(string id, CancellationToken token);

        /// <summary>
        /// Stops the long-running analyses in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        Task StopAnalysesAsync(string id, CancellationToken token);

        /// <summary>
        /// Alerts the users before deleting the long-standing networks and analyses from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        Task AlertUsersAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes the long-standing unconfirmed users from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        Task DeleteUnconfirmedUsersAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes the long-standing guest users from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        Task DeleteGuestUsersAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes the long-standing networks from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        Task DeleteNetworksAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes the long-standing analyses from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        Task DeleteAnalysesAsync(string id, CancellationToken token);
    }
}
