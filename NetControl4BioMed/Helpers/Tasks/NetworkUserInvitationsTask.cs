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
    /// Implements a task to update network user invitations in the database.
    /// </summary>
    public class NetworkUserInvitationsTask
    {
        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<NetworkUserInvitationInputModel> Items { get; set; }

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
                var batchIds = Items.Skip(index * ApplicationDbContext.BatchSize).Take(ApplicationDbContext.BatchSize).Select(item => (item.NetworkId, item.Email));
                // Create a new scope.
                using var scope = serviceProvider.CreateScope();
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                var networkUserInvitations = context.NetworkUserInvitations
                    .Where(item => batchIds.Any(item1 => item1.NetworkId == item.Network.Id && item1.Email == item.Email));
                // Try to delete the items.
                try
                {
                    // Delete the items.
                    IQueryableExtensions.Delete(networkUserInvitations, context, token);
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
