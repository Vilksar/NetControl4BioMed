using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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
                entityCount = context.AnalysisNetworks
                    .Where(item => networkIds.Contains(item.Network.Id))
                    .Select(item => item.Analysis)
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
                    batchItemInputs = context.AnalysisNetworks
                        .Where(item => networkIds.Contains(item.Network.Id))
                        .Select(item => item.Analysis)
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
        /// Deletes the dependent generic edges of the corresponding networks.
        /// </summary>
        /// <param name="networkIds">The networks whose entities should be deleted.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public static async Task DeleteDependentGenericEdgesAsync(IEnumerable<string> networkIds, IServiceProvider serviceProvider, CancellationToken token)
        {
            // Define a variable to store the total number of entities.
            var entityCount = 0;
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items in the current batch.
                entityCount = context.NetworkEdges
                    .Where(item => networkIds.Contains(item.Network.Id))
                    .Where(item => item.Network.NetworkDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Select(item => item.Edge)
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
                var edges = new List<Edge>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items in the current batch.
                    edges = context.NetworkEdges
                        .Where(item => networkIds.Contains(item.Network.Id))
                        .Where(item => item.Network.NetworkDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                        .Select(item => item.Edge)
                        .Distinct()
                        .Take(ApplicationDbContext.BatchSize)
                        .ToList();
                    // Check if there were no items found.
                    if (edges == null || !edges.Any())
                    {
                        // Continue.
                        continue;
                    }
                }
                // Get the IDs of the items.
                var edgeIds = edges
                    .Select(item => item.Id);
                // Delete the related entities.
                await EdgeExtensions.DeleteRelatedEntitiesAsync<EdgeNode>(edgeIds, serviceProvider, token);
                await EdgeExtensions.DeleteRelatedEntitiesAsync<DatabaseEdgeFieldEdge>(edgeIds, serviceProvider, token);
                await EdgeExtensions.DeleteRelatedEntitiesAsync<DatabaseEdge>(edgeIds, serviceProvider, token);
                // Delete the items.
                await IEnumerableExtensions.DeleteAsync(edges, serviceProvider, token);
            }
        }

        /// <summary>
        /// Deletes the dependent generic nodes of the corresponding networks.
        /// </summary>
        /// <param name="networkIds">The networks whose entities should be deleted.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public static async Task DeleteDependentGenericNodesAsync(IEnumerable<string> networkIds, IServiceProvider serviceProvider, CancellationToken token)
        {
            // Define a variable to store the total number of entities.
            var entityCount = 0;
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items in the current batch.
                entityCount = context.NetworkNodes
                    .Where(item => networkIds.Contains(item.Network.Id))
                    .Where(item => item.Network.NetworkDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Select(item => item.Node)
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
                var nodes = new List<Node>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items in the current batch.
                    nodes = context.NetworkNodes
                        .Where(item => networkIds.Contains(item.Network.Id))
                        .Where(item => item.Network.NetworkDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                        .Select(item => item.Node)
                        .Distinct()
                        .Take(ApplicationDbContext.BatchSize)
                        .ToList();
                    // Check if there were no items found.
                    if (nodes == null || !nodes.Any())
                    {
                        // Continue.
                        continue;
                    }
                }
                // Get the IDs of the items.
                var nodeIds = nodes
                    .Select(item => item.Id);
                // Delete the related entities.
                await NodeExtensions.DeleteRelatedEntitiesAsync<DatabaseNodeFieldNode>(nodeIds, serviceProvider, token);
                await NodeExtensions.DeleteRelatedEntitiesAsync<DatabaseNode>(nodeIds, serviceProvider, token);
                // Delete the items.
                await IEnumerableExtensions.DeleteAsync(nodes, serviceProvider, token);
            }
        }

        /// <summary>
        /// Gets the Cytoscape view model corresponding to the provided network.
        /// </summary>
        /// <param name="network">The current network.</param>
        /// <param name="linkGenerator">The link generator.</param>
        /// <returns>The Cytoscape view model corresponding to the provided network.</returns>
        public static CytoscapeViewModel GetCytoscapeViewModel(this Network network, HttpContext httpContext, LinkGenerator linkGenerator, ApplicationDbContext context)
        {
            // Get the default values.
            var emptyEnumerable = Enumerable.Empty<string>();
            var interactionType = context.NetworkDatabases
                .Where(item => item.Network == network)
                .Select(item => item.Database.DatabaseType.Name)
                .FirstOrDefault();
            var isGeneric = interactionType == "Generic";
            // Return the view model.
            return new CytoscapeViewModel
            {
                Elements = new CytoscapeViewModel.CytoscapeElements
                {
                    Nodes = context.NetworkNodes
                        .Where(item => item.Network == network)
                        .Where(item => item.Type == NetworkNodeType.None)
                        .Select(item => item.Node)
                        .Select(item => new
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Alias = item.DatabaseNodeFieldNodes
                                .Where(item1 => item1.DatabaseNodeField.IsSearchable)
                                .Select(item1 => item1.Value),
                            Classes = item.NetworkNodes
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
                                Href = isGeneric ? string.Empty : linkGenerator.GetUriByPage(httpContext, "/Content/Data/Nodes/Details", handler: null, values: new { id = item.Id }),
                                Alias = item.Alias
                            },
                            Classes = item.Classes
                        }),
                    Edges = context.NetworkEdges
                        .Where(item => item.Network == network)
                        .Select(item => item.Edge)
                        .Select(item => new
                        {
                            Id = item.Id,
                            Name = item.Name,
                            SourceNodeId = item.EdgeNodes
                                .Where(item1 => item1.Type == EdgeNodeType.Source)
                                .Select(item1 => item1.Node)
                                .Where(item1 => item1 != null)
                                .Select(item1 => item1.Id)
                                .FirstOrDefault(),
                            TargetNodeId = item.EdgeNodes
                                .Where(item1 => item1.Type == EdgeNodeType.Target)
                                .Select(item1 => item1.Node)
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
                                Source = item.SourceNodeId,
                                Target = item.TargetNodeId,
                                Interaction = interactionType
                            },
                            Classes = emptyEnumerable
                        })
                },
                Layout = CytoscapeViewModel.DefaultLayout,
                Styles = CytoscapeViewModel.DefaultStyles.Concat(CytoscapeViewModel.DefaultNetworkStyles)
            };
        }
    }
}
