using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.InputModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Tasks
{
    /// <summary>
    /// Implements a task to update users in the database.
    /// </summary>
    public class UsersTask
    {
        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<UserInputModel> Items { get; set; }

        /// <summary>
        /// Creates the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns>The created items.</returns>
        public IEnumerable<User> Create(IServiceProvider serviceProvider, CancellationToken token)
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
                // Use a new user manager instance.
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
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
                    throw new ArgumentException("Two or more of the manually provided IDs are duplicated.");
                }
                // Get the valid IDs, that do not appear in the database.
                var validBatchIds = batchIds
                    .Except(context.DatabaseTypes
                        .Where(item => batchIds.Contains(item.Id))
                        .Select(item => item.Id));
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Check if the ID of the item is not valid.
                    if (!string.IsNullOrEmpty(batchItem.Id) && !validBatchIds.Contains(batchItem.Id))
                    {
                        // Continue.
                        continue;
                    }
                    // Define the corresponding item.
                    var user = new User
                    {
                        UserName = batchItem.Email,
                        Email = batchItem.Email,
                        DateTimeCreated = DateTime.Now
                    };
                    // Check if there is any ID provided.
                    if (!string.IsNullOrEmpty(batchItem.Id))
                    {
                        // Assign it to the item.
                        user.Id = batchItem.Id;
                    }
                    // Define a new identity result.
                    var result = IdentityResult.Success;
                    // Check the type of the item.
                    if (batchItem.Type == "Password")
                    {
                        // Try to get the passsord from the data.
                        if (!batchItem.Data.TryDeserializeJsonObject<string>(out var password))
                        {
                            // Throw an exception.
                            throw new ArgumentException("The provided data couldn't be deserialized.");
                        }
                        // Try to create the new user.
                        result = result.Succeeded? Task.Run(() => userManager.CreateAsync(user, password)).Result : result;
                    }
                    else if (batchItem.Type == "External")
                    {
                        // Try to get the passsord from the data.
                        if (!batchItem.Data.TryDeserializeJsonObject<ExternalLoginInfo>(out var info))
                        {
                            // Throw an exception.
                            throw new ArgumentException("The provided data couldn't be deserialized.");
                        }
                        // Try to create the new user.
                        result = result.Succeeded ? Task.Run(() => userManager.CreateAsync(user)).Result : result;
                        // Add the external login.
                        result = result.Succeeded ? Task.Run(() => userManager.AddLoginAsync(user, info)).Result : result;
                    }
                    else
                    {
                        // Throw an exception.
                        throw new ArgumentException("The provided data type is invalid.");
                    }
                    // Check if the e-mail should be set as confirmed.
                    if (batchItem.EmailConfirmed)
                    {
                        // Generate the token for e-mail confirmation.
                        var confirmationToken = Task.Run(() => userManager.GenerateEmailConfirmationTokenAsync(user)).Result;
                        // Confirm the e-mail address.
                        result = result.Succeeded ? Task.Run(() => userManager.ConfirmEmailAsync(user, confirmationToken)).Result : result;
                    }
                    // Check if any of the operations has failed.
                    if (!result.Succeeded)
                    {
                        // Define the exception message.
                        var message = string.Empty;
                        // Go over each of the encountered errors.
                        foreach (var error in result.Errors)
                        {
                            // Add the error to the message.
                            message += error.Description;
                        }
                        // Throw an exception.
                        throw new DbUpdateException(message);
                    }
                    // Get all the databases, networks and analyses to which the user already has access.
                    var databaseUserInvitations = context.DatabaseUserInvitations
                        .Where(item => item.Email == user.Email);
                    var networkUserInvitations = context.NetworkUserInvitations
                        .Where(item => item.Email == user.Email);
                    var analysisUserInvitations = context.AnalysisUserInvitations
                        .Where(item => item.Email == user.Email);
                    // Create, for each, a corresponding user entry.
                    var databaseUsers = databaseUserInvitations.Select(item => new DatabaseUser
                    {
                        DatabaseId = item.DatabaseId,
                        Database = item.Database,
                        UserId = user.Id,
                        User = user,
                        DateTimeCreated = item.DateTimeCreated
                    });
                    var networkUsers = networkUserInvitations.Select(item => new NetworkUser
                    {
                        NetworkId = item.NetworkId,
                        Network = item.Network,
                        UserId = user.Id,
                        User = user,
                        DateTimeCreated = item.DateTimeCreated
                    });
                    var analysisUsers = analysisUserInvitations.Select(item => new AnalysisUser
                    {
                        AnalysisId = item.AnalysisId,
                        Analysis = item.Analysis,
                        UserId = user.Id,
                        User = user,
                        DateTimeCreated = item.DateTimeCreated
                    });
                    // Create the items.
                    IEnumerableExtensions.Create(databaseUsers, context, token);
                    IEnumerableExtensions.Create(networkUsers, context, token);
                    IEnumerableExtensions.Create(analysisUsers, context, token);
                    // Delete the items
                    IQueryableExtensions.Delete(databaseUserInvitations, context, token);
                    IQueryableExtensions.Delete(networkUserInvitations, context, token);
                    IQueryableExtensions.Delete(analysisUserInvitations, context, token);
                    // Yield return the item.
                    yield return user;
                }
            }
        }

        /// <summary>
        /// Edits the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns>The edited items.</returns>
        public IEnumerable<User> Edit(IServiceProvider serviceProvider, CancellationToken token)
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
                // Use a new user manager instance.
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                // Get the items in the current batch.
                var batchItems = Items
                    .Skip(index * ApplicationDbContext.BatchSize)
                    .Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Get the items corresponding to the current batch.
                var users = context.Users
                    .Where(item => batchIds.Contains(item.Id));
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding item.
                    var user = users
                        .FirstOrDefault(item => item.Id == batchItem.Id);
                    // Check if there was no item found.
                    if (user == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Define a new identity result.
                    var result = IdentityResult.Success;
                    // Check if the e-mail is different from the current one.
                    if (batchItem.Email != user.Email)
                    {
                        // Try to update the username.
                        result = Task.Run(() => userManager.SetUserNameAsync(user, batchItem.Email)).Result;
                        // Try to update the e-mail.
                        result = result.Succeeded ? Task.Run(() => userManager.SetEmailAsync(user, batchItem.Email)).Result : result;
                    }
                    // Check if the e-mail should be set as confirmed.
                    if (!user.EmailConfirmed && batchItem.EmailConfirmed)
                    {
                        // Generate the token and try to set it.
                        var confirmationToken = Task.Run(() => userManager.GenerateEmailConfirmationTokenAsync(user)).Result;
                        result = result.Succeeded ? Task.Run(() => userManager.ConfirmEmailAsync(user, confirmationToken)).Result : result;
                    }
                    // Check if any of the operations has failed.
                    if (!result.Succeeded)
                    {
                        // Define the exception message.
                        var message = string.Empty;
                        // Go over each of the encountered errors.
                        foreach (var error in result.Errors)
                        {
                            // Add the error to the message.
                            message += error.Description;
                        }
                        // Throw an exception.
                        throw new DbUpdateException(message);
                    }
                    // Yield return the item.
                    yield return user;
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
                // Use a new user manager instance.
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                // Get the items in the current batch.
                var batchItems = Items
                    .Skip(index * ApplicationDbContext.BatchSize)
                    .Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems.Select(item => item.Id);
                // Get the items with the provided IDs.
                var users = context.Users
                    .Where(item => batchIds.Contains(item.Id));
                // Go over each item.
                foreach (var user in users.ToList())
                {
                    // Delete it.
                    var result = Task.Run(() => userManager.DeleteAsync(user)).Result;
                    // Check if the operation has failed.
                    if (!result.Succeeded)
                    {
                        // Define the exception message.
                        var message = string.Empty;
                        // Go over each of the encountered errors.
                        foreach (var error in result.Errors)
                        {
                            // Add the error to the message.
                            message += error.Description;
                        }
                        // Throw an exception.
                        throw new DbUpdateException(message);
                    }
                }
                // Get the related entities that use the items.
                var networks = context.Networks
                    .Where(item => !item.NetworkUsers.Any());
                var analyses = context.Analyses
                    .Where(item => !item.AnalysisUsers.Any() || item.AnalysisNetworks.Any(item1 => networks.Contains(item1.Network)));
                // Get the generic entities among them.
                var genericNetworks = networks
                    .Where(item => item.NetworkDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"));
                var genericNodes = context.Nodes
                    .Where(item => item.NetworkNodes.Any(item1 => genericNetworks.Contains(item1.Network)));
                var genericEdges = context.Edges
                    .Where(item => item.NetworkEdges.Any(item1 => genericNetworks.Contains(item1.Network)) || item.EdgeNodes.Any(item1 => genericNodes.Contains(item1.Node)));
                // Delete the items.
                IQueryableExtensions.Delete(analyses, context, token);
                IQueryableExtensions.Delete(networks, context, token);
                IQueryableExtensions.Delete(genericEdges, context, token);
                IQueryableExtensions.Delete(genericNodes, context, token);
            }
        }
    }
}
