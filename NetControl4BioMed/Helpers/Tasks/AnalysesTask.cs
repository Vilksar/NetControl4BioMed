using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Exceptions;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Tasks
{
    /// <summary>
    /// Implements a task to update analyses in the database.
    /// </summary>
    public class AnalysesTask
    {
        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<AnalysisInputModel> Items { get; set; }

        /// <summary>
        /// Gets or sets the HTTP context scheme.
        /// </summary>
        public string Scheme { get; set; }

        /// <summary>
        /// Gets or sets the HTTP context host value.
        /// </summary>
        public string HostValue { get; set; }

        /// <summary>
        /// Creates the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public IEnumerable<Analysis> Create(IServiceProvider serviceProvider, CancellationToken token)
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
                    throw new TaskException("One or more of the manually provided IDs are duplicated.");
                }
                // Get the valid IDs, that do not appear in the database.
                var validBatchIds = batchIds
                    .Except(context.Analyses
                        .Where(item => batchIds.Contains(item.Id))
                        .Select(item => item.Id));
                // Get the IDs of the related entities that appear in the current batch.
                var batchUserIds = batchItems
                    .Where(item => item.AnalysisUsers != null)
                    .Select(item => item.AnalysisUsers)
                    .SelectMany(item => item)
                    .Where(item => item.User != null)
                    .Select(item => item.User)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                var batchNodeCollectionIds = batchItems
                    .Where(item => item.AnalysisNodeCollections != null)
                    .Select(item => item.AnalysisNodeCollections)
                    .SelectMany(item => item)
                    .Where(item => item.NodeCollection != null)
                    .Select(item => item.NodeCollection)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                var batchNetworkIds = batchItems
                    .Where(item => item.AnalysisNetworks != null)
                    .Select(item => item.AnalysisNetworks)
                    .SelectMany(item => item)
                    .Where(item => item.Network != null)
                    .Select(item => item.Network)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Get the related entities that appear in the current batch.
                var batchUsers = context.Users
                    .Where(item => batchUserIds.Contains(item.Id));
                var batchNodeCollections = context.NodeCollections
                    .Include(item => item.NodeCollectionDatabases)
                        .ThenInclude(item => item.Database)
                    .Where(item => batchNodeCollectionIds.Contains(item.Id));
                var batchNetworks = context.Networks
                    .Include(item => item.NetworkDatabases)
                        .ThenInclude(item => item.Database)
                    .Where(item => batchNetworkIds.Contains(item.Id));
                // Save the items to add.
                var analysesToAdd = new List<Analysis>();
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Check if the ID of the item is not valid.
                    if (!string.IsNullOrEmpty(batchItem.Id) && !validBatchIds.Contains(batchItem.Id))
                    {
                        // Continue.
                        continue;
                    }
                    // Check if there are no analysis users provided.
                    if (batchItem.AnalysisUsers == null || !batchItem.AnalysisUsers.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no analysis users provided.", showExceptionItem, batchItem);
                    }
                    // Get the analysis users.
                    var analysisUsers = batchItem.AnalysisUsers
                        .Where(item => item.User != null)
                        .Where(item => !string.IsNullOrEmpty(item.User.Id))
                        .Select(item => item.User.Id)
                        .Distinct()
                        .Where(item => batchUsers.Any(item1 => item1.Id == item))
                        .Select(item => new AnalysisUser
                        {
                            DateTimeCreated = DateTime.Now,
                            UserId = item,
                            User = batchUsers
                                .FirstOrDefault(item1 => item1.Id == item)
                        })
                        .Where(item => item.User != null);
                    // Check if there were no analysis users found.
                    if (analysisUsers == null || !analysisUsers.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no analysis users found.", showExceptionItem, batchItem);
                    }
                    // Check if there are no analysis networks provided.
                    if (batchItem.AnalysisNetworks == null || !batchItem.AnalysisNetworks.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no analysis networks provided.", showExceptionItem, batchItem);
                    }
                    // Get the analysis networks.
                    var analysisNetworks = batchItem.AnalysisNetworks
                        .Where(item => item.Network != null)
                        .Where(item => !string.IsNullOrEmpty(item.Network.Id))
                        .Select(item => item.Network.Id)
                        .Distinct()
                        .Where(item => batchNetworks.Any(item1 => item1.Id == item))
                        .Select(item => new AnalysisNetwork
                        {
                            NetworkId = item,
                            Network = batchNetworks
                                .FirstOrDefault(item1 => item1.Id == item)
                        })
                        .Where(item => item.Network != null);
                    // Check if there were no analysis networks found.
                    if (analysisNetworks == null || !analysisNetworks.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no analysis networks found.", showExceptionItem, batchItem);
                    }
                    // Get the node analysis databases.
                    var nodeAnalysisDatabases = analysisNetworks
                            .Select(item => item.Network)
                            .Select(item => item.NetworkDatabases)
                            .SelectMany(item => item)
                            .Where(item => item.Type == NetworkDatabaseType.Node)
                            .Select(item => item.Database)
                            .Distinct()
                            .Select(item => new AnalysisDatabase
                            {
                                DatabaseId = item.Id,
                                Database = item,
                                Type = AnalysisDatabaseType.Node
                            });
                    // Check if there were no node analysis databases found.
                    if (nodeAnalysisDatabases == null || !nodeAnalysisDatabases.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no node analysis databases found.", showExceptionItem, batchItem);
                    }
                    // Get the edge analysis databases.
                    var edgeAnalysisDatabases = analysisNetworks
                            .Select(item => item.Network)
                            .Select(item => item.NetworkDatabases)
                            .SelectMany(item => item)
                            .Where(item => item.Type == NetworkDatabaseType.Edge)
                            .Select(item => item.Database)
                            .Distinct()
                            .Select(item => new AnalysisDatabase
                            {
                                DatabaseId = item.Id,
                                Database = item,
                                Type = AnalysisDatabaseType.Edge
                            });
                    // Check if there were no edge analysis databases found.
                    if (edgeAnalysisDatabases == null || !edgeAnalysisDatabases.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no edge analysis databases found.", showExceptionItem, batchItem);
                    }
                    // Get the node databases.
                    var nodeDatabases = nodeAnalysisDatabases
                        .Select(item => item.Database);
                    // Get the analysis node collections.
                    var analysisNodeCollections = batchItem.AnalysisNodeCollections != null ?
                        batchItem.AnalysisNodeCollections
                            .Where(item => item.NodeCollection != null)
                            .Where(item => !string.IsNullOrEmpty(item.NodeCollection.Id))
                            .Where(item => item.Type == "Source" || item.Type == "Target")
                            .Select(item => (item.NodeCollection.Id, item.Type))
                            .Distinct()
                            .Where(item => batchNodeCollections.Any(item1 => item1.Id == item.Item1))
                            .Select(item => new AnalysisNodeCollection
                            {
                                NodeCollectionId = item.Item1,
                                NodeCollection = batchNodeCollections
                                    .FirstOrDefault(item1 => item1.Id == item.Item1 && item1.NodeCollectionDatabases.Any(item2 => nodeDatabases.Contains(item2.Database))),
                                Type = EnumerationExtensions.GetEnumerationValue<AnalysisNodeCollectionType>(item.Item2)
                            })
                            .Where(item => item.NodeCollection != null) :
                        Enumerable.Empty<AnalysisNodeCollection>();
                    // Define the new item.
                    var analysis = new Analysis
                    {
                        DateTimeCreated = DateTime.Now,
                        Name = batchItem.Name,
                        Description = batchItem.Description,
                        Status = AnalysisStatus.Defined,
                        Log = JsonSerializer.Serialize(Enumerable.Empty<string>()),
                        Data = batchItem.Data,
                        MaximumIterations = batchItem.MaximumIterations,
                        MaximumIterationsWithoutImprovement = batchItem.MaximumIterationsWithoutImprovement,
                        Parameters = batchItem.Parameters,
                        AnalysisUsers = analysisUsers.ToList(),
                        AnalysisDatabases = nodeAnalysisDatabases
                            .Concat(edgeAnalysisDatabases)
                            .ToList(),
                        AnalysisNodeCollections = analysisNodeCollections.ToList(),
                        AnalysisNetworks = analysisNetworks.ToList()
                    };
                    // Try to get the algorithm.
                    try
                    {
                        // Get the algorithm.
                        analysis.Algorithm = EnumerationExtensions.GetEnumerationValue<AnalysisAlgorithm>(batchItem.Algorithm);
                    }
                    catch (Exception exception)
                    {
                        // Get the exception message.
                        var message = string.IsNullOrEmpty(exception.Message) ? string.Empty : " " + exception.Message;
                        throw new TaskException("The algorithm couldn't be determined from the provided string." + message, showExceptionItem, batchItem);
                    }
                    // Append a message to the log.
                    analysis.Log = analysis.AppendToLog("The analysis has been defined and stored in the database.");
                    // Check if there is any ID provided.
                    if (!string.IsNullOrEmpty(batchItem.Id))
                    {
                        // Assign it to the item.
                        analysis.Id = batchItem.Id;
                    }
                    // Add the new item to the list.
                    analysesToAdd.Add(analysis);
                }
                // Edit the items.
                IEnumerableExtensions.Create(analysesToAdd, context, token);
                // Go over each item.
                foreach (var analysisToAdd in analysesToAdd)
                {
                    // Yield return it.
                    yield return analysisToAdd;
                }
            }
        }

        /// <summary>
        /// Edits the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public IEnumerable<Analysis> Edit(IServiceProvider serviceProvider, CancellationToken token)
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
                // Get the items with the provided IDs.
                var analyses = context.Analyses
                    .Where(item => batchIds.Contains(item.Id));
                // Save the items to add.
                var analysesToEdit = new List<Analysis>();
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding item.
                    var analysis = analyses
                        .FirstOrDefault(item => item.Id == batchItem.Id);
                    // Check if there was no item found.
                    if (analysis == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Update the data.
                    analysis.Name = batchItem.Name;
                    analysis.Description = batchItem.Description;
                    // Append a message to the log.
                    analysis.Log = analysis.AppendToLog("The analysis details have been updated.");
                    // Add the item to the list.
                    analysesToEdit.Add(analysis);
                }
                // Edit the items.
                IEnumerableExtensions.Edit(analysesToEdit, context, token);
                // Go over each item.
                foreach (var analysisToEdit in analysesToEdit)
                {
                    // Yield return it.
                    yield return analysisToEdit;
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
                // Get the items with the provided IDs.
                var analyses = context.Analyses
                    .Where(item => batchIds.Contains(item.Id));
                // Delete the items.
                IQueryableExtensions.Delete(analyses, context, token);
            }
        }

        /// <summary>
        /// Generates the items.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void Generate(IServiceProvider serviceProvider, CancellationToken token)
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
                // Get the items with the provided IDs.
                var analyses = context.Analyses
                    .Include(item => item.AnalysisDatabases)
                        .ThenInclude(item => item.Database)
                    .Include(item => item.AnalysisNodeCollections)
                        .ThenInclude(item => item.NodeCollection)
                    .Include(item => item.AnalysisNetworks)
                        .ThenInclude(item => item.Network)
                    .Where(item => batchIds.Contains(item.Id));
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding item.
                    var analysis = analyses
                        .FirstOrDefault(item => item.Id == batchItem.Id);
                    // Check if there was no item found.
                    if (analysis == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Update the status of the item.
                    analysis.Status = AnalysisStatus.Generating;
                    // Add a message to the log.
                    analysis.Log = analysis.AppendToLog("The analysis is now generating.");
                    // Edit the network.
                    IEnumerableExtensions.Edit(analysis.Yield(), context, token);
                    // Try to deserialize the data.
                    if (!analysis.Data.TryDeserializeJsonObject<IEnumerable<AnalysisNodeInputModel>>(out var data) || data == null)
                    {
                        // Update the status of the item.
                        analysis.Status = AnalysisStatus.Error;
                        // Add a message to the log.
                        analysis.Log = analysis.AppendToLog("The source and / or target data corresponding to the analysis could not be deserialized.");
                        // Edit the analysis.
                        IEnumerableExtensions.Edit(analysis.Yield(), context, token);
                        // Continue.
                        continue;
                    }
                    // Get the IDs of the required related data.
                    var nodeDatabaseIds = analysis.AnalysisDatabases != null ?
                        analysis.AnalysisDatabases
                            .Where(item => item.Type == AnalysisDatabaseType.Node)
                            .Select(item => item.Database)
                            .Distinct()
                            .Select(item => item.Id) :
                        Enumerable.Empty<string>();
                    var edgeDatabaseIds = analysis.AnalysisDatabases != null ?
                        analysis.AnalysisDatabases
                            .Where(item => item.Type == AnalysisDatabaseType.Edge)
                            .Select(item => item.Database)
                            .Distinct()
                            .Select(item => item.Id) :
                        Enumerable.Empty<string>();
                    var sourceNodeCollectionIds = analysis.AnalysisNodeCollections != null ?
                        analysis.AnalysisNodeCollections
                            .Where(item => item.Type == AnalysisNodeCollectionType.Source)
                            .Select(item => item.NodeCollection)
                            .Distinct()
                            .Select(item => item.Id) :
                        Enumerable.Empty<string>();
                    var targetNodeCollectionIds = analysis.AnalysisNodeCollections != null ?
                        analysis.AnalysisNodeCollections
                            .Where(item => item.Type == AnalysisNodeCollectionType.Target)
                            .Select(item => item.NodeCollection)
                            .Distinct()
                            .Select(item => item.Id) :
                        Enumerable.Empty<string>();
                    var networkIds = analysis.AnalysisNetworks != null ?
                        analysis.AnalysisNetworks
                            .Select(item => item.Network)
                            .Distinct()
                            .Select(item => item.Id) :
                        Enumerable.Empty<string>();
                    // Get the identifiers of the nodes in the provided data.
                    var sourceNodeIdentifiers = data
                        .Where(item => item.Type == "Source")
                        .Where(item => item.Node != null)
                        .Select(item => item.Node)
                        .Where(item => !string.IsNullOrEmpty(item.Id))
                        .Select(item => item.Id)
                        .Distinct();
                    var targetNodeIdentifiers = data
                        .Where(item => item.Type == "Target")
                        .Where(item => item.Node != null)
                        .Select(item => item.Node)
                        .Where(item => !string.IsNullOrEmpty(item.Id))
                        .Select(item => item.Id)
                        .Distinct();
                    // Get the available nodes.
                    var availableNodes = context.Nodes
                        .Where(item => item.DatabaseNodes.Any(item1 => nodeDatabaseIds.Contains(item1.Database.Id)));
                    // Get the nodes by identifier.
                    var sourceNodesByIdentifier = availableNodes
                        .Where(item => sourceNodeIdentifiers.Contains(item.Id) || item.DatabaseNodeFieldNodes.Any(item1 => item1.DatabaseNodeField.IsSearchable && sourceNodeIdentifiers.Contains(item1.Value)));
                    var targetNodesByIdentifier = availableNodes
                        .Where(item => targetNodeIdentifiers.Contains(item.Id) || item.DatabaseNodeFieldNodes.Any(item1 => item1.DatabaseNodeField.IsSearchable && targetNodeIdentifiers.Contains(item1.Value)));
                    // Get the nodes by node collection.
                    var sourceNodesByNodeCollection = availableNodes
                        .Where(item => item.NodeCollectionNodes.Any(item1 => sourceNodeCollectionIds.Contains(item1.NodeCollection.Id)));
                    var targetNodesByNodeCollection = availableNodes
                        .Where(item => item.NodeCollectionNodes.Any(item1 => targetNodeCollectionIds.Contains(item1.NodeCollection.Id)));
                    // Get the nodes in the analysis.
                    var nodes = availableNodes
                        .Where(item => item.NetworkNodes.Any(item1 => networkIds.Contains(item1.Network.Id)));
                    // Check if there haven't been any nodes found.
                    if (nodes == null || !nodes.Any())
                    {
                        // Update the status of the item.
                        analysis.Status = AnalysisStatus.Error;
                        // Add a message to the log.
                        analysis.Log = analysis.AppendToLog("No nodes could be found within the provided networks.");
                        // Edit the analysis.
                        IEnumerableExtensions.Edit(analysis.Yield(), context, token);
                        // Continue.
                        continue;
                    }
                    // Get the available edges.
                    var availableEdges = context.Edges
                        .Where(item => item.DatabaseEdges.Any(item1 => edgeDatabaseIds.Contains(item1.Database.Id)));
                    // Get the edges in the analysis.
                    var edges = availableEdges
                        .Where(item => item.NetworkEdges.Any(item1 => networkIds.Contains(item1.Network.Id)));
                    // Check if there haven't been any edges found.
                    if (edges == null || !edges.Any())
                    {
                        // Update the status of the item.
                        analysis.Status = AnalysisStatus.Error;
                        // Add a message to the log.
                        analysis.Log = analysis.AppendToLog("No edges could be found within the provided networks.");
                        // Edit the analysis.
                        IEnumerableExtensions.Edit(analysis.Yield(), context, token);
                        // Continue.
                        continue;
                    }
                    // Get the nodes in the analysis.
                    var sourceNodes = sourceNodesByIdentifier
                        .Concat(sourceNodesByNodeCollection)
                        .Distinct()
                        .Intersect(nodes);
                    var targetNodes = targetNodesByIdentifier
                        .Concat(targetNodesByNodeCollection)
                        .Distinct()
                        .Intersect(nodes);
                    // Check if there haven't been any target nodes found.
                    if (targetNodes == null || !targetNodes.Any())
                    {
                        // Update the status of the item.
                        analysis.Status = AnalysisStatus.Error;
                        // Add a message to the log.
                        analysis.Log = analysis.AppendToLog("No target nodes could be found with the provided target data.");
                        // Edit the analysis.
                        IEnumerableExtensions.Edit(analysis.Yield(), context, token);
                        // Continue.
                        continue;
                    }
                    // Get the analysis nodes.
                    var analysisNodes = nodes
                        .Select(item => new AnalysisNode
                        {
                            NodeId = item.Id,
                            Node = item,
                            Type = AnalysisNodeType.None
                        });
                    var sourceAnalysisNodes = sourceNodes
                        .Select(item => new AnalysisNode
                        {
                            NodeId = item.Id,
                            Node = item,
                            Type = AnalysisNodeType.Source
                        });
                    var targetAnalysisNodes = targetNodes
                        .Select(item => new AnalysisNode
                        {
                            NodeId = item.Id,
                            Node = item,
                            Type = AnalysisNodeType.Target
                        });
                    // Get the analysis edges.
                    var analysisEdges = edges
                        .Select(item => new AnalysisEdge
                        {
                            EdgeId = item.Id,
                            Edge = item
                        });
                    // Define the related entities.
                    analysis.AnalysisNodes = analysisNodes
                        .Concat(sourceAnalysisNodes)
                        .Concat(targetAnalysisNodes)
                        .ToList();
                    analysis.AnalysisEdges = analysisEdges
                        .ToList();
                    // Update the status of the item.
                    analysis.Status = AnalysisStatus.Scheduled;
                    // Add a message to the log.
                    analysis.Log = analysis.AppendToLog("The analysis has been successfully generated.");
                    // Remove the generation data.
                    analysis.Data = null;
                    // Edit the analysis.
                    IEnumerableExtensions.Edit(analysis.Yield(), context, token);
                }
            }
        }

        /// <summary>
        /// Starts the items.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void Start(IServiceProvider serviceProvider, CancellationToken token)
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
                // Get the items with the provided IDs.
                var analyses = context.Analyses
                    .Where(item => batchIds.Contains(item.Id));
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding item.
                    var analysis = analyses
                        .FirstOrDefault(item => item.Id == batchItem.Id);
                    // Check if there was no item found.
                    if (analysis == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Check if the status is not valid.
                    if (analysis.Status != AnalysisStatus.Scheduled)
                    {
                        // Update the analysis log.
                        analysis.Log = analysis.AppendToLog("The status of the analysis is not valid in order to be started.");
                        // Update the analysis status.
                        analysis.Status = AnalysisStatus.Error;
                        // Update the start time.
                        analysis.DateTimeStarted = DateTime.Now;
                        // Update the end time.
                        analysis.DateTimeEnded = DateTime.Now;
                        // Edit the analysis.
                        IEnumerableExtensions.Edit(analysis.Yield(), context, token);
                        // Continue.
                        continue;
                    }
                    // Try to run the analysis.
                    try
                    {
                        // Check the algorithm to run the analysis.
                        switch (analysis.Algorithm)
                        {
                            case AnalysisAlgorithm.Algorithm1:
                                // Run the algorithm on the analysis.
                                Algorithms.Algorithm1.Algorithm.Run(analysis, context, token);
                                // End the switch.
                                break;
                            case AnalysisAlgorithm.Algorithm2:
                                // Run the algorithm on the analysis.
                                Algorithms.Algorithm2.Algorithm.Run(analysis, context, token);
                                // End the switch.
                                break;
                            default:
                                // Update the analysis log.
                                analysis.Log = analysis.AppendToLog("The running algorithm is not valid.");
                                // Update the analysis status.
                                analysis.Status = AnalysisStatus.Error;
                                // Update the start time.
                                analysis.DateTimeStarted = DateTime.Now;
                                // Update the end time.
                                analysis.DateTimeEnded = DateTime.Now;
                                // Edit the analysis.
                                IEnumerableExtensions.Edit(analysis.Yield(), context, token);
                                // End the switch.
                                break;
                        }
                    }
                    catch (Exception exception)
                    {
                        // Check if there was no item found.
                        if (analysis == null)
                        {
                            // Continue.
                            continue;
                        }
                        // Get the error message
                        var message = string.IsNullOrEmpty(exception.Message) ? string.Empty : " " + exception.Message;
                        // Update the analysis log.
                        analysis.Log = analysis.AppendToLog("An error occured while running the analysis." + message);
                        // Update the analysis status.
                        analysis.Status = AnalysisStatus.Error;
                        // Update the start time, if needed.
                        analysis.DateTimeStarted = analysis.DateTimeStarted ?? DateTime.Now;
                        // Update the end time.
                        analysis.DateTimeEnded = DateTime.Now;
                        // Edit the analysis.
                        IEnumerableExtensions.Edit(analysis.Yield(), context, token);
                        // Continue.
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// Stops the items.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void Stop(IServiceProvider serviceProvider, CancellationToken token)
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
                // Get the items with the provided IDs.
                var analyses = context.Analyses
                    .Where(item => batchIds.Contains(item.Id));
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding item.
                    var analysis = analyses
                        .FirstOrDefault(item => item.Id == batchItem.Id);
                    // Check if there was no item found.
                    if (analysis == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Update the log.
                    analysis.Log = analysis.AppendToLog("The analysis has been scheduled to stop.");
                    // Update the status.
                    analysis.Status = AnalysisStatus.Stopping;
                    // Update the items.
                    IEnumerableExtensions.Edit(analysis.Yield(), context, token);
                }
            }
        }



        /// <summary>
        /// Sends the e-mails to the corresponding users once the items have ended.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void SendEndedEmails(IServiceProvider serviceProvider, CancellationToken token)
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
                // Use a new e-mail sender instance.
                var emailSender = scope.ServiceProvider.GetRequiredService<ISendGridEmailSender>();
                // Use a new link generator instance.
                var linkGenerator = scope.ServiceProvider.GetRequiredService<LinkGenerator>();
                // Get the items in the current batch.
                var batchItems = Items
                    .Skip(index * ApplicationDbContext.BatchSize)
                    .Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems.Select(item => item.Id);
                // Get the items with the provided IDs.
                var analyses = context.Analyses
                    .Include(item => item.AnalysisUsers)
                        .ThenInclude(item => item.User)
                    .Where(item => batchIds.Contains(item.Id));
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding item.
                    var analysis = analyses
                        .FirstOrDefault(item => item.Id == batchItem.Id);
                    // Check if there was no item found.
                    if (analysis == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Define the HTTP context host.
                    var host = new HostString(HostValue);
                    // Go over each registered user in the analysis.
                    foreach (var user in analysis.AnalysisUsers.Select(item => item.User))
                    {
                        // Send an analysis ending e-mail.
                        Task.Run(() => emailSender.SendAnalysisEndedEmailAsync(new EmailAnalysisEndedViewModel
                        {
                            Email = user.Email,
                            Id = analysis.Id,
                            Name = analysis.Name,
                            Status = analysis.Status.GetDisplayName(),
                            Url = linkGenerator.GetUriByPage("/Content/Created/Analyses/Details/Index", handler: null, values: new { id = analysis.Id }, scheme: Scheme, host: host),
                            ApplicationUrl = linkGenerator.GetUriByPage("/Index", handler: null, values: null, scheme: Scheme, host: host)
                        })).Wait();
                    }
                }
            }
        }
    }
}
