using Hangfire;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Interfaces
{
    /// <summary>
    /// Provides an abstraction for the administration task manager.
    /// </summary>
    [DisableConcurrentExecution(86400)]
    public interface IAdministrationTaskManager
    {
        /// <summary>
        /// Creates proteins in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task CreateProteinsAsync(string id, CancellationToken token);

        /// <summary>
        /// Creates interactions in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task CreateInteractionsAsync(string id, CancellationToken token);

        /// <summary>
        /// Creates protein collections in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task CreateProteinCollectionsAsync(string id, CancellationToken token);

        /// <summary>
        /// Edits proteins in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task EditProteinsAsync(string id, CancellationToken token);

        /// <summary>
        /// Edits interactions in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task EditInteractionsAsync(string id, CancellationToken token);

        /// <summary>
        /// Edits protein collections in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task EditProteinCollectionsAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes users from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteUsersAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes roles from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteRolesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes user roles from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteUserRolesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes databases from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteDatabasesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes database users from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteDatabaseUsersAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes database protein fields from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteDatabaseProteinFieldsAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes database interaction fields from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteDatabaseInteractionFieldsAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes proteins from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteProteinsAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes interactions from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteInteractionsAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes protein collections from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteProteinCollectionsAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes networks from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteNetworksAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes analyses from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteAnalysesAsync(string id, CancellationToken token);

        /// <summary>
        /// Stops analyses in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task StopAnalysesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes all proteins from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteAllProteinsAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes all interactions from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteAllInteractionsAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes all protein collections from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteAllProteinCollectionsAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes all networks from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteAllNetworksAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes all analyses from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteAllAnalysesAsync(string id, CancellationToken token);
    }
}
