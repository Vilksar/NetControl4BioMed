using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.InputModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NetControl4BioMed.Helpers.Extensions;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Tasks
{
    /// <summary>
    /// Implements a task to update edges in the database.
    /// </summary>
    public class EdgesTask
    {
        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<EdgeInputModel> Items { get; set; }

        /// <summary>
        /// Creates the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void Create(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if there weren't any valid items found.
            if (Items == null)
            {
                // Throw an exception.
                throw new ArgumentException("No valid items could be found with the provided data.");
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)Items.Count() / ApplicationDbContext.BatchSize);
            // Go over each batch.
            for (var index = 0; index < count; index++)
            {
                // Check if the cancellation was requested.
                if (token.IsCancellationRequested)
                {
                    // Break.
                    break;
                }
                // Create a new scope.
                using var scope = serviceProvider.CreateScope();
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items in the current batch.
                var batchItems = Items.Skip(index * ApplicationDbContext.BatchSize).Take(ApplicationDbContext.BatchSize);
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
                    .Where(item => item.EdgeNodes != null)
                    .Select(item => item.EdgeNodes)
                    .SelectMany(item => item)
                    .Where(item => !string.IsNullOrEmpty(item.NodeId) && (item.Type == "Source" || item.Type == "Target"))
                    .Select(item => item.NodeId)
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
                    .Where(item => item.DatabaseEdgeFieldEdges != null)
                    .Select(item => item.DatabaseEdgeFieldEdges)
                    .SelectMany(item => item)
                    .Where(item => !string.IsNullOrEmpty(item.DatabaseEdgeFieldId) && !string.IsNullOrEmpty(item.Value))
                    .Select(item => item.DatabaseEdgeFieldId)
                    .Distinct();
                // Get the edge fields that are to be updated.
                var edgeFields = context.DatabaseEdgeFields
                    .Where(item => item.Database.DatabaseType.Name != "Generic")
                    .Where(item => itemEdgeFieldIds.Contains(item.Id))
                    .Include(item => item.Database)
                    .AsEnumerable();
                // Get the IDs of all of the databases that are to be updated.
                var itemDatabaseIds = batchItems
                    .Where(item => item.DatabaseEdges != null)
                    .Select(item => item.DatabaseEdges)
                    .SelectMany(item => item)
                    .Select(item => item.DatabaseId)
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
                foreach (var batchItem in batchItems)
                {
                    // Check if the ID of the current item is valid.
                    if (!string.IsNullOrEmpty(batchItem.Id) && !validItemEdgeIds.Contains(batchItem.Id))
                    {
                        // Continue.
                        continue;
                    }
                    // Check if there aren't any edge nodes.
                    if (batchItem.EdgeNodes == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Get the valid item nodes and the edge nodes to add.
                    var edgeNodes = batchItem.EdgeNodes != null ?
                        batchItem.EdgeNodes
                            .Where(item => item.Type == "Source" || item.Type == "Target")
                            .Select(item => (item.NodeId, item.Type))
                            .Distinct()
                            .Where(item => validItemNodeIds.Contains(item.NodeId))
                            .Select(item => new EdgeNode
                            {
                                NodeId = item.NodeId,
                                Node = nodes.FirstOrDefault(item1 => item.NodeId == item1.Id),
                                Type = EnumerationExtensions.GetEnumerationValue<EdgeNodeType>(item.Type)
                            })
                            .Where(item1 => item1.Node != null) :
                        Enumerable.Empty<EdgeNode>();
                    // Check if there weren't any nodes found, or if there isn't at least one source node and one target node.
                    if (edgeNodes == null || !edgeNodes.Any() || edgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Source) == null || edgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Target) == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Get the valid item fields and the edge field edges to add.
                    var edgeFieldEdges = batchItem.DatabaseEdgeFieldEdges != null ?
                        batchItem.DatabaseEdgeFieldEdges
                            .Select(item => (item.DatabaseEdgeFieldId, item.Value))
                            .Distinct()
                            .Where(item => validItemEdgeFieldIds.Contains(item.DatabaseEdgeFieldId))
                            .Select(item => new DatabaseEdgeFieldEdge
                            {
                                DatabaseEdgeFieldId = item.DatabaseEdgeFieldId,
                                DatabaseEdgeField = edgeFields.FirstOrDefault(item1 => item.DatabaseEdgeFieldId == item1.Id),
                                Value = item.Value
                            })
                            .Where(item => item.DatabaseEdgeField != null) :
                        Enumerable.Empty<DatabaseEdgeFieldEdge>();
                    // Get the valid item databases and the database edges to add.
                    var databaseEdges = batchItem.DatabaseEdges != null ?
                        batchItem.DatabaseEdges
                            .Select(item => item.DatabaseId)
                            .Where(item => validItemDatabaseIds.Contains(item))
                            .Concat(edgeFieldEdges.Select(item1 => item1.DatabaseEdgeField.Database.Id))
                            .Distinct()
                            .Select(item => new DatabaseEdge
                            {
                                DatabaseId = item,
                                Database = databases.FirstOrDefault(item1 => item == item1.Id)
                            })
                            .Where(item => item.Database != null) :
                        Enumerable.Empty<DatabaseEdge>();
                    // Check if there weren't any databases or edge fields found.
                    if (databaseEdges == null || !databaseEdges.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Define the new edge.
                    var edge = new Edge
                    {
                        Name = string.Concat(edgeNodes.First(item => item.Type == EdgeNodeType.Source).Node.Name, " - ", edgeNodes.First(item1 => item1.Type == EdgeNodeType.Target).Node.Name),
                        Description = batchItem.Description,
                        DateTimeCreated = DateTime.Now,
                        EdgeNodes = new List<EdgeNode>
                        {
                            edgeNodes.First(item => item.Type == EdgeNodeType.Source),
                            edgeNodes.First(item => item.Type == EdgeNodeType.Target)
                        },
                        DatabaseEdgeFieldEdges = edgeFieldEdges.ToList(),
                        DatabaseEdges = databaseEdges.ToList()
                    };
                    // Check if there is any ID provided.
                    if (!string.IsNullOrEmpty(batchItem.Id))
                    {
                        // Assign it to the node.
                        edge.Id = batchItem.Id;
                    }
                    // Add the new node to the list.
                    edges.Add(edge);
                }
                // Create the items.
                IEnumerableExtensions.Create(edges, context, token);
            }
        }

        /// <summary>
        /// Edits the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void Edit(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if there weren't any valid items found.
            if (Items == null)
            {
                // Throw an exception.
                throw new ArgumentException("No valid items could be found with the provided data.");
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)Items.Count() / ApplicationDbContext.BatchSize);
            // Go over each batch.
            for (var index = 0; index < count; index++)
            {
                // Check if the cancellation was requested.
                if (token.IsCancellationRequested)
                {
                    // Break.
                    break;
                }
                // Create a new scope.
                using var scope = serviceProvider.CreateScope();
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items in the current batch.
                var batchItems = Items.Skip(index * ApplicationDbContext.BatchSize).Take(ApplicationDbContext.BatchSize);
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
                    .Where(item => item.EdgeNodes != null)
                    .Select(item => item.EdgeNodes)
                    .SelectMany(item => item)
                    .Where(item => !string.IsNullOrEmpty(item.NodeId) && (item.Type == "Source" || item.Type == "Target"))
                    .Select(item => item.NodeId)
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
                    .Where(item => item.DatabaseEdgeFieldEdges != null)
                    .Select(item => item.DatabaseEdgeFieldEdges)
                    .SelectMany(item => item)
                    .Where(item => !string.IsNullOrEmpty(item.DatabaseEdgeFieldId) && !string.IsNullOrEmpty(item.Value))
                    .Select(item => item.DatabaseEdgeFieldId)
                    .Distinct();
                // Get the edge fields that are to be updated.
                var edgeFields = context.DatabaseEdgeFields
                    .Where(item => item.Database.DatabaseType.Name != "Generic")
                    .Where(item => itemEdgeFieldIds.Contains(item.Id))
                    .Include(item => item.Database)
                    .AsEnumerable();
                // Get the IDs of all of the databases that are to be updated.
                var itemDatabaseIds = batchItems
                    .Where(item => item.DatabaseEdges != null)
                    .Select(item => item.DatabaseEdges)
                    .SelectMany(item => item)
                    .Select(item => item.DatabaseId)
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
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding edge.
                    var edge = edges.FirstOrDefault(item => batchItem.Id == item.Id);
                    // Check if there was no edge found.
                    if (edge == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Get the valid item nodes and the edge nodes to add.
                    var edgeNodes = batchItem.EdgeNodes != null ?
                        batchItem.EdgeNodes
                            .Where(item => item.Type == "Source" || item.Type == "Target")
                            .Select(item => (item.NodeId, item.Type))
                            .Distinct()
                            .Where(item => validItemNodeIds.Contains(item.NodeId))
                            .Select(item => new EdgeNode
                            {
                                EdgeId = edge.Id,
                                Edge = edge,
                                NodeId = item.NodeId,
                                Node = nodes.FirstOrDefault(item1 => item.NodeId == item1.Id),
                                Type = EnumerationExtensions.GetEnumerationValue<EdgeNodeType>(item.Type)
                            })
                            .Where(item => item.Edge != null && item.Node != null) :
                        Enumerable.Empty<EdgeNode>();
                    // Check if there weren't any nodes found, or if there isn't at least one source node and one target node.
                    if (edgeNodes == null || !edgeNodes.Any() || edgeNodes.FirstOrDefault(item => item.Type == EdgeNodeType.Source) == null || edgeNodes.FirstOrDefault(item => item.Type == EdgeNodeType.Target) == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Get the valid item fields and the edge field edges to add.
                    var edgeFieldEdges = batchItem.DatabaseEdgeFieldEdges != null ?
                        batchItem.DatabaseEdgeFieldEdges
                            .Select(item => (item.DatabaseEdgeFieldId, item.Value))
                            .Distinct()
                            .Where(item => validItemEdgeFieldIds.Contains(item.DatabaseEdgeFieldId))
                            .Select(item => new DatabaseEdgeFieldEdge
                            {
                                DatabaseEdgeFieldId = item.DatabaseEdgeFieldId,
                                DatabaseEdgeField = edgeFields.FirstOrDefault(item1 => item.DatabaseEdgeFieldId == item1.Id),
                                EdgeId = edge.Id,
                                Edge = edge,
                                Value = item.Value
                            })
                            .Where(item => item.DatabaseEdgeField != null && item.Edge != null) :
                        Enumerable.Empty<DatabaseEdgeFieldEdge>();
                    // Get the valid item databases and the database edges to add.
                    var databaseEdges = batchItem.DatabaseEdges != null ?
                        batchItem.DatabaseEdges
                            .Select(item => item.DatabaseId)
                            .Where(item => validItemDatabaseIds.Contains(item))
                            .Concat(edgeFieldEdges.Select(item => item.DatabaseEdgeField.Database.Id))
                            .Distinct()
                            .Select(item => new DatabaseEdge
                            {
                                DatabaseId = item,
                                Database = databases.FirstOrDefault(item1 => item == item1.Id),
                                EdgeId = edge.Id,
                                Edge = edge
                            })
                            .Where(item => item.Database != null && item.Edge != null) :
                        Enumerable.Empty<DatabaseEdge>();
                    // Check if there weren't any databases or edge fields found.
                    if (databaseEdges == null || !databaseEdges.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Update the edge.
                    edge.Name = string.Concat(edgeNodes.First(item => item.Type == EdgeNodeType.Source).Node.Name, " - ", edgeNodes.First(item => item.Type == EdgeNodeType.Target).Node.Name);
                    edge.Description = batchItem.Description;
                    edge.EdgeNodes = new List<EdgeNode>
                    {
                        edgeNodes.First(item => item.Type == EdgeNodeType.Source),
                        edgeNodes.First(item => item.Type == EdgeNodeType.Target)
                    };
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
                // Delete the items.
                IQueryableExtensions.Delete(analyses, context, token);
                IQueryableExtensions.Delete(networks, context, token);
                // Update the items.
                IEnumerableExtensions.Edit(edgesToUpdate, context, token);
            }
        }

        /// <summary>
        /// Deletes the items from the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void Delete(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if there weren't any valid items found.
            if (Items == null)
            {
                // Throw an exception.
                throw new ArgumentException("No valid items could be found with the provided data.");
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)Items.Count() / ApplicationDbContext.BatchSize);
            // Go over each batch.
            for (var index = 0; index < count; index++)
            {
                // Check if the cancellation was requested.
                if (token.IsCancellationRequested)
                {
                    // Break.
                    break;
                }
                // Create a new scope.
                using var scope = serviceProvider.CreateScope();
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items in the current batch.
                var batchItems = Items.Skip(index * ApplicationDbContext.BatchSize).Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems.Select(item => item.Id);
                // Get the items with the current batch IDs.
                var edges = context.Edges
                    .Where(item => batchIds.Contains(item.Id));
                // Get the related entities that use the items.
                var networks = context.Networks
                    .Where(item => item.NetworkEdges.Any(item1 => edges.Contains(item1.Edge)));
                var analyses = context.Analyses
                    .Where(item => item.AnalysisEdges.Any(item1 => edges.Contains(item1.Edge)));
                // Delete the items.
                IQueryableExtensions.Delete(analyses, context, token);
                IQueryableExtensions.Delete(networks, context, token);
                IQueryableExtensions.Delete(edges, context, token);
            }
        }
    }
}
