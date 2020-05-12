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
    public interface IAdministrationTaskManager
    {
        /// <summary>
        /// Creates nodes in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        void CreateNodes(string id, CancellationToken token);

        /// <summary>
        /// Creates edges in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        void CreateEdges(string id, CancellationToken token);

        /// <summary>
        /// Creates node collections in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        void CreateNodeCollections(string id, CancellationToken token);

        /// <summary>
        /// Edits nodes in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        void EditNodes(string id, CancellationToken token);

        /// <summary>
        /// Edits edges in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        void EditEdges(string id, CancellationToken token);

        /// <summary>
        /// Edits node collections in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        void EditNodeCollections(string id, CancellationToken token);

        /// <summary>
        /// Deletes users from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        void DeleteUsers(string id, CancellationToken token);

        /// <summary>
        /// Deletes roles from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        void DeleteRoles(string id, CancellationToken token);

        /// <summary>
        /// Deletes user roles from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        void DeleteUserRoles(string id, CancellationToken token);

        /// <summary>
        /// Deletes database types from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        void DeleteDatabaseTypes(string id, CancellationToken token);

        /// <summary>
        /// Deletes databases from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        void DeleteDatabases(string id, CancellationToken token);

        /// <summary>
        /// Deletes database users from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        void DeleteDatabaseUsers(string id, CancellationToken token);

        /// <summary>
        /// Deletes database user invitations from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        void DeleteDatabaseUserInvitations(string id, CancellationToken token);

        /// <summary>
        /// Deletes database node fields from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        void DeleteDatabaseNodeFields(string id, CancellationToken token);

        /// <summary>
        /// Deletes database edge fields from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        void DeleteDatabaseEdgeFields(string id, CancellationToken token);

        /// <summary>
        /// Deletes nodes from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        void DeleteNodes(string id, CancellationToken token);

        /// <summary>
        /// Deletes edges from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        void DeleteEdges(string id, CancellationToken token);

        /// <summary>
        /// Deletes node collections from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        void DeleteNodeCollections(string id, CancellationToken token);

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

        /// <summary>
        /// Cleans the database of long-standing items.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(86400)]
        void Clean(string id, CancellationToken token);
    }
}
