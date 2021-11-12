using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
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
    /// Implements a task to update proteins in the database.
    /// </summary>
    public class ProteinsTask
    {
        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<ProteinInputModel> Items { get; set; }

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
                // Get the IDs of the related entities that appear in the current batch.
                var batchDatabaseProteinFieldIds = batchItems
                    .Where(item => item.DatabaseProteinFieldProteins != null)
                    .Select(item => item.DatabaseProteinFieldProteins)
                    .SelectMany(item => item)
                    .Where(item => item.DatabaseProteinField != null)
                    .Select(item => item.DatabaseProteinField)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Define the list of items to get.
                var validBatchIds = new List<string>();
                var databaseProteinFields = new List<DatabaseProteinField>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the related entities that appear in the current batch.
                    databaseProteinFields = context.DatabaseProteinFields
                        .Include(item => item.Database)
                        .Where(item => batchDatabaseProteinFieldIds.Contains(item.Id))
                        .ToList();
                    // Get the valid IDs, that do not appear in the database.
                    validBatchIds = batchIds
                        .Except(context.Proteins
                            .Where(item => batchIds.Contains(item.Id))
                            .Select(item => item.Id))
                        .ToList();
                }
                // Save the items to add.
                var proteinsToAdd = new List<Protein>();
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Check if the ID of the item is not valid.
                    if (!string.IsNullOrEmpty(batchItem.Id) && !validBatchIds.Contains(batchItem.Id))
                    {
                        // Continue.
                        continue;
                    }
                    // Check if there are no database protein field proteins provided.
                    if (batchItem.DatabaseProteinFieldProteins == null || !batchItem.DatabaseProteinFieldProteins.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no database protein field proteins provided.", showExceptionItem, batchItem);
                    }
                    // Get the database protein field proteins.
                    var databaseProteinFieldProteins = batchItem.DatabaseProteinFieldProteins
                        .Where(item => item.DatabaseProteinField != null)
                        .Where(item => !string.IsNullOrEmpty(item.DatabaseProteinField.Id))
                        .Where(item => !string.IsNullOrEmpty(item.Value))
                        .Select(item => (item.DatabaseProteinField.Id, item.Value))
                        .Distinct()
                        .Where(item => databaseProteinFields.Any(item1 => item1.Id == item.Item1))
                        .Select(item => new DatabaseProteinFieldProtein
                        {
                            DatabaseProteinFieldId = item.Item1,
                            Value = item.Item2
                        });
                    // Check if there were no database protein fields found.
                    if (databaseProteinFieldProteins == null)
                    {
                        // Throw an exception.
                        throw new TaskException("There were no database protein fields found.", showExceptionItem, batchItem);
                    }
                    // Check if there were no searchable database protein fields found.
                    var databaseProteinFieldIds = databaseProteinFieldProteins
                        .Select(item => item.DatabaseProteinFieldId)
                        .Distinct();
                    var currentDatabaseProteinFields = databaseProteinFields
                        .Where(item => databaseProteinFieldIds.Contains(item.Id));
                    if (!currentDatabaseProteinFields.Any(item => item.IsSearchable))
                    {
                        // Throw an exception.
                        throw new TaskException("There were no searchable database protein fields found.", showExceptionItem, batchItem);
                    }
                    // Define the new protein.
                    var protein = new Protein
                    {
                        DateTimeCreated = DateTime.UtcNow,
                        Name = !string.IsNullOrEmpty(batchItem.Name) ? batchItem.Name :
                            (databaseProteinFieldProteins
                                .FirstOrDefault(item => item.DatabaseProteinFieldId == currentDatabaseProteinFields
                                    .FirstOrDefault(item => item.IsSearchable)?.Id)?.Value ??
                            "Unnamed protein"),
                        Description = batchItem.Description,
                        DatabaseProteinFieldProteins = databaseProteinFieldProteins.ToList(),
                        DatabaseProteins = currentDatabaseProteinFields
                            .Select(item => item.Database)
                            .Distinct()
                            .Select(item => new DatabaseProtein
                            {
                                DatabaseId = item.Id
                            })
                            .ToList()
                    };
                    // Check if there is any ID provided.
                    if (!string.IsNullOrEmpty(batchItem.Id))
                    {
                        // Assign it to the item.
                        protein.Id = batchItem.Id;
                    }
                    // Add the item to the list.
                    proteinsToAdd.Add(protein);
                }
                // Create the items.
                await IEnumerableExtensions.CreateAsync(proteinsToAdd, serviceProvider, token);
            }
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
                var batchIds = batchItems
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Get the IDs of the related entities that appear in the current batch.
                var batchDatabaseProteinFieldIds = batchItems
                    .Where(item => item.DatabaseProteinFieldProteins != null)
                    .Select(item => item.DatabaseProteinFieldProteins)
                    .SelectMany(item => item)
                    .Where(item => item.DatabaseProteinField != null)
                    .Select(item => item.DatabaseProteinField)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Define the list of items to get.
                var proteins = new List<Protein>();
                var databaseProteinFields = new List<DatabaseProteinField>();
                // Define the dependent list of items to get.
                var analysisInputs = new List<AnalysisInputModel>();
                var networkInputs = new List<NetworkInputModel>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    var items = context.Proteins
                        .Where(item => batchIds.Contains(item.Id));
                    // Check if there were no items found.
                    if (items == null || !items.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Get the items found.
                    proteins = items
                        .ToList();
                    // Get the related entities that appear in the current batch.
                    databaseProteinFields = context.DatabaseProteinFields
                        .Include(item => item.Database)
                        .Where(item => batchDatabaseProteinFieldIds.Contains(item.Id))
                        .ToList();
                }
                // Get the IDs of the items.
                var proteinIds = proteins
                    .Select(item => item.Id);
                // Save the items to edit.
                var proteinsToEdit = new List<Protein>();
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding item.
                    var protein = proteins
                        .FirstOrDefault(item => item.Id == batchItem.Id);
                    // Check if there was no item found.
                    if (protein == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Check if there are no database protein field proteins provided.
                    if (batchItem.DatabaseProteinFieldProteins == null || !batchItem.DatabaseProteinFieldProteins.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no database protein field proteins provided.", showExceptionItem, batchItem);
                    }
                    // Get the database protein field proteins.
                    var databaseProteinFieldProteins = batchItem.DatabaseProteinFieldProteins
                        .Where(item => item.DatabaseProteinField != null)
                        .Where(item => !string.IsNullOrEmpty(item.DatabaseProteinField.Id))
                        .Where(item => !string.IsNullOrEmpty(item.Value))
                        .Select(item => (item.DatabaseProteinField.Id, item.Value))
                        .Distinct()
                        .Where(item => databaseProteinFields.Any(item1 => item1.Id == item.Item1))
                        .Select(item => new DatabaseProteinFieldProtein
                        {
                            DatabaseProteinFieldId = item.Item1,
                            Value = item.Item2
                        });
                    // Check if there were no database protein fields found.
                    if (databaseProteinFieldProteins == null)
                    {
                        // Throw an exception.
                        throw new TaskException("There were no database protein fields found.", showExceptionItem, batchItem);
                    }
                    // Check if there were no searchable database protein fields found.
                    var databaseProteinFieldIds = databaseProteinFieldProteins
                        .Select(item => item.DatabaseProteinFieldId)
                        .Distinct();
                    var currentDatabaseProteinFields = databaseProteinFields
                        .Where(item => databaseProteinFieldIds.Contains(item.Id));
                    if (!currentDatabaseProteinFields.Any(item => item.IsSearchable))
                    {
                        // Throw an exception.
                        throw new TaskException("There were no searchable database protein fields found.", showExceptionItem, batchItem);
                    }
                    // Update the protein.
                    protein.Name = !string.IsNullOrEmpty(batchItem.Name) ? batchItem.Name :
                        (databaseProteinFieldProteins
                            .FirstOrDefault(item => item.DatabaseProteinFieldId == currentDatabaseProteinFields
                                .FirstOrDefault(item => item.IsSearchable)?.Id)?.Value ??
                        "Unnamed protein");
                    protein.Description = batchItem.Description;
                    protein.DatabaseProteinFieldProteins = databaseProteinFieldProteins.ToList();
                    protein.DatabaseProteins = currentDatabaseProteinFields
                        .Select(item => item.Database)
                        .Distinct()
                        .Select(item => new DatabaseProtein
                        {
                            DatabaseId = item.Id
                        })
                        .ToList();
                    // Add the protein to the list.
                    proteinsToEdit.Add(protein);
                }
                // Delete the related entities.
                await ProteinExtensions.DeleteRelatedEntitiesAsync<DatabaseProteinFieldProtein>(proteinIds, serviceProvider, token);
                await ProteinExtensions.DeleteRelatedEntitiesAsync<DatabaseProtein>(proteinIds, serviceProvider, token);
                // Edit the items.
                await IEnumerableExtensions.EditAsync(proteinsToEdit, serviceProvider, token);
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
                // Get the items in the current batch.
                var batchItems = Items
                    .Skip(index * ApplicationDbContext.BatchSize)
                    .Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems.Select(item => item.Id);
                // Define the list of items to get.
                var proteins = new List<Protein>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    var items = context.Proteins
                        .Where(item => batchIds.Contains(item.Id));
                    // Check if there were no items found.
                    if (items == null || !items.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Get the items found.
                    proteins = items
                        .ToList();
                }
                // Get the IDs of the items.
                var proteinIds = proteins
                    .Select(item => item.Id);
                // Delete the dependent entities.
                await ProteinExtensions.DeleteDependentAnalysesAsync(proteinIds, serviceProvider, token);
                await ProteinExtensions.DeleteDependentNetworksAsync(proteinIds, serviceProvider, token);
                await ProteinExtensions.DeleteDependentProteinCollectionsAsync(proteinIds, serviceProvider, token);
                await ProteinExtensions.DeleteDependentInteractionsAsync(proteinIds, serviceProvider, token);
                // Delete the related entities.
                await ProteinExtensions.DeleteRelatedEntitiesAsync<DatabaseProteinFieldProtein>(proteinIds, serviceProvider, token);
                await ProteinExtensions.DeleteRelatedEntitiesAsync<DatabaseProtein>(proteinIds, serviceProvider, token);
                // Delete the items.
                await IEnumerableExtensions.DeleteAsync(proteins, serviceProvider, token);
            }
        }
    }
}
