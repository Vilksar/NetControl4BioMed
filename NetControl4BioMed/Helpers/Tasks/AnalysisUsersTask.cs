using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Exceptions;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.InputModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Tasks
{
    /// <summary>
    /// Implements a task to update analysis users in the database.
    /// </summary>
    public class AnalysisUsersTask
    {
        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<AnalysisUserInputModel> Items { get; set; }

        /// <summary>
        /// Creates the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task CreateAsync(IServiceProvider serviceProvider, CancellationToken token)
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
                // Get the IDs of the related entities that appear in the current batch.
                var batchAnalysisIds = batchItems
                    .Where(item => item.Analysis != null)
                    .Select(item => item.Analysis)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                var batchUserIds = batchItems
                    .Where(item => item.User != null)
                    .Select(item => item.User)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Get the related entities that appear in the current batch.
                var batchAnalyses = context.Analyses
                    .Where(item => batchAnalysisIds.Contains(item.Id));
                var batchUsers = context.Users
                    .Where(item => batchUserIds.Contains(item.Id));
                // Save the items to add.
                var analysisUsersToAdd = new List<AnalysisUser>();
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Check if there was no analysis provided.
                    if (batchItem.Analysis == null || string.IsNullOrEmpty(batchItem.Analysis.Id))
                    {
                        // Throw an exception.
                        throw new TaskException("There was no analysis provided.", showExceptionItem, batchItem);
                    }
                    // Get the analysis.
                    var analysis = batchAnalyses
                        .FirstOrDefault(item => item.Id == batchItem.Analysis.Id);
                    // Check if there was no analysis found.
                    if (analysis == null)
                    {
                        // Throw an exception.
                        throw new TaskException($"There was no analysis found.", showExceptionItem, batchItem);
                    }
                    // Check if there was no user provided.
                    if (batchItem.User == null || string.IsNullOrEmpty(batchItem.User.Id))
                    {
                        // Throw an exception.
                        throw new TaskException("There was no user provided.", showExceptionItem, batchItem);
                    }
                    // Get the user.
                    var user = batchUsers
                        .FirstOrDefault(item => item.Id == batchItem.User.Id);
                    // Check if there was no user found.
                    if (user == null)
                    {
                        // Throw an exception.
                        throw new TaskException($"There was no user found.", showExceptionItem, batchItem);
                    }
                    // Define the new item.
                    var analysisUser = new AnalysisUser
                    {
                        DateTimeCreated = DateTime.UtcNow,
                        AnalysisId = analysis.Id,
                        Analysis = analysis,
                        UserId = user.Id,
                        User = user
                    };
                    // Add the item to the list.
                    analysisUsersToAdd.Add(analysisUser);
                }
                // Create the items.
                await IEnumerableExtensions.CreateAsync(analysisUsersToAdd, context, token);
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
                    .Where(item => item.Analysis != null && !string.IsNullOrEmpty(item.Analysis.Id))
                    .Where(item => item.User != null && !string.IsNullOrEmpty(item.User.Id))
                    .Select(item => (item.Analysis.Id, item.User.Id));
                // Get the IDs of all individual items.
                var batchIdsAnalysisIds = batchIds
                    .Select(item => item.Item1);
                var batchIdsUserIds = batchIds
                    .Select(item => item.Item2);
                // Get the items with the provided IDs.
                var analysisUsers = context.AnalysisUsers
                    .Include(item => item.Analysis)
                    .Include(item => item.User)
                    .Where(item => batchIdsAnalysisIds.Contains(item.Analysis.Id))
                    .Where(item => batchIdsUserIds.Contains(item.User.Id))
                    .AsEnumerable()
                    .Where(item => batchIds.Any(item1 => item1.Item1 == item.Analysis.Id && item1.Item2 == item.User.Id))
                    .AsQueryable();
                // Delete the items.
                await IQueryableExtensions.DeleteAsync(analysisUsers, context, token);
            }
        }
    }
}
