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
    /// Provides an abstraction for the content task manager.
    /// </summary>
    [AutomaticRetry(Attempts = 0)]
    public interface IContentTaskManager
    {
        /// <summary>
        /// Deletes networks from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        void DeleteNetworks(string id, CancellationToken token);

        /// <summary>
        /// Deletes analyses from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        void DeleteAnalyses(string id, CancellationToken token);

        /// <summary>
        /// Generates networks.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        void GenerateNetworks(string id, CancellationToken token);

        /// <summary>
        /// Generates analyses.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        void GenerateAnalyses(string id, CancellationToken token);

        /// <summary>
        /// Starts analyses.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        void StartAnalyses(string id, CancellationToken token);

        /// <summary>
        /// Stops analyses.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        void StopAnalyses(string id, CancellationToken token);
    }
}
