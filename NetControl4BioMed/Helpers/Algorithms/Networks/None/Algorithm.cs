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
        /// Represents the model of the required database data for an edge.
        /// </summary>
        private class EdgeItemModel
        {
            /// <summary>
            /// Represents the ID of the edge.
            /// </summary>
            public string EdgeId { get; set; }

            /// <summary>
            /// Represents the ID of the source node of the edge.
            /// </summary>
            public string SourceNodeId { get; set; }

            /// <summary>
            /// Represents the ID of the target node of the edge.
            /// </summary>
            public string TargetNodeId { get; set; }
        }

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
            var data = new List<NetworkEdgeInputModel>();
            var databaseTypeName = string.Empty;
            var nodeDatabaseIds = new List<string>();
            var edgeDatabaseIds = new List<string>();
            var databaseNodeFieldIds = new List<string>();
            var databaseEdgeFieldIds = new List<string>();
            var seedNodeCollectionIds = new List<string>();
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
                    await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
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
                    await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
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
                    await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
                    // End the function.
                    return;
                }
                // Get the database type name.
                databaseTypeName = databaseTypes.Select(item => item.Name).First();
                // Try to deserialize the data.
                if (!network.Data.TryDeserializeJsonObject<List<NetworkEdgeInputModel>>(out data) || data == null)
                {
                    // Update the status of the item.
                    network.Status = NetworkStatus.Error;
                    // Add a message to the log.
                    network.Log = network.AppendToLog("The seed data corresponding to the network could not be deserialized.");
                    // Edit the network.
                    await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
                    // End the function.
                    return;
                }
                // Get the related entities.
                nodeDatabaseIds = context.NetworkDatabases
                    .Where(item => item.Network == network)
                    .Where(item => item.Type == NetworkDatabaseType.Node)
                    .Select(item => item.Database)
                    .Distinct()
                    .Select(item => item.Id)
                    .ToList();
                edgeDatabaseIds = context.NetworkDatabases
                    .Where(item => item.Network == network)
                    .Where(item => item.Type == NetworkDatabaseType.Edge)
                    .Select(item => item.Database)
                    .Distinct()
                    .Select(item => item.Id)
                    .ToList();
                databaseNodeFieldIds = context.NetworkDatabases
                    .Where(item => item.Network == network)
                    .Where(item1 => item1.Type == NetworkDatabaseType.Node)
                    .Select(item1 => item1.Database)
                    .Select(item1 => item1.DatabaseNodeFields)
                    .SelectMany(item1 => item1)
                    .Distinct()
                    .Select(item => item.Id)
                    .ToList();
                databaseEdgeFieldIds = context.NetworkDatabases
                    .Where(item => item.Network == network)
                    .Where(item => item.Type == NetworkDatabaseType.Edge)
                    .Select(item => item.Database)
                    .Select(item => item.DatabaseEdgeFields)
                    .SelectMany(item => item)
                    .Distinct()
                    .Select(item => item.Id)
                    .ToList();
                seedNodeCollectionIds = context.NetworkNodeCollections
                    .Where(item => item.Network == network)
                    .Where(item => item.Type == NetworkNodeCollectionType.Seed)
                    .Select(item => item.NodeCollection)
                    .Distinct()
                    .Select(item => item.Id)
                    .ToList();
            }
            // Get the related data.
            var nodeTitle = databaseTypeName == "PPI" ? "Protein" : "Node";
            var edgeTitle = databaseTypeName == "PPI" ? "Interaction" : "Edge";
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
                    network.Log = network.AppendToLog($"The seed data corresponding to the network does not contain any valid {edgeTitle.ToLower()}s.");
                    // Edit the network.
                    await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
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
            var networkNodes = new List<NetworkNode>();
            var networkEdges = new List<NetworkEdge>();
            // Check the database type.
            if (databaseTypeName == "Generic")
            {
                // Define the related entities.
                networkNodes = seedNodes
                    .Select(item => new NetworkNode
                    {
                        Node = new Node
                        {
                            DateTimeCreated = DateTime.UtcNow,
                            Name = item,
                            Description = $"This is an automatically generated {nodeTitle.ToLower()} for the network \"{networkId}\".",
                            DatabaseNodes = nodeDatabaseIds
                                .Select(item1 => new DatabaseNode
                                {
                                    DatabaseId = item1,
                                })
                                .ToList(),
                            DatabaseNodeFieldNodes = databaseNodeFieldIds
                                .Select(item1 => new DatabaseNodeFieldNode
                                {
                                    DatabaseNodeFieldId = item1,
                                    Value = item
                                })
                                .ToList()
                        },
                        Type = NetworkNodeType.None
                    })
                    .ToList();
                networkEdges = seedEdges
                    .Select(item => new NetworkEdge
                    {
                        Edge = new Edge
                        {
                            DateTimeCreated = DateTime.UtcNow,
                            Name = $"{item.Item1} - {item.Item2}",
                            Description = $"This is an automatically generated {edgeTitle.ToLower()} for the network \"{networkId}\".",
                            DatabaseEdges = edgeDatabaseIds
                                .Select(item1 => new DatabaseEdge
                                {
                                    DatabaseId = item1,
                                })
                                .ToList(),
                            DatabaseEdgeFieldEdges = databaseEdgeFieldIds
                                .Select(item1 => new DatabaseEdgeFieldEdge
                                {
                                    DatabaseEdgeFieldId = item1,
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
            }
            else
            {
                // Define the required data.
                var seedNodeIds = new List<string>();
                var seedEdgesWithIds = new List<(string, string)>();
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
                    // Get the available nodes.
                    var availableNodes = context.Nodes
                        .Where(item => item.DatabaseNodes.Any(item1 => nodeDatabaseIds.Contains(item1.Database.Id)));
                    // Check if there haven't been any available nodes found.
                    if (availableNodes == null || !availableNodes.Any())
                    {
                        // Update the status of the item.
                        network.Status = NetworkStatus.Error;
                        // Add a message to the log.
                        network.Log = network.AppendToLog($"No available {nodeTitle.ToLower()}s could be found in the selected databases.");
                        // Edit the network.
                        await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
                        // End the function.
                        return;
                    }
                    // Get the available edges.
                    var availableEdges = context.Edges
                        .Where(item => item.DatabaseEdges.Any(item1 => edgeDatabaseIds.Contains(item1.Database.Id)))
                        .Where(item => item.EdgeNodes.All(item1 => availableNodes.Contains(item1.Node)));
                    // Check if there haven't been any available edges found.
                    if (availableEdges == null || !availableEdges.Any())
                    {
                        // Update the status of the item.
                        network.Status = NetworkStatus.Error;
                        // Add a message to the log.
                        network.Log = network.AppendToLog($"No available {edgeTitle.ToLower()}s could be found in the selected databases.");
                        // Edit the network.
                        await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
                        // End the function.
                        return;
                    }
                    // Get the seed nodes.
                    var seedNodesByIdentifier = availableNodes
                        .Where(item => seedNodes.Contains(item.Id) || item.DatabaseNodeFieldNodes.Any(item1 => item1.DatabaseNodeField.IsSearchable && seedNodes.Contains(item1.Value)));
                    var seedNodesByNodeCollection = availableNodes
                        .Where(item => item.NodeCollectionNodes.Any(item1 => seedNodeCollectionIds.Contains(item1.NodeCollection.Id)));
                    var seedNodeItems = seedNodesByIdentifier
                        .Concat(seedNodesByNodeCollection)
                        .Distinct()
                        .Select(item => new
                        {
                            Id = item.Id,
                            Values = item.DatabaseNodeFieldNodes
                                .Where(item1 => databaseNodeFieldIds.Contains(item1.DatabaseNodeField.Id))
                                .Where(item1 => item1.DatabaseNodeField.IsSearchable)
                                .Select(item1 => item1.Value)
                                .ToList()
                        })
                        .ToList();
                    // Check if there haven't been any seed nodes found.
                    if (seedNodeItems == null || !seedNodeItems.Any())
                    {
                        // Update the status of the item.
                        network.Status = NetworkStatus.Error;
                        // Add a message to the log.
                        network.Log = network.AppendToLog($"No seed {nodeTitle.ToLower()}s could be found with the provided seed data.");
                        // Edit the network.
                        await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
                        // End the function.
                        return;
                    }
                    // Get the seed node IDs.
                    seedNodeIds = seedNodeItems
                        .Select(item => item.Id)
                        .ToList();
                    // Define the list to store the updated edges.
                    seedEdgesWithIds = seedEdges
                        .Select(item => (
                            seedNodeItems
                                .Where(item1 => item1.Values.Contains(item.Item1))
                                .Select(item1 => item1.Id)
                                .FirstOrDefault(),
                            seedNodeItems
                                .Where(item1 => item1.Values.Contains(item.Item2))
                                .Select(item1 => item1.Id)
                                .FirstOrDefault()))
                        .Where(item => !string.IsNullOrEmpty(item.Item1) && !string.IsNullOrEmpty(item.Item2))
                        .ToList();
                    // Check if there haven't been any seed nodes found.
                    if (seedEdgesWithIds == null || !seedEdgesWithIds.Any())
                    {
                        // Update the status of the item.
                        network.Status = NetworkStatus.Error;
                        // Add a message to the log.
                        network.Log = network.AppendToLog($"No seed {edgeTitle.ToLower()}s with valid {nodeTitle.ToLower()}s could be found with the provided seed data.");
                        // Edit the network.
                        await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
                        // End the function.
                        return;
                    }
                }
                // Define the list to store the edges.
                var currentEdgeList = new List<EdgeItemModel>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Define the edges of the network.
                    currentEdgeList = context.Edges
                        .Where(item => item.DatabaseEdges.Any(item1 => edgeDatabaseIds.Contains(item1.Database.Id)))
                        .Where(item => item.EdgeNodes.All(item1 => seedNodeIds.Contains(item1.Node.Id)))
                        .Select(item => new EdgeItemModel
                        {
                            EdgeId = item.Id,
                            SourceNodeId = item.EdgeNodes
                                .Where(item1 => item1.Type == EdgeNodeType.Source)
                                .Select(item1 => item1.Node.Id)
                                .FirstOrDefault(),
                            TargetNodeId = item.EdgeNodes
                                .Where(item => item.Type == EdgeNodeType.Target)
                                .Select(item => item.Node.Id)
                                .FirstOrDefault()
                        })
                        .Where(item => !string.IsNullOrEmpty(item.EdgeId) && !string.IsNullOrEmpty(item.SourceNodeId) && !string.IsNullOrEmpty(item.TargetNodeId))
                        .AsEnumerable()
                        .Where(item => seedEdgesWithIds.Any(item1 => item1.Item1 == item.SourceNodeId && item1.Item2 == item.TargetNodeId))
                        .ToList();
                }
                // Check if there haven't been any seed nodes found.
                if (currentEdgeList == null || !currentEdgeList.Any())
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
                        network.Log = network.AppendToLog($"No seed {edgeTitle.ToLower()}s could be found with the provided seed data.");
                        // Edit the network.
                        await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
                    }
                    // End the function.
                    return;
                }
                // Define the edges of the network.
                var edgeIds = currentEdgeList
                    .Select(item => item.EdgeId)
                    .Distinct()
                    .ToList();
                // Get all of the nodes used by the found edges.
                var nodeIds = currentEdgeList
                    .Select(item => item.SourceNodeId)
                    .Concat(currentEdgeList
                        .Select(item => item.TargetNodeId))
                    .Distinct()
                    .ToList();
                // Update the seed node IDs.
                seedNodeIds = seedNodeIds
                    .Intersect(nodeIds)
                    .ToList();
                // Define the related entities.
                networkNodes = nodeIds
                    .Select(item => new NetworkNode
                    {
                        NodeId = item,
                        Type = NetworkNodeType.None
                    })
                    .Concat(seedNodeIds
                        .Select(item => new NetworkNode
                        {
                            NodeId = item,
                            Type = NetworkNodeType.Seed
                        }))
                    .ToList();
                networkEdges = edgeIds
                    .Select(item => new NetworkEdge
                    {
                        EdgeId = item
                    })
                    .ToList();
            }
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
                await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
            }
        }
    }
}
