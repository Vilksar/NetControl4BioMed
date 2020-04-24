using Hangfire;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Interfaces
{
    /// <summary>
    /// Provides an abstraction for the database data manager.
    /// </summary>
    [DisableConcurrentExecution(86400)]
    [AutomaticRetry(Attempts = 0)]
    public interface IDatabaseDataManager
    {
        /// <summary>
        /// Creates the provided nodes in the database.
        /// </summary>
        /// <param name="items">The nodes to be created.</param>
        /// <returns></returns>
        Task CreateNodesAsync(IEnumerable<DataUpdateNodeViewModel> items);

        /// <summary>
        /// Creates the provided edges in the database.
        /// </summary>
        /// <param name="items">The edges to be created.</param>
        /// <returns></returns>
        Task CreateEdgesAsync(IEnumerable<DataUpdateEdgeViewModel> items);

        /// <summary>
        /// Creates the provided node collections in the database.
        /// </summary>
        /// <param name="items">The node collections to be created.</param>
        /// <returns></returns>
        Task CreateNodeCollectionsAsync(IEnumerable<DataUpdateNodeCollectionViewModel> items);

        /// <summary>
        /// Updates the provided nodes in the database.
        /// </summary>
        /// <param name="items">The nodes to be updated.</param>
        /// <returns></returns>
        Task UpdateNodesAsync(IEnumerable<DataUpdateNodeViewModel> items);

        /// <summary>
        /// Updates the provided edges in the database.
        /// </summary>
        /// <param name="items">The edges to be updated.</param>
        /// <returns></returns>
        Task UpdateEdgesAsync(IEnumerable<DataUpdateEdgeViewModel> items);

        /// <summary>
        /// Updates the provided node collections in the database.
        /// </summary>
        /// <param name="items">The node collections to be updated.</param>
        /// <returns></returns>
        Task UpdateNodeCollectionsAsync(IEnumerable<DataUpdateNodeCollectionViewModel> items);

        /// <summary>
        /// Deletes the nodes with the provided IDs from the database.
        /// </summary>
        /// <param name="ids">The IDs of the items to be deleted.</param>
        /// <returns></returns>
        Task DeleteNodesAsync(IEnumerable<string> ids);

        /// <summary>
        /// Deletes the edges with the provided IDs from the database.
        /// </summary>
        /// <param name="ids">The IDs of the items to be deleted.</param>
        /// <returns></returns>
        Task DeleteEdgesAsync(IEnumerable<string> ids);

        /// <summary>
        /// Deletes the node collections with the provided IDs from the database.
        /// </summary>
        /// <param name="ids">The IDs of the items to be deleted.</param>
        /// <returns></returns>
        Task DeleteNodeCollectionsAsync(IEnumerable<string> ids);

        /// <summary>
        /// Deletes the networks with the provided IDs from the database.
        /// </summary>
        /// <param name="ids">The IDs of the items to be deleted.</param>
        /// <returns></returns>
        Task DeleteNetworksAsync(IEnumerable<string> ids);

        /// <summary>
        /// Deletes the analyses with the provided IDs from the database.
        /// </summary>
        /// <param name="ids">The IDs of the items to be deleted.</param>
        /// <returns></returns>
        Task DeleteAnalysesAsync(IEnumerable<string> ids);
    }
}
