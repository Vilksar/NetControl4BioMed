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
        Task CreateNodesAsync(string id, CancellationToken token);

        /// <summary>
        /// Creates edges in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        Task CreateEdgesAsync(string id, CancellationToken token);

        /// <summary>
        /// Creates node collections in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        Task CreateNodeCollectionsAsync(string id, CancellationToken token);

        /// <summary>
        /// Edits nodes in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        Task EditNodesAsync(string id, CancellationToken token);

        /// <summary>
        /// Edits edges in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        Task EditEdgesAsync(string id, CancellationToken token);

        /// <summary>
        /// Edits node collections in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        Task EditNodeCollectionsAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes users from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        Task DeleteUsersAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes roles from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        Task DeleteRolesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes user roles from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        Task DeleteUserRolesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes database types from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        Task DeleteDatabaseTypesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes databases from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        Task DeleteDatabasesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes database users from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        Task DeleteDatabaseUsersAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes database user invitations from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        Task DeleteDatabaseUserInvitationsAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes database node fields from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        Task DeleteDatabaseNodeFieldsAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes database edge fields from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        Task DeleteDatabaseEdgeFieldsAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes nodes from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        Task DeleteNodesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes edges from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        Task DeleteEdgesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes node collections from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        Task DeleteNodeCollectionsAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes samples from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        Task DeleteSamplesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes networks from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        Task DeleteNetworksAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes analyses from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        Task DeleteAnalysesAsync(string id, CancellationToken token);

        /// <summary>
        /// Stops analyses in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        Task StopAnalysesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes all nodes from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        Task DeleteAllNodesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes all edges from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        Task DeleteAllEdgesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes all node collections from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        Task DeleteAllNodeCollectionsAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes all networks from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        Task DeleteAllNetworksAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes all analyses from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        Task DeleteAllAnalysesAsync(string id, CancellationToken token);

        /// <summary>
        /// Deletes all samples from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        [AutomaticRetry(Attempts = 2)]
        [DisableConcurrentExecution(86400)]
        Task DeleteAllSamplesAsync(string id, CancellationToken token);
    }
}
