using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.InputModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Algorithms.Networks.Gap
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
        public static async Task Run(string networkId, int gapValue, IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if the gap value is not valid.
            if (gapValue < 0)
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
                    network.Log = network.AppendToLog("The network algorithm is not valid.");
                    // Edit the network.
                    await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
                }
                // End the function.
                return;
            }
            // Define the required data.
            var data = new List<NetworkNodeInputModel>();
            var nodeDatabaseIds = new List<string>();
            var edgeDatabaseIds = new List<string>();
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
                // Check if the database types are not valid.
                if (databaseTypes.Any(item => item.Name == "Generic"))
                {
                    // Update the status of the item.
                    network.Status = NetworkStatus.Error;
                    // Add a message to the log.
                    network.Log = network.AppendToLog("The database type corresponding to the network databases and the network algorithm don't match.");
                    // Edit the network.
                    await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
                    // End the function.
                    return;
                }
                // Try to deserialize the data.
                if (!network.Data.TryDeserializeJsonObject<List<NetworkNodeInputModel>>(out data) || data == null)
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
                // Get the IDs of the required related data.
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
                seedNodeCollectionIds = context.NetworkNodeCollections
                    .Where(item => item.Network == network)
                    .Where(item => item.Type == NetworkNodeCollectionType.Seed)
                    .Select(item => item.NodeCollection)
                    .Distinct()
                    .Select(item => item.Id)
                    .ToList();
            }
            // Get the node identifiers from the data.
            var seedNodeIdentifiers = data
                .Where(item => item.Type == "Seed")
                .Select(item => item.Node)
                .Where(item => item != null)
                .Select(item => item.Id)
                .Where(item => !string.IsNullOrEmpty(item))
                .Distinct();
            // Define the required data.
            var seedNodeIds = new List<string>();
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
                    network.Log = network.AppendToLog("No available nodes could be found in the selected databases.");
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
                    network.Log = network.AppendToLog("No available edges could be found in the selected databases.");
                    // Edit the network.
                    await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
                    // End the function.
                    return;
                }
                // Get the seed nodes.
                var seedNodesByIdentifier = availableNodes
                    .Where(item => seedNodeIdentifiers.Contains(item.Id) || item.DatabaseNodeFieldNodes.Any(item1 => item1.DatabaseNodeField.IsSearchable && seedNodeIdentifiers.Contains(item1.Value)));
                var seedNodesByNodeCollection = availableNodes
                    .Where(item => item.NodeCollectionNodes.Any(item1 => seedNodeCollectionIds.Contains(item1.NodeCollection.Id)));
                seedNodeIds = seedNodesByIdentifier
                    .Concat(seedNodesByNodeCollection)
                    .Distinct()
                    .Select(item => item.Id)
                    .ToList();
                // Check if there haven't been any seed nodes found.
                if (seedNodeIds == null || !seedNodeIds.Any())
                {
                    // Update the status of the item.
                    network.Status = NetworkStatus.Error;
                    // Add a message to the log.
                    network.Log = network.AppendToLog("No seed nodes could be found with the provided seed data.");
                    // Edit the network.
                    await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
                    // End the function.
                    return;
                }
            }
            // Define the list to store the edges.
            var currentEdgeList = new List<List<EdgeItemModel>>();
            // For "gap" times, for all terminal nodes, add all possible edges.
            for (int gapIndex = 0; gapIndex < gapValue + 1; gapIndex++)
            {
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the available nodes.
                    var availableNodes = context.Nodes
                        .Where(item => item.DatabaseNodes.Any(item1 => nodeDatabaseIds.Contains(item1.Database.Id)));
                    // Get the terminal nodes (the seed nodes for the first iteration, the target nodes of all edges in the previous iteration for the subsequent iterations).
                    var terminalNodeIds = gapIndex == 0 ?
                        seedNodeIds
                            .ToList() :
                        currentEdgeList
                            .Last()
                            .Select(item => item.TargetNodeId)
                            .Distinct()
                            .ToList();
                    // Get all edges that start in the terminal nodes.
                    var temporaryList = context.Edges
                        .Where(item => item.DatabaseEdges.Any(item1 => edgeDatabaseIds.Contains(item1.Database.Id)))
                        .Where(item => item.EdgeNodes.All(item1 => availableNodes.Contains(item1.Node)))
                        .Where(item => item.EdgeNodes.Any(item1 => item1.Type == EdgeNodeType.Source && terminalNodeIds.Contains(item1.Node.Id)))
                        .Select(item => new EdgeItemModel
                        {
                            EdgeId = item.Id,
                            SourceNodeId = item.EdgeNodes
                                .Where(item => item.Type == EdgeNodeType.Source)
                                .Select(item => item.Node.Id)
                                .FirstOrDefault(),
                            TargetNodeId = item.EdgeNodes
                                .Where(item => item.Type == EdgeNodeType.Target)
                                .Select(item => item.Node.Id)
                                .FirstOrDefault()
                        })
                        .Where(item => !string.IsNullOrEmpty(item.EdgeId) && !string.IsNullOrEmpty(item.SourceNodeId) && !string.IsNullOrEmpty(item.TargetNodeId))
                        .ToList();
                    // Add them to the list.
                    currentEdgeList.Add(temporaryList);
                }
            }
            // Define a variable to store, at each step, the nodes to keep.
            var nodeIdsToKeep = seedNodeIds.ToList();
            // Starting from the right, mark all terminal nodes that are not seed nodes for removal.
            for (int gapIndex = gapValue; gapIndex >= 0; gapIndex--)
            {
                // Remove from the list all edges that do not end in nodes to keep.
                currentEdgeList.ElementAt(gapIndex)
                    .RemoveAll(item => !nodeIdsToKeep.Contains(item.TargetNodeId));
                // Update the nodes to keep to be the source nodes of the interactions of the current step together with the seed nodes.
                nodeIdsToKeep = currentEdgeList
                    .ElementAt(gapIndex)
                    .Select(item => item.SourceNodeId)
                    .Concat(seedNodeIds)
                    .Distinct()
                    .ToList();
            }
            // Define the edges of the network.
            var edgeIds = currentEdgeList
                .SelectMany(item => item)
                .Select(item => item.EdgeId)
                .Distinct()
                .ToList();
            // Check if there haven't been any edges found.
            if (edgeIds == null || !edgeIds.Any())
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
                    network.Log = network.AppendToLog("No edges could be found with the provided data using the provided algorithm.");
                    // Edit the network.
                    await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
                }
                // End the function.
                return;
            }
            // Get all of the nodes used by the found edges.
            var nodeIds = currentEdgeList
                .SelectMany(item => item)
                .Select(item => item.SourceNodeId)
                .Concat(currentEdgeList
                    .SelectMany(item => item)
                    .Select(item => item.TargetNodeId))
                .Distinct()
                .ToList();
            // Update the seed node IDs.
            seedNodeIds = seedNodeIds
                .Intersect(nodeIds)
                .ToList();
            // Define the required items.
            var networkNodes = new List<NetworkNode>();
            // Get the total number of batches.
            var seedNodeBatchCount = Math.Ceiling((double)seedNodeIds.Count() / ApplicationDbContext.BatchSize);
            // Go over each batch.
            for (var index = 0; index < seedNodeBatchCount; index++)
            {
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items in the current batch.
                    var batchItems = seedNodeIds
                        .Skip(index * ApplicationDbContext.BatchSize)
                        .Take(ApplicationDbContext.BatchSize);
                    // Get the corresponding items.
                    var items = context.Nodes
                        .Where(item => batchItems.Contains(item.Id))
                        .Select(item => new NetworkNode
                        {
                            NodeId = item.Id,
                            Type = NetworkNodeType.Seed
                        });
                    // Add the items to the list.
                    networkNodes.AddRange(items);
                }
            }
            // Get the total number of batches.
            var nodeBatchCount = Math.Ceiling((double)nodeIds.Count() / ApplicationDbContext.BatchSize);
            // Go over each batch.
            for (var index = 0; index < nodeBatchCount; index++)
            {
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items in the current batch.
                    var batchItems = nodeIds
                        .Skip(index * ApplicationDbContext.BatchSize)
                        .Take(ApplicationDbContext.BatchSize);
                    // Get the corresponding items.
                    var items = context.Nodes
                        .Where(item => batchItems.Contains(item.Id))
                        .Select(item => new NetworkNode
                        {
                            NodeId = item.Id,
                            Type = NetworkNodeType.None
                        });
                    // Add the items to the list.
                    networkNodes.AddRange(items);
                }
            }
            // Define the required items.
            var networkEdges = new List<NetworkEdge>();
            // Get the total number of batches.
            var edgeBatchCount = Math.Ceiling((double)edgeIds.Count() / ApplicationDbContext.BatchSize);
            // Go over each batch.
            for (var index = 0; index < edgeBatchCount; index++)
            {
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items in the current batch.
                    var batchItems = edgeIds
                        .Skip(index * ApplicationDbContext.BatchSize)
                        .Take(ApplicationDbContext.BatchSize);
                    // Get the corresponding items.
                    var items = context.Edges
                        .Where(item => batchItems.Contains(item.Id))
                        .Select(item => new NetworkEdge
                        {
                            EdgeId = item.Id
                        });
                    // Add the items to the list.
                    networkEdges.AddRange(items);
                }
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
                // Define the related entities.
                network.NetworkNodes = networkNodes;
                network.NetworkEdges = networkEdges;
                // Edit the network.
                await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
            }
        }
    }
}
