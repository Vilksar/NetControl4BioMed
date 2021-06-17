using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Interfaces;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Tasks;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Extensions
{
    /// <summary>
    /// Provides extensions for the networks.
    /// </summary>
    public static class NetworkExtensions
    {
        /// <summary>
        /// Represents the maximum size for which visualization will be enabled.
        /// </summary>
        public readonly static int MaximumSizeForVisualization = 500;

        /// <summary>
        /// Gets the log of the network.
        /// </summary>
        /// <param name="network">The current network.</param>
        /// <returns>The log of the network.</returns>
        public static List<LogEntryViewModel> GetLog(this Network network)
        {
            // Return the list of log entries.
            return JsonSerializer.Deserialize<List<LogEntryViewModel>>(network.Log);
        }

        /// <summary>
        /// Appends to the list a new entry to the log of the network and returns the updated list.
        /// </summary>
        /// <param name="network">The current network.</param>
        /// <param name="message">The message to add as a new entry to the network log.</param>
        /// <returns>The updated log of the network.</returns>
        public static string AppendToLog(this Network network, string message)
        {
            // Return the log entries with the message appended.
            return JsonSerializer.Serialize(network.GetLog().Append(new LogEntryViewModel { DateTime = DateTime.UtcNow, Message = message }));
        }

        /// <summary>
        /// Deletes the related entities of the corresponding networks.
        /// </summary>
        /// <typeparam name="T">The type of the related entities.</typeparam>
        /// <param name="itemIds">The IDs of the items whose entities should be deleted.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public static async Task DeleteRelatedEntitiesAsync<T>(IEnumerable<string> itemIds, IServiceProvider serviceProvider, CancellationToken token) where T : class, INetworkDependent
        {
            // Define a variable to store the total number of entities.
            var entityCount = 0;
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the corresponding database set.
                var set = context.Set<T>();
                // Get the items in the current batch.
                entityCount = set
                    .Where(item => itemIds.Contains(item.Network.Id))
                    .Count();
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)entityCount / ApplicationDbContext.BatchSize);
            // Go over each batch.
            for (int index = 0; index < count; index++)
            {
                // Check if the cancellation was requested.
                if (token.IsCancellationRequested)
                {
                    // Break.
                    break;
                }
                // Define the batch items.
                var batchItems = new List<T>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the corresponding database set.
                    var set = context.Set<T>();
                    // Get the items in the current batch.
                    batchItems = set
                        .Where(item => itemIds.Contains(item.Network.Id))
                        .Take(ApplicationDbContext.BatchSize)
                        .ToList();
                    // Check if there were no items found.
                    if (batchItems == null || !batchItems.Any())
                    {
                        // Continue.
                        continue;
                    }
                }
                // Delete the items.
                await IEnumerableExtensions.DeleteAsync<T>(batchItems, serviceProvider, token);
            }
        }

        /// <summary>
        /// Deletes the dependent analyses of the corresponding networks.
        /// </summary>
        /// <param name="networkIds">The networks whose entities should be deleted.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public static async Task DeleteDependentAnalysesAsync(IEnumerable<string> networkIds, IServiceProvider serviceProvider, CancellationToken token)
        {
            // Define a variable to store the total number of entities.
            var entityCount = 0;
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items in the current batch.
                entityCount = context.Analyses
                    .Where(item => networkIds.Contains(item.Network.Id))
                    .Distinct()
                    .Count();
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)entityCount / ApplicationDbContext.BatchSize);
            // Go over each batch.
            for (int index = 0; index < count; index++)
            {
                // Check if the cancellation was requested.
                if (token.IsCancellationRequested)
                {
                    // Break.
                    break;
                }
                // Define the batch items.
                var batchItemInputs = new List<AnalysisInputModel>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items in the current batch.
                    batchItemInputs = context.Analyses
                        .Where(item => networkIds.Contains(item.Network.Id))
                        .Distinct()
                        .Select(item => new AnalysisInputModel
                        {
                            Id = item.Id
                        })
                        .Take(ApplicationDbContext.BatchSize)
                        .ToList();
                    // Check if there were no items found.
                    if (batchItemInputs == null || !batchItemInputs.Any())
                    {
                        // Continue.
                        continue;
                    }
                }
                // Delete the items.
                await new AnalysesTask { Items = batchItemInputs }.DeleteAsync(serviceProvider, token);
            }
        }

        /// <summary>
        /// Deletes the dependent generic interactions of the corresponding networks.
        /// </summary>
        /// <param name="networkIds">The networks whose entities should be deleted.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public static async Task DeleteDependentInteractionsAsync(IEnumerable<string> networkIds, IServiceProvider serviceProvider, CancellationToken token)
        {
            // Define a variable to store the total number of entities.
            var entityCount = 0;
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items in the current batch.
                entityCount = context.NetworkInteractions
                    .Where(item => networkIds.Contains(item.Network.Id))
                    .Where(item => item.Network.Algorithm == NetworkAlgorithm.None)
                    .Select(item => item.Interaction)
                    .Distinct()
                    .Count();
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)entityCount / ApplicationDbContext.BatchSize);
            // Go over each batch.
            for (int index = 0; index < count; index++)
            {
                // Check if the cancellation was requested.
                if (token.IsCancellationRequested)
                {
                    // Break.
                    break;
                }
                // Define the batch items.
                var interactions = new List<Interaction>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items in the current batch.
                    interactions = context.NetworkInteractions
                        .Where(item => networkIds.Contains(item.Network.Id))
                        .Where(item => item.Network.Algorithm == NetworkAlgorithm.None)
                        .Select(item => item.Interaction)
                        .Distinct()
                        .Take(ApplicationDbContext.BatchSize)
                        .ToList();
                    // Check if there were no items found.
                    if (interactions == null || !interactions.Any())
                    {
                        // Continue.
                        continue;
                    }
                }
                // Get the IDs of the items.
                var interactionIds = interactions
                    .Select(item => item.Id);
                // Delete the related entities.
                await InteractionExtensions.DeleteRelatedEntitiesAsync<InteractionProtein>(interactionIds, serviceProvider, token);
                await InteractionExtensions.DeleteRelatedEntitiesAsync<DatabaseInteractionFieldInteraction>(interactionIds, serviceProvider, token);
                await InteractionExtensions.DeleteRelatedEntitiesAsync<DatabaseInteraction>(interactionIds, serviceProvider, token);
                // Delete the items.
                await IEnumerableExtensions.DeleteAsync(interactions, serviceProvider, token);
            }
        }

        /// <summary>
        /// Deletes the dependent generic proteins of the corresponding networks.
        /// </summary>
        /// <param name="networkIds">The networks whose entities should be deleted.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public static async Task DeleteDependentProteinsAsync(IEnumerable<string> networkIds, IServiceProvider serviceProvider, CancellationToken token)
        {
            // Define a variable to store the total number of entities.
            var entityCount = 0;
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items in the current batch.
                entityCount = context.NetworkProteins
                    .Where(item => networkIds.Contains(item.Network.Id))
                    .Where(item => item.Network.Algorithm == NetworkAlgorithm.None)
                    .Select(item => item.Protein)
                    .Distinct()
                    .Count();
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)entityCount / ApplicationDbContext.BatchSize);
            // Go over each batch.
            for (int index = 0; index < count; index++)
            {
                // Check if the cancellation was requested.
                if (token.IsCancellationRequested)
                {
                    // Break.
                    break;
                }
                // Define the batch items.
                var proteins = new List<Protein>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items in the current batch.
                    proteins = context.NetworkProteins
                        .Where(item => networkIds.Contains(item.Network.Id))
                        .Where(item => item.Network.Algorithm == NetworkAlgorithm.None)
                        .Select(item => item.Protein)
                        .Distinct()
                        .Take(ApplicationDbContext.BatchSize)
                        .ToList();
                    // Check if there were no items found.
                    if (proteins == null || !proteins.Any())
                    {
                        // Continue.
                        continue;
                    }
                }
                // Get the IDs of the items.
                var proteinIds = proteins
                    .Select(item => item.Id);
                // Delete the related entities.
                await ProteinExtensions.DeleteRelatedEntitiesAsync<DatabaseProteinFieldProtein>(proteinIds, serviceProvider, token);
                await ProteinExtensions.DeleteRelatedEntitiesAsync<DatabaseProtein>(proteinIds, serviceProvider, token);
                // Delete the items.
                await IEnumerableExtensions.DeleteAsync(proteins, serviceProvider, token);
            }
        }

        /// <summary>
        /// Gets the Cytoscape view model corresponding to the provided network.
        /// </summary>
        /// <param name="network">The current network.</param>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="linkGenerator">The link generator.</param>
        /// <param name="context">The application database context.</param>
        /// <returns>The Cytoscape view model corresponding to the provided network.</returns>
        public static CytoscapeViewModel GetCytoscapeViewModel(this Network network, HttpContext httpContext, LinkGenerator linkGenerator, ApplicationDbContext context)
        {
            // Return the view model.
            return new CytoscapeViewModel
            {
                Elements = new CytoscapeViewModel.CytoscapeElements
                {
                    Nodes = context.NetworkProteins
                        .Where(item => item.Network == network)
                        .Where(item => item.Type == NetworkProteinType.None)
                        .Select(item => item.Protein)
                        .Select(item => new
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Classes = item.NetworkProteins
                                .Where(item1 => item1.Network == network)
                                .Select(item => item.Type.ToString().ToLower())
                        })
                        .AsEnumerable()
                        .Select(item => new CytoscapeViewModel.CytoscapeElements.CytoscapeNode
                        {
                            Data = new CytoscapeViewModel.CytoscapeElements.CytoscapeNode.CytoscapeNodeData
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Href = linkGenerator.GetUriByPage(httpContext, $"/AvailableData/Data/Proteins/Details", handler: null, values: new { id = item.Id })
                            },
                            Classes = item.Classes
                        }),
                    Edges = context.NetworkInteractions
                        .Where(item => item.Network == network)
                        .Select(item => item.Interaction)
                        .Select(item => new
                        {
                            Id = item.Id,
                            Name = item.Name,
                            SourceProteinId = item.InteractionProteins
                                .Where(item1 => item1.Type == InteractionProteinType.Source)
                                .Select(item1 => item1.Protein)
                                .Where(item1 => item1 != null)
                                .Select(item1 => item1.Id)
                                .FirstOrDefault(),
                            TargetProteinId = item.InteractionProteins
                                .Where(item1 => item1.Type == InteractionProteinType.Target)
                                .Select(item1 => item1.Protein)
                                .Where(item1 => item1 != null)
                                .Select(item1 => item1.Id)
                                .FirstOrDefault()
                        })
                        .AsEnumerable()
                        .Select(item => new CytoscapeViewModel.CytoscapeElements.CytoscapeEdge
                        {
                            Data = new CytoscapeViewModel.CytoscapeElements.CytoscapeEdge.CytoscapeEdgeData
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Href = linkGenerator.GetUriByPage(httpContext, $"/AvailableData/Data/Interactions/Details", handler: null, values: new { id = item.Id }),
                                Source = item.SourceProteinId,
                                Target = item.TargetProteinId
                            }
                        })
                },
                Layout = CytoscapeViewModel.DefaultLayout,
                Styles = CytoscapeViewModel.DefaultStyles.Concat(CytoscapeViewModel.DefaultNetworkStyles)
            };
        }

        /// <summary>
        /// Gets the content of an overview text file to download corresponding to the provided networks.
        /// </summary>
        /// <param name="networkIds">The IDs of the current networks.</param>
        /// <param name="stream">The stream to which to write to.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="scheme">The HTTP context scheme.</param>
        /// <param name="host">The HTTP context host.</param>
        /// <returns></returns>
        public static async Task WriteToStreamOverviewTextFileContent(IEnumerable<string> networkIds, Stream stream, IServiceProvider serviceProvider, string scheme, HostString host)
        {
            // Use a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Use a new link generator instance.
            var linkGenerator = scope.ServiceProvider.GetRequiredService<LinkGenerator>();
            // Define the stream writer for the file.
            using var streamWriter = new StreamWriter(stream);
            // Get the required data.
            var data = string.Concat("The following networks have been downloaded:\n\n", string.Join("\n", context.Networks
                .Where(item => networkIds.Contains(item.Id))
                .Select(item => new
                {
                    Id = item.Id,
                    Name = item.Name
                })
                .AsEnumerable()
                .Select(item => $"{item.Name} - {linkGenerator.GetUriByPage($"/AvailableData/Created/Networks/Details/Index", handler: null, values: new { id = item.Id }, scheme: scheme, host: host)}")));
            // Write the data to the stream.
            await streamWriter.WriteAsync(data);
        }

        /// <summary>
        /// Gets the content of a text file to download corresponding to the provided network.
        /// </summary>
        /// <param name="network">The current network.</param>
        /// <param name="stream">The stream to which to write to.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <returns>The content of the text file corresponding to the provided network.</returns>
        public static async Task WriteToStreamTxtFileContent(this Network network, Stream stream, IServiceProvider serviceProvider)
        {
            // Use a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Define the stream writer for the file.
            using var streamWriter = new StreamWriter(stream);
            // Get the required data.
            var data = string.Join("\n", context.NetworkInteractions
                .Where(item => item.Network == network)
                .Select(item => item.Interaction)
                .Select(item => new
                {
                    SourceProteinName = item.InteractionProteins
                        .Where(item1 => item1.Type == InteractionProteinType.Source)
                        .Select(item1 => item1.Protein)
                        .Where(item1 => item1 != null)
                        .Select(item1 => item1.Name)
                        .FirstOrDefault(),
                    TargetProteinName = item.InteractionProteins
                        .Where(item1 => item1.Type == InteractionProteinType.Target)
                        .Select(item1 => item1.Protein)
                        .Where(item1 => item1 != null)
                        .Select(item1 => item1.Name)
                        .FirstOrDefault()
                })
                .Where(item => !string.IsNullOrEmpty(item.SourceProteinName) && !string.IsNullOrEmpty(item.TargetProteinName))
                .Select(item => $"{item.SourceProteinName};{item.TargetProteinName}"));
            // Write the data to the stream.
            await streamWriter.WriteAsync(data);
        }

        /// <summary>
        /// Gets the content of a SIF file to download corresponding to the provided network.
        /// </summary>
        /// <param name="network">The current network.</param>
        /// <param name="stream">The stream to which to write to.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <returns>The content of the SIF file corresponding to the provided network.</returns>
        public static async Task WriteToStreamSifFileContent(this Network network, Stream stream, IServiceProvider serviceProvider)
        {
            // Use a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Define the stream writer for the file.
            using var streamWriter = new StreamWriter(stream);
            // Get the required data.
            var data = string.Join("\n", context.NetworkInteractions
                .Where(item => item.Network == network)
                .Select(item => item.Interaction)
                .Select(item => new
                {
                    SourceProteinName = item.InteractionProteins
                        .Where(item1 => item1.Type == InteractionProteinType.Source)
                        .Select(item1 => item1.Protein)
                        .Where(item1 => item1 != null)
                        .Select(item1 => item1.Name)
                        .FirstOrDefault(),
                    TargetProteinName = item.InteractionProteins
                        .Where(item1 => item1.Type == InteractionProteinType.Target)
                        .Select(item1 => item1.Protein)
                        .Where(item1 => item1 != null)
                        .Select(item1 => item1.Name)
                        .FirstOrDefault()
                })
                .Where(item => !string.IsNullOrEmpty(item.SourceProteinName) && !string.IsNullOrEmpty(item.TargetProteinName))
                .Select(item => $"{item.SourceProteinName}\tinteracts with\t{item.TargetProteinName}"));
            // Write the data to the stream.
            await streamWriter.WriteAsync(data);
        }

        /// <summary>
        /// Gets the content of a JSON file to download corresponding to the provided network.
        /// </summary>
        /// <param name="network">The current network.</param>
        /// <param name="stream">The stream to which to write to.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <returns>The content of the JSON file corresponding to the provided network.</returns>
        public static async Task WriteToStreamJsonFileContent(this Network network, Stream stream, IServiceProvider serviceProvider)
        {
            // Use a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Get the required data.
            var data = new
            {
                Type = "Network",
                Id = network.Id,
                Name = network.Name,
                Description = network.Description,
                IsPublic = network.IsPublic,
                Algorithm = network.Algorithm.ToString(),
                ProteinDatabaseData = context.NetworkDatabases
                    .Where(item => item.Network == network)
                    .Where(item => item.Type == NetworkDatabaseType.Protein)
                    .Select(item => item.Database.Id),
                InteractionDatabaseData = context.NetworkDatabases
                    .Where(item => item.Network == network)
                    .Where(item => item.Type == NetworkDatabaseType.Interaction)
                    .Select(item => item.Database.Id),
                SeedProteinData = context.NetworkProteins
                    .Where(item => item.Network == network)
                    .Where(item => item.Type == NetworkProteinType.Seed)
                    .Select(item => item.Protein.Name),
                SeedProteinCollectionData = context.NetworkProteinCollections
                    .Where(item => item.Network == network)
                    .Where(item => item.Type == NetworkProteinCollectionType.Seed)
                    .Select(item => item.ProteinCollection.Id),
                SeedInteractionData = context.NetworkInteractions
                    .Where(item => item.Network == network)
                    .Select(item => item.Interaction)
                    .Select(item => new
                    {
                        SourceProtein = item.InteractionProteins
                            .Where(item1 => item1.Type == InteractionProteinType.Source)
                            .Select(item1 => item1.Protein)
                            .Where(item1 => item1 != null)
                            .Select(item1 => item1.Name)
                            .FirstOrDefault(),
                        TargetProtein = item.InteractionProteins
                            .Where(item1 => item1.Type == InteractionProteinType.Target)
                            .Select(item1 => item1.Protein)
                            .Where(item1 => item1 != null)
                            .Select(item1 => item1.Name)
                            .FirstOrDefault()
                    })
                    .Where(item => !string.IsNullOrEmpty(item.SourceProtein) && !string.IsNullOrEmpty(item.TargetProtein))
            };
            // Write the data corresponding to the file.
            await JsonSerializer.SerializeAsync(stream, data, new JsonSerializerOptions { IgnoreNullValues = true });
        }

        /// <summary>
        /// Gets the content of a JSON file to download corresponding to the provided network.
        /// </summary>
        /// <param name="network">The current network.</param>
        /// <param name="stream">The stream to which to write to.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <returns>The content of the JSON file corresponding to the provided network.</returns>
        public static async Task WriteToStreamCyjsFileContent(this Network network, Stream stream, IServiceProvider serviceProvider)
        {
            // Use a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Get the required data.
            var data = new CytoscapeViewModel
            {
                Data = new CytoscapeViewModel.CytoscapeData
                {
                    Id = network.Id,
                    Name = network.Name,
                    Description = network.Description
                },
                Elements = new CytoscapeViewModel.CytoscapeElements
                {
                    Nodes = context.NetworkProteins
                        .Where(item => item.Network == network)
                        .Where(item => item.Type == NetworkProteinType.None)
                        .Select(item => item.Protein)
                        .Select(item => new
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Classes = item.NetworkProteins
                                .Where(item1 => item1.Network == network)
                                .Select(item => item.Type.ToString().ToLower())
                        })
                        .AsEnumerable()
                        .Select(item => new CytoscapeViewModel.CytoscapeElements.CytoscapeNode
                        {
                            Data = new CytoscapeViewModel.CytoscapeElements.CytoscapeNode.CytoscapeNodeData
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Type = string.Join(",", item.Classes.OrderBy(item => item))
                            }
                        }),
                    Edges = context.NetworkInteractions
                        .Where(item => item.Network == network)
                        .Select(item => item.Interaction)
                        .Select(item => new
                        {
                            Id = item.Id,
                            Name = item.Name,
                            SourceProteinId = item.InteractionProteins
                                .Where(item1 => item1.Type == InteractionProteinType.Source)
                                .Select(item1 => item1.Protein)
                                .Where(item1 => item1 != null)
                                .Select(item1 => item1.Id)
                                .FirstOrDefault(),
                            TargetProteinId = item.InteractionProteins
                                .Where(item1 => item1.Type == InteractionProteinType.Target)
                                .Select(item1 => item1.Protein)
                                .Where(item1 => item1 != null)
                                .Select(item1 => item1.Id)
                                .FirstOrDefault()
                        })
                        .AsEnumerable()
                        .Select(item => new CytoscapeViewModel.CytoscapeElements.CytoscapeEdge
                        {
                            Data = new CytoscapeViewModel.CytoscapeElements.CytoscapeEdge.CytoscapeEdgeData
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Source = item.SourceProteinId,
                                Target = item.TargetProteinId
                            }
                        })
                }
            };
            // Write the data corresponding to the file.
            await JsonSerializer.SerializeAsync(stream, data, new JsonSerializerOptions { IgnoreNullValues = true });
        }

        /// <summary>
        /// Gets the content of an Excel file to download corresponding to the provided network.
        /// </summary>
        /// <param name="network">The current network.</param>
        /// <param name="user">The current user.</param>
        /// <param name="stream">The stream to which to write to.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <returns>The content of the Excel file corresponding to the provided network.</returns>
        public static async Task WriteToStreamXlsxFileContent(this Network network, User user, Stream stream, IServiceProvider serviceProvider)
        {
            // Use a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Get the required data.
            var databaseProteinFields = context.NetworkDatabases
                .Where(item => item.Network == network)
                .Where(item => item.Type == NetworkDatabaseType.Protein)
                .Where(item => item.Database.IsPublic || item.Database.DatabaseUsers.Any(item => item.User == user))
                .Select(item => item.Database.DatabaseProteinFields)
                .SelectMany(item => item)
                .ToList();
            var databaseInteractionFields = context.NetworkDatabases
                .Where(item => item.Network == network)
                .Where(item => item.Type == NetworkDatabaseType.Interaction)
                .Where(item => item.Database.IsPublic || item.Database.DatabaseUsers.Any(item => item.User == user))
                .Select(item => item.Database.DatabaseInteractionFields)
                .SelectMany(item => item)
                .ToList();
            // Define the rows in the first sheet.
            var worksheet1Rows = new List<List<string>>
            {
                new List<string>
                {
                    "Internal ID",
                    network.Id
                },
                new List<string>
                {
                    "Date",
                    network.DateTimeCreated.ToString()
                },
                new List<string>
                {
                    "Name",
                    network.Name
                },
                new List<string>
                {
                    "Description",
                    network.Description
                },
                new List<string>
                {
                    "Algorithm",
                    network.Algorithm.ToString()
                }
            };
            // Define the rows in the second sheet.
            var worksheet2Rows = new List<List<string>>
            {
                new List<string>
                {
                    "Internal ID",
                    "Name",
                    "Type"
                }
            }.Concat(context.NetworkDatabases
                .Where(item => item.Network == network)
                .Where(item => item.Database.IsPublic || item.Database.DatabaseUsers.Any(item => item.User == user))
                .Select(item => item.Database)
                .Select(item => new
                {
                    Id = item.Id,
                    Name = item.Name,
                    Types = item.NetworkDatabases
                        .Where(item1 => item1.Network == network)
                        .Select(item => item.Type.ToString().ToLower())
                })
                .AsEnumerable()
                .Select(item => new List<string>
                {
                    item.Id,
                    item.Name,
                    string.Join(",", item.Types)
                })
                .ToList())
            .ToList();
            // Define the rows in the third sheet.
            var worksheet3Rows = new List<List<string>>
            {
                new List<string>
                {
                    "Internal ID",
                    "Name",
                    "Type"
                }.Concat(databaseProteinFields
                    .Select(item => item.Name)
                    .ToList())
                .ToList()
            }
            .Concat(context.NetworkProteins
                .Where(item => item.Network == network)
                .Where(item => item.Type == NetworkProteinType.None)
                .Select(item => item.Protein)
                .Select(item => new
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Values = item.DatabaseProteinFieldProteins
                        .Select(item1 => new
                        {
                            DatabaseProteinFieldId = item1.DatabaseProteinField.Id,
                            Value = item1.Value
                        }),
                    Types = item.NetworkProteins
                        .Where(item1 => item1.Network == network)
                        .Select(item => item.Type.ToString().ToLower())
                })
                .AsEnumerable()
                .Select(item => new List<string>
                    {
                        item.Id,
                        item.Name,
                        string.Join(",", item.Types)
                    }.Concat(databaseProteinFields
                        .Select(item1 => item.Values.FirstOrDefault(item2 => item2.DatabaseProteinFieldId == item1.Id))
                        .Select(item1 => item1 == null ? string.Empty : item1.Value)
                        .ToList())))
            .ToList();
            // Define the rows in the fourth sheet.
            var worksheet4Rows = new List<List<string>>
            {
                new List<string>
                {
                    "Internal ID",
                    $"Source protein ID",
                    $"Source protein name",
                    $"Target protein ID",
                    $"Target protein name"
                }.Concat(databaseInteractionFields
                    .Select(item => item.Name)
                    .ToList())
                .ToList()
            }
            .Concat(context.NetworkInteractions
                .Where(item => item.Network == network)
                .Select(item => item.Interaction)
                .Select(item => new
                {
                    Id = item.Id,
                    SourceProtein = item.InteractionProteins
                        .Where(item1 => item1.Type == InteractionProteinType.Source)
                        .Select(item1 => item1.Protein)
                        .FirstOrDefault(),
                    TargetProtein = item.InteractionProteins
                        .Where(item1 => item1.Type == InteractionProteinType.Target)
                        .Select(item1 => item1.Protein)
                        .FirstOrDefault(),
                    Values = item.DatabaseInteractionFieldInteractions
                        .Select(item1 => new
                        {
                            DatabaseInteractionFieldId = item1.DatabaseInteractionField.Id,
                            Value = item1.Value
                        })
                })
                .Where(item => item.SourceProtein != null && item.TargetProtein != null)
                .AsEnumerable()
                .Select(item => new List<string>
                {
                    item.Id,
                    item.SourceProtein.Id,
                    item.SourceProtein.Name,
                    item.TargetProtein.Id,
                    item.TargetProtein.Name
                }.Concat(databaseInteractionFields
                    .Select(item1 => item.Values.FirstOrDefault(item2 => item2.DatabaseInteractionFieldId == item1.Id))
                    .Select(item1 => item1 == null ? string.Empty : item1.Value)
                    .ToList())))
            .ToList();
            // Define the stream for the file.
            var fileStream = new MemoryStream();
            // Define the Excel file.
            using SpreadsheetDocument document = SpreadsheetDocument.Create(fileStream, SpreadsheetDocumentType.Workbook);
            // Definte a new workbook part.
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();
            var worksheets = workbookPart.Workbook.AppendChild(new Sheets());
            // Define the first worksheet.
            var worksheet1Part = workbookPart.AddNewPart<WorksheetPart>();
            var worksheet1Data = new SheetData();
            var worksheet1 = new Sheet { Id = workbookPart.GetIdOfPart(worksheet1Part), SheetId = 1, Name = "Details" };
            worksheet1Part.Worksheet = new Worksheet(worksheet1Data);
            worksheet1Data.Append(worksheet1Rows.Select(item => new Row(item.Select(item1 => new Cell { DataType = CellValues.String, CellValue = new CellValue(item1) }))));
            worksheets.Append(worksheet1);
            // Define the second worksheet.
            var worksheet2Part = workbookPart.AddNewPart<WorksheetPart>();
            var worksheet2Data = new SheetData();
            var worksheet2 = new Sheet { Id = workbookPart.GetIdOfPart(worksheet2Part), SheetId = 2, Name = "Databases" };
            worksheet2Part.Worksheet = new Worksheet(worksheet2Data);
            worksheet2Data.Append(worksheet2Rows.Select(item => new Row(item.Select(item1 => new Cell { DataType = CellValues.String, CellValue = new CellValue(item1) }))));
            worksheets.Append(worksheet2);
            // Define the third worksheet.
            var worksheet3Part = workbookPart.AddNewPart<WorksheetPart>();
            var worksheet3Data = new SheetData();
            var worksheet3 = new Sheet { Id = workbookPart.GetIdOfPart(worksheet3Part), SheetId = 3, Name = $"Proteins" };
            worksheet3Part.Worksheet = new Worksheet(worksheet3Data);
            worksheet3Data.Append(worksheet3Rows.Select(item => new Row(item.Select(item1 => new Cell { DataType = CellValues.String, CellValue = new CellValue(item1) }))));
            worksheets.Append(worksheet3);
            // Define the fourth worksheet.
            var worksheet4Part = workbookPart.AddNewPart<WorksheetPart>();
            var worksheet4Data = new SheetData();
            var worksheet4 = new Sheet { Id = workbookPart.GetIdOfPart(worksheet4Part), SheetId = 4, Name = $"Interactions" };
            worksheet4Part.Worksheet = new Worksheet(worksheet4Data);
            worksheet4Data.Append(worksheet4Rows.Select(item => new Row(item.Select(item1 => new Cell { DataType = CellValues.String, CellValue = new CellValue(item1) }))));
            worksheets.Append(worksheet4);
            // Close the document.
            document.Close();
            // Reset the stream position.
            fileStream.Position = 0;
            // Copy it to the archive stream.
            await fileStream.CopyToAsync(stream);
        }
    }
}
