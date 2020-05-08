using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.BackgroundJobs
{
    /// <summary>
    /// Implements a background job to delete users in the database.
    /// </summary>
    public class DeleteUsersBackgroundJob : BaseBackgroundJob
    {
        /// <summary>
        /// Gets or sets the IDs of the items to be deleted.
        /// </summary>
        public IEnumerable<string> Ids { get; set; }

        /// <summary>
        /// Runs the current job.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public override void Run(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if the IDs don't exist.
            if (Ids == null)
            {
                // Throw an exception.
                throw new ArgumentNullException(nameof(Ids));
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)Ids.Count() / _batchSize);
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
                var batchIds = Ids.Skip(index * _batchSize).Take(_batchSize);
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
                    Delete(analyses, context, token);
                    Delete(networks, context, token);
                    Delete(genericEdges, context, token);
                    Delete(genericNodes, context, token);
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
