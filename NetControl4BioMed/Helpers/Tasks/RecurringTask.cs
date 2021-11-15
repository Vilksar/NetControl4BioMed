using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
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
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["Users"] = await context.Users
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["Roles"] = await context.Roles
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["Databases"] = await context.Databases
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["DatabaseProteinFields"] = await context.DatabaseProteinFields
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["DatabaseInteractionFields"] = await context.DatabaseInteractionFields
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["Proteins"] = await context.Proteins
                    .Where(item => item.DatabaseProteins.Any())
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["Interactions"] = await context.Interactions
                    .Where(item => item.DatabaseInteractions.Any())
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["ProteinCollections"] = await context.ProteinCollections
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["Networks"] = await context.Networks
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["Analyses"] = await context.Analyses
                    .CountAsync();
            }
            // Return the dictionary.
            return dictionary;
        }

        /// <summary>
        /// Counts the public items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns>A dictionary containing the number of public items in the database.</returns>
        public async Task<Dictionary<string, int>> CountPublicItemsAsync(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Define the dictionary to return.
            var dictionary = new Dictionary<string, int>();
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["Databases"] = await context.Databases
                    .Where(item => item.IsPublic)
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["DatabaseProteinFields"] = await context.DatabaseProteinFields
                    .Where(item => item.Database.IsPublic)
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["DatabaseInteractionFields"] = await context.DatabaseInteractionFields
                    .Where(item => item.Database.IsPublic)
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["Proteins"] = await context.Proteins
                    .Where(item => item.DatabaseProteins.Any(item1 => item1.Database.IsPublic))
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["Interactions"] = await context.Interactions
                    .Where(item => item.DatabaseInteractions.Any(item1 => item1.Database.IsPublic))
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["ProteinCollections"] = await context.ProteinCollections
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["Networks"] = await context.Networks
                    .Where(item => item.IsPublic)
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["Analyses"] = await context.Analyses
                    .Where(item => item.IsPublic)
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
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["Databases"] = await context.Databases
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
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["DatabaseProteinFields"] = await context.DatabaseProteinFields
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
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["DatabaseInteractionFields"] = await context.DatabaseInteractionFields
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
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["DatabaseProteinFieldProteins"] = await context.DatabaseProteinFieldProteins
                    .Where(item => item.DatabaseProteinField.IsSearchable)
                    .GroupBy(item => new { item.DatabaseProteinFieldId, item.Value })
                    .Where(item => item.Count() > 1)
                    .Select(item => item.Key)
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["DatabaseInteractionFieldInteractions"] = await context.DatabaseInteractionFieldInteractions
                    .Where(item => item.DatabaseInteractionField.IsSearchable)
                    .GroupBy(item => new { item.DatabaseInteractionFieldId, item.Value })
                    .Where(item => item.Count() > 1)
                    .Select(item => item.Key)
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["Proteins"] = await context.Proteins
                    .Where(item => item.DatabaseProteins.Any())
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
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["Interactions"] = await context.Interactions
                    .Where(item => item.DatabaseInteractions.Any())
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
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["ProteinCollections"] = await context.ProteinCollections
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
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["Proteins"] = await context.Proteins
                    .Where(item => !item.DatabaseProteins.Any() && !item.NetworkProteins.Any())
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["Interactions"] = await context.Interactions
                    .Where(item => (!item.DatabaseInteractions.Any() && !item.NetworkInteractions.Any()) || item.InteractionProteins.Count() < 2)
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["ProteinCollections"] = await context.ProteinCollections
                    .Where(item => !item.ProteinCollectionProteins.Any())
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["Networks"] = await context.Networks
                    .Where(item => !item.NetworkProteins.Any() || !item.NetworkInteractions.Any())
                    .CountAsync();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the items with the provided IDs.
                dictionary["Analyses"] = await context.Analyses
                    .Where(item => !item.AnalysisProteins.Any() || !item.AnalysisInteractions.Any() || item.Network == null)
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
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
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
        /// Extends the time until the demonstration items are automatically deleted from the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task ExtendTimeUntilDeleteDemonstrationItemsAsync(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Define the limit date.
            var limitDate = DateTime.Today - TimeSpan.FromDays(ApplicationDbContext.DaysBeforeAlert + 1);
            // Define the IDs of the items to get.
            var networkIds = new List<string>();
            var analysisIds = new List<string>();
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the IDs of the items to stop.
                networkIds = context.Networks
                    .Where(item => item.IsDemonstration)
                    .Where(item => item.DateTimeCreated < limitDate)
                    .Select(item => item.Id)
                    .ToList();
                analysisIds = context.Analyses
                    .Where(item => item.IsDemonstration)
                    .Where(item => item.DateTimeCreated < limitDate)
                    .Select(item => item.Id)
                    .ToList();
            }
            // Define the new tasks.
            var analysisTask = new AnalysesTask
            {
                Items = analysisIds
                    .Select(item => new AnalysisInputModel
                    {
                        Id = item
                    })
            };
            var networkTask = new NetworksTask
            {
                Items = networkIds
                    .Select(item => new NetworkInputModel
                    {
                        Id = item
                    })
            };
            // Run the tasks.
            await networkTask.ExtendTimeUntilDeleteAsync(serviceProvider, token);
            await analysisTask.ExtendTimeUntilDeleteAsync(serviceProvider, token);
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
            var limitDateStart = DateTime.Today + TimeSpan.FromDays(ApplicationDbContext.DaysBeforeDelete - ApplicationDbContext.DaysBeforeAlert);
            var limitDateEnd = DateTime.Today + TimeSpan.FromDays(ApplicationDbContext.DaysBeforeDelete - ApplicationDbContext.DaysBeforeAlert + 1);
            // Define the items to get.
            var alertNetworks = new List<AlertItemModel>();
            var alertAnalyses = new List<AlertItemModel>();
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the networks and analyses.
                alertNetworks = context.Networks
                    .Where(item => limitDateStart < item.DateTimeToDelete && item.DateTimeToDelete < limitDateEnd)
                    .Select(item => new AlertItemModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Emails = item.NetworkUsers
                            .Select(item1 => item1.User.Email)
                    })
                    .ToList();
                alertAnalyses = context.Analyses
                    .Where(item => item.Status == AnalysisStatus.Stopped || item.Status == AnalysisStatus.Completed || item.Status == AnalysisStatus.Error)
                    .Where(item => limitDateStart < item.DateTimeToDelete && item.DateTimeToDelete < limitDateEnd)
                    .Select(item => new AlertItemModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Emails = item.AnalysisUsers
                            .Select(item1 => item1.User.Email)
                    })
                    .ToList()
                    .Concat(context.Networks
                        .Where(item => limitDateStart < item.DateTimeToDelete && item.DateTimeToDelete < limitDateEnd)
                        .Select(item => item.Analyses)
                        .SelectMany(item => item)
                        .Select(item => new AlertItemModel
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Emails = item.AnalysisUsers
                                .Select(item1 => item1.User.Email)
                        })
                        .ToList())
                    .DistinctBy(item => item.Id)
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
                                Url = linkGenerator.GetUriByPage($"/CreatedData/Networks/Details/Index", handler: null, values: new { id = item1.Id }, scheme: Scheme, host: host)
                            }),
                        AnalysisItems = alertAnalyses
                            .Where(item1 => item1.Emails.Contains(item))
                            .Select(item1 => new EmailAlertDeleteViewModel.ItemModel
                            {
                                Id = item1.Id,
                                Name = item1.Name,
                                Url = linkGenerator.GetUriByPage($"/CreatedData/Analyses/Details/Index", handler: null, values: new { id = item1.Id }, scheme: Scheme, host: host)
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
            var userIds = new List<string>();
            var databaseUserItemIds = new List<(string, string)>();
            var networkUserItemIds = new List<(string, string)>();
            var analysisUserItemIds = new List<(string, string)>();
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the IDs of the items to delete.
                userIds = context.Users
                    .Where(item => !item.EmailConfirmed)
                    .Where(item => item.DateTimeCreated < limitDate)
                    .Select(item => item.Id)
                    .ToList();
                databaseUserItemIds = context.DatabaseUsers
                    .Where(item => item.User == null)
                    .Where(item => item.DateTimeCreated < limitDate)
                    .Select(item => new
                    {
                        DatabaseId = item.Database.Id,
                        Email = item.Email
                    })
                    .AsEnumerable()
                    .Select(item => (item.DatabaseId, item.Email))
                    .ToList();
                networkUserItemIds = context.NetworkUsers
                    .Where(item => item.User == null)
                    .Where(item => item.DateTimeCreated < limitDate)
                    .Select(item => new
                    {
                        NetworkId = item.Network.Id,
                        Email = item.Email
                    })
                    .AsEnumerable()
                    .Select(item => (item.NetworkId, item.Email))
                    .ToList();
                analysisUserItemIds = context.AnalysisUsers
                    .Where(item => item.User == null)
                    .Where(item => item.DateTimeCreated < limitDate)
                    .Select(item => new
                    {
                        AnalysisId = item.Analysis.Id,
                        Email = item.Email
                    })
                    .AsEnumerable()
                    .Select(item => (item.AnalysisId, item.Email))
                    .ToList();
            }
            // Define the new tasks.
            var userTask = new UsersTask
            {
                Items = userIds
                    .Select(item => new UserInputModel
                    {
                        Id = item
                    })
            };
            var databaseUserTask = new DatabaseUsersTask
            {
                Items = databaseUserItemIds
                    .Select(item => new DatabaseUserInputModel
                    {
                        Database = new DatabaseInputModel
                        {
                            Id = item.Item1
                        },
                        Email = item.Item2
                    })
            };
            var networkUserTask = new NetworkUsersTask
            {
                Items = networkUserItemIds
                    .Select(item => new NetworkUserInputModel
                    {
                        Network = new NetworkInputModel
                        {
                            Id = item.Item1
                        },
                        Email = item.Item2
                    })
            };
            var analysisUserTask = new AnalysisUsersTask
            {
                Items = analysisUserItemIds
                    .Select(item => new AnalysisUserInputModel
                    {
                        Analysis = new AnalysisInputModel
                        {
                            Id = item.Item1
                        },
                        Email = item.Item2
                    })
            };
            // Run the task.
            await userTask.DeleteAsync(serviceProvider, token);
            await databaseUserTask.DeleteAsync(serviceProvider, token);
            await networkUserTask.DeleteAsync(serviceProvider, token);
            await analysisUserTask.DeleteAsync(serviceProvider, token);
        }

        /// <summary>
        /// Deletes the orphaned items from the database.
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
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the input models of the items to delete.
                var itemInputModels = context.Proteins
                    .Where(item => !item.DatabaseProteins.Any() && !item.NetworkProteins.Any())
                    .Select(item => new ProteinInputModel
                    {
                        Id = item.Id
                    })
                    .ToList();
                // Run a new task.
                await new ProteinsTask { Items = itemInputModels }.DeleteAsync(serviceProvider, token);
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the input models of the items to delete.
                var itemInputModels = context.Interactions
                    .Where(item => (!item.DatabaseInteractions.Any() && !item.NetworkInteractions.Any()) || item.InteractionProteins.Count() < 2)
                    .Select(item => new InteractionInputModel
                    {
                        Id = item.Id
                    })
                    .ToList();
                // Run a new task.
                await new InteractionsTask { Items = itemInputModels }.DeleteAsync(serviceProvider, token);
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the input models of the items to delete.
                var itemInputModels = context.ProteinCollections
                    .Where(item => !item.ProteinCollectionProteins.Any())
                    .Select(item => new ProteinCollectionInputModel
                    {
                        Id = item.Id
                    })
                    .ToList();
                // Run a new task.
                await new ProteinCollectionsTask { Items = itemInputModels }.DeleteAsync(serviceProvider, token);
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the input models of the items to delete.
                var itemInputModels = context.Networks
                    .Where(item => !item.NetworkProteins.Any() || !item.NetworkInteractions.Any())
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
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the input models of the items to delete.
                var itemInputModels = context.Analyses
                    .Where(item => !item.AnalysisProteins.Any() || !item.AnalysisInteractions.Any() || item.Network == null)
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
            var limitDate = DateTime.Today;
            // Define the IDs of the items to get.
            var itemIds = new List<string>();
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the IDs of the items to delete.
                itemIds = context.Networks
                    .Where(item => item.DateTimeToDelete < limitDate)
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
            var limitDate = DateTime.Today;
            // Define the IDs of the items to get.
            var itemIds = new List<string>();
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Increase the command timeout.
                context.Database.SetCommandTimeout(60);
                // Get the IDs of the items to delete.
                itemIds = context.Analyses
                    .Where(item => item.Status == AnalysisStatus.Stopped || item.Status == AnalysisStatus.Completed || item.Status == AnalysisStatus.Error)
                    .Where(item => item.DateTimeToDelete < limitDate)
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
