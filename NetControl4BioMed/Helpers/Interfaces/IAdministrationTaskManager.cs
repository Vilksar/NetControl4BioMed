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
        [Queue("default")]
        Task CreateNodesAsync(string id, CancellationToken token);

        /// <summary>
        /// Creates edges in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task CreateEdgesAsync(string id, CancellationToken token);

        /// <summary>
        /// Creates node collections in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task CreateNodeCollectionsAsync(string id, CancellationToken token);

        /// <summary>
        /// Creates samples in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task CreateSamplesAsync(string id, CancellationToken token);

        /// <summary>
        /// Edits nodes in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task EditNodesAsync(string id, CancellationToken token);

        /// <summary>
        /// Edits edges in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task EditEdgesAsync(string id, CancellationToken token);

        /// <summary>
        /// Edits node collections in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task EditNodeCollectionsAsync(string id, CancellationToken token);

        /// <summary>
        /// Edits samples in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task EditSamplesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes users from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task DeleteUsersAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes roles from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task DeleteRolesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes user roles from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task DeleteUserRolesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes database types from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task DeleteDatabaseTypesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes databases from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task DeleteDatabasesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes database users from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task DeleteDatabaseUsersAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes database user invitations from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task DeleteDatabaseUserInvitationsAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes database node fields from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task DeleteDatabaseNodeFieldsAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes database edge fields from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task DeleteDatabaseEdgeFieldsAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes nodes from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task DeleteNodesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes edges from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task DeleteEdgesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes node collections from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task DeleteNodeCollectionsAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes samples from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task DeleteSamplesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes networks from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task DeleteNetworksAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes analyses from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task DeleteAnalysesAsync(string id, CancellationToken token);

        /// <summary>
        /// Stops analyses in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task StopAnalysesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes all nodes from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task DeleteAllNodesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes all edges from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task DeleteAllEdgesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes all node collections from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task DeleteAllNodeCollectionsAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes all networks from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task DeleteAllNetworksAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes all analyses from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task DeleteAllAnalysesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes all samples from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        [Queue("default")]
        Task DeleteAllSamplesAsync(string id, CancellationToken token);
    }
}
