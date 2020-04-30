using Hangfire;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Interfaces
{
    /// <summary>
    /// Provides an abstraction for the database data manager.
    /// </summary>
    [AutomaticRetry(Attempts = 0)]
    [DisableConcurrentExecution(86400)]
    public interface IDatabaseDataManager
    {
        /// <summary>
        /// Creates the provided nodes in the database.
        /// </summary>
        /// <param name="items">The nodes to be created.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [DisableConcurrentExecution(86400)]
        void CreateNodes(IEnumerable<DataUpdateNodeViewModel> items, CancellationToken token);

        /// <summary>
        /// Creates the provided nodes in the database.
        /// </summary>
        /// <param name="items">The nodes to be created.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [DisableConcurrentExecution(86400)]
        Task CreateNodesAsync(IEnumerable<DataUpdateNodeViewModel> items, CancellationToken token);

        /// <summary>
        /// Creates the provided edges in the database.
        /// </summary>
        /// <param name="items">The edges to be created.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [DisableConcurrentExecution(86400)]
        void CreateEdges(IEnumerable<DataUpdateEdgeViewModel> items, CancellationToken token);

        /// <summary>
        /// Creates the provided edges in the database.
        /// </summary>
        /// <param name="items">The edges to be created.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [DisableConcurrentExecution(86400)]
        Task CreateEdgesAsync(IEnumerable<DataUpdateEdgeViewModel> items, CancellationToken token);

        /// <summary>
        /// Creates the provided node collections in the database.
        /// </summary>
        /// <param name="items">The node collections to be created.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [DisableConcurrentExecution(86400)]
        void CreateNodeCollections(IEnumerable<DataUpdateNodeCollectionViewModel> items, CancellationToken token);

        /// <summary>
        /// Creates the provided node collections in the database.
        /// </summary>
        /// <param name="items">The node collections to be created.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [DisableConcurrentExecution(86400)]
        Task CreateNodeCollectionsAsync(IEnumerable<DataUpdateNodeCollectionViewModel> items, CancellationToken token);

        /// <summary>
        /// Updates the provided nodes in the database.
        /// </summary>
        /// <param name="items">The nodes to be updated.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [DisableConcurrentExecution(86400)]
        void UpdateNodes(IEnumerable<DataUpdateNodeViewModel> items, CancellationToken token);

        /// <summary>
        /// Updates the provided nodes in the database.
        /// </summary>
        /// <param name="items">The nodes to be updated.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [DisableConcurrentExecution(86400)]
        Task UpdateNodesAsync(IEnumerable<DataUpdateNodeViewModel> items, CancellationToken token);

        /// <summary>
        /// Updates the provided edges in the database.
        /// </summary>
        /// <param name="items">The edges to be updated.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [DisableConcurrentExecution(86400)]
        void UpdateEdges(IEnumerable<DataUpdateEdgeViewModel> items, CancellationToken token);

        /// <summary>
        /// Updates the provided edges in the database.
        /// </summary>
        /// <param name="items">The edges to be updated.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [DisableConcurrentExecution(86400)]
        Task UpdateEdgesAsync(IEnumerable<DataUpdateEdgeViewModel> items, CancellationToken token);

        /// <summary>
        /// Updates the provided node collections in the database.
        /// </summary>
        /// <param name="items">The node collections to be updated.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [DisableConcurrentExecution(86400)]
        void UpdateNodeCollections(IEnumerable<DataUpdateNodeCollectionViewModel> items, CancellationToken token);

        /// <summary>
        /// Updates the provided node collections in the database.
        /// </summary>
        /// <param name="items">The node collections to be updated.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [DisableConcurrentExecution(86400)]
        Task UpdateNodeCollectionsAsync(IEnumerable<DataUpdateNodeCollectionViewModel> items, CancellationToken token);

        /// <summary>
        /// Deletes the nodes with the provided IDs from the database.
        /// </summary>
        /// <param name="ids">The IDs of the items to be deleted.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [DisableConcurrentExecution(86400)]
        void DeleteNodes(IEnumerable<string> ids, CancellationToken token);

        /// <summary>
        /// Deletes the nodes with the provided IDs from the database.
        /// </summary>
        /// <param name="ids">The IDs of the items to be deleted.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [DisableConcurrentExecution(86400)]
        Task DeleteNodesAsync(IEnumerable<string> ids, CancellationToken token);

        /// <summary>
        /// Deletes the edges with the provided IDs from the database.
        /// </summary>
        /// <param name="ids">The IDs of the items to be deleted.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [DisableConcurrentExecution(86400)]
        void DeleteEdges(IEnumerable<string> ids, CancellationToken token);

        /// <summary>
        /// Deletes the edges with the provided IDs from the database.
        /// </summary>
        /// <param name="ids">The IDs of the items to be deleted.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [DisableConcurrentExecution(86400)]
        Task DeleteEdgesAsync(IEnumerable<string> ids, CancellationToken token);

        /// <summary>
        /// Deletes the node collections with the provided IDs from the database.
        /// </summary>
        /// <param name="ids">The IDs of the items to be deleted.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [DisableConcurrentExecution(86400)]
        void DeleteNodeCollections(IEnumerable<string> ids, CancellationToken token);

        /// <summary>
        /// Deletes the node collections with the provided IDs from the database.
        /// </summary>
        /// <param name="ids">The IDs of the items to be deleted.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [DisableConcurrentExecution(86400)]
        Task DeleteNodeCollectionsAsync(IEnumerable<string> ids, CancellationToken token);

        /// <summary>
        /// Deletes the networks with the provided IDs from the database.
        /// </summary>
        /// <param name="ids">The IDs of the items to be deleted.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [DisableConcurrentExecution(86400)]
        void DeleteNetworks(IEnumerable<string> ids, CancellationToken token);

        /// <summary>
        /// Deletes the networks with the provided IDs from the database.
        /// </summary>
        /// <param name="ids">The IDs of the items to be deleted.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [DisableConcurrentExecution(86400)]
        Task DeleteNetworksAsync(IEnumerable<string> ids, CancellationToken token);

        /// <summary>
        /// Deletes the analyses with the provided IDs from the database.
        /// </summary>
        /// <param name="ids">The IDs of the items to be deleted.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [DisableConcurrentExecution(86400)]
        void DeleteAnalyses(IEnumerable<string> ids, CancellationToken token);

        /// <summary>
        /// Deletes the analyses with the provided IDs from the database.
        /// </summary>
        /// <param name="ids">The IDs of the items to be deleted.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        [DisableConcurrentExecution(86400)]
        Task DeleteAnalysesAsync(IEnumerable<string> ids, CancellationToken token);
    }
}
