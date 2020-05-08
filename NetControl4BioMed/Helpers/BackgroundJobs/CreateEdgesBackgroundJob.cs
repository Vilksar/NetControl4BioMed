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
    /// Implements a background job to create edges in the database.
    /// </summary>
    public class CreateEdgesBackgroundJob : BaseBackgroundJob
    {
        /// <summary>
        /// Gets or sets the items to be created.
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
                // Get the manually provided IDs of all the items that are to be created.
                var itemEdgeIds = batchItems
                .Where(item => !string.IsNullOrEmpty(item.Id))
                .Select(item => item.Id);
                // Check if any of the manually provided IDs are repeating in the list.
                if (itemEdgeIds.Distinct().Count() != itemEdgeIds.Count())
                {
                    // Throw an exception.
                    throw new ArgumentException("One or more of the manually provided IDs are duplicated.");
                }
                // Get the valid manually provided IDs, that do not appear in the database.
                var validItemEdgeIds = itemEdgeIds
                    .Except(context.Edges
                        .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                        .Where(item => itemEdgeIds.Contains(item.Id))
                        .Select(item => item.Id));
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
                // Get the valid database edge field IDs.
                var validItemEdgeFieldIds = edgeFields
                    .Select(item => item.Id);
                // Save the edges to add.
                var edges = new List<Edge>();
                // Go over each of the items.
                foreach (var item in batchItems)
                {
                    // Check if the ID of the current item is valid.
                    if (!string.IsNullOrEmpty(item.Id) && !validItemEdgeIds.Contains(item.Id))
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
                        .Select(item1 => new DatabaseEdgeFieldEdge { DatabaseEdgeFieldId = item1.Key, DatabaseEdgeField = edgeFields.FirstOrDefault(item2 => item1.Key == item2.Id), Value = item1.Value })
                        .Where(item1 => item1.DatabaseEdgeField != null);
                    // Get the valid item databases and the database edges to add.
                    var databaseEdges = item.DatabaseIds
                        .Where(item1 => validItemDatabaseIds.Contains(item1))
                        .Concat(edgeFieldEdges.Select(item1 => item1.DatabaseEdgeField.Database.Id))
                        .Distinct()
                        .Select(item1 => new DatabaseEdge { DatabaseId = item1, Database = databases.FirstOrDefault(item2 => item1 == item2.Id) });
                    // Check if there weren't any databases or edge fields found.
                    if (databaseEdges == null || !databaseEdges.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Define the new edge.
                    var edge = new Edge
                    {
                        Name = string.Concat(edgeNodes.First(item1 => item1.Type == EdgeNodeType.Source).Node.Name, " - ", edgeNodes.First(item1 => item1.Type == EdgeNodeType.Target).Node.Name),
                        Description = item.Description,
                        DateTimeCreated = DateTime.Now,
                        EdgeNodes = new List<EdgeNode> { edgeNodes.First(item1 => item1.Type == EdgeNodeType.Source), edgeNodes.First(item1 => item1.Type == EdgeNodeType.Target) },
                        DatabaseEdgeFieldEdges = edgeFieldEdges.ToList(),
                        DatabaseEdges = databaseEdges.ToList()
                    };
                    // Check if there is any ID provided.
                    if (!string.IsNullOrEmpty(item.Id))
                    {
                        // Assign it to the node.
                        edge.Id = item.Id;
                    }
                    // Add the new node to the list.
                    edges.Add(edge);
                }
                // Try to create the items.
                try
                {
                    // Create the items.
                    Create(edges, context, token);
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
