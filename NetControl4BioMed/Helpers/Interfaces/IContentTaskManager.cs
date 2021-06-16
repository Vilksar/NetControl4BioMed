using Hangfire;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Interfaces
{
    /// <summary>
    /// Provides an abstraction for the content task manager.
    /// </summary>
    public interface IContentTaskManager
    {
        /// <summary>
        /// Deletes networks from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [Queue("default")]
        Task DeleteNetworksAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes analyses from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [Queue("default")]
        Task DeleteAnalysesAsync(string id, CancellationToken token);

        /// <summary>
        /// Generates networks.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [Queue("background")]
        Task GenerateNetworksAsync(string id, CancellationToken token);

        /// <summary>
        /// Generates analyses.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [Queue("background")]
        Task GenerateAnalysesAsync(string id, CancellationToken token);

        /// <summary>
        /// Starts analyses.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [Queue("background")]
        Task StartAnalysesAsync(string id, CancellationToken token);

        /// <summary>
        /// Stops analyses.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [Queue("default")]
        Task StopAnalysesAsync(string id, CancellationToken token);

        /// <summary>
        /// Sends ended e-mails to the corresponding network users.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [Queue("default")]
        Task SendNetworksEndedEmailsAsync(string id, CancellationToken token);

        /// <summary>
        /// Sends ended e-mails to the corresponding analysis users.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [Queue("default")]
        Task SendAnalysesEndedEmailsAsync(string id, CancellationToken token);
    }
}
