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
    [DisableConcurrentExecution(86400)]
    public interface IAdministrationTaskManager
    {
        /// <summary>
        /// Creates nodes in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task CreateNodesAsync(string id, CancellationToken token);

        /// <summary>
        /// Creates edges in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task CreateEdgesAsync(string id, CancellationToken token);

        /// <summary>
        /// Creates node collections in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task CreateNodeCollectionsAsync(string id, CancellationToken token);

        /// <summary>
        /// Creates samples in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task CreateSamplesAsync(string id, CancellationToken token);

        /// <summary>
        /// Edits nodes in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task EditNodesAsync(string id, CancellationToken token);

        /// <summary>
        /// Edits edges in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task EditEdgesAsync(string id, CancellationToken token);

        /// <summary>
        /// Edits node collections in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task EditNodeCollectionsAsync(string id, CancellationToken token);

        /// <summary>
        /// Edits samples in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task EditSamplesAsync(string id, CancellationToken token);

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
        /// Deletes database types from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteDatabaseTypesAsync(string id, CancellationToken token);

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
        /// Deletes database user invitations from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteDatabaseUserInvitationsAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes database node fields from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteDatabaseNodeFieldsAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes database edge fields from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteDatabaseEdgeFieldsAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes nodes from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteNodesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes edges from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteEdgesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes node collections from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteNodeCollectionsAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes samples from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteSamplesAsync(string id, CancellationToken token);

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
        /// Deletes all nodes from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteAllNodesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes all edges from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteAllEdgesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes all node collections from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteAllNodeCollectionsAsync(string id, CancellationToken token);

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

        /// <summary>
        /// Deletes all samples from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("administration")]
        Task DeleteAllSamplesAsync(string id, CancellationToken token);
    }
}
