using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.BackgroundJobs
{
    /// <summary>
    /// Implements a background job to update nodes in the database.
    /// </summary>
    public class EditNodesBackgroundJob : BaseBackgroundJob
    {
        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<DataUpdateNodeViewModel> Items { get; set; }

        /// <summary>
        /// Runs the current job.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public override void Run(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if there weren't any valid items found.
            if (Items == null || !Items.Any())
            {
                // Throw an exception.
                throw new ArgumentException("No valid items could be found with the provided data.");
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)Items.Count() / _batchSize);
            // Go over each batch.
            for (var index = 0; index < count; index++)
            {
                // Check if the cancellation was requested.
                if (token.IsCancellationRequested)
                {
                    // Break.
                    break;
                }
                // Get the items in the current batch.
                var batchItems = Items.Skip(index * _batchSize).Take(_batchSize);
                // Create a new scope.
                using var scope = serviceProvider.CreateScope();
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the list of IDs from the provided items.
                var itemIds = batchItems.Select(item => item.Id);
                // Get the nodes from the database that have the given IDs.
                var nodes = context.Nodes
                    .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => itemIds.Contains(item.Id))
                    .Include(item => item.DatabaseNodeFieldNodes)
                    .Include(item => item.DatabaseNodes)
                    .AsEnumerable();
                // Check if there weren't any nodes found.
                if (nodes == null || !nodes.Any())
                {
                    // Throw an exception.
                    throw new ArgumentException("No nodes could be found in the database with the provided IDs.");
                }
                // Get the IDs of all of the node fields that are to be updated.
                var itemNodeFieldIds = batchItems
                    .Select(item => item.Fields)
                    .SelectMany(item => item)
                    .Where(item => !string.IsNullOrEmpty(item.Key) && !string.IsNullOrEmpty(item.Value))
                    .Select(item => item.Key)
                    .Distinct();
                // Get the node fields that are to be updated.
                var nodeFields = context.DatabaseNodeFields
                    .Where(item => item.Database.DatabaseType.Name != "Generic")
                    .Where(item => itemNodeFieldIds.Contains(item.Id))
                    .Include(item => item.Database)
                    .AsEnumerable();
                // Check if there weren't any node fields found.
                if (nodeFields == null || !nodeFields.Any())
                {
                    // Throw an exception.
                    throw new ArgumentException("No database node fields could be found in the database with the provided IDs.");
                }
                // Get the valid database node field IDs.
                var validItemNodeFieldIds = nodeFields
                    .Select(item => item.Id);
                // Save the nodes to update.
                var nodesToUpdate = new List<Node>();
                // Go over each of the valid items.
                foreach (var item in batchItems)
                {
                    // Get the corresponding node.
                    var node = nodes.FirstOrDefault(item1 => item.Id == item1.Id);
                    // Check if there was no node found.
                    if (node == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Get the valid item fields and the node field nodes to add.
                    var nodeFieldNodes = item.Fields
                        .Select(item1 => (item1.Key, item1.Value))
                        .Distinct()
                        .Where(item1 => validItemNodeFieldIds.Contains(item1.Key))
                        .Select(item1 => new DatabaseNodeFieldNode { DatabaseNodeFieldId = item1.Key, DatabaseNodeField = nodeFields.FirstOrDefault(item2 => item1.Key == item2.Id), NodeId = node.Id, Node = node, Value = item1.Value })
                        .Where(item1 => item1.DatabaseNodeField != null && item1.Node != null);
                    // Check if there weren't any node fields found.
                    if (nodeFieldNodes == null || !nodeFieldNodes.Any() || !nodeFieldNodes.Any(item1 => item1.DatabaseNodeField.IsSearchable))
                    {
                        // Continue.
                        continue;
                    }
                    // Update the node.
                    node.Name = nodeFieldNodes.First(item1 => item1.DatabaseNodeField.IsSearchable).Value;
                    node.Description = item.Description;
                    node.DatabaseNodeFieldNodes = nodeFieldNodes.ToList();
                    node.DatabaseNodes = nodeFieldNodes
                        .Select(item1 => item1.DatabaseNodeField.Database)
                        .Distinct()
                        .Select(item1 => new DatabaseNode { DatabaseId = item1.Id, Database = item1, NodeId = node.Id, Node = node })
                        .ToList();
                    // Add the node to the list.
                    nodesToUpdate.Add(node);
                }
                // Get the networks and analyses that contain the nodes.
                var networks = context.Networks
                    .Where(item => item.NetworkNodes.Any(item1 => nodesToUpdate.Contains(item1.Node)));
                var analyses = context.Analyses
                    .Where(item => item.AnalysisNodes.Any(item1 => nodesToUpdate.Contains(item1.Node)));
                // Try to update the items.
                try
                {
                    // Delete the items.
                    Delete(analyses, context, token);
                    Delete(networks, context, token);
                    // Update the items.
                    Update(nodesToUpdate, context, token);
                }
                catch (Exception exception)
                {
                    // Throw an exception.
                    throw exception;
                }
                // Get the edges that contain the nodes.
                var edges = context.Edges
                    .Where(item => item.EdgeNodes.Any(item1 => nodesToUpdate.Contains(item1.Node)))
                    .Include(item => item.EdgeNodes)
                        .ThenInclude(item => item.Node)
                    .AsEnumerable();
                // Go over each edge.
                foreach (var edge in edges)
                {
                    // Update its name.
                    edge.Name = string.Concat(edge.EdgeNodes.First(item => item.Type == EdgeNodeType.Source).Node.Name, " -> ", edge.EdgeNodes.First(item => item.Type == EdgeNodeType.Target).Node.Name);
                }
                // Try to update the items.
                try
                {
                    // Update the items.
                    Update(edges, context, token);
                }
                catch (Exception exception)
                {
                    // Throw an exception.
                    throw exception;
                }
            }
        }
    }
}
