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
using NetControl4BioMed.Helpers.Exceptions;

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
        /// <returns>The created items.</returns>
        public IEnumerable<Edge> Create(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if there weren't any valid items found.
            if (Items == null)
            {
                // Throw an exception.
                throw new TaskException("No valid items could be found with the provided data.");
            }
            // Check if the exception item should be shown.
            var showExceptionItem = Items.Count() > 1;
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
                var batchItems = Items
                    .Skip(index * ApplicationDbContext.BatchSize)
                    .Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id);
                // Check if any of the IDs are repeating in the list.
                if (batchIds.Distinct().Count() != batchIds.Count())
                {
                    // Throw an exception.
                    throw new TaskException("Two or more of the manually provided IDs are duplicated.");
                }
                // Get the valid IDs, that do not appear in the database.
                var validBatchIds = batchIds
                    .Except(context.Edges
                        .Where(item => batchIds.Contains(item.Id))
                        .Select(item => item.Id));
                // Get the IDs of the related entities that appear in the current batch.
                var batchDatabaseIds = batchItems
                    .Where(item => item.DatabaseEdges != null)
                    .Select(item => item.DatabaseEdges)
                    .SelectMany(item => item)
                    .Where(item => item.Database != null)
                    .Select(item => item.Database)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                var batchDatabaseEdgeFieldIds = batchItems
                    .Where(item => item.DatabaseEdgeFieldEdges != null)
                    .Select(item => item.DatabaseEdgeFieldEdges)
                    .SelectMany(item => item)
                    .Where(item => item.DatabaseEdgeField != null)
                    .Select(item => item.DatabaseEdgeField)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                var batchNodeIds = batchItems
                    .Where(item => item.EdgeNodes != null)
                    .Select(item => item.EdgeNodes)
                    .SelectMany(item => item)
                    .Where(item => item.Node != null)
                    .Select(item => item.Node)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Get the related entities that appear in the current batch.
                var batchDatabaseEdgeFields = context.DatabaseEdgeFields
                    .Include(item => item.Database)
                    .Where(item => item.Database.DatabaseType.Name != "Generic")
                    .Where(item => batchDatabaseEdgeFieldIds.Contains(item.Id));
                var batchDatabases = context.Databases
                    .Where(item => item.DatabaseType.Name != "Generic")
                    .Where(item => batchDatabaseIds.Contains(item.Id))
                    .Concat(batchDatabaseEdgeFields
                        .Select(item => item.Database))
                    .Distinct();
                var batchNodes = context.Nodes
                    .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => batchNodeIds.Contains(item.Id));
                // Save the items to add.
                var edgesToAdd = new List<Edge>();
                // Go over each of the items.
                foreach (var batchItem in batchItems)
                {
                    // Check if the ID of the item is not valid.
                    if (!string.IsNullOrEmpty(batchItem.Id) && !validBatchIds.Contains(batchItem.Id))
                    {
                        // Continue.
                        continue;
                    }
                    // Check if there were no edge nodes provided.
                    if (batchItem.EdgeNodes == null || !batchItem.EdgeNodes.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no edge nodes provided.", showExceptionItem, batchItem);
                    }
                    // Get the edge nodes.
                    var edgeNodes = batchItem.EdgeNodes
                        .Where(item => item.Node != null)
                        .Where(item => !string.IsNullOrEmpty(item.Node.Id))
                        .Where(item => item.Type == "Source" || item.Type == "Target")
                        .Select(item => (item.Node.Id, item.Type))
                        .Distinct()
                        .Where(item => batchNodes.Any(item1 => item1.Id == item.Item1))
                        .Select(item => new EdgeNode
                        {
                            NodeId = item.Item1,
                            Node = batchNodes
                                .FirstOrDefault(item1 => item1.Id == item.Item1),
                            Type = EnumerationExtensions.GetEnumerationValue<EdgeNodeType>(item.Item2)
                        })
                        .Where(item => item.Node != null);
                    // Check if there were no edge nodes found.
                    if (edgeNodes == null || !edgeNodes.Any(item => item.Type == EdgeNodeType.Source) || !edgeNodes.Any(item => item.Type == EdgeNodeType.Target))
                    {
                        // Throw an exception.
                        throw new TaskException("There were no edge nodes found.", showExceptionItem, batchItem);
                    }
                    // Check if there were no database edges or database edge field edges provided.
                    if ((batchItem.DatabaseEdges == null || !batchItem.DatabaseEdges.Any()) && (batchItem.DatabaseEdgeFieldEdges == null || !batchItem.DatabaseEdgeFieldEdges.Any()))
                    {
                        // Throw an exception.
                        throw new TaskException("There were no database edges or database edge field edges provided.", showExceptionItem, batchItem);
                    }
                    // Get the database edge field edges.
                    var databaseEdgeFieldEdges = batchItem.DatabaseEdgeFieldEdges != null ?
                        batchItem.DatabaseEdgeFieldEdges
                            .Where(item => item.DatabaseEdgeField != null)
                            .Where(item => !string.IsNullOrEmpty(item.DatabaseEdgeField.Id))
                            .Where(item => !string.IsNullOrEmpty(item.Value))
                            .Select(item => (item.DatabaseEdgeField.Id, item.Value))
                            .Distinct()
                            .Where(item => batchDatabaseEdgeFields.Any(item1 => item1.Id == item.Item1))
                            .Select(item => new DatabaseEdgeFieldEdge
                            {
                                DatabaseEdgeFieldId = item.Item1,
                                DatabaseEdgeField = batchDatabaseEdgeFields
                                    .FirstOrDefault(item1 => item1.Id == item.Item1),
                                Value = item.Item2
                            })
                            .Where(item => item.DatabaseEdgeField != null) :
                        Enumerable.Empty<DatabaseEdgeFieldEdge>();
                    // Get the database edges.
                    var databaseEdges = batchItem.DatabaseEdges != null ?
                        batchItem.DatabaseEdges
                            .Where(item => item.Database != null)
                            .Where(item => !string.IsNullOrEmpty(item.Database.Id))
                            .Select(item => item.Database.Id)
                            .Concat(databaseEdgeFieldEdges.Select(item => item.DatabaseEdgeField.Database.Id))
                            .Distinct()
                            .Where(item => batchDatabases.Any(item1 => item1.Id == item))
                            .Select(item => new DatabaseEdge
                            {
                                DatabaseId = item,
                                Database = batchDatabases
                                    .FirstOrDefault(item1 => item1.Id == item)
                            })
                            .Where(item => item.Database != null) :
                        Enumerable.Empty<DatabaseEdge>();
                    // Check if there were no database edges found.
                    if (databaseEdges == null || !databaseEdges.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no database edges found.", showExceptionItem, batchItem);
                    }
                    // Define the new edge.
                    var edge = new Edge
                    {
                        DateTimeCreated = DateTime.Now,
                        Name = string.Concat(edgeNodes.First(item => item.Type == EdgeNodeType.Source).Node.Name, " - ", edgeNodes.First(item1 => item1.Type == EdgeNodeType.Target).Node.Name),
                        Description = batchItem.Description,
                        EdgeNodes = new List<EdgeNode>
                        {
                            edgeNodes.First(item => item.Type == EdgeNodeType.Source),
                            edgeNodes.First(item => item.Type == EdgeNodeType.Target)
                        },
                        DatabaseEdgeFieldEdges = databaseEdgeFieldEdges.ToList(),
                        DatabaseEdges = databaseEdges.ToList()
                    };
                    // Check if there is any ID provided.
                    if (!string.IsNullOrEmpty(batchItem.Id))
                    {
                        // Assign it to the node.
                        edge.Id = batchItem.Id;
                    }
                    // Add the new node to the list.
                    edgesToAdd.Add(edge);
                }
                // Create the items.
                IEnumerableExtensions.Create(edgesToAdd, context, token);
                // Go over each item.
                foreach (var edge in edgesToAdd)
                {
                    // Yield return it.
                    yield return edge;
                }
            }
        }

        /// <summary>
        /// Edits the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns>The edited items.</returns>
        public IEnumerable<Edge> Edit(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if there weren't any valid items found.
            if (Items == null)
            {
                // Throw an exception.
                throw new TaskException("No valid items could be found with the provided data.");
            }
            // Check if the exception item should be shown.
            var showExceptionItem = Items.Count() > 1;
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
                var batchItems = Items
                    .Skip(index * ApplicationDbContext.BatchSize)
                    .Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id);
                // Get the IDs of the related entities that appear in the current batch.
                var batchDatabaseIds = batchItems
                    .Where(item => item.DatabaseEdges != null)
                    .Select(item => item.DatabaseEdges)
                    .SelectMany(item => item)
                    .Where(item => item.Database != null)
                    .Select(item => item.Database)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                var batchDatabaseEdgeFieldIds = batchItems
                    .Where(item => item.DatabaseEdgeFieldEdges != null)
                    .Select(item => item.DatabaseEdgeFieldEdges)
                    .SelectMany(item => item)
                    .Where(item => item.DatabaseEdgeField != null)
                    .Select(item => item.DatabaseEdgeField)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                var batchNodeIds = batchItems
                    .Where(item => item.EdgeNodes != null)
                    .Select(item => item.EdgeNodes)
                    .SelectMany(item => item)
                    .Where(item => item.Node != null)
                    .Select(item => item.Node)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Get the related entities that appear in the current batch.
                var batchDatabaseEdgeFields = context.DatabaseEdgeFields
                    .Include(item => item.Database)
                    .Where(item => item.Database.DatabaseType.Name != "Generic")
                    .Where(item => batchDatabaseEdgeFieldIds.Contains(item.Id));
                var batchDatabases = context.Databases
                    .Where(item => item.DatabaseType.Name != "Generic")
                    .Where(item => batchDatabaseIds.Contains(item.Id))
                    .Concat(batchDatabaseEdgeFields
                        .Select(item => item.Database))
                    .Distinct();
                var batchNodes = context.Nodes
                    .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => batchNodeIds.Contains(item.Id));
                // Get the items corresponding to the current batch.
                var edges = context.Edges
                    .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => batchIds.Contains(item.Id));
                // Get the related entities to delete.
                var batchEdgeNodes = context.EdgeNodes
                    .Where(item => edges.Contains(item.Edge));
                var batchDatabaseEdges = context.DatabaseEdges
                    .Where(item => edges.Contains(item.Edge));
                var batchDatabaseEdgeFieldEdges = context.DatabaseEdgeFieldEdges
                    .Where(item => edges.Contains(item.Edge));
                // Save the items to edit.
                var edgesToEdit = new List<Edge>();
                // Go over each of the valid items.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding item.
                    var edge = edges
                        .FirstOrDefault(item => batchItem.Id == item.Id);
                    // Check if there was no item found.
                    if (edge == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Check if there were no edge nodes provided.
                    if (batchItem.EdgeNodes == null || !batchItem.EdgeNodes.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no edge nodes provided.", showExceptionItem, batchItem);
                    }
                    // Get the edge nodes.
                    var edgeNodes = batchItem.EdgeNodes
                        .Where(item => item.Node != null)
                        .Where(item => !string.IsNullOrEmpty(item.Node.Id))
                        .Where(item => item.Type == "Source" || item.Type == "Target")
                        .Where(item => batchNodes.Any(item1 => item1.Id == item.Node.Id))
                        .Select(item => new EdgeNode
                        {
                            NodeId = item.Node.Id,
                            Node = batchNodes
                                .FirstOrDefault(item1 => item1.Id == item.Node.Id),
                            Type = EnumerationExtensions.GetEnumerationValue<EdgeNodeType>(item.Type)
                        })
                        .Where(item => item.Node != null);
                    // Check if there were no edge nodes found.
                    if (edgeNodes == null || !edgeNodes.Any(item => item.Type == EdgeNodeType.Source) || !edgeNodes.Any(item => item.Type == EdgeNodeType.Target))
                    {
                        // Throw an exception.
                        throw new TaskException("There were no edge nodes found.", showExceptionItem, batchItem);
                    }
                    // Check if there were no database edges or database edge field edges provided.
                    if ((batchItem.DatabaseEdges == null || !batchItem.DatabaseEdges.Any()) && (batchItem.DatabaseEdgeFieldEdges == null || !batchItem.DatabaseEdgeFieldEdges.Any()))
                    {
                        // Throw an exception.
                        throw new TaskException("There were no database edges or database edge field edges provided.", showExceptionItem, batchItem);
                    }
                    // Get the database edge field edges.
                    var databaseEdgeFieldEdges = batchItem.DatabaseEdgeFieldEdges != null ?
                        batchItem.DatabaseEdgeFieldEdges
                            .Where(item => item.DatabaseEdgeField != null)
                            .Where(item => !string.IsNullOrEmpty(item.DatabaseEdgeField.Id))
                            .Where(item => !string.IsNullOrEmpty(item.Value))
                            .Select(item => (item.DatabaseEdgeField.Id, item.Value))
                            .Distinct()
                            .Where(item => batchDatabaseEdgeFields.Any(item1 => item1.Id == item.Item1))
                            .Select(item => new DatabaseEdgeFieldEdge
                            {
                                DatabaseEdgeFieldId = item.Item1,
                                DatabaseEdgeField = batchDatabaseEdgeFields
                                    .FirstOrDefault(item1 => item1.Id == item.Item1),
                                Value = item.Item2
                            })
                            .Where(item => item.DatabaseEdgeField != null) :
                        Enumerable.Empty<DatabaseEdgeFieldEdge>();
                    // Get the database edges.
                    var databaseEdges = batchItem.DatabaseEdges != null ?
                        batchItem.DatabaseEdges
                            .Where(item => item.Database != null)
                            .Where(item => !string.IsNullOrEmpty(item.Database.Id))
                            .Select(item => item.Database.Id)
                            .Concat(databaseEdgeFieldEdges.Select(item => item.DatabaseEdgeField.Database.Id))
                            .Distinct()
                            .Where(item => batchDatabases.Any(item1 => item1.Id == item))
                            .Select(item => new DatabaseEdge
                            {
                                DatabaseId = item,
                                Database = batchDatabases
                                    .FirstOrDefault(item1 => item1.Id == item)
                            })
                            .Where(item => item.Database != null) :
                        Enumerable.Empty<DatabaseEdge>();
                    // Check if there were no database edges found.
                    if (databaseEdges == null || !databaseEdges.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no database edges found.", showExceptionItem, batchItem);
                    }
                    // Delete all related entities that appear in the current batch.
                    IQueryableExtensions.Delete(batchEdgeNodes.Where(item => item.Edge == edge), context, token);
                    IQueryableExtensions.Delete(batchDatabaseEdges.Where(item => item.Edge == edge), context, token);
                    IQueryableExtensions.Delete(batchDatabaseEdgeFieldEdges.Where(item => item.Edge == edge), context, token);
                    // Update the edge.
                    edge.Name = string.Concat(edgeNodes.First(item => item.Type == EdgeNodeType.Source).Node.Name, " - ", edgeNodes.First(item => item.Type == EdgeNodeType.Target).Node.Name);
                    edge.Description = batchItem.Description;
                    edge.EdgeNodes = new List<EdgeNode>
                    {
                        edgeNodes.First(item => item.Type == EdgeNodeType.Source),
                        edgeNodes.First(item => item.Type == EdgeNodeType.Target)
                    };
                    edge.DatabaseEdgeFieldEdges = databaseEdgeFieldEdges.ToList();
                    edge.DatabaseEdges = databaseEdges.ToList();
                    // Add the edge to the list.
                    edgesToEdit.Add(edge);
                }
                // Get the networks and analyses that contain the edges.
                var networks = context.Networks
                    .Where(item => item.NetworkEdges.Any(item1 => edgesToEdit.Contains(item1.Edge)));
                var analyses = context.Analyses
                    .Where(item => item.AnalysisEdges.Any(item1 => edgesToEdit.Contains(item1.Edge)));
                // Delete the items.
                IQueryableExtensions.Delete(analyses, context, token);
                IQueryableExtensions.Delete(networks, context, token);
                // Update the items.
                IEnumerableExtensions.Edit(edgesToEdit, context, token);
                // Go over each item.
                foreach (var edge in edgesToEdit)
                {
                    // Yield return it.
                    yield return edge;
                }
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
                throw new TaskException("No valid items could be found with the provided data.");
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
                var batchItems = Items
                    .Skip(index * ApplicationDbContext.BatchSize)
                    .Take(ApplicationDbContext.BatchSize);
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
