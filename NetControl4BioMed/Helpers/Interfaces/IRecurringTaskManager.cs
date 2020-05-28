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
        /// Alerts users before deleting networks and analyses.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        void AlertUsers(string id, CancellationToken token);

        /// <summary>
        /// Deletes networks from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        void DeleteNetworks(string id, CancellationToken token);

        /// <summary>
        /// Deletes analyses from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        void DeleteAnalyses(string id, CancellationToken token);

        /// <summary>
        /// Stops analyses in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        void StopAnalyses(string id, CancellationToken token);
    }
}
