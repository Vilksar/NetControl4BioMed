using DocumentFormat.OpenXml.Office.CustomUI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Data.Seed;
using NetControl4BioMed.Helpers.Exceptions;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.InputModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Tasks
{
    /// <summary>
    /// Implements a task to update networks in the database.
    /// </summary>
    public class NetworksTask
    {
        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<NetworkInputModel> Items { get; set; }

        /// <summary>
        /// Creates the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns>The created items.</returns>
        public IEnumerable<Network> Create(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if there weren't any valid items found.
            if (Items == null)
            {
                // Throw an exception.
                throw new TaskException("No valid items could be found with the provided data.");
            }
            // Check if the exception item should be shown.
            var showExceptionItem = Items.Count() > 1;
            // Get the total number of batches.
            var count = Math.Ceiling((double)Items.Count() / ApplicationDbContext.BatchSize);
            // Go over each batch.
            for (var index = 0; index < count; index++)
            {
                // Check if the cancellation was requested.
                if (token.IsCancellationRequested)
                {
                    // Break.
                    break;
                }
                // Create a new scope.
                using var scope = serviceProvider.CreateScope();
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items in the current batch.
                var batchItems = Items.Skip(index * ApplicationDbContext.BatchSize).Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id);
                // Check if any of the IDs are repeating in the list.
                if (batchIds.Distinct().Count() != batchIds.Count())
                {
                    // Throw an exception.
                    throw new TaskException("One or more of the manually provided IDs are duplicated.");
                }
                // Get the valid IDs, that do not appear in the database.
                var validBatchIds = batchIds
                    .Except(context.Networks
                        .Where(item => batchIds.Contains(item.Id))
                        .Select(item => item.Id));
                // Get the IDs of the related entities that appear in the current batch.
                var batchUserIds = batchItems
                    .Where(item => item.NetworkUsers != null)
                    .Select(item => item.NetworkUsers)
                    .SelectMany(item => item)
                    .Where(item => item.User != null)
                    .Select(item => item.User)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                var batchDatabaseIds = batchItems
                    .Where(item => item.NetworkDatabases != null)
                    .Select(item => item.NetworkDatabases)
                    .SelectMany(item => item)
                    .Where(item => item.Database != null)
                    .Select(item => item.Database)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                var batchNodeCollectionIds = batchItems
                    .Where(item => item.NetworkNodeCollections != null)
                    .Select(item => item.NetworkNodeCollections)
                    .SelectMany(item => item)
                    .Where(item => item.NodeCollection != null)
                    .Select(item => item.NodeCollection)
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                // Get the related entities that appear in the current batch.
                var batchUsers = context.Users
                    .Where(item => batchUserIds.Contains(item.Id));
                var batchDatabases = context.Databases
                    .Include(item => item.DatabaseType)
                    .Where(item => batchDatabaseIds.Contains(item.Id));
                var batchNodeCollections = context.NodeCollections
                    .Where(item => batchNodeCollectionIds.Contains(item.Id));
                // Save the items to add.
                var networksToAdd = new List<Network>();
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Check if the ID of the item is not valid.
                    if (!string.IsNullOrEmpty(batchItem.Id) && !validBatchIds.Contains(batchItem.Id))
                    {
                        // Continue.
                        continue;
                    }
                    // Check if there are no network users provided.
                    if (batchItem.NetworkUsers == null || !batchItem.NetworkUsers.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no network users provided.", showExceptionItem, batchItem);
                    }
                    // Get the network users.
                    var networkUsers = batchItem.NetworkUsers
                        .Where(item => item.User != null)
                        .Where(item => !string.IsNullOrEmpty(item.User.Id))
                        .Select(item => item.User.Id)
                        .Distinct()
                        .Where(item => batchUsers.Any(item1 => item1.Id == item))
                        .Select(item => new NetworkUser
                        {
                            DateTimeCreated = DateTime.Now,
                            UserId = item,
                            User = batchUsers
                                .FirstOrDefault(item1 => item1.Id == item)
                        })
                        .Where(item => item.User != null);
                    // Check if there were no network users found.
                    if (networkUsers == null || !networkUsers.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no network users found.", showExceptionItem, batchItem);
                    }
                    // Check if there are no network databases provided.
                    if (batchItem.NetworkDatabases == null || !batchItem.NetworkDatabases.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no network databases provided.", showExceptionItem, batchItem);
                    }
                    // Get the network databases.
                    var networkDatabases = batchItem.NetworkDatabases
                        .Where(item => item.Database != null)
                        .Where(item => !string.IsNullOrEmpty(item.Database.Id))
                        .Where(item => item.Type == "Node" || item.Type == "Edge")
                        .Select(item => (item.Database.Id, item.Type))
                        .Distinct()
                        .Where(item => batchDatabases.Any(item1 => item1.Id == item.Item1))
                        .Select(item => new NetworkDatabase
                        {
                            DatabaseId = item.Item1,
                            Database = batchDatabases
                                .FirstOrDefault(item1 => item1.Id == item.Item1),
                            Type = EnumerationExtensions.GetEnumerationValue<NetworkDatabaseType>(item.Item2)
                        })
                        .Where(item => item.Database != null);
                    // Check if there were no network databases found.
                    if (networkDatabases == null || !networkDatabases.Any())
                    {
                        // Throw an exception.
                        throw new TaskException("There were no network databases found.", showExceptionItem, batchItem);
                    }
                    // Check if the network databases have different database types.
                    if (networkDatabases.Select(item => item.Database.DatabaseType).Distinct().Count() > 1)
                    {
                        // Throw an exception.
                        throw new TaskException("The network databases found have different database types.", showExceptionItem, batchItem);
                    }
                    // Get the network node collections.
                    var networkNodeCollections = batchItem.NetworkNodeCollections != null ?
                        batchItem.NetworkNodeCollections
                            .Where(item => item.NodeCollection != null)
                            .Where(item => !string.IsNullOrEmpty(item.NodeCollection.Id))
                            .Where(item => item.Type == "Seed")
                            .Select(item => (item.NodeCollection.Id, item.Type))
                            .Distinct()
                            .Where(item => batchNodeCollections.Any(item1 => item1.Id == item.Item1))
                            .Select(item => new NetworkNodeCollection
                            {
                                NodeCollectionId = item.Item1,
                                NodeCollection = batchNodeCollections
                                    .FirstOrDefault(item1 => item1.Id == item.Item1),
                                Type = EnumerationExtensions.GetEnumerationValue<NetworkNodeCollectionType>(item.Item2)
                            })
                            .Where(item => item.NodeCollection != null) :
                        Enumerable.Empty<NetworkNodeCollection>();
                    // Define the new item.
                    var network = new Network
                    {
                        DateTimeCreated = DateTime.Now,
                        Name = batchItem.Name,
                        Description = batchItem.Description,
                        Status = NetworkStatus.Defined,
                        Log = JsonSerializer.Serialize(Enumerable.Empty<string>()),
                        Data = batchItem.Data,
                        NetworkDatabases = networkDatabases.ToList(),
                        NetworkUsers = networkUsers.ToList(),
                        NetworkNodeCollections = networkNodeCollections.ToList()
                    };
                    // Try to get the algorithm.
                    try
                    {
                        // Get the algorithm.
                        network.Algorithm = EnumerationExtensions.GetEnumerationValue<NetworkAlgorithm>(batchItem.Algorithm);
                    }
                    catch (Exception exception)
                    {
                        // Get the exception message.
                        var message = string.IsNullOrEmpty(exception.Message) ? string.Empty : " " + exception.Message;
                        throw new TaskException("The algorithm couldn't be determined from the provided string." + message, showExceptionItem, batchItem);
                    }
                    // Append a message to the log.
                    network.Log = network.AppendToLog("The network has been defined and stored in the database.");
                    // Check if there is any ID provided.
                    if (!string.IsNullOrEmpty(batchItem.Id))
                    {
                        // Assign it to the item.
                        network.Id = batchItem.Id;
                    }
                    // Add the new item to the list.
                    networksToAdd.Add(network);
                }
                // Create the items.
                IEnumerableExtensions.Create(networksToAdd, context, token);
                // Go over each item.
                foreach (var networkToAdd in networksToAdd)
                {
                    // Yield return it.
                    yield return networkToAdd;
                }
            }
        }

        /// <summary>
        /// Edits the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns>The edited items.</returns>
        public IEnumerable<Network> Edit(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if there weren't any valid items found.
            if (Items == null)
            {
                // Throw an exception.
                throw new ArgumentException("No valid items could be found with the provided data.");
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)Items.Count() / ApplicationDbContext.BatchSize);
            // Go over each batch.
            for (var index = 0; index < count; index++)
            {
                // Check if the cancellation was requested.
                if (token.IsCancellationRequested)
                {
                    // Break.
                    break;
                }
                // Create a new scope.
                using var scope = serviceProvider.CreateScope();
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items in the current batch.
                var batchItems = Items
                    .Skip(index * ApplicationDbContext.BatchSize)
                    .Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems.Select(item => item.Id);
                // Get the items with the provided IDs.
                var networks = context.Networks
                    .Where(item => batchIds.Contains(item.Id));
                // Save the items to add.
                var networksToEdit = new List<Network>();
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding item.
                    var network = networks.FirstOrDefault(item => item.Id == batchItem.Id);
                    // Check if there was no item found.
                    if (network == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Update the data.
                    network.Name = batchItem.Name;
                    network.Description = batchItem.Description;
                    // Append a message to the log.
                    network.Log = network.AppendToLog("The network name and / or description have been updated.");
                    // Add the item to the list.
                    networksToEdit.Add(network);
                }
                // Edit the items.
                IEnumerableExtensions.Edit(networksToEdit, context, token);
                // Go over each item.
                foreach (var networkToEdit in networksToEdit)
                {
                    // Yield return it.
                    yield return networkToEdit;
                }
            }
        }

        /// <summary>
        /// Deletes the items from the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void Delete(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if there weren't any valid items found.
            if (Items == null)
            {
                // Throw an exception.
                throw new ArgumentException("No valid items could be found with the provided data.");
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)Items.Count() / ApplicationDbContext.BatchSize);
            // Go over each batch.
            for (var index = 0; index < count; index++)
            {
                // Check if the cancellation was requested.
                if (token.IsCancellationRequested)
                {
                    // Break.
                    break;
                }
                // Create a new scope.
                using var scope = serviceProvider.CreateScope();
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items in the current batch.
                var batchItems = Items
                    .Skip(index * ApplicationDbContext.BatchSize)
                    .Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems.Select(item => item.Id);
                // Get the items with the provided IDs.
                var networks = context.Networks
                    .Where(item => batchIds.Contains(item.Id));
                // Get the related entities that use the items.
                var analyses = context.Analyses
                    .Where(item => item.AnalysisNetworks.Any(item1 => networks.Contains(item1.Network)));
                // Get the generic entities among them.
                var genericNetworks = networks.Where(item => item.NetworkDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"));
                var genericNodes = context.Nodes.Where(item => item.NetworkNodes.Any(item1 => genericNetworks.Contains(item1.Network)));
                var genericEdges = context.Edges.Where(item => item.NetworkEdges.Any(item1 => genericNetworks.Contains(item1.Network)) || item.EdgeNodes.Any(item1 => genericNodes.Contains(item1.Node)));
                // Delete the items.
                IQueryableExtensions.Delete(analyses, context, token);
                IQueryableExtensions.Delete(networks, context, token);
                IQueryableExtensions.Delete(genericEdges, context, token);
                IQueryableExtensions.Delete(genericNodes, context, token);
            }
        }

        /// <summary>
        /// Generates the items.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void Generate(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if there weren't any valid items found.
            if (Items == null)
            {
                // Throw an exception.
                throw new ArgumentException("No valid items could be found with the provided data.");
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)Items.Count() / ApplicationDbContext.BatchSize);
            // Go over each batch.
            for (var index = 0; index < count; index++)
            {
                // Check if the cancellation was requested.
                if (token.IsCancellationRequested)
                {
                    // Break.
                    break;
                }
                // Create a new scope.
                using var scope = serviceProvider.CreateScope();
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items in the current batch.
                var batchItems = Items
                    .Skip(index * ApplicationDbContext.BatchSize)
                    .Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems.Select(item => item.Id);
                // Get the items with the provided IDs.
                var networks = context.Networks
                    .Include(item => item.NetworkDatabases)
                        .ThenInclude(item => item.Database)
                            .ThenInclude(item => item.DatabaseType)
                    .Include(item => item.NetworkDatabases)
                        .ThenInclude(item => item.Database)
                            .ThenInclude(item => item.DatabaseNodeFields)
                    .Include(item => item.NetworkDatabases)
                        .ThenInclude(item => item.Database)
                            .ThenInclude(item => item.DatabaseEdgeFields)
                    .Where(item => batchIds.Contains(item.Id));
                // Save the items to edit.
                var networksToEdit = new List<Network>();
                // Go over each item in the current batch.
                foreach (var batchItem in batchItems)
                {
                    // Get the corresponding item.
                    var network = networks
                        .FirstOrDefault(item => item.Id == batchItem.Id);
                    // Check if there was no item found.
                    if (network == null)
                    {
                        // Continue.
                        continue;
                    }
                    // Get the related data of the item.
                    var databaseTypes = network.NetworkDatabases
                        .Select(item => item.Database.DatabaseType);
                    // Check if there was any error in retrieving related data from the database.
                    if (databaseTypes == null)
                    {
                        // Update the status of the item.
                        network.Status = NetworkStatus.Error;
                        // Add a message to the log.
                        network.Log = network.AppendToLog("There was an error in retrieving related data from the database.");
                        // Add the item to the list.
                        networksToEdit.Add(network);
                        // Continue.
                        continue;
                    }
                    // Check if there weren't any database types found.
                    if (!databaseTypes.Any())
                    {
                        // Update the status of the item.
                        network.Status = NetworkStatus.Error;
                        // Add a message to the log.
                        network.Log = network.AppendToLog("No database types corresponding to the network databases could be found.");
                        // Add the item to the list.
                        networksToEdit.Add(network);
                        // Continue.
                        continue;
                    }
                    // Check if the database types are different.
                    if (databaseTypes.Distinct().Count() > 1)
                    {
                        // Update the status of the item.
                        network.Status = NetworkStatus.Error;
                        // Add a message to the log.
                        network.Log = network.AppendToLog("The database types corresponding to the network databases are different.");
                        // Add the item to the list.
                        networksToEdit.Add(network);
                        // Continue.
                        continue;
                    }
                    // Get the database type.
                    var databaseType = databaseTypes.First();
                    // Check if the database type and the algorithm don't match.
                    if (databaseType.Name == "Generic" ^ network.Algorithm == NetworkAlgorithm.None)
                    {
                        // Update the status of the item.
                        network.Status = NetworkStatus.Error;
                        // Add a message to the log.
                        network.Log = network.AppendToLog("The database type corresponding to the network databases and the network algorithm don't match.");
                        // Add the item to the list.
                        networksToEdit.Add(network);
                        // Continue.
                        continue;
                    }
                    // Check the type of the database.
                    if (databaseType.Name == "Generic")
                    {
                        // Try to deserialize the data.
                        if (!network.Data.TryDeserializeJsonObject<IEnumerable<NetworkEdgeInputModel>>(out var data) || data == null)
                        {
                            // Update the status of the item.
                            network.Status = NetworkStatus.Error;
                            // Add a message to the log.
                            network.Log = network.AppendToLog("The seed data corresponding to the network could not be deserialized.");
                            // Add the item to the list.
                            networksToEdit.Add(network);
                            // Continue.
                            continue;
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
                            // Update the status of the item.
                            network.Status = NetworkStatus.Error;
                            // Add a message to the log.
                            network.Log = network.AppendToLog("The seed data corresponding to the network does not contain any valid edges.");
                            // Add the item to the list.
                            networksToEdit.Add(network);
                            // Continue.
                            continue;
                        }
                        // Get the seed nodes from the seed edges.
                        var seedNodes = seedEdges
                            .Select(item => item.Item1)
                            .Concat(seedEdges.Select(item => item.Item2))
                            .Distinct();
                        // Define the related entities.
                        network.NetworkNodes = seedNodes
                            .Select(item => new NetworkNode
                            {
                                Node = new Node
                                {
                                    DateTimeCreated = DateTime.Now,
                                    Name = item,
                                    Description = $"This is an automatically generated node for the network \"{network.Id}\".",
                                    DatabaseNodes = network.NetworkDatabases
                                        .Where(item1 => item1.Type == NetworkDatabaseType.Node)
                                        .Select(item1 => item1.Database)
                                        .Distinct()
                                        .Select(item1 => new DatabaseNode
                                        {
                                            DatabaseId = item1.Id,
                                            Database = item1
                                        })
                                        .ToList(),
                                    DatabaseNodeFieldNodes = network.NetworkDatabases
                                        .Where(item1 => item1.Type == NetworkDatabaseType.Node)
                                        .Select(item1 => item1.Database)
                                        .Select(item1 => item1.DatabaseNodeFields)
                                        .SelectMany(item1 => item1)
                                        .Distinct()
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
                        network.NetworkEdges = seedEdges
                            .Select(item => new NetworkEdge
                            {
                                Edge = new Edge
                                {
                                    DateTimeCreated = DateTime.Now,
                                    Name = $"{item.Item1} - {item.Item2}",
                                    Description = $"This is an automatically generated edge for the network \"{network.Id}\".",
                                    DatabaseEdges = network.NetworkDatabases
                                        .Where(item1 => item1.Type == NetworkDatabaseType.Edge)
                                        .Select(item1 => item1.Database)
                                        .Distinct()
                                        .Select(item1 => new DatabaseEdge
                                        {
                                            DatabaseId = item1.Id,
                                            Database = item1
                                        })
                                        .ToList(),
                                    DatabaseEdgeFieldEdges = network.NetworkDatabases
                                        .Where(item1 => item1.Type == NetworkDatabaseType.Edge)
                                        .Select(item1 => item1.Database)
                                        .Select(item1 => item1.DatabaseEdgeFields)
                                        .SelectMany(item1 => item1)
                                        .Distinct()
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
                                            Node = network.NetworkNodes
                                                .FirstOrDefault(item1 => item1.Node.Name == item.Item1)?.Node,
                                            Type = EdgeNodeType.Source
                                        },
                                        new EdgeNode
                                        {
                                            Node = network.NetworkNodes
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
                        // Try to deserialize the data.
                        if (!network.Data.TryDeserializeJsonObject<IEnumerable<NetworkNodeInputModel>>(out var data) || data == null)
                        {
                            // Update the status of the item.
                            network.Status = NetworkStatus.Error;
                            // Add a message to the log.
                            network.Log = network.AppendToLog("The seed data corresponding to the network could not be deserialized.");
                            // Add the item to the list.
                            networksToEdit.Add(network);
                            // Continue.
                            continue;
                        }
                        // Get the IDs of the required related data.
                        var nodeDatabaseIds = network.NetworkDatabases
                            .Where(item => item.Type == NetworkDatabaseType.Node)
                            .Select(item => item.Database)
                            .Distinct()
                            .Select(item => item.Id);
                        var edgeDatabaseIds = network.NetworkDatabases
                            .Where(item => item.Type == NetworkDatabaseType.Edge)
                            .Select(item => item.Database)
                            .Distinct()
                            .Select(item => item.Id);
                        var nodeCollectionIds = network.NetworkNodeCollections
                            .Where(item => item.Type == NetworkNodeCollectionType.Seed)
                            .Select(item => item.NodeCollection)
                            .Distinct()
                            .Select(item => item.Id);
                        // Get the seed node identifiers from the data.
                        var seedNodeIdentifiers = data
                            .Where(item => item.Node != null)
                            .Select(item => item.Node)
                            .Where(item => !string.IsNullOrEmpty(item.Id))
                            .Select(item => item.Id)
                            .Distinct();
                        // Get the seed nodes in the provided data.
                        var seedNodesByIdentifiers = context.Databases
                            .Where(item => nodeDatabaseIds.Contains(item.Id))
                            .Select(item => item.DatabaseNodeFields)
                            .SelectMany(item => item)
                            .Where(item => item.IsSearchable)
                            .Select(item => item.DatabaseNodeFieldNodes)
                            .SelectMany(item => item)
                            .Where(item => seedNodeIdentifiers.Contains(item.Node.Id) || seedNodeIdentifiers.Contains(item.Value))
                            .Select(item => item.Node)
                            .Distinct();
                        // Get the seed nodes in the provided node collections.
                        var seedNodesByNodeCollections = context.NodeCollections
                            .Where(item => nodeCollectionIds.Contains(item.Id))
                            .Select(item => item.NodeCollectionNodes)
                            .SelectMany(item => item)
                            .Select(item => item.Node)
                            .Where(item => item.DatabaseNodeFieldNodes.Any(item1 => nodeDatabaseIds.Contains(item1.DatabaseNodeField.Database.Id)))
                            .Distinct();
                        // Get the seed nodes.
                        var seedNodes = seedNodesByIdentifiers.Concat(seedNodesByNodeCollections);
                        // Check if there haven't been any seed nodes found.
                        if (seedNodes == null || !seedNodes.Any())
                        {
                            // Update the status of the item.
                            network.Status = NetworkStatus.Error;
                            // Add a message to the log.
                            network.Log = network.AppendToLog("No seed nodes could be found with the provided seed data.");
                            // Add the item to the list.
                            networksToEdit.Add(network);
                            // Continue.
                            continue;
                        }
                        // Get all of the edges in the provided edge databases that match the given data.
                        var seedEdges = context.Databases
                            .Where(item => edgeDatabaseIds.Contains(item.Id))
                            .Select(item => item.DatabaseEdges)
                            .SelectMany(item => item)
                            .Select(item => item.Edge)
                            .Distinct();
                        // Check if there haven't been any seed edges found.
                        if (seedEdges == null || !seedEdges.Any())
                        {
                            // Update the status of the item.
                            network.Status = NetworkStatus.Error;
                            // Add a message to the log.
                            network.Log = network.AppendToLog("No seed edges could be found in the selected databases.");
                            // Add the item to the list.
                            networksToEdit.Add(network);
                            // Continue.
                            continue;
                        }
                        // Define the edges of the network.
                        var edges = new List<Edge>();
                        // Check which algorithm is selected.
                        if (network.Algorithm == NetworkAlgorithm.Neighbors)
                        {
                            // Get all edges that contain the seed nodes.
                            var currentEdges = seedEdges.Where(item => item.EdgeNodes.Any(item1 => seedNodes.Contains(item1.Node)));
                            // Add the edges to the list.
                            edges.AddRange(currentEdges);
                        }
                        else if (network.Algorithm == NetworkAlgorithm.Gap0 || network.Algorithm == NetworkAlgorithm.Gap1 || network.Algorithm == NetworkAlgorithm.Gap2 || network.Algorithm == NetworkAlgorithm.Gap3 || network.Algorithm == NetworkAlgorithm.Gap4)
                        {
                            // Get the gap value.
                            var gap = network.Algorithm == NetworkAlgorithm.Gap0 ? 0 :
                                network.Algorithm == NetworkAlgorithm.Gap1 ? 1 :
                                network.Algorithm == NetworkAlgorithm.Gap2 ? 2 :
                                network.Algorithm == NetworkAlgorithm.Gap3 ? 3 : 4;
                            // Define the list to store the edges.
                            var currentEdgeList = new List<List<Edge>>();
                            // For "gap" times, for all terminal nodes, add all possible edges.
                            for (int gapIndex = 0; gapIndex < gap + 1; gapIndex++)
                            {
                                // Get the terminal nodes (the seed nodes for the first iteration, the target nodes of all edges in the previous iteration for the subsequent iterations).
                                var terminalNodes = gapIndex == 0 ?
                                    seedNodes :
                                    currentEdgeList
                                        .Last()
                                        .Select(item => item.EdgeNodes
                                            .Where(item => item.Type == EdgeNodeType.Target)
                                            .Select(item => item.Node))
                                        .SelectMany(item => item);
                                // Get all edges that start in the terminal nodes.
                                var temporaryList = seedEdges
                                    .Where(item => item.EdgeNodes
                                        .Any(item1 => item1.Type == EdgeNodeType.Source && terminalNodes.Contains(item1.Node)))
                                    .ToList();
                                // Add them to the list.
                                currentEdgeList.Add(temporaryList);
                            }
                            // Define a variable to store, at each step, the nodes to keep.
                            var nodesToKeep = seedNodes.AsEnumerable();
                            // Starting from the right, mark all terminal nodes that are not seed nodes for removal.
                            for (int gapIndex = gap; gapIndex >= 0; gapIndex--)
                            {
                                // Remove from the list all edges that do not end in nodes to keep.
                                currentEdgeList.ElementAt(gapIndex)
                                    .RemoveAll(item => item.EdgeNodes
                                        .Any(item1 => item1.Type == EdgeNodeType.Target && !nodesToKeep.Contains(item1.Node)));
                                // Update the nodes to keep to be the source nodes of the interactions of the current step together with the seed nodes.
                                nodesToKeep = currentEdgeList.ElementAt(gapIndex)
                                    .Select(item => item.EdgeNodes
                                        .Where(item1 => item1.Type == EdgeNodeType.Source)
                                        .Select(item1 => item1.Node))
                                    .SelectMany(item => item)
                                    .Concat(seedNodes)
                                    .Distinct();
                            }
                            // Get the remaining edges.
                            var currentEdges = currentEdgeList.SelectMany(item => item).Distinct();
                            // Add all of the remaining edges.
                            edges.AddRange(currentEdges);
                        }
                        else
                        {
                            // Update the status of the item.
                            network.Status = NetworkStatus.Error;
                            // Add a message to the log.
                            network.Log = network.AppendToLog("The provided network generation algorithm is invalid.");
                            // Add the item to the list.
                            networksToEdit.Add(network);
                            // Continue.
                            continue;
                        }
                        // Check if there haven't been any edges found.
                        if (edges == null || !edges.Any())
                        {
                            // Update the status of the item.
                            network.Status = NetworkStatus.Error;
                            // Add a message to the log.
                            network.Log = network.AppendToLog("No edges could be found with the provided data using the provided algorithm.");
                            // Add the item to the list.
                            networksToEdit.Add(network);
                            // Continue.
                            continue;
                        }
                        // Get all of the nodes used by the found edges.
                        var nodes = edges
                            .Select(item => item.EdgeNodes)
                            .SelectMany(item => item)
                            .Select(item => item.Node)
                            .Distinct();
                        // Define the related entities.
                        network.NetworkNodes = nodes
                            .Intersect(seedNodes)
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
                    }
                    // Update the status of the item.
                    network.Status = NetworkStatus.Generated;
                    // Add a message to the log.
                    network.Log = network.AppendToLog("The network has been successfully generated.");
                    // Remove the generation data.
                    network.Data = null;
                    // Add the item to the list.
                    networksToEdit.Add(network);
                }
                // Edit the items.
                IEnumerableExtensions.Edit(networksToEdit, context, token);
            }
        }
    }
}
