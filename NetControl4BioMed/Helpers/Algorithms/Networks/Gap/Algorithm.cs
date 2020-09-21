﻿using Microsoft.EntityFrameworkCore;
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
        /// Runs the algorithm on the network with the provided details, using the given parameters.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="network">The network on which to run the algorithm.</param>
        public static async Task Run(Network network, int gapValue, ApplicationDbContext context, CancellationToken token)
        {
            // Check if the gap value is not valid.
            if (gapValue < 0)
            {
                // Update the status of the item.
                network.Status = NetworkStatus.Error;
                // Add a message to the log.
                network.Log = network.AppendToLog("The network algorithm is not valid.");
                // Edit the network.
                await IEnumerableExtensions.EditAsync(network.Yield(), context, token);
                // End the function.
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
            if (databaseTypes.Any(item => item.Name == "Generic"))
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
            if (!network.Data.TryDeserializeJsonObject<IEnumerable<NetworkNodeInputModel>>(out var data) || data == null)
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
            // Get the IDs of the required related data.
            var nodeDatabaseIds = context.NetworkDatabases
                .Where(item => item.Network == network)
                .Where(item => item.Type == NetworkDatabaseType.Node)
                .Select(item => item.Database)
                .Distinct()
                .Select(item => item.Id);
            var edgeDatabaseIds = context.NetworkDatabases
                .Where(item => item.Network == network)
                .Where(item => item.Type == NetworkDatabaseType.Edge)
                .Select(item => item.Database)
                .Distinct()
                .Select(item => item.Id);
            var seedNodeCollectionIds = context.NetworkNodeCollections
                .Where(item => item.Network == network)
                .Where(item => item.Type == NetworkNodeCollectionType.Seed)
                .Select(item => item.NodeCollection)
                .Distinct()
                .Select(item => item.Id);
            // Get the node identifiers from the data.
            var seedNodeIdentifiers = data
                .Where(item => item.Type == "Seed")
                .Select(item => item.Node)
                .Where(item => item != null)
                .Select(item => item.Id)
                .Where(item => !string.IsNullOrEmpty(item))
                .Distinct();
            // Get the available nodes.
            var availableNodes = context.Nodes
                .Where(item => item.DatabaseNodes.Any(item1 => nodeDatabaseIds.Contains(item1.Database.Id)));
            // Get the seed nodes.
            var seedNodesByIdentifier = availableNodes
                .Where(item => seedNodeIdentifiers.Contains(item.Id) || item.DatabaseNodeFieldNodes.Any(item1 => item1.DatabaseNodeField.IsSearchable && seedNodeIdentifiers.Contains(item1.Value)));
            var seedNodesByNodeCollection = availableNodes
                .Where(item => item.NodeCollectionNodes.Any(item1 => seedNodeCollectionIds.Contains(item1.NodeCollection.Id)));
            var seedNodes = seedNodesByIdentifier
                .Concat(seedNodesByNodeCollection)
                .Distinct()
                .ToList();
            // Check if there haven't been any seed nodes found.
            if (seedNodes == null || !seedNodes.Any())
            {
                // Update the status of the item.
                network.Status = NetworkStatus.Error;
                // Add a message to the log.
                network.Log = network.AppendToLog("No seed nodes could be found with the provided seed data.");
                // Edit the network.
                await IEnumerableExtensions.EditAsync(network.Yield(), context, token);
                // End the function.
                return;
            }
            // Get the available edges.
            var availableEdges = context.Edges
                .Include(item => item.EdgeNodes)
                    .ThenInclude(item => item.Node)
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
                await IEnumerableExtensions.EditAsync(network.Yield(), context, token);
                // End the function.
                return;
            }
            // Define the list to store the edges.
            var currentEdgeList = new List<List<Edge>>();
            // For "gap" times, for all terminal nodes, add all possible edges.
            for (int gapIndex = 0; gapIndex < gapValue + 1; gapIndex++)
            {
                // Get the terminal nodes (the seed nodes for the first iteration, the target nodes of all edges in the previous iteration for the subsequent iterations).
                var terminalNodes = gapIndex == 0 ?
                    seedNodes :
                    currentEdgeList
                        .Last()
                        .Select(item => item.EdgeNodes
                            .Where(item => item.Type == EdgeNodeType.Target)
                            .Select(item => item.Node))
                        .SelectMany(item => item)
                        .Distinct()
                        .ToList();
                // Get all edges that start in the terminal nodes.
                var temporaryList = availableEdges
                    .Where(item => item.EdgeNodes
                        .Any(item1 => item1.Type == EdgeNodeType.Source && terminalNodes.Contains(item1.Node)))
                    .ToList();
                // Add them to the list.
                currentEdgeList.Add(temporaryList);
            }
            // Define a variable to store, at each step, the nodes to keep.
            var nodesToKeep = seedNodes
                .ToList();
            // Starting from the right, mark all terminal nodes that are not seed nodes for removal.
            for (int gapIndex = gapValue; gapIndex >= 0; gapIndex--)
            {
                // Remove from the list all edges that do not end in nodes to keep.
                currentEdgeList.ElementAt(gapIndex)
                    .RemoveAll(item => item.EdgeNodes
                        .Any(item1 => item1.Type == EdgeNodeType.Target && !nodesToKeep.Contains(item1.Node)));
                // Update the nodes to keep to be the source nodes of the interactions of the current step together with the seed nodes.
                nodesToKeep = currentEdgeList
                    .ElementAt(gapIndex)
                    .Select(item => item.EdgeNodes
                        .Where(item1 => item1.Type == EdgeNodeType.Source)
                        .Select(item1 => item1.Node))
                    .SelectMany(item => item)
                    .Concat(seedNodes)
                    .Distinct()
                    .ToList();
            }
            // Define the edges of the network.
            var edges = currentEdgeList
                .SelectMany(item => item)
                .Distinct()
                .ToList();
            // Check if there haven't been any edges found.
            if (edges == null || !edges.Any())
            {
                // Update the status of the item.
                network.Status = NetworkStatus.Error;
                // Add a message to the log.
                network.Log = network.AppendToLog("No edges could be found with the provided data using the provided algorithm.");
                // Edit the network.
                await IEnumerableExtensions.EditAsync(network.Yield(), context, token);
                // End the function.
                return;
            }
            // Get all of the nodes used by the found edges.
            var nodes = edges
                .Select(item => item.EdgeNodes)
                .SelectMany(item => item)
                .Select(item => item.Node)
                .Distinct();
            // Define the related entities.
            network.NetworkNodes = seedNodes
                .Intersect(nodes)
                .Select(item => new NetworkNode
                {
                    Node = item,
                    Type = NetworkNodeType.Seed
                })
                .Concat(nodes
                    .Select(item => new NetworkNode
                    {
                        Node = item,
                        Type = NetworkNodeType.None
                    }))
                .ToList();
            network.NetworkEdges = edges
                .Select(item => new NetworkEdge
                {
                    Edge = item
                })
                .ToList();
            // Edit the network.
            await IEnumerableExtensions.EditAsync(network.Yield(), context, token);
        }
    }
}
