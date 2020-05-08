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
    /// Implements a background job to update edges in the database.
    /// </summary>
    public class EditEdgesBackgroundJob : BaseBackgroundJob
    {
        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<DataUpdateEdgeViewModel> Items { get; set; }

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
                // Get the edges from the database that have the given IDs.
                var edges = context.Edges
                    .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => itemIds.Contains(item.Id))
                    .Include(item => item.EdgeNodes)
                        .ThenInclude(item => item.Node)
                    .Include(item => item.DatabaseEdges)
                        .ThenInclude(item => item.Database)
                    .Include(item => item.DatabaseEdgeFieldEdges)
                    .AsEnumerable();
                // Check if there weren't any edges found.
                if (edges == null || !edges.Any())
                {
                    // Throw an exception.
                    throw new ArgumentException("No edges could be found in the database with the provided IDs.");
                }
                // Get the IDs of all of the nodes that are to be added to the edges.
                var itemNodeIds = batchItems
                    .Select(item => item.Nodes)
                    .SelectMany(item => item)
                    .Where(item => !string.IsNullOrEmpty(item.Id) && (item.Type == "Source" || item.Type == "Target"))
                    .Select(item => item.Id)
                    .Distinct();
                // Get the nodes that are to be added to the edges.
                var nodes = context.Nodes
                    .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => itemNodeIds.Contains(item.Id))
                    .AsEnumerable();
                // Check if there weren't any nodes found.
                if (nodes == null || !nodes.Any())
                {
                    // Throw an exception.
                    throw new ArgumentException("No nodes could be found in the database with the provided IDs.");
                }
                // Get the valid database node field IDs.
                var validItemNodeIds = nodes
                    .Select(item => item.Id);
                // Get the IDs of all of the edge fields that are to be updated.
                var itemEdgeFieldIds = batchItems
                    .Select(item => item.Fields)
                    .SelectMany(item => item)
                    .Where(item => !string.IsNullOrEmpty(item.Key) && !string.IsNullOrEmpty(item.Value))
                    .Select(item => item.Key)
                    .Distinct();
                // Get the edge fields that are to be updated.
                var edgeFields = context.DatabaseEdgeFields
                    .Where(item => item.Database.DatabaseType.Name != "Generic")
                    .Where(item => itemEdgeFieldIds.Contains(item.Id))
                    .Include(item => item.Database)
                    .AsEnumerable();
                // Get the IDs of all of the databases that are to be updated.
                var itemDatabaseIds = batchItems
                    .Select(item => item.DatabaseIds)
                    .SelectMany(item => item)
                    .Where(item => !string.IsNullOrEmpty(item))
                    .Concat(edgeFields.Select(item => item.Database.Id))
                    .Distinct();
                // Get all of the databases that are to be updated.
                var databases = context.Databases
                    .Where(item => item.DatabaseType.Name != "Generic")
                    .Where(item => itemDatabaseIds.Contains(item.Id))
                    .AsEnumerable();
                // Check if there weren't any databases or edge fields found.
                if (databases == null || !databases.Any())
                {
                    // Throw an exception.
                    throw new ArgumentException("No databases could be found in the database with the provided IDs.");
                }
                // Get the valid database IDs.
                var validItemDatabaseIds = databases
                    .Select(item => item.Id);
                // Get the valid database node field IDs.
                var validItemEdgeFieldIds = edgeFields
                    .Select(item => item.Id);
                // Save the edges to update.
                var edgesToUpdate = new List<Edge>();
                // Go over each of the valid items.
                foreach (var item in batchItems)
                {
                    // Get the corresponding edge.
                    var edge = edges.FirstOrDefault(item1 => item.Id == item1.Id);
                    // Check if there was no edge found.
                    if (edge == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Get the valid item nodes and the edge nodes to add.
                    var edgeNodes = item.Nodes
                        .Where(item1 => item1.Type == "Source" || item1.Type == "Target")
                        .Select(item1 => (item1.Id, item1.Type))
                        .Distinct()
                        .Where(item1 => validItemNodeIds.Contains(item1.Id))
                        .Select(item1 => new EdgeNode { NodeId = item1.Id, Node = nodes.FirstOrDefault(item2 => item1.Id == item2.Id), Type = item1.Type == "Source" ? EdgeNodeType.Source : EdgeNodeType.Target })
                        .Where(item1 => item1.Node != null);
                    // Check if there weren't any nodes found, or if there isn't at least one source node and one target node.
                    if (edgeNodes == null || !edgeNodes.Any() || edgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Source) == null || edgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Target) == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Get the valid item fields and the edge field edges to add.
                    var edgeFieldEdges = item.Fields
                        .Select(item1 => (item1.Key, item1.Value))
                        .Distinct()
                        .Where(item1 => validItemEdgeFieldIds.Contains(item1.Key))
                        .Select(item1 => new DatabaseEdgeFieldEdge { DatabaseEdgeFieldId = item1.Key, DatabaseEdgeField = edgeFields.FirstOrDefault(item2 => item1.Key == item2.Id), EdgeId = edge.Id, Edge = edge, Value = item1.Value })
                        .Where(item1 => item1.DatabaseEdgeField != null);
                    // Get the valid item databases and the database edges to add.
                    var databaseEdges = item.DatabaseIds
                        .Where(item1 => validItemDatabaseIds.Contains(item1))
                        .Concat(edgeFieldEdges.Select(item1 => item1.DatabaseEdgeField.Database.Id))
                        .Distinct()
                        .Select(item1 => new DatabaseEdge { DatabaseId = item1, Database = databases.FirstOrDefault(item2 => item1 == item2.Id), EdgeId = edge.Id, Edge = edge });
                    // Check if there weren't any databases or edge fields found.
                    if (databaseEdges == null || !databaseEdges.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Update the edge.
                    edge.Name = string.Concat(edgeNodes.First(item1 => item1.Type == EdgeNodeType.Source).Node.Name, " - ", edgeNodes.First(item1 => item1.Type == EdgeNodeType.Target).Node.Name);
                    edge.Description = item.Description;
                    edge.EdgeNodes = new List<EdgeNode> { edgeNodes.First(item1 => item1.Type == EdgeNodeType.Source), edgeNodes.First(item1 => item1.Type == EdgeNodeType.Target) };
                    edge.DatabaseEdgeFieldEdges = edgeFieldEdges.ToList();
                    edge.DatabaseEdges = databaseEdges.ToList().ToList();
                    // Add the edge to the list.
                    edgesToUpdate.Add(edge);
                }
                // Get the networks and analyses that contain the edges.
                var networks = context.Networks
                    .Where(item => item.NetworkEdges.Any(item1 => edgesToUpdate.Contains(item1.Edge)));
                var analyses = context.Analyses
                    .Where(item => item.AnalysisEdges.Any(item1 => edgesToUpdate.Contains(item1.Edge)));
                // Try to update the items.
                try
                {
                    // Delete the items.
                    Delete(analyses, context, token);
                    Delete(networks, context, token);
                    // Update the items.
                    Update(edgesToUpdate, context, token);
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
