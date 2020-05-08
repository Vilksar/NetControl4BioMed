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
    /// Implements a background job to delete user roles in the database.
    /// </summary>
    public class DeleteUserRolesBackgroundJob : BaseBackgroundJob
    {
        /// <summary>
        /// Gets or sets the IDs of the items to be deleted.
        /// </summary>
        public IEnumerable<string> UserIds { get; set; }

        /// <summary>
        /// Gets or sets the IDs of the items to be deleted.
        /// </summary>
        public IEnumerable<string> RoleIds { get; set; }

        /// <summary>
        /// Runs the current job.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public override void Run(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if the IDs don't exist.
            if (UserIds == null)
            {
                // Throw an exception.
                throw new ArgumentNullException(nameof(UserIds));
            }
            // Check if the IDs don't exist.
            if (RoleIds == null)
            {
                // Throw an exception.
                throw new ArgumentNullException(nameof(RoleIds));
            }
            // Check if the IDs don't exist.
            if (UserIds.Count() != RoleIds.Count())
            {
                // Throw an exception.
                throw new ArgumentException("There is a mistmatch between the number of user IDs and the number of role IDs.");
            }
            // Get the IDs of all items.
            var ids = UserIds.Zip(RoleIds);
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
                // Use a new user manager instance.
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                // Get the items with the provided IDs.
                var userRoles = context.UserRoles
                    .Where(item => UserIds.Contains(item.User.Id) && RoleIds.Contains(item.Role.Id))
                    .Include(item => item.User)
                    .Include(item => item.Role)
                    .AsEnumerable()
                    .Where(item => ids.Contains((item.User.Id, item.Role.Id)))
                    .AsQueryable();
                // Try to delete the items.
                try
                {
                    // Go over each of the item.
                    foreach (var userRole in userRoles.ToList())
                    {
                        // Delete it.
                        Task.Run(() => userManager.RemoveFromRoleAsync(userRole.User, userRole.Role.Name)).Wait();
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
