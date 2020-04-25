using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Services
{
    /// <summary>
    /// Implements a manager for the database data.
    /// </summary>
    public class DatabaseDataManager : IDatabaseDataManager
    {
        /// <summary>
        /// Represents the batch size for the length of the list of a database range operation.
        /// </summary>
        private readonly int _batchSize = 200;

        /// <summary>
        /// Represents the application database context.
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public DatabaseDataManager(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates the provided nodes in the database.
        /// </summary>
        /// <param name="items">The nodes to be created.</param>
        /// <returns></returns>
        public async Task CreateNodesAsync(IEnumerable<DataUpdateNodeViewModel> items)
        {
            // Check if there weren't any valid items found.
            if (items == null || !items.Any())
            {
                // Throw an exception.
                throw new ArgumentException("No valid items could be found with the provided data.");
            }
            // Get the manually provided IDs of all the items that are to be created.
            var itemNodeIds = items
                .Where(item => !string.IsNullOrEmpty(item.Id))
                .Select(item => item.Id);
            // Check if any of the manually provided IDs are repeating in the list.
            if (itemNodeIds.Distinct().Count() != itemNodeIds.Count())
            {
                // Throw an exception.
                throw new ArgumentException("One or more of the manually provided IDs are duplicated.");
            }
            // Get the valid manually provided IDs, that do not appear in the database.
            var validItemNodeIds = itemNodeIds
                .Except(_context.Nodes
                    .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => itemNodeIds.Contains(item.Id))
                    .Select(item => item.Id));
            // Get the IDs of all of the node fields that are to be updated.
            var itemNodeFieldIds = items
                .Select(item => item.Fields)
                .SelectMany(item => item)
                .Where(item => !string.IsNullOrEmpty(item.Key) && !string.IsNullOrEmpty(item.Value))
                .Select(item => item.Key)
                .Distinct();
            // Get the node fields that are to be updated.
            var nodeFields = _context.DatabaseNodeFields
                .Where(item => item.Database.DatabaseType.Name != "Generic")
                .Where(item => itemNodeFieldIds.Contains(item.Id))
                .Include(item => item.Database)
                .AsEnumerable();
            // Check if there weren't any node fields found.
            if (nodeFields == null || !nodeFields.Any())
            {
                // Throw an exception.
                throw new ArgumentException("No database node fields could be found in the database with the provided IDs.");
            }
            // Get the valid database node field IDs.
            var validItemNodeFieldIds = nodeFields
                .Select(item => item.Id);
            // Save the nodes to add.
            var nodes = new List<Node>();
            // Go over each of the items.
            foreach (var item in items)
            {
                // Check if the ID of the current item is valid.
                if (!string.IsNullOrEmpty(item.Id) && !validItemNodeIds.Contains(item.Id))
                {
                    // Continue.
                    continue;
                }
                // Get the valid item fields and the node field nodes to add.
                var nodeFieldNodes = item.Fields
                    .Select(item1 => (item1.Key, item1.Value))
                    .Distinct()
                    .Where(item1 => validItemNodeFieldIds.Contains(item1.Key))
                    .Select(item1 => new DatabaseNodeFieldNode { DatabaseNodeFieldId = item1.Key, DatabaseNodeField = nodeFields.FirstOrDefault(item2 => item1.Key == item2.Id), Value = item1.Value })
                    .Where(item1 => item1.DatabaseNodeField != null);
                // Check if there weren't any node fields found.
                if (nodeFieldNodes == null || !nodeFieldNodes.Any() || !nodeFieldNodes.Any(item1 => item1.DatabaseNodeField.IsSearchable))
                {
                    // Continue.
                    continue;
                }
                // Define the new node.
                var node = new Node
                {
                    Name = nodeFieldNodes.First(item1 => item1.DatabaseNodeField.IsSearchable).Value,
                    Description = item.Description,
                    DateTimeCreated = DateTime.Now,
                    DatabaseNodeFieldNodes = nodeFieldNodes.ToList(),
                    DatabaseNodes = nodeFieldNodes
                        .Select(item1 => item1.DatabaseNodeField.Database)
                        .Distinct()
                        .Select(item1 => new DatabaseNode { DatabaseId = item1.Id, Database = item1 })
                        .ToList()
                };
                // Check if there is any ID provided.
                if (!string.IsNullOrEmpty(item.Id))
                {
                    // Assign it to the node.
                    node.Id = item.Id;
                }
                // Add the new node to the list.
                nodes.Add(node);
            }
            // Create the items.
            await CreateAsync(nodes);
        }

        /// <summary>
        /// Creates the provided edges in the database.
        /// </summary>
        /// <param name="items">The edges to be created.</param>
        /// <returns></returns>
        public async Task CreateEdgesAsync(IEnumerable<DataUpdateEdgeViewModel> items)
        {// Check if there weren't any valid items found.
            if (items == null || !items.Any())
            {
                // Throw an exception.
                throw new ArgumentException("No valid items could be found with the provided data.");
            }
            // Get the manually provided IDs of all the items that are to be created.
            var itemEdgeIds = items
                .Where(item => !string.IsNullOrEmpty(item.Id))
                .Select(item => item.Id);
            // Check if any of the manually provided IDs are repeating in the list.
            if (itemEdgeIds.Distinct().Count() != itemEdgeIds.Count())
            {
                // Throw an exception.
                throw new ArgumentException("One or more of the manually provided IDs are duplicated.");
            }
            // Get the valid manually provided IDs, that do not appear in the database.
            var validItemEdgeIds = itemEdgeIds
                .Except(_context.Edges
                    .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => itemEdgeIds.Contains(item.Id))
                    .Select(item => item.Id));
            // Get the IDs of all of the nodes that are to be added to the edges.
            var itemNodeIds = items
                .Select(item => item.Nodes)
                .SelectMany(item => item)
                .Where(item => !string.IsNullOrEmpty(item.Id) && (item.Type == "Source" || item.Type == "Target"))
                .Select(item => item.Id)
                .Distinct();
            // Get the nodes that are to be added to the edges.
            var nodes = _context.Nodes
                .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                .Where(item => itemNodeIds.Contains(item.Id))
                .AsEnumerable();
            // Check if there weren't any nodes found.
            if (nodes == null || !nodes.Any())
            {
                // Throw an exception.
                throw new ArgumentException("No nodes could be found in the database with the provided IDs.");
            }
            // Get the valid database node field IDs.
            var validItemNodeIds = nodes
                .Select(item => item.Id);
            // Get the IDs of all of the edge fields that are to be updated.
            var itemEdgeFieldIds = items
                .Select(item => item.Fields)
                .SelectMany(item => item)
                .Where(item => !string.IsNullOrEmpty(item.Key) && !string.IsNullOrEmpty(item.Value))
                .Select(item => item.Key)
                .Distinct();
            // Get the edge fields that are to be updated.
            var edgeFields = _context.DatabaseEdgeFields
                .Where(item => item.Database.DatabaseType.Name != "Generic")
                .Where(item => itemEdgeFieldIds.Contains(item.Id))
                .Include(item => item.Database)
                .AsEnumerable();
            // Get the IDs of all of the databases that are to be updated.
            var itemDatabaseIds = items
                .Select(item => item.DatabaseIds)
                .SelectMany(item => item)
                .Where(item => !string.IsNullOrEmpty(item))
                .Concat(edgeFields.Select(item => item.Database.Id))
                .Distinct();
            // Get all of the databases that are to be updated.
            var databases = _context.Databases
                .Where(item => item.DatabaseType.Name != "Generic")
                .Where(item => itemDatabaseIds.Contains(item.Id))
                .AsEnumerable();
            // Check if there weren't any databases or edge fields found.
            if (databases == null || !databases.Any())
            {
                // Throw an exception.
                throw new ArgumentException("No databases could be found in the database with the provided IDs.");
            }
            // Get the valid database IDs.
            var validItemDatabaseIds = databases
                .Select(item => item.Id);
            // Get the valid database edge field IDs.
            var validItemEdgeFieldIds = edgeFields
                .Select(item => item.Id);
            // Save the edges to add.
            var edges = new List<Edge>();
            // Go over each of the items.
            foreach (var item in items)
            {
                // Check if the ID of the current item is valid.
                if (!string.IsNullOrEmpty(item.Id) && !validItemEdgeIds.Contains(item.Id))
                {
                    // Continue.
                    continue;
                }
                // Get the valid item nodes and the edge nodes to add.
                var edgeNodes = item.Nodes
                    .Where(item1 => item1.Type == "Source" || item1.Type == "Target")
                    .Select(item1 => (item1.Id, item1.Type))
                    .Distinct()
                    .Where(item1 => validItemNodeIds.Contains(item1.Id))
                    .Select(item1 => new EdgeNode { NodeId = item1.Id, Node = nodes.FirstOrDefault(item2 => item1.Id == item2.Id), Type = item1.Type == "Source" ? EdgeNodeType.Source : EdgeNodeType.Target })
                    .Where(item1 => item1.Node != null);
                // Check if there weren't any nodes found, or if there isn't at least one source node and one target node.
                if (edgeNodes == null || !edgeNodes.Any() || edgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Source) == null || edgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Target) == null)
                {
                    // Continue.
                    continue;
                }
                // Get the valid item fields and the edge field edges to add.
                var edgeFieldEdges = item.Fields
                    .Select(item1 => (item1.Key, item1.Value))
                    .Distinct()
                    .Where(item1 => validItemEdgeFieldIds.Contains(item1.Key))
                    .Select(item1 => new DatabaseEdgeFieldEdge { DatabaseEdgeFieldId = item1.Key, DatabaseEdgeField = edgeFields.FirstOrDefault(item2 => item1.Key == item2.Id), Value = item1.Value })
                    .Where(item1 => item1.DatabaseEdgeField != null);
                // Get the valid item databases and the database edges to add.
                var databaseEdges = item.DatabaseIds
                    .Where(item1 => validItemDatabaseIds.Contains(item1))
                    .Concat(edgeFieldEdges.Select(item1 => item1.DatabaseEdgeField.Database.Id))
                    .Distinct()
                    .Select(item1 => new DatabaseEdge { DatabaseId = item1, Database = databases.FirstOrDefault(item2 => item1 == item2.Id) });
                // Check if there weren't any databases or edge fields found.
                if (databaseEdges == null || !databaseEdges.Any())
                {
                    // Continue.
                    continue;
                }
                // Define the new edge.
                var edge = new Edge
                {
                    Name = string.Concat(edgeNodes.First(item1 => item1.Type == EdgeNodeType.Source).Node.Name, " - ", edgeNodes.First(item1 => item1.Type == EdgeNodeType.Target).Node.Name),
                    Description = item.Description,
                    DateTimeCreated = DateTime.Now,
                    EdgeNodes = new List<EdgeNode> { edgeNodes.First(item1 => item1.Type == EdgeNodeType.Source), edgeNodes.First(item1 => item1.Type == EdgeNodeType.Target) },
                    DatabaseEdgeFieldEdges = edgeFieldEdges.ToList(),
                    DatabaseEdges = databaseEdges.ToList()
                };
                // Check if there is any ID provided.
                if (!string.IsNullOrEmpty(item.Id))
                {
                    // Assign it to the node.
                    edge.Id = item.Id;
                }
                // Add the new node to the list.
                edges.Add(edge);
            }
            // Create the items.
            await CreateAsync(edges);
        }

        /// <summary>
        /// Creates the provided node collections in the database.
        /// </summary>
        /// <param name="items">The node collections to be created.</param>
        /// <returns></returns>
        public async Task CreateNodeCollectionsAsync(IEnumerable<DataUpdateNodeCollectionViewModel> items)
        {
            // Check if there weren't any valid items found.
            if (items == null || !items.Any())
            {
                // Throw an exception.
                throw new ArgumentException("No valid items could be found with the provided data.");
            }
            // Get the manually provided IDs of all the items that are to be created.
            var itemNodeCollectionIds = items
                .Where(item => !string.IsNullOrEmpty(item.Id))
                .Select(item => item.Id);
            // Check if any of the manually provided IDs are repeating in the list.
            if (itemNodeCollectionIds.Distinct().Count() != itemNodeCollectionIds.Count())
            {
                // Throw an exception.
                throw new ArgumentException("One or more of the manually provided IDs are duplicated.");
            }
            // Get the valid manually provided IDs, that do not appear in the database.
            var validItemNodeCollectionIds = itemNodeCollectionIds
                .Except(_context.NodeCollections
                    .Where(item => itemNodeCollectionIds.Contains(item.Id))
                    .Select(item => item.Id));
            // Get the IDs of all of the databases that are to be used by the collections.
            var itemDatabaseIds = items
                .Select(item => item.DatabaseIds)
                .SelectMany(item => item)
                .Where(item => !string.IsNullOrEmpty(item))
                .Distinct();
            // Get the databases that are to be used by the collections.
            var databases = _context.Databases
                .Where(item => item.DatabaseType.Name != "Generic")
                .Where(item => itemDatabaseIds.Contains(item.Id))
                .Include(item => item.DatabaseNodes)
                    .ThenInclude(item => item.Node)
                .AsEnumerable();
            // Get the IDs of all of the nodes that are to be added to the collections.
            var itemNodeIds = items
                .Select(item => item.NodeIds)
                .SelectMany(item => item)
                .Where(item => !string.IsNullOrEmpty(item))
                .Distinct();
            // Get the nodes that are to be added to the collections.
            var nodes = _context.Nodes
                .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                .Where(item => itemNodeIds.Contains(item.Id))
                .Include(item => item.DatabaseNodes)
                    .ThenInclude(item => item.Database)
                .AsEnumerable();
            // Get the valid database IDs.
            var validItemDatabaseIds = databases
                .Select(item => item.Id);
            // Get the valid node IDs.
            var validItemNodeIds = nodes
                .Select(item => item.Id);
            // Save the node collections to add.
            var nodeCollections = new List<NodeCollection>();
            // Go over each of the items.
            foreach (var item in items)
            {
                // Check if the ID of the current item is valid.
                if (!string.IsNullOrEmpty(item.Id) && !validItemNodeCollectionIds.Contains(item.Id))
                {
                    // Continue.
                    continue;
                }
                // Get the valid databases and the node collection databases to add.
                var nodeCollectionDatabases = item.DatabaseIds
                    .Where(item1 => validItemDatabaseIds.Contains(item1))
                    .Distinct()
                    .Select(item1 =>
                        new NodeCollectionDatabase
                        {
                            DatabaseId = item1,
                            Database = databases.FirstOrDefault(item2 => item1 == item2.Id)
                        })
                    .Where(item1 => item1.Database != null);
                // Get the valid nodes and the node collection nodes to add.
                var nodeCollectionNodes = item.NodeIds
                    .Where(item1 => validItemNodeIds.Contains(item1))
                    .Distinct()
                    .Select(item1 =>
                        new NodeCollectionNode
                        {
                            NodeId = item1,
                            Node = nodes.FirstOrDefault(item2 => item1 == item2.Id)
                        })
                    .Where(item1 => item1.Node != null);
                // Define the new node collection.
                var nodeCollection = new NodeCollection
                {
                    Name = item.Name,
                    Description = item.Description,
                    DateTimeCreated = DateTime.Now,
                    NodeCollectionDatabases = nodeCollectionDatabases
                        .Where(item1 => item1.Database.DatabaseNodes.Any(item2 => validItemNodeIds.Contains(item2.Node.Id)))
                        .ToList(),
                    NodeCollectionNodes = nodeCollectionNodes
                        .Where(item1 => item1.Node.DatabaseNodes.Any(item1 => validItemDatabaseIds.Contains(item1.Database.Id)))
                        .ToList()
                };
                // Check if there is any ID provided.
                if (!string.IsNullOrEmpty(item.Id))
                {
                    // Assign it to the node.
                    nodeCollection.Id = item.Id;
                }
                // Add the new node to the list.
                nodeCollections.Add(nodeCollection);
            }
            // Create the items.
            await CreateAsync(nodeCollections);
        }

        /// <summary>
        /// Updates the provided nodes in the database.
        /// </summary>
        /// <param name="items">The nodes to be updated.</param>
        /// <returns></returns>
        public async Task UpdateNodesAsync(IEnumerable<DataUpdateNodeViewModel> items)
        {
            // Check if there weren't any valid items found.
            if (items == null || !items.Any())
            {
                // Throw an exception.
                throw new ArgumentException("No valid items could be found with the provided data.");
            }
            // Get the list of IDs from the provided items.
            var itemIds = items.Select(item => item.Id);
            // Get the nodes from the database that have the given IDs.
            var nodes = _context.Nodes
                .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                .Where(item => itemIds.Contains(item.Id))
                .Include(item => item.DatabaseNodeFieldNodes)
                .Include(item => item.DatabaseNodes)
                .AsEnumerable();
            // Check if there weren't any nodes found.
            if (nodes == null || !nodes.Any())
            {
                // Throw an exception.
                throw new ArgumentException("No nodes could be found in the database with the provided IDs.");
            }
            // Get the IDs of all of the node fields that are to be updated.
            var itemNodeFieldIds = items
                .Select(item => item.Fields)
                .SelectMany(item => item)
                .Where(item => !string.IsNullOrEmpty(item.Key) && !string.IsNullOrEmpty(item.Value))
                .Select(item => item.Key)
                .Distinct();
            // Get the node fields that are to be updated.
            var nodeFields = _context.DatabaseNodeFields
                .Where(item => item.Database.DatabaseType.Name != "Generic")
                .Where(item => itemNodeFieldIds.Contains(item.Id))
                .Include(item => item.Database)
                .AsEnumerable();
            // Check if there weren't any node fields found.
            if (nodeFields == null || !nodeFields.Any())
            {
                // Throw an exception.
                throw new ArgumentException("No database node fields could be found in the database with the provided IDs.");
            }
            // Get the valid database node field IDs.
            var validItemNodeFieldIds = nodeFields
                .Select(item => item.Id);
            // Save the nodes to update.
            var nodesToUpdate = new List<Node>();
            // Go over each of the valid items.
            foreach (var item in items)
            {
                // Get the corresponding node.
                var node = nodes.FirstOrDefault(item1 => item.Id == item1.Id);
                // Check if there was no node found.
                if (node == null)
                {
                    // Continue.
                    continue;
                }
                // Get the valid item fields and the node field nodes to add.
                var nodeFieldNodes = item.Fields
                    .Select(item1 => (item1.Key, item1.Value))
                    .Distinct()
                    .Where(item1 => validItemNodeFieldIds.Contains(item1.Key))
                    .Select(item1 => new DatabaseNodeFieldNode { DatabaseNodeFieldId = item1.Key, DatabaseNodeField = nodeFields.FirstOrDefault(item2 => item1.Key == item2.Id), NodeId = node.Id, Node = node, Value = item1.Value })
                    .Where(item1 => item1.DatabaseNodeField != null && item1.Node != null);
                // Check if there weren't any node fields found.
                if (nodeFieldNodes == null || !nodeFieldNodes.Any() || !nodeFieldNodes.Any(item1 => item1.DatabaseNodeField.IsSearchable))
                {
                    // Continue.
                    continue;
                }
                // Update the node.
                node.Name = nodeFieldNodes.First(item1 => item1.DatabaseNodeField.IsSearchable).Value;
                node.Description = item.Description;
                node.DatabaseNodeFieldNodes = nodeFieldNodes.ToList();
                node.DatabaseNodes = nodeFieldNodes
                    .Select(item1 => item1.DatabaseNodeField.Database)
                    .Distinct()
                    .Select(item1 => new DatabaseNode { DatabaseId = item1.Id, Database = item1, NodeId = node.Id, Node = node })
                    .ToList();
                // Add the node to the list.
                nodesToUpdate.Add(node);
            }
            // Get the networks and analyses that contain the nodes.
            var networks = _context.Networks
                .Where(item => item.NetworkNodes.Any(item1 => nodesToUpdate.Contains(item1.Node)));
            var analyses = _context.Analyses
                .Where(item => item.AnalysisNodes.Any(item1 => nodesToUpdate.Contains(item1.Node)));
            // Delete the items.
            await DeleteAsync(analyses);
            await DeleteAsync(networks);
            // Update the nodes.
            await UpdateAsync(nodesToUpdate);
            // Get the edges that contain the nodes.
            var edges = _context.Edges
                .Where(item => item.EdgeNodes.Any(item1 => nodesToUpdate.Contains(item1.Node)))
                .Include(item => item.EdgeNodes)
                    .ThenInclude(item => item.Node)
                .AsEnumerable();
            // Go over each edge.
            foreach (var edge in edges)
            {
                // Update its name.
                edge.Name = string.Concat(edge.EdgeNodes.First(item => item.Type == EdgeNodeType.Source).Node.Name, " -> ", edge.EdgeNodes.First(item => item.Type == EdgeNodeType.Target).Node.Name);
            }
            // Update the edges.
            await UpdateAsync(edges);
        }

        /// <summary>
        /// Updates the provided edges in the database.
        /// </summary>
        /// <param name="items">The edges to be updated.</param>
        /// <returns></returns>
        public async Task UpdateEdgesAsync(IEnumerable<DataUpdateEdgeViewModel> items)
        {
            // Check if there weren't any valid items found.
            if (items == null || !items.Any())
            {
                // Throw an exception.
                throw new ArgumentException("No valid items could be found with the provided data.");
            }
            // Get the list of IDs from the provided items.
            var itemIds = items.Select(item => item.Id);
            // Get the edges from the database that have the given IDs.
            var edges = _context.Edges
                .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                .Where(item => itemIds.Contains(item.Id))
                .Include(item => item.EdgeNodes)
                    .ThenInclude(item => item.Node)
                .Include(item => item.DatabaseEdges)
                    .ThenInclude(item => item.Database)
                .Include(item => item.DatabaseEdgeFieldEdges)
                .AsEnumerable();
            // Check if there weren't any edges found.
            if (edges == null || !edges.Any())
            {
                // Throw an exception.
                throw new ArgumentException("No edges could be found in the database with the provided IDs.");
            }
            // Get the IDs of all of the nodes that are to be added to the edges.
            var itemNodeIds = items
                .Select(item => item.Nodes)
                .SelectMany(item => item)
                .Where(item => !string.IsNullOrEmpty(item.Id) && (item.Type == "Source" || item.Type == "Target"))
                .Select(item => item.Id)
                .Distinct();
            // Get the nodes that are to be added to the edges.
            var nodes = _context.Nodes
                .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                .Where(item => itemNodeIds.Contains(item.Id))
                .AsEnumerable();
            // Check if there weren't any nodes found.
            if (nodes == null || !nodes.Any())
            {
                // Throw an exception.
                throw new ArgumentException("No nodes could be found in the database with the provided IDs.");
            }
            // Get the valid database node field IDs.
            var validItemNodeIds = nodes
                .Select(item => item.Id);
            // Get the IDs of all of the edge fields that are to be updated.
            var itemEdgeFieldIds = items
                .Select(item => item.Fields)
                .SelectMany(item => item)
                .Where(item => !string.IsNullOrEmpty(item.Key) && !string.IsNullOrEmpty(item.Value))
                .Select(item => item.Key)
                .Distinct();
            // Get the edge fields that are to be updated.
            var edgeFields = _context.DatabaseEdgeFields
                .Where(item => item.Database.DatabaseType.Name != "Generic")
                .Where(item => itemEdgeFieldIds.Contains(item.Id))
                .Include(item => item.Database)
                .AsEnumerable();
            // Get the IDs of all of the databases that are to be updated.
            var itemDatabaseIds = items
                .Select(item => item.DatabaseIds)
                .SelectMany(item => item)
                .Where(item => !string.IsNullOrEmpty(item))
                .Concat(edgeFields.Select(item => item.Database.Id))
                .Distinct();
            // Get all of the databases that are to be updated.
            var databases = _context.Databases
                .Where(item => item.DatabaseType.Name != "Generic")
                .Where(item => itemDatabaseIds.Contains(item.Id))
                .AsEnumerable();
            // Check if there weren't any databases or edge fields found.
            if (databases == null || !databases.Any())
            {
                // Throw an exception.
                throw new ArgumentException("No databases could be found in the database with the provided IDs.");
            }
            // Get the valid database IDs.
            var validItemDatabaseIds = databases
                .Select(item => item.Id);
            // Get the valid database node field IDs.
            var validItemEdgeFieldIds = edgeFields
                .Select(item => item.Id);
            // Save the edges to update.
            var edgesToUpdate = new List<Edge>();
            // Go over each of the valid items.
            foreach (var item in items)
            {
                // Get the corresponding edge.
                var edge = edges.FirstOrDefault(item1 => item.Id == item1.Id);
                // Check if there was no edge found.
                if (edge == null)
                {
                    // Continue.
                    continue;
                }
                // Get the valid item nodes and the edge nodes to add.
                var edgeNodes = item.Nodes
                    .Where(item1 => item1.Type == "Source" || item1.Type == "Target")
                    .Select(item1 => (item1.Id, item1.Type))
                    .Distinct()
                    .Where(item1 => validItemNodeIds.Contains(item1.Id))
                    .Select(item1 => new EdgeNode { NodeId = item1.Id, Node = nodes.FirstOrDefault(item2 => item1.Id == item2.Id), Type = item1.Type == "Source" ? EdgeNodeType.Source : EdgeNodeType.Target })
                    .Where(item1 => item1.Node != null);
                // Check if there weren't any nodes found, or if there isn't at least one source node and one target node.
                if (edgeNodes == null || !edgeNodes.Any() || edgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Source) == null || edgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Target) == null)
                {
                    // Continue.
                    continue;
                }
                // Get the valid item fields and the edge field edges to add.
                var edgeFieldEdges = item.Fields
                    .Select(item1 => (item1.Key, item1.Value))
                    .Distinct()
                    .Where(item1 => validItemEdgeFieldIds.Contains(item1.Key))
                    .Select(item1 => new DatabaseEdgeFieldEdge { DatabaseEdgeFieldId = item1.Key, DatabaseEdgeField = edgeFields.FirstOrDefault(item2 => item1.Key == item2.Id), EdgeId = edge.Id, Edge = edge, Value = item1.Value })
                    .Where(item1 => item1.DatabaseEdgeField != null);
                // Get the valid item databases and the database edges to add.
                var databaseEdges = item.DatabaseIds
                    .Where(item1 => validItemDatabaseIds.Contains(item1))
                    .Concat(edgeFieldEdges.Select(item1 => item1.DatabaseEdgeField.Database.Id))
                    .Distinct()
                    .Select(item1 => new DatabaseEdge { DatabaseId = item1, Database = databases.FirstOrDefault(item2 => item1 == item2.Id), EdgeId = edge.Id, Edge = edge });
                // Check if there weren't any databases or edge fields found.
                if (databaseEdges == null || !databaseEdges.Any())
                {
                    // Continue.
                    continue;
                }
                // Update the edge.
                edge.Name = string.Concat(edgeNodes.First(item1 => item1.Type == EdgeNodeType.Source).Node.Name, " - ", edgeNodes.First(item1 => item1.Type == EdgeNodeType.Target).Node.Name);
                edge.Description = item.Description;
                edge.EdgeNodes = new List<EdgeNode> { edgeNodes.First(item1 => item1.Type == EdgeNodeType.Source), edgeNodes.First(item1 => item1.Type == EdgeNodeType.Target) };
                edge.DatabaseEdgeFieldEdges = edgeFieldEdges.ToList();
                edge.DatabaseEdges = databaseEdges.ToList().ToList();
                // Add the edge to the list.
                edgesToUpdate.Add(edge);
            }
            // Get the networks and analyses that contain the edges.
            var networks = _context.Networks
                .Where(item => item.NetworkEdges.Any(item1 => edgesToUpdate.Contains(item1.Edge)));
            var analyses = _context.Analyses
                .Where(item => item.AnalysisEdges.Any(item1 => edgesToUpdate.Contains(item1.Edge)));
            // Delete the items.
            await DeleteAsync(analyses);
            await DeleteAsync(networks);
            // Update the edges.
            await UpdateAsync(edgesToUpdate);
        }

        /// <summary>
        /// Updates the provided node collections in the database.
        /// </summary>
        /// <param name="items">The node collections to be updated.</param>
        /// <returns></returns>
        public async Task UpdateNodeCollectionsAsync(IEnumerable<DataUpdateNodeCollectionViewModel> items)
        {
            // Check if there weren't any valid items found.
            if (items == null || !items.Any())
            {
                // Throw an exception.
                throw new ArgumentException("No valid items could be found with the provided data.");
            }
            // Get the list of IDs from the provided items.
            var itemIds = items.Select(item => item.Id);
            // Get the node collections from the database that have the given IDs.
            var nodeCollections = _context.NodeCollections
                .Where(item => itemIds.Contains(item.Id))
                .AsEnumerable();
            // Check if there weren't any node collections found.
            if (nodeCollections == null || !nodeCollections.Any())
            {
                // Throw an exception.
                throw new ArgumentException("No node collections could be found in the database with the provided IDs.");
            }
            // Get the IDs of all of the databases that are to be used by the collections.
            var itemDatabaseIds = items
                .Select(item => item.DatabaseIds)
                .SelectMany(item => item)
                .Where(item => !string.IsNullOrEmpty(item))
                .Distinct();
            // Get the databases that are to be used by the collections.
            var databases = _context.Databases
                .Where(item => item.DatabaseType.Name != "Generic")
                .Where(item => itemDatabaseIds.Contains(item.Id))
                .Include(item => item.DatabaseNodes)
                    .ThenInclude(item => item.Node)
                .AsEnumerable();
            // Get the IDs of all of the nodes that are to be added to the collections.
            var itemNodeIds = items
                .Select(item => item.NodeIds)
                .SelectMany(item => item)
                .Where(item => !string.IsNullOrEmpty(item))
                .Distinct();
            // Get the nodes that are to be added to the collections.
            var nodes = _context.Nodes
                .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                .Where(item => itemNodeIds.Contains(item.Id))
                .Include(item => item.DatabaseNodes)
                    .ThenInclude(item => item.Database)
                .AsEnumerable();
            // Get the valid database IDs.
            var validItemDatabaseIds = databases
                .Select(item => item.Id);
            // Get the valid node IDs.
            var validItemNodeIds = nodes
                .Select(item => item.Id);
            // Save the nodes to update.
            var nodeCollectionsToUpdate = new List<NodeCollection>();
            // Go over each of the valid items.
            foreach (var item in items)
            {
                // Get the corresponding node collection.
                var nodeCollection = nodeCollections.FirstOrDefault(item1 => item.Id == item1.Id);
                // Check if there was no node collection found.
                if (nodeCollection == null)
                {
                    // Continue.
                    continue;
                }
                // Get the valid databases and the node collection databases to add.
                var nodeCollectionDatabases = item.DatabaseIds
                    .Where(item1 => validItemDatabaseIds.Contains(item1))
                    .Distinct()
                    .Select(item1 =>
                        new NodeCollectionDatabase
                        {
                            NodeCollectionId = nodeCollection.Id,
                            NodeCollection = nodeCollection,
                            DatabaseId = item1,
                            Database = databases.FirstOrDefault(item2 => item1 == item2.Id)
                        })
                    .Where(item1 => item1.NodeCollection != null && item1.Database != null);
                // Get the valid nodes and the node collection nodes to add.
                var nodeCollectionNodes = item.NodeIds
                    .Where(item1 => validItemNodeIds.Contains(item1))
                    .Distinct()
                    .Select(item1 =>
                        new NodeCollectionNode
                        {
                            NodeCollectionId = nodeCollection.Id,
                            NodeCollection = nodeCollection,
                            NodeId = item1,
                            Node = nodes.FirstOrDefault(item2 => item1 == item2.Id)
                        })
                    .Where(item1 => item1.NodeCollection != null && item1.Node != null);
                // Update the node collection.
                nodeCollection.Name = item.Name;
                nodeCollection.Description = item.Description;
                nodeCollection.NodeCollectionDatabases = nodeCollectionDatabases
                        .Where(item1 => item1.Database.DatabaseNodes.Any(item1 => validItemNodeIds.Contains(item1.Node.Id)))
                        .ToList();
                nodeCollection.NodeCollectionNodes = nodeCollectionNodes
                        .Where(item1 => item1.Node.DatabaseNodes.Any(item1 => validItemDatabaseIds.Contains(item1.Database.Id)))
                        .ToList();
                // Add the node collection to the list.
                nodeCollectionsToUpdate.Add(nodeCollection);
            }
            // Get the networks and analyses that use the node collections.
            var networks = _context.Networks
                .Where(item => item.NetworkNodeCollections.Any(item1 => nodeCollectionsToUpdate.Contains(item1.NodeCollection)));
            var analyses = _context.Analyses
                .Where(item => item.AnalysisNodeCollections.Any(item1 => nodeCollectionsToUpdate.Contains(item1.NodeCollection)));
            // Delete the items.
            await DeleteAsync(analyses);
            await DeleteAsync(networks);
            // Update the edges.
            await UpdateAsync(nodeCollectionsToUpdate);
        }

        /// <summary>
        /// Deletes the nodes with the provided IDs from the database.
        /// </summary>
        /// <param name="ids">The IDs of the nodes to be deleted.</param>
        /// <returns></returns>
        public async Task DeleteNodesAsync(IEnumerable<string> ids)
        {
            // Check if the IDs don't exist.
            if (ids == null)
            {
                // Throw an exception.
                throw new ArgumentNullException(nameof(ids));
            }
            // Get the items with the provided IDs.
            var nodes = _context.Nodes
                .Where(item => ids.Contains(item.Id));
            // Get the related entities that use the items.
            var edges = _context.Edges
                .Where(item => item.EdgeNodes.Any(item1 => nodes.Contains(item1.Node)));
            var networks = _context.Networks
                .Where(item => item.NetworkNodes.Any(item1 => nodes.Contains(item1.Node)));
            var analyses = _context.Analyses
                .Where(item => item.AnalysisNodes.Any(item1 => nodes.Contains(item1.Node)));
            // Delete the items.
            await DeleteAsync(analyses);
            await DeleteAsync(networks);
            await DeleteAsync(edges);
            await DeleteAsync(nodes);
        }

        /// <summary>
        /// Deletes the edges with the provided IDs from the database.
        /// </summary>
        /// <param name="ids">The IDs of the edges to be deleted.</param>
        /// <returns></returns>
        public async Task DeleteEdgesAsync(IEnumerable<string> ids)
        {
            // Check if the IDs don't exist.
            if (ids == null)
            {
                // Throw an exception.
                throw new ArgumentNullException(nameof(ids));
            }
            // Get the items with the provided IDs.
            var edges = _context.Edges
                .Where(item => ids.Contains(item.Id));
            // Get the related entities that use the items.
            var networks = _context.Networks
                .Where(item => item.NetworkEdges.Any(item1 => edges.Contains(item1.Edge)));
            var analyses = _context.Analyses
                .Where(item => item.AnalysisEdges.Any(item1 => edges.Contains(item1.Edge)));
            // Delete the items.
            await DeleteAsync(analyses);
            await DeleteAsync(networks);
            await DeleteAsync(edges);
        }

        /// <summary>
        /// Deletes the node collections with the provided IDs from the database.
        /// </summary>
        /// <param name="ids">The IDs of the node collections to be deleted.</param>
        /// <returns></returns>
        public async Task DeleteNodeCollectionsAsync(IEnumerable<string> ids)
        {
            // Check if the IDs don't exist.
            if (ids == null)
            {
                // Throw an exception.
                throw new ArgumentNullException(nameof(ids));
            }
            // Get the items with the provided IDs.
            var nodeCollections = _context.NodeCollections
                .Where(item => ids.Contains(item.Id));
            // Get the related entities that use the items.
            var networks = _context.Networks.Where(item => item.NetworkNodeCollections.Any(item1 => nodeCollections.Contains(item1.NodeCollection)));
            var analyses = _context.Analyses.Where(item => item.AnalysisNodeCollections.Any(item1 => nodeCollections.Contains(item1.NodeCollection)) || item.AnalysisNetworks.Any(item1 => networks.Contains(item1.Network)));
            // Delete the items.
            await DeleteAsync(analyses);
            await DeleteAsync(networks);
            await DeleteAsync(nodeCollections);
        }

        /// <summary>
        /// Deletes the networks with the provided IDs from the database.
        /// </summary>
        /// <param name="ids">The IDs of the networks to be deleted.</param>
        /// <returns></returns>
        public async Task DeleteNetworksAsync(IEnumerable<string> ids)
        {
            // Check if the IDs don't exist.
            if (ids == null)
            {
                // Throw an exception.
                throw new ArgumentNullException(nameof(ids));
            }
            // Get the items with the provided IDs.
            var networks = _context.Networks
                .Where(item => ids.Contains(item.Id));
            // Get the related entities that use the items.
            var analyses = _context.Analyses
                .Where(item => item.AnalysisNetworks.Any(item1 => networks.Contains(item1.Network)));
            // Get the generic entities among them.
            var genericNetworks = networks.Where(item => item.NetworkDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"));
            var genericNodes = _context.Nodes.Where(item => item.NetworkNodes.Any(item1 => genericNetworks.Contains(item1.Network)));
            var genericEdges = _context.Edges.Where(item => item.NetworkEdges.Any(item1 => genericNetworks.Contains(item1.Network)) || item.EdgeNodes.Any(item1 => genericNodes.Contains(item1.Node)));
            // Delete the items.
            await DeleteAsync(analyses);
            await DeleteAsync(networks);
            await DeleteAsync(genericEdges);
            await DeleteAsync(genericNodes);
        }

        /// <summary>
        /// Deletes the analyses with the provided IDs from the database.
        /// </summary>
        /// <param name="ids">The IDs of the analyses to be deleted.</param>
        /// <returns></returns>
        public async Task DeleteAnalysesAsync(IEnumerable<string> ids)
        {
            // Check if the IDs don't exist.
            if (ids == null)
            {
                // Throw an exception.
                throw new ArgumentNullException(nameof(ids));
            }
            // Get the items with the provided IDs.
            var analyses = _context.Analyses
                .Where(item => ids.Contains(item.Id));
            // Delete the items.
            await DeleteAsync(analyses);
        }

        /// <summary>
        /// Creates the provided items in the database.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="items">The items to be created.</param>
        /// <returns></returns>
        private async Task CreateAsync<T>(IEnumerable<T> items) where T : class
        {
            // Get the current type of the items.
            var type = typeof(T);
            // Check if the type is invalid.
            if (type != typeof(Node) && type != typeof(Edge) && type != typeof(NodeCollection))
            {
                // Throw an exception.
                throw new ArgumentException("The type is not valid in this context.");
            }
            // Check if the items don't exist.
            if (items == null)
            {
                // Throw an exception.
                throw new ArgumentNullException(nameof(items));
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)items.Count() / _batchSize);
            // Go over each batch.
            for (var index = 0; index < count; index++)
            {
                // Mark the items for addition.
                _context.Set<T>().AddRange(items.Skip(index * _batchSize).Take(_batchSize));
                // Save the changes to the database.
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Updates the provided items in the database.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="items">The items to be updated.</param>
        /// <returns></returns>
        private async Task UpdateAsync<T>(IEnumerable<T> items) where T : class
        {
            // Get the current type of the items.
            var type = typeof(T);
            // Check if the type is invalid.
            if (type != typeof(Node) && type != typeof(Edge) && type != typeof(NodeCollection))
            {
                // Throw an exception.
                throw new ArgumentException("The type is not valid in this context.");
            }
            // Check if the items don't exist.
            if (items == null)
            {
                // Throw an exception.
                throw new ArgumentNullException(nameof(items));
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)items.Count() / _batchSize);
            // Go over each batch.
            for (var index = 0; index < count; index++)
            {
                // Mark the items for update.
                _context.Set<T>().UpdateRange(items.Skip(index * _batchSize).Take(_batchSize));
                // Save the changes to the database.
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Deletes the provided items from the database.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="items">The items to be deleted.</param>
        /// <returns></returns>
        private async Task DeleteAsync<T>(IQueryable<T> items) where T : class
        {
            // Get the current type of the items.
            var type = typeof(T);
            // Check if the type is invalid.
            if (type != typeof(Node) && type != typeof(Edge) && type != typeof(NodeCollection) && type != typeof(Network) && type != typeof(Analysis))
            {
                // Throw an exception.
                throw new ArgumentException("The type is not valid in this context.");
            }
            // Check if the items don't exist.
            if (items == null)
            {
                // Throw an exception.
                throw new ArgumentNullException(nameof(items));
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)items.Count() / _batchSize);
            // Go over each batch.
            for (var index = 0; index < count; index++)
            {
                // Mark the items for deletion.
                _context.Set<T>().RemoveRange(items.Take(_batchSize));
                // Save the changes to the database.
                await _context.SaveChangesAsync();
            }
        }
    }
}
