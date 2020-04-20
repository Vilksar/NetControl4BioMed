using NetControl4BioMed.Data;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Services
{
    /// <summary>
    /// Implements a deleter for the database data.
    /// </summary>
    public class DataDeleter : IDataDeleter
    {
        /// <summary>
        /// Represents the application database context.
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public DataDeleter(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Deletes the data specified by the model.
        /// </summary>
        /// <param name="viewModel">The model of the data to remove.</param>
        /// <returns></returns>
        public async Task Run(DataDeleterViewModel viewModel)
        {
            // Check the items to delete.
            if (viewModel.DeleteNodes)
            {
                // Delete the items.
                await DeleteNodesAsync();
            }
            // Check the items to delete.
            if (viewModel.DeleteEdges)
            {
                // Delete the items.
                await DeleteEdgesAsync();
            }
            // Check the items to delete.
            if (viewModel.DeleteNodeCollections)
            {
                // Delete the items.
                await DeleteNodeCollectionsAsync();
            }
            // Check the items to delete.
            if (viewModel.DeleteNetworks)
            {
                // Delete the items.
                await DeleteNetworksAsync();
            }
            // Check the items to delete.
            if (viewModel.DeleteAnalyses)
            {
                // Delete the items.
                await DeleteAnalysesAsync();
            }
        }

        /// <summary>
        /// Deletes all nodes in the database.
        /// </summary>
        /// <returns></returns>
        private async Task DeleteNodesAsync()
        {
            // Get the items to delete.
            var nodes = _context.Nodes
                .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"));
            // Get the related entities that use the items.
            var edges = _context.Edges
                .Where(item => item.EdgeNodes.Any(item1 => nodes.Contains(item1.Node)));
            var networks = _context.Networks
                .Where(item => item.NetworkNodes.Any(item1 => nodes.Contains(item1.Node)));
            var analyses = _context.Analyses
                .Where(item => item.AnalysisNodes.Any(item1 => nodes.Contains(item1.Node)));
            // Mark the items for deletion.
            _context.Analyses.RemoveRange(analyses);
            _context.Networks.RemoveRange(networks);
            _context.Edges.RemoveRange(edges);
            _context.Nodes.RemoveRange(nodes);
            // Save the changes to the database.
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes all edges in the database.
        /// </summary>
        /// <returns></returns>
        private async Task DeleteEdgesAsync()
        {
            // Get the items to delete.
            var edges = _context.Edges
                .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"));
            // Get the related entities that use the items.
            var networks = _context.Networks
                .Where(item => item.NetworkEdges.Any(item1 => edges.Contains(item1.Edge)));
            var analyses = _context.Analyses
                .Where(item => item.AnalysisEdges.Any(item1 => edges.Contains(item1.Edge)));
            // Mark the items for deletion.
            _context.Analyses.RemoveRange(analyses);
            _context.Networks.RemoveRange(networks);
            _context.Edges.RemoveRange(edges);
            // Save the changes to the database.
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes all node collections in the database.
        /// </summary>
        /// <returns></returns>
        private async Task DeleteNodeCollectionsAsync()
        {
            // Get the items to delete.
            var nodeCollections = _context.NodeCollections
                .AsQueryable();
            // Get the related entities that use the items.
            var networks = _context.Networks.Where(item => item.NetworkNodeCollections.Any(item1 => nodeCollections.Contains(item1.NodeCollection)));
            var analyses = _context.Analyses.Where(item => item.AnalysisNodeCollections.Any(item1 => nodeCollections.Contains(item1.NodeCollection)) || item.AnalysisNetworks.Any(item1 => networks.Contains(item1.Network)));
            // Mark the items for deletion.
            _context.Analyses.RemoveRange(analyses);
            _context.Networks.RemoveRange(networks);
            _context.NodeCollections.RemoveRange(nodeCollections);
            // Save the changes to the database.
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes all networks in the database.
        /// </summary>
        /// <returns></returns>
        private async Task DeleteNetworksAsync()
        {
            // Get the items to delete.
            var networks = _context.Networks
                .AsQueryable();
            // Get the related entities that use the items.
            var analyses = _context.Analyses
                .Where(item => item.AnalysisNetworks.Any(item1 => networks.Contains(item1.Network)));
            // Get the generic entities among them.
            var genericNetworks = networks.Where(item => item.NetworkDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"));
            var genericNodes = _context.Nodes.Where(item => item.NetworkNodes.Any(item1 => genericNetworks.Contains(item1.Network)));
            var genericEdges = _context.Edges.Where(item => item.NetworkEdges.Any(item1 => genericNetworks.Contains(item1.Network)) || item.EdgeNodes.Any(item1 => genericNodes.Contains(item1.Node)));
            // Mark the items for deletion.
            _context.Analyses.RemoveRange(analyses);
            _context.Networks.RemoveRange(networks);
            _context.Edges.RemoveRange(genericEdges);
            _context.Nodes.RemoveRange(genericNodes);
            // Save the changes to the database.
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes all analyses in the database.
        /// </summary>
        /// <returns></returns>
        private async Task DeleteAnalysesAsync()
        {
            // Get the items to delete.
            var analyses = _context.Analyses
                .AsQueryable();
            // Mark the items for deletion.
            _context.Analyses.RemoveRange(analyses);
            // Save the changes to the database.
            await _context.SaveChangesAsync();
        }
    }
}
