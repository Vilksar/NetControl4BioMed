﻿using Microsoft.EntityFrameworkCore;
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
    /// Implements a task to update database user invitations in the database.
    /// </summary>
    public class DatabaseUserInvitationsTask
    {
        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<DatabaseUserInvitationInputModel> Items { get; set; }

        /// <summary>
        /// Creates the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns>The created items.</returns>
        public IEnumerable<DatabaseUserInvitation> Create(IServiceProvider serviceProvider, CancellationToken token)
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
                var batchDatabaseIds = batchItems
                    .Where(item => item.Database != null)
                    .Select(item => item.Database)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                var batchUserEmails = batchItems
                    .Where(item => !string.IsNullOrEmpty(item.Email))
                    .Select(item => item.Email)
                    .Distinct();
                // Get the related entities that appear in the current batch.
                var batchDatabases = context.Databases
                    .Where(item => batchDatabaseIds.Contains(item.Id));
                var batchUsers = context.Users
                    .Where(item => batchUserEmails.Contains(item.Email));
                // Save the items to add.
                var databaseUserInvitationsToAdd = new List<DatabaseUserInvitation>();
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Check if there was no database provided.
                    if (batchItem.Database == null || string.IsNullOrEmpty(batchItem.Database.Id))
                    {
                        // Throw an exception.
                        throw new TaskException("There was no database provided.", showExceptionItem, batchItem);
                    }
                    // Get the database.
                    var database = batchDatabases
                        .FirstOrDefault(item => item.Id == batchItem.Database.Id);
                    // Check if there was no database found.
                    if (database == null)
                    {
                        // Throw an exception.
                        throw new TaskException("There was no database found.", showExceptionItem, batchItem);
                    }
                    // Check if there was no e-mail provided.
                    if (string.IsNullOrEmpty(batchItem.Email))
                    {
                        // Throw an exception.
                        throw new TaskException("There was no e-mail provided.", showExceptionItem, batchItem);
                    }
                    // Try to get the user.
                    var user = batchUsers
                        .FirstOrDefault(item => item.Email == batchItem.Email);
                    // Check if there was a user found.
                    if (user != null)
                    {
                        // Throw an exception.
                        throw new TaskException("The user with the provided e-mail already exists.", showExceptionItem, batchItem);
                    }
                    // Define the new item.
                    var databaseUserInvitation = new DatabaseUserInvitation
                    {
                        DateTimeCreated = DateTime.Now,
                        DatabaseId = database.Id,
                        Database = database,
                        Email = batchItem.Email
                    };
                    // Add the item to the list.
                    databaseUserInvitationsToAdd.Add(databaseUserInvitation);
                }
                // Create the items.
                IEnumerableExtensions.Create(databaseUserInvitationsToAdd, context, token);
                // Go over each item.
                foreach (var databaseUserInvitation in databaseUserInvitationsToAdd)
                {
                    // Yield return it.
                    yield return databaseUserInvitation;
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
                var batchIds = batchItems
                    .Where(item => item.Database != null && !string.IsNullOrEmpty(item.Database.Id))
                    .Where(item => !string.IsNullOrEmpty(item.Email))
                    .Select(item => (item.Database.Id, item.Email));
                // Get the items with the provided IDs.
                var databaseUserInvitations = context.DatabaseUserInvitations
                    .Where(item => batchIds.Any(item1 => item1.Item1 == item.Database.Id && item1.Item2 == item.Email));
                // Delete the items.
                IQueryableExtensions.Delete(databaseUserInvitations, context, token);
            }
        }
    }
}
