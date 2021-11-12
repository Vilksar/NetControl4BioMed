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
    /// Implements a task to update interactions in the database.
    /// </summary>
    public class InteractionsTask
    {
        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<InteractionInputModel> Items { get; set; }

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
                var batchDatabaseIds = batchItems
                    .Where(item => item.DatabaseInteractions != null)
                    .Select(item => item.DatabaseInteractions)
                    .SelectMany(item => item)
                    .Where(item => item.Database != null)
                    .Select(item => item.Database)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                var batchDatabaseInteractionFieldIds = batchItems
                    .Where(item => item.DatabaseInteractionFieldInteractions != null)
                    .Select(item => item.DatabaseInteractionFieldInteractions)
                    .SelectMany(item => item)
                    .Where(item => item.DatabaseInteractionField != null)
                    .Select(item => item.DatabaseInteractionField)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                var batchProteinIds = batchItems
                    .Where(item => item.InteractionProteins != null)
                    .Select(item => item.InteractionProteins)
                    .SelectMany(item => item)
                    .Where(item => item.Protein != null)
                    .Select(item => item.Protein)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Define the list of items to get.
                var validBatchIds = new List<string>();
                var databaseInteractionFields = new List<DatabaseInteractionField>();
                var databases = new List<Database>();
                var proteins = new List<Protein>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the related entities that appear in the current batch.
                    databaseInteractionFields = context.DatabaseInteractionFields
                        .Where(item => batchDatabaseInteractionFieldIds.Contains(item.Id))
                        .ToList();
                    databases = context.Databases
                        .Where(item => batchDatabaseIds.Contains(item.Id))
                        .Concat(context.DatabaseInteractionFields
                            .Where(item => batchDatabaseInteractionFieldIds.Contains(item.Id))
                            .Select(item => item.Database))
                        .Distinct()
                        .ToList();
                    proteins = context.Proteins
                        .Where(item => batchProteinIds.Contains(item.Id))
                        .ToList();
                    // Get the valid IDs, that do not appear in the database.
                    validBatchIds = batchIds
                        .Except(context.Interactions
                            .Where(item => batchIds.Contains(item.Id))
                            .Select(item => item.Id))
                        .ToList();
                }
                // Save the items to add.
                var interactionsToAdd = new List<Interaction>();
                // Go over each of the items.
                foreach (var batchItem in batchItems)
                {
                    // Check if the ID of the item is not valid.
                    if (!string.IsNullOrEmpty(batchItem.Id) && !validBatchIds.Contains(batchItem.Id))
                    {
                        // Continue.
                        continue;
                    }
                    // Check if there were no interaction proteins provided.
                    if (batchItem.InteractionProteins == null || !batchItem.InteractionProteins.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no interaction proteins provided.", showExceptionItem, batchItem);
                    }
                    // Get the interaction proteins.
                    var interactionProteins = batchItem.InteractionProteins
                        .Where(item => item.Protein != null)
                        .Where(item => !string.IsNullOrEmpty(item.Protein.Id))
                        .Where(item => item.Type == "Source" || item.Type == "Target")
                        .Select(item => (item.Protein.Id, item.Type))
                        .Distinct()
                        .Where(item => proteins.Any(item1 => item1.Id == item.Item1))
                        .Select(item => new InteractionProtein
                        {
                            ProteinId = item.Item1,
                            Type = EnumerationExtensions.GetEnumerationValue<InteractionProteinType>(item.Item2)
                        });
                    // Check if there were no interaction proteins found.
                    if (interactionProteins == null || !interactionProteins.Any(item => item.Type == InteractionProteinType.Source) || !interactionProteins.Any(item => item.Type == InteractionProteinType.Target))
                    {
                        // Throw an exception.
                        throw new TaskException("There were no interaction proteins found.", showExceptionItem, batchItem);
                    }
                    // Check if there were no database interactions or database interaction field interactions provided.
                    if ((batchItem.DatabaseInteractions == null || !batchItem.DatabaseInteractions.Any()) && (batchItem.DatabaseInteractionFieldInteractions == null || !batchItem.DatabaseInteractionFieldInteractions.Any()))
                    {
                        // Throw an exception.
                        throw new TaskException("There were no database interactions or database interaction field interactions provided.", showExceptionItem, batchItem);
                    }
                    // Get the database interaction field interactions.
                    var databaseInteractionFieldInteractions = batchItem.DatabaseInteractionFieldInteractions != null ?
                        batchItem.DatabaseInteractionFieldInteractions
                            .Where(item => item.DatabaseInteractionField != null)
                            .Where(item => !string.IsNullOrEmpty(item.DatabaseInteractionField.Id))
                            .Where(item => !string.IsNullOrEmpty(item.Value))
                            .Select(item => (item.DatabaseInteractionField.Id, item.Value))
                            .Distinct()
                            .Where(item => databaseInteractionFields.Any(item1 => item1.Id == item.Item1))
                            .Select(item => new DatabaseInteractionFieldInteraction
                            {
                                DatabaseInteractionFieldId = item.Item1,
                                Value = item.Item2
                            }) :
                        Enumerable.Empty<DatabaseInteractionFieldInteraction>();
                    // Get the database interactions.
                    var databaseInteractionFieldIds = databaseInteractionFieldInteractions
                        .Select(item => item.DatabaseInteractionFieldId)
                        .Distinct();
                    var currentDatabaseInteractionFields = databaseInteractionFields
                        .Where(item => databaseInteractionFieldIds.Contains(item.Id));
                    var databaseInteractions = batchItem.DatabaseInteractions != null ?
                        batchItem.DatabaseInteractions
                            .Where(item => item.Database != null)
                            .Where(item => !string.IsNullOrEmpty(item.Database.Id))
                            .Select(item => item.Database.Id)
                            .Concat(currentDatabaseInteractionFields
                                .Select(item => item.Database.Id))
                            .Distinct()
                            .Where(item => databases.Any(item1 => item1.Id == item))
                            .Select(item => new DatabaseInteraction
                            {
                                DatabaseId = item,
                            }) :
                        Enumerable.Empty<DatabaseInteraction>();
                    // Check if there were no database interactions found.
                    if (databaseInteractions == null || !databaseInteractions.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no database interactions found.", showExceptionItem, batchItem);
                    }
                    // Define the new interaction.
                    var interaction = new Interaction
                    {
                        DateTimeCreated = DateTime.UtcNow,
                        Name = string.Concat(proteins.First(item => item.Id == interactionProteins.First(item => item.Type == InteractionProteinType.Source).ProteinId).Name, " - ", proteins.First(item => item.Id == interactionProteins.First(item => item.Type == InteractionProteinType.Target).ProteinId).Name),
                        Description = batchItem.Description,
                        InteractionProteins = new List<InteractionProtein>
                        {
                            interactionProteins.First(item => item.Type == InteractionProteinType.Source),
                            interactionProteins.First(item => item.Type == InteractionProteinType.Target)
                        },
                        DatabaseInteractionFieldInteractions = databaseInteractionFieldInteractions.ToList(),
                        DatabaseInteractions = databaseInteractions.ToList()
                    };
                    // Check if there is any ID provided.
                    if (!string.IsNullOrEmpty(batchItem.Id))
                    {
                        // Assign it to the protein.
                        interaction.Id = batchItem.Id;
                    }
                    // Add the new protein to the list.
                    interactionsToAdd.Add(interaction);
                }
                // Create the items.
                await IEnumerableExtensions.CreateAsync(interactionsToAdd, serviceProvider, token);
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
                    .Select(item => item.Id);
                // Get the IDs of the related entities that appear in the current batch.
                var batchDatabaseIds = batchItems
                    .Where(item => item.DatabaseInteractions != null)
                    .Select(item => item.DatabaseInteractions)
                    .SelectMany(item => item)
                    .Where(item => item.Database != null)
                    .Select(item => item.Database)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                var batchDatabaseInteractionFieldIds = batchItems
                    .Where(item => item.DatabaseInteractionFieldInteractions != null)
                    .Select(item => item.DatabaseInteractionFieldInteractions)
                    .SelectMany(item => item)
                    .Where(item => item.DatabaseInteractionField != null)
                    .Select(item => item.DatabaseInteractionField)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                var batchProteinIds = batchItems
                    .Where(item => item.InteractionProteins != null)
                    .Select(item => item.InteractionProteins)
                    .SelectMany(item => item)
                    .Where(item => item.Protein != null)
                    .Select(item => item.Protein)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Define the list of items to get.
                var interactions = new List<Interaction>();
                var databases = new List<Database>();
                var databaseInteractionFields = new List<DatabaseInteractionField>();
                var proteins = new List<Protein>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    var items = context.Interactions
                        .Where(item => batchIds.Contains(item.Id));
                    // Check if there were no items found.
                    if (items == null || !items.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Get the items found.
                    interactions = items
                        .ToList();
                    // Get the related entities that appear in the current batch.
                    databaseInteractionFields = context.DatabaseInteractionFields
                        .Where(item => batchDatabaseInteractionFieldIds.Contains(item.Id))
                        .ToList();
                    databases = context.Databases
                        .Where(item => batchDatabaseIds.Contains(item.Id))
                        .Concat(context.DatabaseInteractionFields
                            .Where(item => batchDatabaseInteractionFieldIds.Contains(item.Id))
                            .Select(item => item.Database))
                        .Distinct()
                        .ToList();
                    proteins = context.Proteins
                        .Where(item => batchProteinIds.Contains(item.Id))
                        .ToList();
                }
                // Get the IDs of the items.
                var interactionIds = interactions
                    .Select(item => item.Id);
                // Save the items to edit.
                var interactionsToEdit = new List<Interaction>();
                // Go over each of the valid items.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding item.
                    var interaction = interactions
                        .FirstOrDefault(item => batchItem.Id == item.Id);
                    // Check if there was no item found.
                    if (interaction == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Check if there were no interaction proteins provided.
                    if (batchItem.InteractionProteins == null || !batchItem.InteractionProteins.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no interaction proteins provided.", showExceptionItem, batchItem);
                    }
                    // Get the interaction proteins.
                    var interactionProteins = batchItem.InteractionProteins
                        .Where(item => item.Protein != null)
                        .Where(item => !string.IsNullOrEmpty(item.Protein.Id))
                        .Where(item => item.Type == "Source" || item.Type == "Target")
                        .Select(item => (item.Protein.Id, item.Type))
                        .Distinct()
                        .Where(item => proteins.Any(item1 => item1.Id == item.Item1))
                        .Select(item => new InteractionProtein
                        {
                            ProteinId = item.Item1,
                            Type = EnumerationExtensions.GetEnumerationValue<InteractionProteinType>(item.Item2)
                        });
                    // Check if there were no interaction proteins found.
                    if (interactionProteins == null || !interactionProteins.Any(item => item.Type == InteractionProteinType.Source) || !interactionProteins.Any(item => item.Type == InteractionProteinType.Target))
                    {
                        // Throw an exception.
                        throw new TaskException("There were no interaction proteins found.", showExceptionItem, batchItem);
                    }
                    // Check if there were no database interactions or database interaction field interactions provided.
                    if ((batchItem.DatabaseInteractions == null || !batchItem.DatabaseInteractions.Any()) && (batchItem.DatabaseInteractionFieldInteractions == null || !batchItem.DatabaseInteractionFieldInteractions.Any()))
                    {
                        // Throw an exception.
                        throw new TaskException("There were no database interactions or database interaction field interactions provided.", showExceptionItem, batchItem);
                    }
                    // Get the database interaction field interactions.
                    var databaseInteractionFieldInteractions = batchItem.DatabaseInteractionFieldInteractions != null ?
                        batchItem.DatabaseInteractionFieldInteractions
                            .Where(item => item.DatabaseInteractionField != null)
                            .Where(item => !string.IsNullOrEmpty(item.DatabaseInteractionField.Id))
                            .Where(item => !string.IsNullOrEmpty(item.Value))
                            .Select(item => (item.DatabaseInteractionField.Id, item.Value))
                            .Distinct()
                            .Where(item => databaseInteractionFields.Any(item1 => item1.Id == item.Item1))
                            .Select(item => new DatabaseInteractionFieldInteraction
                            {
                                DatabaseInteractionFieldId = item.Item1,
                                Value = item.Item2
                            }) :
                        Enumerable.Empty<DatabaseInteractionFieldInteraction>();
                    // Get the database interactions.
                    var databaseInteractionFieldIds = databaseInteractionFieldInteractions
                        .Select(item => item.DatabaseInteractionFieldId)
                        .Distinct();
                    var currentDatabaseInteractionFields = databaseInteractionFields
                        .Where(item => databaseInteractionFieldIds.Contains(item.Id));
                    var databaseInteractions = batchItem.DatabaseInteractions != null ?
                        batchItem.DatabaseInteractions
                            .Where(item => item.Database != null)
                            .Where(item => !string.IsNullOrEmpty(item.Database.Id))
                            .Select(item => item.Database.Id)
                            .Concat(currentDatabaseInteractionFields
                                .Select(item => item.Database.Id))
                            .Distinct()
                            .Where(item => databases.Any(item1 => item1.Id == item))
                            .Select(item => new DatabaseInteraction
                            {
                                DatabaseId = item,
                            }) :
                        Enumerable.Empty<DatabaseInteraction>();
                    // Check if there were no database interactions found.
                    if (databaseInteractions == null || !databaseInteractions.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no database interactions found.", showExceptionItem, batchItem);
                    }
                    // Update the interaction.
                    interaction.Name = string.Concat(proteins.First(item => item.Id == interactionProteins.First(item => item.Type == InteractionProteinType.Source).ProteinId).Name, " - ", proteins.First(item => item.Id == interactionProteins.First(item => item.Type == InteractionProteinType.Target).ProteinId).Name);
                    interaction.Description = batchItem.Description;
                    interaction.InteractionProteins = new List<InteractionProtein>
                    {
                        interactionProteins.First(item => item.Type == InteractionProteinType.Source),
                        interactionProteins.First(item => item.Type == InteractionProteinType.Target)
                    };
                    interaction.DatabaseInteractionFieldInteractions = databaseInteractionFieldInteractions.ToList();
                    interaction.DatabaseInteractions = databaseInteractions.ToList();
                    // Add the interaction to the list.
                    interactionsToEdit.Add(interaction);
                }
                // Delete the related entities.
                await InteractionExtensions.DeleteRelatedEntitiesAsync<InteractionProtein>(interactionIds, serviceProvider, token);
                await InteractionExtensions.DeleteRelatedEntitiesAsync<DatabaseInteractionFieldInteraction>(interactionIds, serviceProvider, token);
                await InteractionExtensions.DeleteRelatedEntitiesAsync<DatabaseInteraction>(interactionIds, serviceProvider, token);
                // Update the items.
                await IEnumerableExtensions.EditAsync(interactionsToEdit, serviceProvider, token);
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
                var interactions = new List<Interaction>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    var items = context.Interactions
                        .Where(item => batchIds.Contains(item.Id));
                    // Check if there were no items found.
                    if (items == null || !items.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Get the items found.
                    interactions = items
                        .ToList();
                }
                // Get the IDs of the items.
                var interactionIds = interactions
                    .Select(item => item.Id);
                // Delete the dependent entities.
                await InteractionExtensions.DeleteDependentAnalysesAsync(interactionIds, serviceProvider, token);
                await InteractionExtensions.DeleteDependentNetworksAsync(interactionIds, serviceProvider, token);
                // Delete the related entities.
                await InteractionExtensions.DeleteRelatedEntitiesAsync<InteractionProtein>(interactionIds, serviceProvider, token);
                await InteractionExtensions.DeleteRelatedEntitiesAsync<DatabaseInteractionFieldInteraction>(interactionIds, serviceProvider, token);
                await InteractionExtensions.DeleteRelatedEntitiesAsync<DatabaseInteraction>(interactionIds, serviceProvider, token);
                // Delete the items.
                await IEnumerableExtensions.DeleteAsync(interactions, serviceProvider, token);
            }
        }
    }
}
