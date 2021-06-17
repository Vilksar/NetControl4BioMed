using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
                var batchProteinCollectionIds = batchItems
                    .Where(item => item.AnalysisProteinCollections != null)
                    .Select(item => item.AnalysisProteinCollections)
                    .SelectMany(item => item)
                    .Where(item => item.ProteinCollection != null)
                    .Select(item => item.ProteinCollection)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                var batchNetworkIds = batchItems
                    .Where(item => item.Network != null)
                    .Select(item => item.Network)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Define the list of items to get.
                var users = new List<User>();
                var proteinCollections = new List<ProteinCollection>();
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
                    proteinCollections = context.ProteinCollections
                        .Where(item => batchProteinCollectionIds.Contains(item.Id))
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
                            UserId = item,
                            Type = AnalysisUserType.Owner
                        });
                    // Check if there were no analysis users found.
                    if (!batchItem.IsPublic && (analysisUsers == null || !analysisUsers.Any()))
                    {
                        // Throw an exception.
                        throw new TaskException("There were no analysis users found and the analysis is not public.", showExceptionItem, batchItem);
                    }
                    // Check if there is no network provided.
                    if (batchItem.Network == null || string.IsNullOrEmpty(batchItem.Network.Id))
                    {
                        // Throw an exception.
                        throw new TaskException("There was no network provided.", showExceptionItem, batchItem);
                    }
                    // Get the network.
                    var network = networks.FirstOrDefault(item => batchItem.Network.Id == item.Id);
                    // Check if there was no network found.
                    if (network == null || network.Status != NetworkStatus.Completed)
                    {
                        // Throw an exception.
                        throw new TaskException("There was no network found or the network is not yet ready to be used in an analysis.", showExceptionItem, batchItem);
                    }
                    // Get the protein analysis databases.
                    var proteinAnalysisDatabases = network.NetworkDatabases
                            .Where(item => item.Type == NetworkDatabaseType.Protein)
                            .Select(item => item.Database)
                            .Distinct()
                            .Select(item => new AnalysisDatabase
                            {
                                DatabaseId = item.Id,
                                Type = AnalysisDatabaseType.Protein
                            });
                    // Get the interaction analysis databases.
                    var interactionAnalysisDatabases = network.NetworkDatabases
                            .Where(item => item.Type == NetworkDatabaseType.Interaction)
                            .Select(item => item.Database)
                            .Distinct()
                            .Select(item => new AnalysisDatabase
                            {
                                DatabaseId = item.Id,
                                Type = AnalysisDatabaseType.Interaction
                            });
                    // Get the analysis protein collections.
                    var analysisProteinCollections = batchItem.AnalysisProteinCollections != null ?
                        batchItem.AnalysisProteinCollections
                            .Where(item => item.ProteinCollection != null)
                            .Where(item => !string.IsNullOrEmpty(item.ProteinCollection.Id))
                            .Where(item => item.Type == "Source" || item.Type == "Target")
                            .Select(item => (item.ProteinCollection.Id, item.Type))
                            .Distinct()
                            .Where(item => proteinCollections.Any(item1 => item1.Id == item.Item1))
                            .Select(item => new AnalysisProteinCollection
                            {
                                ProteinCollectionId = item.Item1,
                                Type = EnumerationExtensions.GetEnumerationValue<AnalysisProteinCollectionType>(item.Item2)
                            }) :
                        Enumerable.Empty<AnalysisProteinCollection>();
                    // Define the new item.
                    var analysis = new Analysis
                    {
                        DateTimeCreated = DateTime.UtcNow,
                        DateTimeToDelete = DateTime.UtcNow + TimeSpan.FromDays(ApplicationDbContext.DaysBeforeDelete),
                        Name = batchItem.Name,
                        Description = batchItem.Description,
                        IsPublic = batchItem.IsPublic,
                        IsDemonstration = false,
                        Status = AnalysisStatus.Defined,
                        Log = JsonSerializer.Serialize(Enumerable.Empty<string>()),
                        Data = batchItem.Data,
                        MaximumIterations = batchItem.MaximumIterations,
                        MaximumIterationsWithoutImprovement = batchItem.MaximumIterationsWithoutImprovement,
                        Parameters = batchItem.Parameters,
                        Network = network,
                        AnalysisUsers = analysisUsers.ToList(),
                        AnalysisDatabases = proteinAnalysisDatabases
                            .Concat(interactionAnalysisDatabases)
                            .ToList(),
                        AnalysisProteinCollections = analysisProteinCollections.ToList()
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
                        .Include(item => item.AnalysisUsers)
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
                }
                // Get the IDs of the items.
                var analysisIds = analyses
                    .Select(item => item.Id);
                // Delete the dependent entities.
                await AnalysisExtensions.DeleteDependentControlPathsAsync(analysisIds, serviceProvider, token);
                // Delete the related entities.
                await AnalysisExtensions.DeleteRelatedEntitiesAsync<AnalysisProteinCollection>(analysisIds, serviceProvider, token);
                await AnalysisExtensions.DeleteRelatedEntitiesAsync<AnalysisInteraction>(analysisIds, serviceProvider, token);
                await AnalysisExtensions.DeleteRelatedEntitiesAsync<AnalysisProtein>(analysisIds, serviceProvider, token);
                await AnalysisExtensions.DeleteRelatedEntitiesAsync<AnalysisDatabase>(analysisIds, serviceProvider, token);
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
                    if (batchAnalysis.Status != AnalysisStatus.Defined && batchAnalysis.Status != AnalysisStatus.Generating)
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
                        var data = new List<AnalysisProteinInputModel>();
                        var proteinDatabaseIds = new List<string>();
                        var interactionDatabaseIds = new List<string>();
                        var sourceProteinCollectionIds = new List<string>();
                        var targetProteinCollectionIds = new List<string>();
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
                            if (!analysis.Data.TryDeserializeJsonObject<List<AnalysisProteinInputModel>>(out data) || data == null)
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
                            proteinDatabaseIds = context.AnalysisDatabases
                                .Where(item => item.Analysis == analysis)
                                .Where(item => item.Type == AnalysisDatabaseType.Protein)
                                .Select(item => item.Database)
                                .Distinct()
                                .Select(item => item.Id)
                                .ToList();
                            interactionDatabaseIds = context.AnalysisDatabases
                                .Where(item => item.Analysis == analysis)
                                .Where(item => item.Type == AnalysisDatabaseType.Interaction)
                                .Select(item => item.Database)
                                .Distinct()
                                .Select(item => item.Id)
                                .ToList();
                            sourceProteinCollectionIds = context.AnalysisProteinCollections
                                .Where(item => item.Analysis == analysis)
                                .Where(item => item.Type == AnalysisProteinCollectionType.Source)
                                .Select(item => item.ProteinCollection)
                                .Distinct()
                                .Select(item => item.Id)
                                .ToList();
                            targetProteinCollectionIds = context.AnalysisProteinCollections
                                .Where(item => item.Analysis == analysis)
                                .Where(item => item.Type == AnalysisProteinCollectionType.Target)
                                .Select(item => item.ProteinCollection)
                                .Distinct()
                                .Select(item => item.Id)
                                .ToList();
                        }
                        // Get the identifiers of the proteins in the provided data.
                        var sourceProteinIdentifiers = data
                            .Where(item => item.Type == "Source")
                            .Where(item => item.Protein != null)
                            .Select(item => item.Protein)
                            .Where(item => !string.IsNullOrEmpty(item.Id))
                            .Select(item => item.Id)
                            .Distinct();
                        var targetProteinIdentifiers = data
                            .Where(item => item.Type == "Target")
                            .Where(item => item.Protein != null)
                            .Select(item => item.Protein)
                            .Where(item => !string.IsNullOrEmpty(item.Id))
                            .Select(item => item.Id)
                            .Distinct();
                        // Define the required data.
                        var sourceProteinIds = new List<string>();
                        var targetProteinIds = new List<string>();
                        var proteinIds = new List<string>();
                        var interactionIds = new List<string>();
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
                            // Get the available proteins.
                            var availableProteins = context.Proteins
                                .Where(item => item.DatabaseProteins.Any(item1 => proteinDatabaseIds.Contains(item1.Database.Id)));
                            // Get the proteins by identifier.
                            var sourceProteinsByIdentifier = availableProteins
                                .Where(item => sourceProteinIdentifiers.Contains(item.Id) || item.DatabaseProteinFieldProteins.Any(item1 => item1.DatabaseProteinField.IsSearchable && sourceProteinIdentifiers.Contains(item1.Value)));
                            var targetProteinsByIdentifier = availableProteins
                                .Where(item => targetProteinIdentifiers.Contains(item.Id) || item.DatabaseProteinFieldProteins.Any(item1 => item1.DatabaseProteinField.IsSearchable && targetProteinIdentifiers.Contains(item1.Value)));
                            // Get the proteins by protein collection.
                            var sourceProteinsByProteinCollection = availableProteins
                                .Where(item => item.ProteinCollectionProteins.Any(item1 => sourceProteinCollectionIds.Contains(item1.ProteinCollection.Id)));
                            var targetProteinsByProteinCollection = availableProteins
                                .Where(item => item.ProteinCollectionProteins.Any(item1 => targetProteinCollectionIds.Contains(item1.ProteinCollection.Id)));
                            // Get the proteins in the analysis.
                            var proteins = availableProteins
                                .Where(item => item.NetworkProteins.Any(item1 => analysis.NetworkId == item1.Network.Id));
                            // Check if there haven't been any proteins found.
                            if (proteins == null || !proteins.Any())
                            {
                                // Update the status of the item.
                                analysis.Status = AnalysisStatus.Error;
                                // Add a message to the log.
                                analysis.Log = analysis.AppendToLog("No proteins could be found within the provided network.");
                                // Edit the analysis.
                                await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                                // Continue.
                                continue;
                            }
                            // Get the interactions in the analysis.
                            var interactions = context.Interactions
                                .Where(item => item.DatabaseInteractions.Any(item1 => interactionDatabaseIds.Contains(item1.Database.Id)))
                                .Where(item => item.NetworkInteractions.Any(item1 => analysis.NetworkId == item1.Network.Id));
                            // Check if there haven't been any interactions found.
                            if (interactions == null || !interactions.Any())
                            {
                                // Update the status of the item.
                                analysis.Status = AnalysisStatus.Error;
                                // Add a message to the log.
                                analysis.Log = analysis.AppendToLog("No interactions could be found within the provided network.");
                                // Edit the analysis.
                                await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                                // Continue.
                                continue;
                            }
                            // Get the proteins in the analysis.
                            sourceProteinIds = sourceProteinsByIdentifier
                                .Concat(sourceProteinsByProteinCollection)
                                .Distinct()
                                .Intersect(proteins)
                                .Select(item => item.Id)
                                .ToList();
                            targetProteinIds = targetProteinsByIdentifier
                                .Concat(targetProteinsByProteinCollection)
                                .Distinct()
                                .Intersect(proteins)
                                .Select(item => item.Id)
                                .ToList();
                            proteinIds = proteins
                                .Distinct()
                                .Select(item => item.Id)
                                .ToList();
                            interactionIds = interactions
                                .Distinct()
                                .Select(item => item.Id)
                                .ToList();
                            // Check if there haven't been any target proteins found.
                            if (targetProteinIds == null || !targetProteinIds.Any())
                            {
                                // Update the status of the item.
                                analysis.Status = AnalysisStatus.Error;
                                // Add a message to the log.
                                analysis.Log = analysis.AppendToLog("No target proteins could be found with the provided target data.");
                                // Edit the analysis.
                                await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                                // Continue.
                                continue;
                            }
                        }
                        // Define the required items.
                        var analysisProteins = new List<AnalysisProtein>();
                        // Get the total number of batches.
                        var sourceProteinBatchCount = Math.Ceiling((double)sourceProteinIds.Count() / ApplicationDbContext.BatchSize);
                        // Go over each batch.
                        for (var currentBatchIndex = 0; currentBatchIndex < sourceProteinBatchCount; currentBatchIndex++)
                        {
                            // Use a new scope.
                            using (var scope = serviceProvider.CreateScope())
                            {
                                // Use a new context instance.
                                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                                // Get the items in the current batch.
                                var currentBatchItems = sourceProteinIds
                                    .Skip(currentBatchIndex * ApplicationDbContext.BatchSize)
                                    .Take(ApplicationDbContext.BatchSize);
                                // Get the corresponding items.
                                var currentItems = context.Proteins
                                    .Where(item => currentBatchItems.Contains(item.Id))
                                    .Select(item => new AnalysisProtein
                                    {
                                        ProteinId = item.Id,
                                        Type = AnalysisProteinType.Source
                                    });
                                // Add the items to the list.
                                analysisProteins.AddRange(currentItems);
                            }
                        }
                        // Get the total number of batches.
                        var targetProteinBatchCount = Math.Ceiling((double)targetProteinIds.Count() / ApplicationDbContext.BatchSize);
                        // Go over each batch.
                        for (var currentBatchIndex = 0; currentBatchIndex < targetProteinBatchCount; currentBatchIndex++)
                        {
                            // Use a new scope.
                            using (var scope = serviceProvider.CreateScope())
                            {
                                // Use a new context instance.
                                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                                // Get the items in the current batch.
                                var currentBatchItems = targetProteinIds
                                    .Skip(currentBatchIndex * ApplicationDbContext.BatchSize)
                                    .Take(ApplicationDbContext.BatchSize);
                                // Get the corresponding items.
                                var currentItems = context.Proteins
                                    .Where(item => currentBatchItems.Contains(item.Id))
                                    .Select(item => new AnalysisProtein
                                    {
                                        ProteinId = item.Id,
                                        Type = AnalysisProteinType.Target
                                    });
                                // Add the items to the list.
                                analysisProteins.AddRange(currentItems);
                            }
                        }
                        // Get the total number of batches.
                        var proteinBatchCount = Math.Ceiling((double)proteinIds.Count() / ApplicationDbContext.BatchSize);
                        // Go over each batch.
                        for (var currentBatchIndex = 0; currentBatchIndex < proteinBatchCount; currentBatchIndex++)
                        {
                            // Use a new scope.
                            using (var scope = serviceProvider.CreateScope())
                            {
                                // Use a new context instance.
                                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                                // Get the items in the current batch.
                                var currentBatchItems = proteinIds
                                    .Skip(currentBatchIndex * ApplicationDbContext.BatchSize)
                                    .Take(ApplicationDbContext.BatchSize);
                                // Get the corresponding items.
                                var currentItems = context.Proteins
                                    .Where(item => currentBatchItems.Contains(item.Id))
                                    .Select(item => new AnalysisProtein
                                    {
                                        ProteinId = item.Id,
                                        Type = AnalysisProteinType.None
                                    });
                                // Add the items to the list.
                                analysisProteins.AddRange(currentItems);
                            }
                        }
                        // Define the required items.
                        var analysisInteractions = new List<AnalysisInteraction>();
                        // Get the total number of batches.
                        var interactionBatchCount = Math.Ceiling((double)interactionIds.Count() / ApplicationDbContext.BatchSize);
                        // Go over each batch.
                        for (var currentBatchIndex = 0; currentBatchIndex < interactionBatchCount; currentBatchIndex++)
                        {
                            // Use a new scope.
                            using (var scope = serviceProvider.CreateScope())
                            {
                                // Use a new context instance.
                                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                                // Get the items in the current batch.
                                var currentBatchItems = interactionIds
                                    .Skip(currentBatchIndex * ApplicationDbContext.BatchSize)
                                    .Take(ApplicationDbContext.BatchSize);
                                // Get the corresponding items.
                                var currentItems = context.Interactions
                                    .Where(item => currentBatchItems.Contains(item.Id))
                                    .Select(item => new AnalysisInteraction
                                    {
                                        InteractionId = item.Id
                                    });
                                // Add the items to the list.
                                analysisInteractions.AddRange(currentItems);
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
                            analysis.AnalysisProteins = analysisProteins;
                            analysis.AnalysisInteractions = analysisInteractions;
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
                        // Check if an error has been encountered.
                        if (analysis.Status == AnalysisStatus.Error)
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
                    if (batchAnalysis.Status != AnalysisStatus.Scheduled && batchAnalysis.Status != AnalysisStatus.Initializing && batchAnalysis.Status != AnalysisStatus.Ongoing && batchAnalysis.Status != AnalysisStatus.Stopping)
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
                            analysis.Status = AnalysisStatus.Scheduled;
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
                        // Check if an error has been encountered.
                        if (analysis.Status == AnalysisStatus.Error)
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
                            Url = linkGenerator.GetUriByPage($"/AvailableData/Created/Analyses/Details/Index", handler: null, values: new { id = analysis.Id }, scheme: Scheme, host: host),
                            ApplicationUrl = linkGenerator.GetUriByPage("/Index", handler: null, values: null, scheme: Scheme, host: host)
                        });
                    }
                }
            }
        }
    }
}
