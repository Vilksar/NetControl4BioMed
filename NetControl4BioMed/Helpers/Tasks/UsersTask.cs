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
        public void Create(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Throw an exception.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Edits the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void Edit(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Throw an exception.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes the items from the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void Delete(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if there weren't any valid items found.
            if (Items == null || !Items.Any())
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
                // Get the IDs of the items in the current batch.
                var batchIds = Items.Skip(index * ApplicationDbContext.BatchSize).Take(ApplicationDbContext.BatchSize).Select(item => item.Id);
                // Create a new scope.
                using var scope = serviceProvider.CreateScope();
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Use a new user manager instance.
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                // Get the items with the provided IDs.
                var users = context.Users
                    .Where(item => batchIds.Contains(item.Id));
                // Try to delete the items.
                try
                {
                    // Go over each of the item.
                    foreach (var user in users.ToList())
                    {
                        // Delete it.
                        Task.Run(() => userManager.DeleteAsync(user)).Wait();
                    }
                }
                catch (Exception exception)
                {
                    // Throw an exception.
                    throw exception;
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
                // Try to delete the items.
                try
                {
                    // Delete the items.
                    IQueryableExtensions.Delete(analyses, context, token);
                    IQueryableExtensions.Delete(networks, context, token);
                    IQueryableExtensions.Delete(genericEdges, context, token);
                    IQueryableExtensions.Delete(genericNodes, context, token);
                }
                catch (Exception exception)
                {
                    // Throw an exception.
                    throw exception;
                }
            }
        }
    }
}
