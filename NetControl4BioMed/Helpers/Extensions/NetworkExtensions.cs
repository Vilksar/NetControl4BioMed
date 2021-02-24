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
using System.Text.Json.Serialization;
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
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="linkGenerator">The link generator.</param>
        /// <param name="context">The application database context.</param>
        /// <returns>The Cytoscape view model corresponding to the provided network.</returns>
        public static CytoscapeViewModel GetCytoscapeViewModel(this Network network, HttpContext httpContext, LinkGenerator linkGenerator, ApplicationDbContext context)
        {
            // Get the required values.
            var databaseTypeName = context.NetworkDatabases
                .Where(item => item.Network == network)
                .Select(item => item.Database.DatabaseType.Name)
                .First();
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
                                Href = linkGenerator.GetUriByPage(httpContext, $"/Content/DatabaseTypes/{databaseTypeName}/Data/Nodes/Details", handler: null, values: new { id = item.Id })
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
                                Href = linkGenerator.GetUriByPage(httpContext, $"/Content/DatabaseTypes/{databaseTypeName}/Data/Edges/Details", handler: null, values: new { id = item.Id }),
                                Source = item.SourceNodeId,
                                Target = item.TargetNodeId
                            }
                        })
                },
                Layout = CytoscapeViewModel.DefaultLayout,
                Styles = CytoscapeViewModel.DefaultStyles.Concat(CytoscapeViewModel.DefaultNetworkStyles)
            };
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
            var data = string.Join("\n", context.NetworkEdges
                .Where(item => item.Network == network)
                .Select(item => item.Edge)
                .Select(item => new
                {
                    SourceNodeName = item.EdgeNodes
                        .Where(item1 => item1.Type == EdgeNodeType.Source)
                        .Select(item1 => item1.Node)
                        .Where(item1 => item1 != null)
                        .Select(item1 => item1.Name)
                        .FirstOrDefault(),
                    TargetNodeName = item.EdgeNodes
                        .Where(item1 => item1.Type == EdgeNodeType.Target)
                        .Select(item1 => item1.Node)
                        .Where(item1 => item1 != null)
                        .Select(item1 => item1.Name)
                        .FirstOrDefault()
                })
                .Where(item => !string.IsNullOrEmpty(item.SourceNodeName) && !string.IsNullOrEmpty(item.TargetNodeName))
                .Select(item => $"{item.SourceNodeName};{item.TargetNodeName}"));
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
            var data = string.Join("\n", context.NetworkEdges
                .Where(item => item.Network == network)
                .Select(item => item.Edge)
                .Select(item => new
                {
                    SourceNodeName = item.EdgeNodes
                        .Where(item1 => item1.Type == EdgeNodeType.Source)
                        .Select(item1 => item1.Node)
                        .Where(item1 => item1 != null)
                        .Select(item1 => item1.Name)
                        .FirstOrDefault(),
                    TargetNodeName = item.EdgeNodes
                        .Where(item1 => item1.Type == EdgeNodeType.Target)
                        .Select(item1 => item1.Node)
                        .Where(item1 => item1 != null)
                        .Select(item1 => item1.Name)
                        .FirstOrDefault()
                })
                .Where(item => !string.IsNullOrEmpty(item.SourceNodeName) && !string.IsNullOrEmpty(item.TargetNodeName))
                .Select(item => $"{item.SourceNodeName}\tinteracts with\t{item.TargetNodeName}"));
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
                DatabaseType = context.NetworkDatabases
                    .Where(item => item.Network == network)
                    .Select(item => item.Database.DatabaseType.Name)
                    .FirstOrDefault(),
                NodeDatabaseData = context.NetworkDatabases
                    .Where(item => item.Network == network)
                    .Where(item => item.Type == NetworkDatabaseType.Node)
                    .Select(item => item.Database.Id),
                EdgeDatabaseData = context.NetworkDatabases
                    .Where(item => item.Network == network)
                    .Where(item => item.Type == NetworkDatabaseType.Edge)
                    .Select(item => item.Database.Id),
                SeedNodeData = context.NetworkNodes
                    .Where(item => item.Network == network)
                    .Where(item => item.Type == NetworkNodeType.Seed)
                    .Select(item => item.Node.Name),
                SeedNodeCollectionData = context.NetworkNodeCollections
                    .Where(item => item.Network == network)
                    .Where(item => item.Type == NetworkNodeCollectionType.Seed)
                    .Select(item => item.NodeCollection.Id),
                SeedEdgeData = context.NetworkEdges
                    .Where(item => item.Network == network)
                    .Select(item => item.Edge)
                    .Select(item => new
                    {
                        SourceNode = item.EdgeNodes
                            .Where(item1 => item1.Type == EdgeNodeType.Source)
                            .Select(item1 => item1.Node)
                            .Where(item1 => item1 != null)
                            .Select(item1 => item1.Name)
                            .FirstOrDefault(),
                        TargetNode = item.EdgeNodes
                            .Where(item1 => item1.Type == EdgeNodeType.Target)
                            .Select(item1 => item1.Node)
                            .Where(item1 => item1 != null)
                            .Select(item1 => item1.Name)
                            .FirstOrDefault()
                    })
                    .Where(item => !string.IsNullOrEmpty(item.SourceNode) && !string.IsNullOrEmpty(item.TargetNode))
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
                    Nodes = context.NetworkNodes
                        .Where(item => item.Network == network)
                        .Where(item => item.Type == NetworkNodeType.None)
                        .Select(item => item.Node)
                        .Select(item => new
                        {
                            Id = item.Id,
                            Name = item.Name,
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
                                Type = string.Join(",", item.Classes.OrderBy(item => item))
                            }
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
                                Target = item.TargetNodeId
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
        /// <param name="stream">The stream to which to write to.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <returns>The content of the Excel file corresponding to the provided network.</returns>
        public static async Task WriteToStreamXlsxFileContent(this Network network, Stream stream, IServiceProvider serviceProvider)
        {
            // Use a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Get the required data.
            var databaseTypeName = context.NetworkDatabases
                .Where(item => item.Network == network)
                .Select(item => item.Database.DatabaseType.Name)
                .FirstOrDefault();
            var databaseNodeFields = context.NetworkDatabases
                .Where(item => item.Network == network)
                .Where(item => item.Type == NetworkDatabaseType.Node)
                .Select(item => item.Database.DatabaseNodeFields)
                .SelectMany(item => item)
                .ToList();
            var databaseEdgeFields = context.NetworkDatabases
                .Where(item => item.Network == network)
                .Where(item => item.Type == NetworkDatabaseType.Edge)
                .Select(item => item.Database.DatabaseEdgeFields)
                .SelectMany(item => item)
                .ToList();
            // Get the required variables.
            var nodeTitle = databaseTypeName == "PPI" ? "Protein" : "Node";
            var edgeTitle = databaseTypeName == "PPI" ? "Interaction" : "Edge";
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
                .Select(item => new
                {
                    DatabaseId = item.Database.Id,
                    DatabaseName = item.Database.Name,
                    Type = item.Type
                })
                .AsEnumerable()
                .Select(item => new List<string>
                {
                    item.DatabaseId,
                    item.DatabaseName,
                    item.Type == NetworkDatabaseType.Node ? nodeTitle : item.Type == NetworkDatabaseType.Edge ? edgeTitle : string.Empty
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
                }.Concat(databaseNodeFields
                    .Select(item => item.Name)
                    .ToList())
                .ToList()
            }
            .Concat(context.NetworkNodes
                .Where(item => item.Network == network)
                .Where(item => item.Type == NetworkNodeType.None)
                .Select(item => item.Node)
                .Select(item => new
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Values = item.DatabaseNodeFieldNodes
                        .Select(item1 => new
                        {
                            DatabaseNodeFieldId = item1.DatabaseNodeField.Id,
                            Value = item1.Value
                        }),
                    Types = item.NetworkNodes
                        .Where(item1 => item1.Network == network)
                        .Select(item => item.Type.ToString().ToLower())
                })
                .AsEnumerable()
                .Select(item => new List<string>
                    {
                        item.Id,
                        item.Name,
                        string.Join(",", item.Types)
                    }.Concat(databaseNodeFields
                        .Select(item1 => item.Values.FirstOrDefault(item2 => item2.DatabaseNodeFieldId == item1.Id))
                        .Select(item1 => item1 == null ? string.Empty : item1.Value)
                        .ToList())))
            .ToList();
            // Define the rows in the fourth sheet.
            var worksheet4Rows = new List<List<string>>
            {
                new List<string>
                {
                    "Internal ID",
                    $"Source {nodeTitle.ToLower()} ID",
                    $"Source {nodeTitle.ToLower()} name",
                    $"Target {nodeTitle.ToLower()} ID",
                    $"Target {nodeTitle.ToLower()} name"
                }.Concat(databaseEdgeFields
                    .Select(item => item.Name)
                    .ToList())
                .ToList()
            }
            .Concat(context.NetworkEdges
                .Where(item => item.Network == network)
                .Select(item => item.Edge)
                .Select(item => new
                {
                    Id = item.Id,
                    SourceNode = item.EdgeNodes
                        .Where(item1 => item1.Type == EdgeNodeType.Source)
                        .Select(item1 => item1.Node)
                        .FirstOrDefault(),
                    TargetNode = item.EdgeNodes
                        .Where(item1 => item1.Type == EdgeNodeType.Target)
                        .Select(item1 => item1.Node)
                        .FirstOrDefault(),
                    Values = item.DatabaseEdgeFieldEdges
                        .Select(item1 => new
                        {
                            DatabaseEdgeFieldId = item1.DatabaseEdgeField.Id,
                            Value = item1.Value
                        })
                })
                .Where(item => item.SourceNode != null && item.TargetNode != null)
                .AsEnumerable()
                .Select(item => new List<string>
                {
                    item.Id,
                    item.SourceNode.Id,
                    item.SourceNode.Name,
                    item.TargetNode.Id,
                    item.TargetNode.Name
                }.Concat(databaseEdgeFields
                    .Select(item1 => item.Values.FirstOrDefault(item2 => item2.DatabaseEdgeFieldId == item1.Id))
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
            var worksheet3 = new Sheet { Id = workbookPart.GetIdOfPart(worksheet3Part), SheetId = 3, Name = $"{nodeTitle}s" };
            worksheet3Part.Worksheet = new Worksheet(worksheet3Data);
            worksheet3Data.Append(worksheet3Rows.Select(item => new Row(item.Select(item1 => new Cell { DataType = CellValues.String, CellValue = new CellValue(item1) }))));
            worksheets.Append(worksheet3);
            // Define the fourth worksheet.
            var worksheet4Part = workbookPart.AddNewPart<WorksheetPart>();
            var worksheet4Data = new SheetData();
            var worksheet4 = new Sheet { Id = workbookPart.GetIdOfPart(worksheet4Part), SheetId = 4, Name = $"{edgeTitle}s" };
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
