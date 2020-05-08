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
    /// Implements a background job to delete roles in the database.
    /// </summary>
    public class DeleteRolesBackgroundJob : BaseBackgroundJob
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
                // Use a new role manager instance.
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
                // Get the items with the provided IDs.
                var roles = context.Roles
                    .Where(item => batchIds.Contains(item.Id));
                // Try to delete the items.
                try
                {
                    // Go over each of the item.
                    foreach (var role in roles.ToList())
                    {
                        // Delete it.
                        Task.Run(() => roleManager.DeleteAsync(role)).Wait();
                    }
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
