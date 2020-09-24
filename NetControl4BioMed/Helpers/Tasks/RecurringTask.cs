using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Tasks
{
    /// <summary>
    /// Implements a recurring task database.
    /// </summary>
    public class RecurringTask
    {
        /// <summary>
        /// Gets or sets the HTTP context scheme.
        /// </summary>
        public string Scheme { get; set; }

        /// <summary>
        /// Gets or sets the HTTP context host value.
        /// </summary>
        public string HostValue { get; set; }

        /// <summary>
        /// Stops the long-running analyses.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task StopAnalysesAsync(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Create a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Define the limit date.
            var limitDate = DateTime.Today - TimeSpan.FromDays(ApplicationDbContext.DaysBeforeStop);
            // Get the items to stop.
            var items = context.Analyses
                .Where(item => item.Status == AnalysisStatus.Initializing || item.Status == AnalysisStatus.Ongoing)
                .Where(item => item.DateTimeStarted < limitDate);
            // Define a new task.
            var task = new AnalysesTask
            {
                Items = items
                    .Select(item => new AnalysisInputModel
                    {
                        Id = item.Id
                    })
            };
            // Run the task.
            await task.StopAsync(serviceProvider, token);
        }

        /// <summary>
        /// Alerts on deleting the long-standing items.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task AlertUsersAsync(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Create a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Use a new e-mail sender instance.
            var emailSender = scope.ServiceProvider.GetRequiredService<ISendGridEmailSender>();
            // Use a new link generator instance.
            var linkGenerator = scope.ServiceProvider.GetRequiredService<LinkGenerator>();
            // Define the host.
            var host = new HostString(HostValue);
            // Define the limit dates.
            var limitDateStart = DateTime.Today - TimeSpan.FromDays(ApplicationDbContext.DaysBeforeAlert + 1);
            var limitDateEnd = DateTime.Today - TimeSpan.FromDays(ApplicationDbContext.DaysBeforeAlert);
            // Get the networks and analyses.
            var networks = context.Networks
                .Where(item => limitDateStart < item.DateTimeCreated && item.DateTimeCreated < limitDateEnd);
            var analyses = context.Analyses
                .Where(item => item.Status == AnalysisStatus.Stopped || item.Status == AnalysisStatus.Completed || item.Status == AnalysisStatus.Error)
                .Where(item => limitDateStart < item.DateTimeEnded && item.DateTimeEnded < limitDateEnd)
                .Concat(networks
                    .Select(item => item.AnalysisNetworks)
                    .SelectMany(item => item)
                    .Select(item => item.Analysis))
                .Distinct();
            // Get the users.
            var networkUsers = networks
                .Select(item => item.NetworkUsers)
                .SelectMany(item => item)
                .Select(item => item.User);
            var analysisUsers = analyses
                .Select(item => item.AnalysisUsers)
                .SelectMany(item => item)
                .Select(item => item.User);
            // Get the users that have access to the items.
            var users = networkUsers
                .Concat(analysisUsers)
                .Include(item => item.NetworkUsers)
                    .ThenInclude(item => item.Network)
                .Include(item => item.AnalysisUsers)
                    .ThenInclude(item => item.Analysis);
            // Go over each of the users.
            foreach (var user in users)
            {
                // Send an alert delete e-mail.
                await emailSender.SendAlertDeleteEmailAsync(new EmailAlertDeleteViewModel
                {
                    Email = user.Email,
                    DateTime = DateTime.Today + TimeSpan.FromDays(ApplicationDbContext.DaysBeforeDelete - ApplicationDbContext.DaysBeforeAlert),
                    NetworkItems = user.NetworkUsers
                        .Select(item => item.Network)
                        .Where(item => networks.Contains(item))
                        .Select(item => new EmailAlertDeleteViewModel.ItemModel
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Url = linkGenerator.GetUriByPage("/Content/Created/Networks/Details/Index", handler: null, values: new { id = item.Id }, scheme: Scheme, host: host)
                        }),
                    AnalysisItems = user.AnalysisUsers
                        .Select(item => item.Analysis)
                        .Where(item => analyses.Contains(item))
                        .Select(item => new EmailAlertDeleteViewModel.ItemModel
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Url = linkGenerator.GetUriByPage("/Content/Created/Analyses/Details/Index", handler: null, values: new { id = item.Id }, scheme: Scheme, host: host)
                        }),
                    ApplicationUrl = linkGenerator.GetUriByPage("/Index", handler: null, values: null, scheme: Scheme, host: host)
                });
            }
        }

        /// <summary>
        /// Deletes the unconfirmed users.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteUnconfirmedUsersAsync(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Create a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Define the limit date.
            var limitDate = DateTime.Today - TimeSpan.FromDays(ApplicationDbContext.DaysBeforeDelete);
            // Get the items to stop.
            var items = context.Users
                .Where(item => !item.EmailConfirmed)
                .Where(item => item.DateTimeCreated < limitDate);
            // Define a new task.
            var task = new UsersTask
            {
                Items = items
                    .Select(item => new UserInputModel
                    {
                        Id = item.Id
                    })
            };
            // Run the task.
            await task.DeleteAsync(serviceProvider, token);
        }

        /// <summary>
        /// Deletes the guest users.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteGuestUsersAsync(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Create a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Define the limit date.
            var limitDate = DateTime.Today - TimeSpan.FromDays(ApplicationDbContext.DaysBeforeGuestDelete);
            // Get the items to stop.
            var items = context.Users
                .Where(item => item.UserRoles.Any(item1 => item1.Role.Name == "Guest"))
                .Where(item => item.DateTimeCreated < limitDate);
            // Define a new task.
            var task = new UsersTask
            {
                Items = items
                    .Select(item => new UserInputModel
                    {
                        Id = item.Id
                    })
            };
            // Run the task.
            await task.DeleteAsync(serviceProvider, token);
        }

        /// <summary>
        /// Deletes the long-standing networks.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteNetworksAsync(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Create a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Define the limit date.
            var limitDate = DateTime.Today - TimeSpan.FromDays(ApplicationDbContext.DaysBeforeDelete);
            // Get the items to delete.
            var items = context.Networks
                .Where(item => item.DateTimeCreated < limitDate);
            // Define a new task.
            var task = new NetworksTask
            {
                Items = items
                    .Select(item => new NetworkInputModel
                    {
                        Id = item.Id
                    })
            };
            // Run the task.
            await task.DeleteAsync(serviceProvider, token);
        }

        /// <summary>
        /// Deletes the long-standing analyses.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteAnalysesAsync(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Create a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Define the limit date.
            var limitDate = DateTime.Today - TimeSpan.FromDays(ApplicationDbContext.DaysBeforeDelete);
            // Get the items to delete.
            var items = context.Analyses
                .Where(item => item.Status == AnalysisStatus.Stopped || item.Status == AnalysisStatus.Completed || item.Status == AnalysisStatus.Error)
                .Where(item => item.DateTimeEnded < limitDate);
            // Define a new task.
            var task = new AnalysesTask
            {
                Items = items
                    .Select(item => new AnalysisInputModel
                    {
                        Id = item.Id
                    })
            };
            // Run the task.
            await task.DeleteAsync(serviceProvider, token);
        }
    }
}
