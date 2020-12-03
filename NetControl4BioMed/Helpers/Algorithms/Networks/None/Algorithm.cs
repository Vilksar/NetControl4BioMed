using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Exceptions;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.InputModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Algorithms.Networks.None
{
    /// <summary>
    /// Defines the algorithm.
    /// </summary>
    public static class Algorithm
    {
        /// <summary>
        /// Runs the algorithm on the network with the provided details, using the given parameters.
        /// </summary>
        /// <param name="networkId">The ID of the network on which to run the algorithm.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        public static async Task Run(string networkId, IServiceProvider serviceProvider, CancellationToken token)
        {
            // Define the required data.
            var data = Enumerable.Empty<NetworkEdgeInputModel>();
            var nodeDatabases = Enumerable.Empty<Database>();
            var edgeDatabases = Enumerable.Empty<Database>();
            var databaseNodeFields = Enumerable.Empty<DatabaseNodeField>();
            var databaseEdgeFields = Enumerable.Empty<DatabaseEdgeField>();
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Reload the network.
                var network = context.Networks
                    .FirstOrDefault(item => item.Id == networkId);
                // Check if there was no item found.
                if (network == null)
                {
                    // Return.
                    return;
                }
                // Get the related data.
                var databaseTypes = context.NetworkDatabases
                    .Where(item => item.Network == network)
                    .Select(item => item.Database.DatabaseType);
                // Check if there was any error in retrieving related data from the database.
                if (databaseTypes == null)
                {
                    // Update the status of the item.
                    network.Status = NetworkStatus.Error;
                    // Add a message to the log.
                    network.Log = network.AppendToLog("There was an error in retrieving related data from the database.");
                    // Edit the network.
                    await IEnumerableExtensions.EditAsync(network.Yield(), context, token);
                    // End the function.
                    return;
                }
                // Check if there weren't any database types found.
                if (!databaseTypes.Any())
                {
                    // Update the status of the item.
                    network.Status = NetworkStatus.Error;
                    // Add a message to the log.
                    network.Log = network.AppendToLog("No database types corresponding to the network databases could be found.");
                    // Edit the network.
                    await IEnumerableExtensions.EditAsync(network.Yield(), context, token);
                    // End the function.
                    return;
                }
                // Check if the database types are different.
                if (databaseTypes.Distinct().Count() > 1)
                {
                    // Update the status of the item.
                    network.Status = NetworkStatus.Error;
                    // Add a message to the log.
                    network.Log = network.AppendToLog("The database types corresponding to the network databases are different.");
                    // Edit the network.
                    await IEnumerableExtensions.EditAsync(network.Yield(), context, token);
                    // End the function.
                    return;
                }
                // Check if the database types are not valid.
                if (databaseTypes.Any(item => item.Name != "Generic"))
                {
                    // Update the status of the item.
                    network.Status = NetworkStatus.Error;
                    // Add a message to the log.
                    network.Log = network.AppendToLog("The database type corresponding to the network databases and the network algorithm don't match.");
                    // Edit the network.
                    await IEnumerableExtensions.EditAsync(network.Yield(), context, token);
                    // End the function.
                    return;
                }
                // Try to deserialize the data.
                if (!network.Data.TryDeserializeJsonObject<IEnumerable<NetworkEdgeInputModel>>(out data) || data == null)
                {
                    // Update the status of the item.
                    network.Status = NetworkStatus.Error;
                    // Add a message to the log.
                    network.Log = network.AppendToLog("The seed data corresponding to the network could not be deserialized.");
                    // Edit the network.
                    await IEnumerableExtensions.EditAsync(network.Yield(), context, token);
                    // End the function.
                    return;
                }
                // Get the related entities.
                nodeDatabases = context.NetworkDatabases
                    .Where(item => item.Network == network)
                    .Where(item1 => item1.Type == NetworkDatabaseType.Node)
                    .Select(item1 => item1.Database)
                    .Distinct()
                    .AsEnumerable();
                edgeDatabases = context.NetworkDatabases
                    .Where(item => item.Network == network)
                    .Where(item1 => item1.Type == NetworkDatabaseType.Edge)
                    .Select(item1 => item1.Database)
                    .Distinct()
                    .AsEnumerable();
                databaseNodeFields = context.NetworkDatabases
                    .Where(item => item.Network == network)
                    .Where(item1 => item1.Type == NetworkDatabaseType.Node)
                    .Select(item1 => item1.Database)
                    .Select(item1 => item1.DatabaseNodeFields)
                    .SelectMany(item1 => item1)
                    .Distinct()
                    .AsEnumerable();
                databaseEdgeFields = context.NetworkDatabases
                    .Where(item => item.Network == network)
                    .Where(item1 => item1.Type == NetworkDatabaseType.Edge)
                    .Select(item1 => item1.Database)
                    .Select(item1 => item1.DatabaseEdgeFields)
                    .SelectMany(item1 => item1)
                    .Distinct()
                    .AsEnumerable();
            }
            // Get the seed edges from the data.
            var seedEdges = data
                .Where(item => item.Edge != null)
                .Select(item => item.Edge)
                .Where(item => item.EdgeNodes != null)
                .Select(item => (item.EdgeNodes.FirstOrDefault(item1 => item1.Type == "Source"), item.EdgeNodes.FirstOrDefault(item1 => item1.Type == "Target")))
                .Where(item => item.Item1 != null && item.Item2 != null)
                .Select(item => (item.Item1.Node, item.Item2.Node))
                .Where(item => item.Item1 != null && item.Item2 != null)
                .Select(item => (item.Item1.Id, item.Item2.Id))
                .Where(item => !string.IsNullOrEmpty(item.Item1) && !string.IsNullOrEmpty(item.Item2))
                .Distinct();
            // Check if there haven't been any edges found.
            if (seedEdges == null || !seedEdges.Any())
            {
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Reload the network.
                    var network = context.Networks
                        .FirstOrDefault(item => item.Id == networkId);
                    // Check if there was no item found.
                    if (network == null)
                    {
                        // Return.
                        return;
                    }
                    // Update the status of the item.
                    network.Status = NetworkStatus.Error;
                    // Add a message to the log.
                    network.Log = network.AppendToLog("The seed data corresponding to the network does not contain any valid edges.");
                    // Edit the network.
                    await IEnumerableExtensions.EditAsync(network.Yield(), context, token);
                }
                // End the function.
                return;
            }
            // Get the seed nodes from the seed edges.
            var seedNodes = seedEdges
                .Select(item => item.Item1)
                .Concat(seedEdges.Select(item => item.Item2))
                .Distinct();
            // Define the related entities.
            var networkNodes = seedNodes
                .Select(item => new NetworkNode
                {
                    Node = new Node
                    {
                        DateTimeCreated = DateTime.UtcNow,
                        Name = item,
                        Description = $"This is an automatically generated node for the network \"{networkId}\".",
                        DatabaseNodes = nodeDatabases
                            .Select(item1 => new DatabaseNode
                            {
                                DatabaseId = item1.Id,
                                Database = item1
                            })
                            .ToList(),
                        DatabaseNodeFieldNodes = databaseNodeFields
                            .Select(item1 => new DatabaseNodeFieldNode
                            {
                                DatabaseNodeFieldId = item1.Id,
                                DatabaseNodeField = item1,
                                Value = item
                            })
                            .ToList()
                    },
                    Type = NetworkNodeType.None
                })
                .ToList();
            var networkEdges = seedEdges
                .Select(item => new NetworkEdge
                {
                    Edge = new Edge
                    {
                        DateTimeCreated = DateTime.UtcNow,
                        Name = $"{item.Item1} - {item.Item2}",
                        Description = $"This is an automatically generated edge for the network \"{networkId}\".",
                        DatabaseEdges = edgeDatabases
                            .Select(item1 => new DatabaseEdge
                            {
                                DatabaseId = item1.Id,
                                Database = item1
                            })
                            .ToList(),
                        DatabaseEdgeFieldEdges = databaseEdgeFields
                            .Select(item1 => new DatabaseEdgeFieldEdge
                            {
                                DatabaseEdgeFieldId = item1.Id,
                                DatabaseEdgeField = item1,
                                Value = $"{item.Item1} - {item.Item2}"
                            })
                            .ToList(),
                        EdgeNodes = new List<EdgeNode>
                        {
                            new EdgeNode
                            {
                                Node = networkNodes
                                    .FirstOrDefault(item1 => item1.Node.Name == item.Item1)?.Node,
                                Type = EdgeNodeType.Source
                            },
                            new EdgeNode
                            {
                                Node = networkNodes
                                    .FirstOrDefault(item1 => item1.Node.Name == item.Item2)?.Node,
                                Type = EdgeNodeType.Target
                            }
                        }
                        .Where(item1 => item1.Node != null)
                        .ToList()
                    }
                })
                .ToList();
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Reload the network.
                var network = context.Networks
                    .FirstOrDefault(item => item.Id == networkId);
                // Check if there was no item found.
                if (network == null)
                {
                    // Return.
                    return;
                }
                // Update the network.
                network.NetworkNodes = networkNodes;
                network.NetworkEdges = networkEdges;
                // Edit the network.
                await IEnumerableExtensions.EditAsync(network.Yield(), context, token);
            }
        }
    }
}
