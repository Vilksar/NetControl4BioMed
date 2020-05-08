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
    /// Implements a background job to delete database user invitations in the database.
    /// </summary>
    public class DeleteDatabaseUserInvitationsBackgroundJob : BaseBackgroundJob
    {
        /// <summary>
        /// Gets or sets the IDs of the items to be deleted.
        /// </summary>
        public IEnumerable<string> DatabaseIds { get; set; }

        /// <summary>
        /// Gets or sets the e-mails to be deleted.
        /// </summary>
        public IEnumerable<string> Emails { get; set; }

        /// <summary>
        /// Runs the current job.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public override void Run(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if the IDs don't exist.
            if (DatabaseIds == null)
            {
                // Throw an exception.
                throw new ArgumentNullException(nameof(DatabaseIds));
            }
            // Check if the e-mails don't exist.
            if (Emails == null)
            {
                // Throw an exception.
                throw new ArgumentNullException(nameof(Emails));
            }
            // Check if the IDs don't exist.
            if (DatabaseIds.Count() != Emails.Count())
            {
                // Throw an exception.
                throw new ArgumentException("There is a mistmatch between the number of database IDs and the number of user e-mails.");
            }
            // Get the IDs of all items.
            var ids = DatabaseIds.Zip(Emails);
            // Get the total number of batches.
            var count = Math.Ceiling((double)ids.Count() / _batchSize);
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
                var batchIds = ids.Skip(index * _batchSize).Take(_batchSize);
                // Create a new scope.
                using var scope = serviceProvider.CreateScope();
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                var databaseUserInvitations = context.DatabaseUserInvitations
                    .Where(item => DatabaseIds.Contains(item.Database.Id) && Emails.Contains(item.Email))
                    .Include(item => item.Database)
                    .AsEnumerable()
                    .Where(item => ids.Contains((item.Database.Id, item.Email)))
                    .AsQueryable();
                // Try to delete the items.
                try
                {
                    // Delete the items.
                    Delete(databaseUserInvitations, context, token);
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
