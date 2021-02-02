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
    /// Implements recurring tasks.
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
        /// Counts the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns>A dictionary containing the number of items in the database.</returns>
        public async Task<Dictionary<string, int>> CountAllItemsAsync(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Define the dictionary to return.
            var dictionary = new Dictionary<string, int>();
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["Users"] = await context.Users
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["Roles"] = await context.Roles
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["DatabaseTypes"] = await context.DatabaseTypes
                    .Where(item => item.Name != "Generic")
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["Databases"] = await context.Databases
                    .Where(item => item.DatabaseType.Name != "Generic")
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["DatabaseNodeFields"] = await context.DatabaseNodeFields
                    .Where(item => item.Database.DatabaseType.Name != "Generic")
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["DatabaseEdgeFields"] = await context.DatabaseEdgeFields
                    .Where(item => item.Database.DatabaseType.Name != "Generic")
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["Nodes"] = await context.Nodes
                    .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["Edges"] = await context.Edges
                    .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["GenericNodes"] = await context.Nodes
                    .Where(item => item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["GenericEdges"] = await context.Edges
                    .Where(item => item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["NodeCollections"] = await context.NodeCollections
                    .Where(item => !item.NodeCollectionDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["Networks"] = await context.Networks
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["Analyses"] = await context.Analyses
                    .CountAsync();
            }
            // Return the dictionary.
            return dictionary;
        }

        /// <summary>
        /// Counts the duplicate items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns>A dictionary containing the number of duplicate items in the database.</returns>
        public async Task<Dictionary<string, int>> CountDuplicateItemsAsync(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Define the dictionary to return.
            var dictionary = new Dictionary<string, int>();
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["DatabaseTypes"] = await context.DatabaseTypes
                    .Where(item => item.Name != "Generic")
                    .GroupBy(item => item.Name)
                    .Where(item => item.Count() > 1)
                    .Select(item => item.Key)
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["Databases"] = await context.Databases
                    .Where(item => item.DatabaseType.Name != "Generic")
                    .GroupBy(item => item.Name)
                    .Where(item => item.Count() > 1)
                    .Select(item => item.Key)
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["DatabaseNodeFields"] = await context.DatabaseNodeFields
                    .Where(item => item.Database.DatabaseType.Name != "Generic")
                    .GroupBy(item => item.Name)
                    .Where(item => item.Count() > 1)
                    .Select(item => item.Key)
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["DatabaseEdgeFields"] = await context.DatabaseEdgeFields
                    .Where(item => item.Database.DatabaseType.Name != "Generic")
                    .GroupBy(item => item.Name)
                    .Where(item => item.Count() > 1)
                    .Select(item => item.Key)
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["DatabaseNodeFieldNodes"] = await context.DatabaseNodeFieldNodes
                    .Where(item => item.DatabaseNodeField.Database.DatabaseType.Name != "Generic")
                    .Where(item => item.DatabaseNodeField.IsSearchable)
                    .GroupBy(item => new { item.DatabaseNodeFieldId, item.Value })
                    .Where(item => item.Count() > 1)
                    .Select(item => item.Key)
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["DatabaseEdgeFieldEdges"] = await context.DatabaseEdgeFieldEdges
                    .Where(item => item.DatabaseEdgeField.Database.DatabaseType.Name != "Generic")
                    .Where(item => item.DatabaseEdgeField.IsSearchable)
                    .GroupBy(item => new { item.DatabaseEdgeFieldId, item.Value })
                    .Where(item => item.Count() > 1)
                    .Select(item => item.Key)
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["Nodes"] = await context.Nodes
                    .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .GroupBy(item => item.Name)
                    .Where(item => item.Count() > 1)
                    .Select(item => item.Key)
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["Edges"] = await context.Edges
                    .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .GroupBy(item => item.Name)
                    .Where(item => item.Count() > 1)
                    .Select(item => item.Key)
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["NodeCollections"] = await context.NodeCollections
                    .Where(item => !item.NodeCollectionDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .GroupBy(item => item.Name)
                    .Where(item => item.Count() > 1)
                    .Select(item => item.Key)
                    .CountAsync();
            }
            // Return the dictionary.
            return dictionary;
        }

        /// <summary>
        /// Counts the orphaned items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns>A dictionary containing the number of orphaned items in the database.</returns>
        public async Task<Dictionary<string, int>> CountOrphanedItemsAsync(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Define the dictionary to return.
            var dictionary = new Dictionary<string, int>(7);
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["Nodes"] = await context.Nodes
                    .Where(item => !item.DatabaseNodeFieldNodes.Any())
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["Edges"] = await context.Edges
                    .Where(item => !item.DatabaseEdges.Any() || item.EdgeNodes.Count() < 2)
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["GenericNodes"] = await context.Nodes
                    .Where(item => item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => !item.NetworkNodes.Any())
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["GenericEdges"] = await context.Edges
                    .Where(item => item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => !item.NetworkEdges.Any())
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["NodeCollections"] = await context.NodeCollections
                    .Where(item => !item.NodeCollectionNodes.Any())
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["Networks"] = await context.Networks
                    .Where(item => !item.NetworkDatabases.Any() || !item.NetworkNodes.Any() || !item.NetworkEdges.Any())
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["Analyses"] = await context.Analyses
                    .Where(item => !item.AnalysisDatabases.Any() || !item.AnalysisNodes.Any() || !item.AnalysisEdges.Any() || !item.AnalysisNetworks.Any())
                    .CountAsync();
            }
            // Return the dictionary.
            return dictionary;
        }

        /// <summary>
        /// Counts the inconsistent items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns>A dictionary containing the number of inconsistent items in the database.</returns>
        public async Task<Dictionary<string, int>> CountInconsistentItemsAsync(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Define the dictionary to return.
            var dictionary = new Dictionary<string, int>(5);
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["Nodes"] = await context.Nodes
                    .Where(item => item.DatabaseNodes.Select(item1 => item1.Database.DatabaseType).Distinct().Count() > 1)
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["Edges"] = await context.Edges
                    .Where(item => item.DatabaseEdges.Select(item1 => item1.Database.DatabaseType).Distinct().Count() > 1)
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["NodeCollections"] = await context.NodeCollections
                    .Where(item => item.NodeCollectionNodes.Select(item1 => item1.Node.DatabaseNodes).SelectMany(item1 => item1).Select(item1 => item1.Database.DatabaseType).Distinct().Count() > 1)
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["Networks"] = await context.Networks
                    .Where(item => item.NetworkDatabases.Select(item1 => item1.Database.DatabaseType).Distinct().Count() > 1)
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items with the provided IDs.
                dictionary["Analyses"] = await context.Analyses
                    .Where(item => item.AnalysisDatabases.Select(item1 => item1.Database.DatabaseType).Distinct().Count() > 1)
                    .CountAsync();
            }
            // Return the dictionary.
            return dictionary;
        }

        /// <summary>
        /// Stops the long-running analyses in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task StopAnalysesAsync(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Define the limit date.
            var limitDate = DateTime.Today - TimeSpan.FromDays(ApplicationDbContext.DaysBeforeStop);
            // Define the IDs of the items to get.
            var itemIds = new List<string>();
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the IDs of the items to stop.
                itemIds = context.Analyses
                    .Where(item => item.Status == AnalysisStatus.Initializing || item.Status == AnalysisStatus.Ongoing)
                    .Where(item => item.DateTimeStarted < limitDate)
                    .Select(item => item.Id)
                    .ToList();
            }
            // Define a new task.
            var task = new AnalysesTask
            {
                Items = itemIds
                    .Select(item => new AnalysisInputModel
                    {
                        Id = item
                    })
            };
            // Run the task.
            await task.StopAsync(serviceProvider, token);
        }

        /// <summary>
        /// Alerts the users before deleting the long-standing networks and analyses from the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task AlertUsersAsync(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Define the host.
            var host = new HostString(HostValue);
            // Define the limit dates.
            var limitDateStart = DateTime.Today - TimeSpan.FromDays(ApplicationDbContext.DaysBeforeAlert + 1);
            var limitDateEnd = DateTime.Today - TimeSpan.FromDays(ApplicationDbContext.DaysBeforeAlert);
            // Define the items to get.
            var alertNetworks = new List<AlertItemModel>();
            var alertAnalyses = new List<AlertItemModel>();
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the networks and analyses.
                alertNetworks = context.Networks
                    .Where(item => limitDateStart < item.DateTimeCreated && item.DateTimeCreated < limitDateEnd)
                    .Select(item => new AlertItemModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        DatabaseTypeName = item.NetworkDatabases
                            .Select(item1 => item1.Database.DatabaseType.Name)
                            .FirstOrDefault(),
                        Emails = item.NetworkUsers
                            .Select(item1 => item1.User.Email)
                    })
                    .Where(item => !string.IsNullOrEmpty(item.DatabaseTypeName))
                    .ToList();
                alertAnalyses = context.Analyses
                    .Where(item => item.Status == AnalysisStatus.Stopped || item.Status == AnalysisStatus.Completed || item.Status == AnalysisStatus.Error)
                    .Where(item => limitDateStart < item.DateTimeEnded && item.DateTimeEnded < limitDateEnd)
                    .Concat(context.Networks
                        .Where(item => limitDateStart < item.DateTimeCreated && item.DateTimeCreated < limitDateEnd)
                        .Select(item => item.AnalysisNetworks)
                        .SelectMany(item => item)
                        .Select(item => item.Analysis))
                    .Distinct()
                    .Select(item => new AlertItemModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        DatabaseTypeName = item.AnalysisDatabases
                            .Select(item1 => item1.Database.DatabaseType.Name)
                            .FirstOrDefault(),
                        Emails = item.AnalysisUsers
                            .Select(item1 => item1.User.Email)
                    })
                    .Where(item => !string.IsNullOrEmpty(item.DatabaseTypeName))
                    .ToList();
            }
            // Define the user items to get.
            var alertUsers = new List<AlertUserModel>();
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new link generator instance.
                var linkGenerator = scope.ServiceProvider.GetRequiredService<LinkGenerator>();
                // Get the user items.
                alertUsers = alertNetworks
                    .Select(item => item.Emails)
                    .Concat(alertAnalyses
                        .Select(item => item.Emails))
                    .SelectMany(item => item)
                    .Distinct()
                    .Select(item => new AlertUserModel
                    {
                        Email = item,
                        NetworkItems = alertNetworks
                            .Where(item1 => item1.Emails.Contains(item))
                            .Select(item1 => new EmailAlertDeleteViewModel.ItemModel
                            {
                                Id = item1.Id,
                                Name = item1.Name,
                                Url = linkGenerator.GetUriByPage($"/Content/DatabaseTypes/{item1.DatabaseTypeName}/Created/Networks/Details/Index", handler: null, values: new { id = item1.Id }, scheme: Scheme, host: host)
                            }),
                        AnalysisItems = alertAnalyses
                            .Where(item1 => item1.Emails.Contains(item))
                            .Select(item1 => new EmailAlertDeleteViewModel.ItemModel
                            {
                                Id = item1.Id,
                                Name = item1.Name,
                                Url = linkGenerator.GetUriByPage($"/Content/DatabaseTypes/{item1.DatabaseTypeName}/Created/Analyses/Details/Index", handler: null, values: new { id = item1.Id }, scheme: Scheme, host: host)
                            })
                    })
                    .ToList();
            }
            // Go over each of the users.
            foreach (var alertUser in alertUsers)
            {
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new e-mail sender instance.
                    var emailSender = scope.ServiceProvider.GetRequiredService<ISendGridEmailSender>();
                    // Use a new link generator instance.
                    var linkGenerator = scope.ServiceProvider.GetRequiredService<LinkGenerator>();
                    // Send an alert delete e-mail.
                    await emailSender.SendAlertDeleteEmailAsync(new EmailAlertDeleteViewModel
                    {
                        Email = alertUser.Email,
                        DateTime = DateTime.Today + TimeSpan.FromDays(ApplicationDbContext.DaysBeforeDelete - ApplicationDbContext.DaysBeforeAlert),
                        NetworkItems = alertUser.NetworkItems,
                        AnalysisItems = alertUser.AnalysisItems,
                        ApplicationUrl = linkGenerator.GetUriByPage("/Index", handler: null, values: null, scheme: Scheme, host: host)
                    });
                }
            }
        }

        /// <summary>
        /// Deletes the long-standing unconfirmed users from the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteUnconfirmedUsersAsync(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Define the limit date.
            var limitDate = DateTime.Today - TimeSpan.FromDays(ApplicationDbContext.DaysBeforeDelete);
            // Define the IDs of the items to get.
            var itemIds = new List<string>();
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the IDs of the items to delete.
                itemIds = context.Users
                    .Where(item => !item.EmailConfirmed)
                    .Where(item => item.DateTimeCreated < limitDate)
                    .Select(item => item.Id)
                    .ToList();
            }
            // Define a new task.
            var task = new UsersTask
            {
                Items = itemIds
                    .Select(item => new UserInputModel
                    {
                        Id = item
                    })
            };
            // Run the task.
            await task.DeleteAsync(serviceProvider, token);
        }

        /// <summary>
        /// Deletes the long-standing analyses from the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteOrphanedItemsAsync(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the input models of the items to delete.
                var itemInputModels = context.Nodes
                    .Where(item => !item.DatabaseNodeFieldNodes.Any())
                    .Select(item => new NodeInputModel
                    {
                        Id = item.Id
                    })
                    .ToList();
                // Run a new task.
                await new NodesTask { Items = itemInputModels }.DeleteAsync(serviceProvider, token);
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the input models of the items to delete.
                var itemInputModels = context.Edges
                    .Where(item => !item.DatabaseEdges.Any() || item.EdgeNodes.Count() < 2)
                    .Select(item => new EdgeInputModel
                    {
                        Id = item.Id
                    })
                    .ToList();
                // Run a new task.
                await new EdgesTask { Items = itemInputModels }.DeleteAsync(serviceProvider, token);
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the input models of the items to delete.
                var itemInputModels = context.Nodes
                    .Where(item => item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => !item.NetworkNodes.Any())
                    .Select(item => new NodeInputModel
                    {
                        Id = item.Id
                    })
                    .ToList();
                // Run a new task.
                await new NodesTask { Items = itemInputModels }.DeleteAsync(serviceProvider, token);
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the input models of the items to delete.
                var itemInputModels = context.Edges
                    .Where(item => item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => !item.NetworkEdges.Any())
                    .Select(item => new EdgeInputModel
                    {
                        Id = item.Id
                    })
                    .ToList();
                // Run a new task.
                await new EdgesTask { Items = itemInputModels }.DeleteAsync(serviceProvider, token);
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the input models of the items to delete.
                var itemInputModels = context.NodeCollections
                    .Where(item => !item.NodeCollectionNodes.Any())
                    .Select(item => new NodeCollectionInputModel
                    {
                        Id = item.Id
                    })
                    .ToList();
                // Run a new task.
                await new NodeCollectionsTask { Items = itemInputModels }.DeleteAsync(serviceProvider, token);
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the input models of the items to delete.
                var itemInputModels = context.Networks
                    .Where(item => !item.NetworkDatabases.Any() || !item.NetworkNodes.Any() || !item.NetworkEdges.Any())
                    .Select(item => new NetworkInputModel
                    {
                        Id = item.Id
                    })
                    .ToList();
                // Run a new task.
                await new NetworksTask { Items = itemInputModels }.DeleteAsync(serviceProvider, token);
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the input models of the items to delete.
                var itemInputModels = context.Analyses
                    .Where(item => !item.AnalysisDatabases.Any() || !item.AnalysisNodes.Any() || !item.AnalysisEdges.Any() || !item.AnalysisNetworks.Any())
                    .Select(item => new AnalysisInputModel
                    {
                        Id = item.Id
                    })
                    .ToList();
                // Run a new task.
                await new AnalysesTask { Items = itemInputModels }.DeleteAsync(serviceProvider, token);
            }
        }

        /// <summary>
        /// Deletes the long-standing networks from the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteNetworksAsync(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Define the limit date.
            var limitDate = DateTime.Today - TimeSpan.FromDays(ApplicationDbContext.DaysBeforeDelete);
            // Define the IDs of the items to get.
            var itemIds = new List<string>();
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the IDs of the items to delete.
                itemIds = context.Networks
                    .Where(item => item.DateTimeCreated < limitDate)
                    .Select(item => item.Id)
                    .ToList();
            }
            // Define a new task.
            var task = new NetworksTask
            {
                Items = itemIds
                    .Select(item => new NetworkInputModel
                    {
                        Id = item
                    })
            };
            // Run the task.
            await task.DeleteAsync(serviceProvider, token);
        }

        /// <summary>
        /// Deletes the long-standing analyses from the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteAnalysesAsync(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Define the limit date.
            var limitDate = DateTime.Today - TimeSpan.FromDays(ApplicationDbContext.DaysBeforeDelete);
            // Define the IDs of the items to get.
            var itemIds = new List<string>();
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the IDs of the items to delete.
                itemIds = context.Analyses
                    .Where(item => item.Status == AnalysisStatus.Stopped || item.Status == AnalysisStatus.Completed || item.Status == AnalysisStatus.Error)
                    .Where(item => item.DateTimeEnded < limitDate)
                    .Select(item => item.Id)
                    .ToList();
            }
            // Define a new task.
            var task = new AnalysesTask
            {
                Items = itemIds
                    .Select(item => new AnalysisInputModel
                    {
                        Id = item
                    })
            };
            // Run the task.
            await task.DeleteAsync(serviceProvider, token);
        }

        /// <summary>
        /// Represents the model of an alert item.
        /// </summary>
        private class AlertItemModel
        {
            /// <summary>
            /// Represents the ID of the alert item.
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// Represents the name of the alert item.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Represents the name of the database type of the alert item.
            /// </summary>
            public string DatabaseTypeName { get; set; }

            /// <summary>
            /// Represents the e-mails of the alert item.
            /// </summary>
            public IEnumerable<string> Emails { get; set; }
        }

        /// <summary>
        /// Represents the model of an alert user.
        /// </summary>
        private class AlertUserModel
        {
            /// <summary>
            /// Represents the e-mail of the alert user.
            /// </summary>
            public string Email { get; set; }

            /// <summary>
            /// Represents the network items of the alert user.
            /// </summary>
            public IEnumerable<EmailAlertDeleteViewModel.ItemModel> NetworkItems { get; set; }

            /// <summary>
            /// Represents the analysis items of the alert user.
            /// </summary>
            public IEnumerable<EmailAlertDeleteViewModel.ItemModel> AnalysisItems { get; set; }
        }
    }
}
