using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Interfaces;
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
        /// Gets the maximum number of retries for a task.
        /// </summary>
        private static int NumberOfRetries { get; } = 2;

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
        public async Task<IEnumerable<string>> CreateAsync(IServiceProvider serviceProvider, CancellationToken token)
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
            // Save the IDs of the created items.
            var ids = Enumerable.Empty<string>();
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
                // Define the list of items to get.
                var users = new List<User>();
                var nodeCollections = new List<NodeCollection>();
                var networks = new List<Network>();
                var validBatchIds = new List<string>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the related entities that appear in the current batch.
                    users = context.Users
                        .Where(item => batchUserIds.Contains(item.Id))
                        .ToList();
                    nodeCollections = context.NodeCollections
                        .Include(item => item.NodeCollectionDatabases)
                            .ThenInclude(item => item.Database)
                        .Where(item => batchNodeCollectionIds.Contains(item.Id))
                        .ToList();
                    networks = context.Networks
                        .Include(item => item.NetworkDatabases)
                            .ThenInclude(item => item.Database)
                        .Where(item => batchNetworkIds.Contains(item.Id))
                        .ToList();
                    // Get the valid IDs, that do not appear in the database.
                    validBatchIds = batchIds
                        .Except(context.Analyses
                            .Where(item => batchIds.Contains(item.Id))
                            .Select(item => item.Id))
                        .ToList();
                }
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
                    if (!batchItem.IsPublic && (batchItem.AnalysisUsers == null || !batchItem.AnalysisUsers.Any()))
                    {
                        // Throw an exception.
                        throw new TaskException("There were no analysis users provided and the analysis is not public.", showExceptionItem, batchItem);
                    }
                    // Get the analysis users.
                    var analysisUsers = batchItem.AnalysisUsers
                        .Where(item => item.User != null)
                        .Where(item => !string.IsNullOrEmpty(item.User.Id))
                        .Select(item => item.User.Id)
                        .Distinct()
                        .Where(item => users.Any(item1 => item1.Id == item))
                        .Select(item => new AnalysisUser
                        {
                            DateTimeCreated = DateTime.UtcNow,
                            UserId = item
                        });
                    // Check if there were no analysis users found.
                    if (!batchItem.IsPublic && (analysisUsers == null || !analysisUsers.Any()))
                    {
                        // Throw an exception.
                        throw new TaskException("There were no analysis users found and the analysis is not public.", showExceptionItem, batchItem);
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
                        .Where(item => networks.Any(item1 => item1.Id == item))
                        .Select(item => new AnalysisNetwork
                        {
                            NetworkId = item
                        });
                    // Check if there were no analysis networks found.
                    if (analysisNetworks == null || !analysisNetworks.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no analysis networks found.", showExceptionItem, batchItem);
                    }
                    // Get the IDs of the used networks.
                    var networkIds = analysisNetworks
                        .Select(item => item.NetworkId);
                    // Get the node analysis databases.
                    var nodeAnalysisDatabases = networks
                            .Where(item => networkIds.Contains(item.Id))
                            .Select(item => item.NetworkDatabases)
                            .SelectMany(item => item)
                            .Where(item => item.Type == NetworkDatabaseType.Node)
                            .Select(item => item.Database)
                            .Distinct()
                            .Select(item => new AnalysisDatabase
                            {
                                DatabaseId = item.Id,
                                Type = AnalysisDatabaseType.Node
                            });
                    // Check if there were no node analysis databases found.
                    if (nodeAnalysisDatabases == null || !nodeAnalysisDatabases.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no node analysis databases found.", showExceptionItem, batchItem);
                    }
                    // Get the edge analysis databases.
                    var edgeAnalysisDatabases = networks
                            .Where(item => networkIds.Contains(item.Id))
                            .Select(item => item.NetworkDatabases)
                            .SelectMany(item => item)
                            .Where(item => item.Type == NetworkDatabaseType.Edge)
                            .Select(item => item.Database)
                            .Distinct()
                            .Select(item => new AnalysisDatabase
                            {
                                DatabaseId = item.Id,
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
                            .Where(item => nodeCollections.Any(item1 => item1.Id == item.Item1 && item1.NodeCollectionDatabases.Any(item2 => nodeDatabases.Contains(item2.Database))))
                            .Select(item => new AnalysisNodeCollection
                            {
                                NodeCollectionId = item.Item1,
                                Type = EnumerationExtensions.GetEnumerationValue<AnalysisNodeCollectionType>(item.Item2)
                            }) :
                        Enumerable.Empty<AnalysisNodeCollection>();
                    // Define the new item.
                    var analysis = new Analysis
                    {
                        DateTimeCreated = DateTime.UtcNow,
                        Name = batchItem.Name,
                        Description = batchItem.Description,
                        IsPublic = batchItem.IsPublic,
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
                        // Throw a new exception.
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
                // Create the items.
                await IEnumerableExtensions.CreateAsync(analysesToAdd, serviceProvider, token);
                // Add the IDs of the created items to the list.
                ids = ids.Concat(analysesToAdd.Select(item => item.Id));
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Define the new background task.
                    var backgroundTask = new BackgroundTask
                    {
                        DateTimeCreated = DateTime.UtcNow,
                        Name = $"{nameof(IContentTaskManager)}.{nameof(IContentTaskManager.GenerateAnalysesAsync)}",
                        IsRecurring = false,
                        Data = JsonSerializer.Serialize(new AnalysesTask
                        {
                            Scheme = Scheme,
                            HostValue = HostValue,
                            Items = analysesToAdd.Select(item => new AnalysisInputModel
                            {
                                Id = item.Id
                            })
                        })
                    };
                    // Mark the task for addition.
                    context.BackgroundTasks.Add(backgroundTask);
                    // Save the changes to the database.
                    await context.SaveChangesAsync();
                    // Create a new Hangfire background job.
                    BackgroundJob.Enqueue<IContentTaskManager>(item => item.GenerateAnalysesAsync(backgroundTask.Id, CancellationToken.None));
                }
            }
            // Return the IDs of the created items.
            return ids;
        }

        /// <summary>
        /// Edits the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task EditAsync(IServiceProvider serviceProvider, CancellationToken token)
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
                // Get the items in the current batch.
                var batchItems = Items
                    .Skip(index * ApplicationDbContext.BatchSize)
                    .Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems.Select(item => item.Id);
                // Define the list of items to get.
                var analyses = new List<Analysis>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    var items = context.Analyses
                        .Where(item => batchIds.Contains(item.Id));
                    // Check if there were no items found.
                    if (items == null || !items.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Get the items found.
                    analyses = items
                        .ToList();
                }
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
                    // Check if there were no analysis users found.
                    if (!batchItem.IsPublic && (analysis.AnalysisUsers == null || !analysis.AnalysisUsers.Any()))
                    {
                        // Throw an exception.
                        throw new TaskException("There were no analysis users found, so the analysis must be public.", showExceptionItem, batchItem);
                    }
                    // Update the data.
                    analysis.Name = batchItem.Name;
                    analysis.Description = batchItem.Description;
                    analysis.IsPublic = batchItem.IsPublic;
                    // Append a message to the log.
                    analysis.Log = analysis.AppendToLog("The analysis details have been updated.");
                    // Add the item to the list.
                    analysesToEdit.Add(analysis);
                }
                // Edit the items.
                await IEnumerableExtensions.EditAsync(analysesToEdit, serviceProvider, token);
            }
        }

        /// <summary>
        /// Deletes the items from the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteAsync(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if there weren't any valid items found.
            if (Items == null)
            {
                // Throw an exception.
                throw new TaskException("No valid items could be found with the provided data.");
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)Items.Count() / 1);
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
                var batchItems = Items
                    .Skip(index * ApplicationDbContext.BatchSize)
                    .Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems.Select(item => item.Id);
                // Define the list of items to get.
                var analyses = new List<Analysis>();
                // Define the dependent list of items to get.
                var controlPathInputs = new List<ControlPathInputModel>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    var items = context.Analyses
                        .Where(item => batchIds.Contains(item.Id));
                    // Check if there were no items found.
                    if (items == null || !items.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Get the IDs of the items found.
                    analyses = items
                        .ToList();
                    // Get the IDs of the dependent items.
                    controlPathInputs = items
                        .Select(item => item.ControlPaths)
                        .SelectMany(item => item)
                        .Distinct()
                        .Select(item => new ControlPathInputModel
                        {
                            Id = item.Id
                        })
                        .ToList();
                }
                // Get the IDs of the items.
                var analysisIds = analyses
                    .Select(item => item.Id);
                // Delete the dependent entities.
                await new ControlPathsTask { Items = controlPathInputs }.DeleteAsync(serviceProvider, token);
                // Delete the related entities.
                await AnalysisExtensions.DeleteRelatedEntitiesAsync<AnalysisNetwork>(analysisIds, serviceProvider, token);
                await AnalysisExtensions.DeleteRelatedEntitiesAsync<AnalysisNodeCollection>(analysisIds, serviceProvider, token);
                await AnalysisExtensions.DeleteRelatedEntitiesAsync<AnalysisEdge>(analysisIds, serviceProvider, token);
                await AnalysisExtensions.DeleteRelatedEntitiesAsync<AnalysisNode>(analysisIds, serviceProvider, token);
                await AnalysisExtensions.DeleteRelatedEntitiesAsync<AnalysisDatabase>(analysisIds, serviceProvider, token);
                await AnalysisExtensions.DeleteRelatedEntitiesAsync<AnalysisUserInvitation>(analysisIds, serviceProvider, token);
                await AnalysisExtensions.DeleteRelatedEntitiesAsync<AnalysisUser>(analysisIds, serviceProvider, token);
                // Delete the items.
                await IEnumerableExtensions.DeleteAsync(analyses, serviceProvider, token);
            }
        }

        /// <summary>
        /// Generates the items.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task GenerateAsync(IServiceProvider serviceProvider, CancellationToken token)
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
                // Get the items in the current batch.
                var batchItems = Items
                    .Skip(index * ApplicationDbContext.BatchSize)
                    .Take(ApplicationDbContext.BatchSize)
                    .ToList();
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems.Select(item => item.Id);
                // Define the batch items.
                var batchAnalyses = new List<Analysis>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    batchAnalyses = context.Analyses
                        .Where(item => batchIds.Contains(item.Id))
                        .ToList();
                }
                // Define the current retry.
                var currentRetry = 0;
                // Go over each item in the current batch.
                for (var batchItemIndex = 0; batchItemIndex < batchItems.Count(); batchItemIndex++)
                {
                    // Get the corresponding batch item.
                    var batchItem = batchItems[batchItemIndex];
                    // Get the corresponding item.
                    var batchAnalysis = batchAnalyses
                        .FirstOrDefault(item => item.Id == batchItem.Id);
                    // Check if there was no item found.
                    if (batchAnalysis == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Check if the status is not valid.
                    if (batchAnalysis.Status != AnalysisStatus.Defined)
                    {
                        // Use a new scope.
                        using (var scope = serviceProvider.CreateScope())
                        {
                            // Use a new context instance.
                            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            // Reload the analysis.
                            var analysis = context.Analyses
                                .FirstOrDefault(item => item.Id == batchAnalysis.Id);
                            // Check if there was no item found.
                            if (analysis == null)
                            {
                                // Continue.
                                continue;
                            }
                            // Update the status of the item.
                            analysis.Status = AnalysisStatus.Error;
                            // Add a message to the log.
                            analysis.Log = analysis.AppendToLog("The status of the analysis is not valid in order to be generated.");
                            // Edit the network.
                            await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                        }
                        // Continue.
                        continue;
                    }
                    // Try to generate the analysis.
                    try
                    {
                        // Use a new scope.
                        using (var scope = serviceProvider.CreateScope())
                        {
                            // Use a new context instance.
                            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            // Reload the analysis.
                            var analysis = context.Analyses
                                .FirstOrDefault(item => item.Id == batchAnalysis.Id);
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
                            await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                        }
                        // Define the required data.
                        var data = new List<AnalysisNodeInputModel>();
                        var nodeDatabaseIds = new List<string>();
                        var edgeDatabaseIds = new List<string>();
                        var sourceNodeCollectionIds = new List<string>();
                        var targetNodeCollectionIds = new List<string>();
                        var networkIds = new List<string>();
                        // Use a new scope.
                        using (var scope = serviceProvider.CreateScope())
                        {
                            // Use a new context instance.
                            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            // Reload the analysis.
                            var analysis = context.Analyses
                                .FirstOrDefault(item => item.Id == batchAnalysis.Id);
                            // Check if there was no item found.
                            if (analysis == null)
                            {
                                // Continue.
                                continue;
                            }
                            // Try to deserialize the data.
                            if (!analysis.Data.TryDeserializeJsonObject<List<AnalysisNodeInputModel>>(out data) || data == null)
                            {
                                // Update the status of the item.
                                analysis.Status = AnalysisStatus.Error;
                                // Add a message to the log.
                                analysis.Log = analysis.AppendToLog("The source and / or target data corresponding to the analysis could not be deserialized.");
                                // Edit the analysis.
                                await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                                // Continue.
                                continue;
                            }
                            // Get the IDs of the required related data.
                            nodeDatabaseIds = context.AnalysisDatabases
                                .Where(item => item.Analysis == analysis)
                                .Where(item => item.Type == AnalysisDatabaseType.Node)
                                .Select(item => item.Database)
                                .Distinct()
                                .Select(item => item.Id)
                                .ToList();
                            edgeDatabaseIds = context.AnalysisDatabases
                                .Where(item => item.Analysis == analysis)
                                .Where(item => item.Type == AnalysisDatabaseType.Edge)
                                .Select(item => item.Database)
                                .Distinct()
                                .Select(item => item.Id)
                                .ToList();
                            sourceNodeCollectionIds = context.AnalysisNodeCollections
                                .Where(item => item.Analysis == analysis)
                                .Where(item => item.Type == AnalysisNodeCollectionType.Source)
                                .Select(item => item.NodeCollection)
                                .Distinct()
                                .Select(item => item.Id)
                                .ToList();
                            targetNodeCollectionIds = context.AnalysisNodeCollections
                                .Where(item => item.Analysis == analysis)
                                .Where(item => item.Type == AnalysisNodeCollectionType.Target)
                                .Select(item => item.NodeCollection)
                                .Distinct()
                                .Select(item => item.Id)
                                .ToList();
                            networkIds = context.AnalysisNetworks
                                .Select(item => item.Network)
                                .Distinct()
                                .Select(item => item.Id)
                                .ToList();
                        }
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
                        // Define the required data.
                        var sourceNodeIds = new List<string>();
                        var targetNodeIds = new List<string>();
                        var nodeIds = new List<string>();
                        var edgeIds = new List<string>();
                        // Use a new scope.
                        using (var scope = serviceProvider.CreateScope())
                        {
                            // Use a new context instance.
                            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            // Reload the analysis.
                            var analysis = context.Analyses
                                .FirstOrDefault(item => item.Id == batchAnalysis.Id);
                            // Check if there was no item found.
                            if (analysis == null)
                            {
                                // Continue.
                                continue;
                            }
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
                                await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                                // Continue.
                                continue;
                            }
                            // Get the edges in the analysis.
                            var edges = context.Edges
                                .Where(item => item.DatabaseEdges.Any(item1 => edgeDatabaseIds.Contains(item1.Database.Id)))
                                .Where(item => item.NetworkEdges.Any(item1 => networkIds.Contains(item1.Network.Id)));
                            // Check if there haven't been any edges found.
                            if (edges == null || !edges.Any())
                            {
                                // Update the status of the item.
                                analysis.Status = AnalysisStatus.Error;
                                // Add a message to the log.
                                analysis.Log = analysis.AppendToLog("No edges could be found within the provided networks.");
                                // Edit the analysis.
                                await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                                // Continue.
                                continue;
                            }
                            // Get the nodes in the analysis.
                            sourceNodeIds = sourceNodesByIdentifier
                                .Concat(sourceNodesByNodeCollection)
                                .Distinct()
                                .Intersect(nodes)
                                .Select(item => item.Id)
                                .ToList();
                            targetNodeIds = targetNodesByIdentifier
                                .Concat(targetNodesByNodeCollection)
                                .Distinct()
                                .Intersect(nodes)
                                .Select(item => item.Id)
                                .ToList();
                            nodeIds = nodes
                                .Distinct()
                                .Select(item => item.Id)
                                .ToList();
                            edgeIds = edges
                                .Distinct()
                                .Select(item => item.Id)
                                .ToList();
                            // Check if there haven't been any target nodes found.
                            if (targetNodeIds == null || !targetNodeIds.Any())
                            {
                                // Update the status of the item.
                                analysis.Status = AnalysisStatus.Error;
                                // Add a message to the log.
                                analysis.Log = analysis.AppendToLog("No target nodes could be found with the provided target data.");
                                // Edit the analysis.
                                await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                                // Continue.
                                continue;
                            }
                        }
                        // Define the required items.
                        var analysisNodes = new List<AnalysisNode>();
                        // Get the total number of batches.
                        var sourceNodeBatchCount = Math.Ceiling((double)sourceNodeIds.Count() / ApplicationDbContext.BatchSize);
                        // Go over each batch.
                        for (var currentBatchIndex = 0; currentBatchIndex < sourceNodeBatchCount; currentBatchIndex++)
                        {
                            // Use a new scope.
                            using (var scope = serviceProvider.CreateScope())
                            {
                                // Use a new context instance.
                                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                                // Get the items in the current batch.
                                var currentBatchItems = sourceNodeIds
                                    .Skip(currentBatchIndex * ApplicationDbContext.BatchSize)
                                    .Take(ApplicationDbContext.BatchSize);
                                // Get the corresponding items.
                                var currentItems = context.Nodes
                                    .Where(item => currentBatchItems.Contains(item.Id))
                                    .Select(item => new AnalysisNode
                                    {
                                        NodeId = item.Id,
                                        Type = AnalysisNodeType.Source
                                    });
                                // Add the items to the list.
                                analysisNodes.AddRange(currentItems);
                            }
                        }
                        // Get the total number of batches.
                        var targetNodeBatchCount = Math.Ceiling((double)targetNodeIds.Count() / ApplicationDbContext.BatchSize);
                        // Go over each batch.
                        for (var currentBatchIndex = 0; currentBatchIndex < targetNodeBatchCount; currentBatchIndex++)
                        {
                            // Use a new scope.
                            using (var scope = serviceProvider.CreateScope())
                            {
                                // Use a new context instance.
                                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                                // Get the items in the current batch.
                                var currentBatchItems = targetNodeIds
                                    .Skip(currentBatchIndex * ApplicationDbContext.BatchSize)
                                    .Take(ApplicationDbContext.BatchSize);
                                // Get the corresponding items.
                                var currentItems = context.Nodes
                                    .Where(item => currentBatchItems.Contains(item.Id))
                                    .Select(item => new AnalysisNode
                                    {
                                        NodeId = item.Id,
                                        Type = AnalysisNodeType.Target
                                    });
                                // Add the items to the list.
                                analysisNodes.AddRange(currentItems);
                            }
                        }
                        // Get the total number of batches.
                        var nodeBatchCount = Math.Ceiling((double)nodeIds.Count() / ApplicationDbContext.BatchSize);
                        // Go over each batch.
                        for (var currentBatchIndex = 0; currentBatchIndex < nodeBatchCount; currentBatchIndex++)
                        {
                            // Use a new scope.
                            using (var scope = serviceProvider.CreateScope())
                            {
                                // Use a new context instance.
                                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                                // Get the items in the current batch.
                                var currentBatchItems = nodeIds
                                    .Skip(currentBatchIndex * ApplicationDbContext.BatchSize)
                                    .Take(ApplicationDbContext.BatchSize);
                                // Get the corresponding items.
                                var currentItems = context.Nodes
                                    .Where(item => currentBatchItems.Contains(item.Id))
                                    .Select(item => new AnalysisNode
                                    {
                                        NodeId = item.Id,
                                        Type = AnalysisNodeType.None
                                    });
                                // Add the items to the list.
                                analysisNodes.AddRange(currentItems);
                            }
                        }
                        // Define the required items.
                        var analysisEdges = new List<AnalysisEdge>();
                        // Get the total number of batches.
                        var edgeBatchCount = Math.Ceiling((double)edgeIds.Count() / ApplicationDbContext.BatchSize);
                        // Go over each batch.
                        for (var currentBatchIndex = 0; currentBatchIndex < edgeBatchCount; currentBatchIndex++)
                        {
                            // Use a new scope.
                            using (var scope = serviceProvider.CreateScope())
                            {
                                // Use a new context instance.
                                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                                // Get the items in the current batch.
                                var currentBatchItems = edgeIds
                                    .Skip(currentBatchIndex * ApplicationDbContext.BatchSize)
                                    .Take(ApplicationDbContext.BatchSize);
                                // Get the corresponding items.
                                var currentItems = context.Edges
                                    .Where(item => currentBatchItems.Contains(item.Id))
                                    .Select(item => new AnalysisEdge
                                    {
                                        EdgeId = item.Id
                                    });
                                // Add the items to the list.
                                analysisEdges.AddRange(currentItems);
                            }
                        }
                        // Use a new scope.
                        using (var scope = serviceProvider.CreateScope())
                        {
                            // Use a new context instance.
                            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            // Reload the analysis.
                            var analysis = context.Analyses
                                .FirstOrDefault(item => item.Id == batchAnalysis.Id);
                            // Check if there was no item found.
                            if (analysis == null)
                            {
                                // Continue.
                                continue;
                            }
                            // Define the related entities.
                            analysis.AnalysisNodes = analysisNodes;
                            analysis.AnalysisEdges = analysisEdges;
                            analysis.ControlPaths = new List<ControlPath>();
                            // Edit the network.
                            await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                        }
                    }
                    catch (Exception exception)
                    {
                        // Use a new scope.
                        using (var scope = serviceProvider.CreateScope())
                        {
                            // Use a new context instance.
                            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            // Reload the analysis.
                            var analysis = context.Analyses
                                .FirstOrDefault(item => item.Id == batchAnalysis.Id);
                            // Check if there was no item found.
                            if (analysis == null)
                            {
                                // Continue.
                                continue;
                            }
                            // Update the status of the item.
                            analysis.Status = AnalysisStatus.Defined;
                            // Add a message to the log.
                            analysis.Log = analysis.AppendToLog($"The try number {currentRetry + 1} ended with an error ({NumberOfRetries - currentRetry} tr{(NumberOfRetries - currentRetry != 1 ? "ies" : "y")} remaining). {(string.IsNullOrEmpty(exception.Message) ? "There was no error message returned." : exception.Message)}");
                            // Edit the analysis.
                            await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                        }
                        // Check if the task should be executed again.
                        if (currentRetry < NumberOfRetries)
                        {
                            // Increase the current retry.
                            currentRetry += 1;
                            // Repeat the loop for the current batch item.
                            batchItemIndex += -1;
                            // Continue.
                            continue;
                        }
                        // Use a new scope.
                        using (var scope = serviceProvider.CreateScope())
                        {
                            // Use a new context instance.
                            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            // Reload the analysis.
                            var analysis = context.Analyses
                                .FirstOrDefault(item => item.Id == batchAnalysis.Id);
                            // Check if there was no item found.
                            if (analysis == null)
                            {
                                // Continue.
                                continue;
                            }
                            // Update the analysis status.
                            analysis.Status = AnalysisStatus.Error;
                            // Update the analysis log.
                            analysis.Log = analysis.AppendToLog("One or more errors occured while generating the analysis.");
                            // Edit the analysis.
                            await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                        }
                        // Continue.
                        continue;
                    }
                    // Reset the current retry.
                    currentRetry = 0;
                    // Use a new scope.
                    using (var scope = serviceProvider.CreateScope())
                    {
                        // Use a new context instance.
                        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        // Reload the analysis.
                        var analysis = context.Analyses
                            .FirstOrDefault(item => item.Id == batchAnalysis.Id);
                        // Check if there was no item found.
                        if (analysis == null)
                        {
                            // Continue.
                            continue;
                        }
                        // Update the status of the item.
                        analysis.Status = AnalysisStatus.Scheduled;
                        // Add a message to the log.
                        analysis.Log = analysis.AppendToLog("The analysis has been successfully generated.");
                        // Remove the generation data.
                        analysis.Data = null;
                        // Edit the analysis.
                        await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                    }
                    // Use a new scope.
                    using (var scope = serviceProvider.CreateScope())
                    {
                        // Use a new context instance.
                        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        // Define the new background task.
                        var backgroundTask = new BackgroundTask
                        {
                            DateTimeCreated = DateTime.UtcNow,
                            Name = $"{nameof(IContentTaskManager)}.{nameof(IContentTaskManager.StartAnalysesAsync)}",
                            IsRecurring = false,
                            Data = JsonSerializer.Serialize(new AnalysesTask
                            {
                                Scheme = Scheme,
                                HostValue = HostValue,
                                Items = batchAnalysis.Yield().Select(item => new AnalysisInputModel
                                {
                                    Id = item.Id
                                })
                            })
                        };
                        // Mark the task for addition.
                        context.BackgroundTasks.Add(backgroundTask);
                        // Save the changes to the database.
                        await context.SaveChangesAsync();
                        // Create a new Hangfire background job.
                        BackgroundJob.Enqueue<IContentTaskManager>(item => item.StartAnalysesAsync(backgroundTask.Id, CancellationToken.None));
                    }
                }
            }
        }

        /// <summary>
        /// Starts the items.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task StartAsync(IServiceProvider serviceProvider, CancellationToken token)
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
                // Get the items in the current batch.
                var batchItems = Items
                    .Skip(index * ApplicationDbContext.BatchSize)
                    .Take(ApplicationDbContext.BatchSize)
                    .ToList();
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems.Select(item => item.Id);
                // Define the batch items.
                var batchAnalyses = new List<Analysis>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    batchAnalyses = context.Analyses
                        .Where(item => batchIds.Contains(item.Id))
                        .ToList();
                }
                // Define the current retry.
                var currentRetry = 0;
                // Go over each item in the current batch.
                for (var batchItemIndex = 0; batchItemIndex < batchItems.Count(); batchItemIndex++)
                {
                    // Get the corresponding batch item.
                    var batchItem = batchItems[batchItemIndex];
                    // Get the corresponding item.
                    var batchAnalysis = batchAnalyses
                        .FirstOrDefault(item => item.Id == batchItem.Id);
                    // Check if there was no item found.
                    if (batchAnalysis == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Check if the status is not valid.
                    if (batchAnalysis.Status != AnalysisStatus.Scheduled)
                    {
                        // Use a new scope.
                        using (var scope = serviceProvider.CreateScope())
                        {
                            // Use a new context instance.
                            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            // Reload the analysis.
                            var analysis = context.Analyses
                                .FirstOrDefault(item => item.Id == batchAnalysis.Id);
                            // Check if there was no item found.
                            if (analysis == null)
                            {
                                // Continue.
                                continue;
                            }
                            // Update the status of the item.
                            analysis.Status = AnalysisStatus.Error;
                            // Add a message to the log.
                            analysis.Log = analysis.AppendToLog("The status of the analysis is not valid in order to be started.");
                            // Update the start time.
                            analysis.DateTimeStarted = DateTime.UtcNow;
                            // Update the end time.
                            analysis.DateTimeEnded = DateTime.UtcNow;
                            // Edit the network.
                            await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                        }
                        // Continue.
                        continue;
                    }
                    // Try to run the analysis.
                    try
                    {
                        // Use a new scope.
                        using (var scope = serviceProvider.CreateScope())
                        {
                            // Use a new context instance.
                            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            // Reload the analysis.
                            var analysis = context.Analyses
                                .FirstOrDefault(item => item.Id == batchAnalysis.Id);
                            // Check if there was no item found.
                            if (analysis == null)
                            {
                                // Continue.
                                continue;
                            }
                            // Update the status of the item.
                            analysis.Status = AnalysisStatus.Initializing;
                            // Add a message to the log.
                            analysis.Log = analysis.AppendToLog("The analysis is now initializing.");
                            // Update the start time of the item.
                            analysis.DateTimeStarted = DateTime.UtcNow;
                            // Edit the network.
                            await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                        }
                        // Check the algorithm to run the analysis.
                        switch (batchAnalysis.Algorithm)
                        {
                            case AnalysisAlgorithm.Greedy:
                                // Run the algorithm on the analysis.
                                await Algorithms.Analyses.Greedy.Algorithm.Run(batchAnalysis.Id, serviceProvider, token);
                                // End the switch.
                                break;
                            case AnalysisAlgorithm.Genetic:
                                // Run the algorithm on the analysis.
                                await Algorithms.Analyses.Genetic.Algorithm.Run(batchAnalysis.Id, serviceProvider, token);
                                // End the switch.
                                break;
                            default:
                                // Use a new scope.
                                using (var scope = serviceProvider.CreateScope())
                                {
                                    // Use a new context instance.
                                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                                    // Reload the analysis.
                                    var analysis = context.Analyses
                                        .FirstOrDefault(item => item.Id == batchAnalysis.Id);
                                    // Check if there was no item found.
                                    if (analysis == null)
                                    {
                                        // Continue.
                                        continue;
                                    }
                                    // Update the analysis status.
                                    analysis.Status = AnalysisStatus.Error;
                                    // Update the analysis log.
                                    analysis.Log = analysis.AppendToLog("The analysis algorithm is not valid.");
                                    // Update the start time.
                                    analysis.DateTimeStarted = DateTime.UtcNow;
                                    // Update the end time.
                                    analysis.DateTimeEnded = DateTime.UtcNow;
                                    // Edit the analysis.
                                    await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                                }
                                // End the switch.
                                break;
                        }
                    }
                    catch (Exception exception)
                    {
                        // Use a new scope.
                        using (var scope = serviceProvider.CreateScope())
                        {
                            // Use a new context instance.
                            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            // Reload the analysis.
                            var analysis = context.Analyses
                                .FirstOrDefault(item => item.Id == batchAnalysis.Id);
                            // Check if there was no item found.
                            if (analysis == null)
                            {
                                // Continue.
                                continue;
                            }
                            // Update the status of the item.
                            analysis.Status = AnalysisStatus.Defined;
                            // Add a message to the log.
                            analysis.Log = analysis.AppendToLog($"The try number {currentRetry + 1} ended with an error ({NumberOfRetries - currentRetry} tr{(NumberOfRetries - currentRetry != 1 ? "ies" : "y")} remaining). {(string.IsNullOrEmpty(exception.Message) ? "There was no error message returned." : exception.Message)}");
                            // Edit the analysis.
                            await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                        }
                        // Check if the task should be executed again.
                        if (currentRetry < NumberOfRetries)
                        {
                            // Increase the current retry.
                            currentRetry += 1;
                            // Repeat the loop for the current batch item.
                            batchItemIndex += -1;
                            // Continue.
                            continue;
                        }
                        // Use a new scope.
                        using (var scope = serviceProvider.CreateScope())
                        {
                            // Use a new context instance.
                            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            // Reload the analysis.
                            var analysis = context.Analyses
                                .FirstOrDefault(item => item.Id == batchAnalysis.Id);
                            // Check if there was no item found.
                            if (analysis == null)
                            {
                                // Continue.
                                continue;
                            }
                            // Update the analysis status.
                            analysis.Status = AnalysisStatus.Error;
                            // Update the analysis log.
                            analysis.Log = analysis.AppendToLog("One or more errors occured while generating the analysis.");
                            // Update the start time, if needed.
                            analysis.DateTimeStarted = analysis.DateTimeStarted ?? DateTime.UtcNow;
                            // Update the end time.
                            analysis.DateTimeEnded = DateTime.UtcNow;
                            // Edit the analysis.
                            await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                        }
                    }
                    // Reset the current retry.
                    currentRetry = 0;
                    // Use a new scope.
                    using (var scope = serviceProvider.CreateScope())
                    {
                        // Use a new context instance.
                        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        // Reload the analysis.
                        var analysis = context.Analyses
                            .FirstOrDefault(item => item.Id == batchAnalysis.Id);
                        // Check if there was no item found.
                        if (analysis == null)
                        {
                            // Continue.
                            continue;
                        }
                        // Update the status of the item.
                        analysis.Status = analysis.CurrentIteration < analysis.MaximumIterations && analysis.CurrentIterationWithoutImprovement < analysis.MaximumIterationsWithoutImprovement ? AnalysisStatus.Stopped : AnalysisStatus.Completed;
                        // Update the end time of the item.
                        analysis.DateTimeEnded = DateTime.UtcNow;
                        // Add a message to the log.
                        analysis.Log = analysis.AppendToLog($"The analysis has ended with the status \"{analysis.Status.GetDisplayName()}\".");
                        // Edit the analysis.
                        await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                    }
                    // Use a new scope.
                    using (var scope = serviceProvider.CreateScope())
                    {
                        // Use a new context instance.
                        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        // Define the new background task.
                        var backgroundTask = new BackgroundTask
                        {
                            DateTimeCreated = DateTime.UtcNow,
                            Name = $"{nameof(IContentTaskManager)}.{nameof(IContentTaskManager.SendAnalysesEndedEmailsAsync)}",
                            IsRecurring = false,
                            Data = JsonSerializer.Serialize(new AnalysesTask
                            {
                                Scheme = Scheme,
                                HostValue = HostValue,
                                Items = batchAnalysis.Yield().Select(item => new AnalysisInputModel
                                {
                                    Id = item.Id
                                })
                            })
                        };
                        // Mark the task for addition.
                        context.BackgroundTasks.Add(backgroundTask);
                        // Save the changes to the database.
                        await context.SaveChangesAsync();
                        // Create a new Hangfire background job.
                        BackgroundJob.Enqueue<IContentTaskManager>(item => item.SendAnalysesEndedEmailsAsync(backgroundTask.Id, CancellationToken.None));
                    }
                }
            }
        }

        /// <summary>
        /// Stops the items.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task StopAsync(IServiceProvider serviceProvider, CancellationToken token)
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
                    // Update the status.
                    analysis.Status = AnalysisStatus.Stopping;
                    // Update the log.
                    analysis.Log = analysis.AppendToLog("The analysis has been scheduled to stop.");
                    // Update the items.
                    await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                }
            }
        }

        /// <summary>
        /// Sends the e-mails to the corresponding users once the items have ended.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task SendEndedEmailsAsync(IServiceProvider serviceProvider, CancellationToken token)
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
                // Use a new user manager instance.
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
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
                        await emailSender.SendAnalysisEndedEmailAsync(new EmailAnalysisEndedViewModel
                        {
                            Email = user.Email,
                            Id = analysis.Id,
                            Name = analysis.Name,
                            Status = analysis.Status.GetDisplayName(),
                            Url = linkGenerator.GetUriByPage("/Content/Created/Analyses/Details/Index", handler: null, values: new { id = analysis.Id }, scheme: Scheme, host: host),
                            ApplicationUrl = linkGenerator.GetUriByPage("/Index", handler: null, values: null, scheme: Scheme, host: host)
                        });
                    }
                }
            }
        }
    }
}
