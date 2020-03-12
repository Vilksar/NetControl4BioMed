using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;

namespace NetControl4BioMed.Pages.Content.Created.Networks
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public CreateModel(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string DatabaseTypeId { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string Name { get; set; }

            [DataType(DataType.MultilineText)]
            public string Description { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public NetworkAlgorithm Algorithm { get; set; }

            [DataType(DataType.MultilineText)]
            [Required(ErrorMessage = "This field is required.")]
            public string SeedData { get; set; }

            public IEnumerable<string> NodeDatabaseIds { get; set; }

            public IEnumerable<string> EdgeDatabaseIds { get; set; }

            public IEnumerable<string> SeedNodeCollectionIds { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public bool IsGeneric { get; set; }

            public IQueryable<Database> NodeDatabases { get; set; }

            public IQueryable<Database> EdgeDatabases { get; set; }

            public IQueryable<NodeCollection> SeedNodeCollections { get; set; }
        }

        public class ItemModel
        {
            public string SourceNode { get; set; } 

            public string TargetNode { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string databaseTypeId = null, string networkId = null)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if the user does not exist.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: An error occured while trying to load the user data. If you are already logged in, please log out and try again.";
                // Redirect to the home page.
                return RedirectToPage("/Index");
            }
            // Try to get the network with the provided ID.
            var network = !string.IsNullOrEmpty(networkId) ? _context.Networks
                .Where(item => item.NetworkUsers.Any(item1 => item1.User == user))
                .FirstOrDefault(item => item.Id == networkId) : null;
            // Check if there was no network found.
            if (!string.IsNullOrEmpty(networkId) && network == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No network could be found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Update the database type ID.
            databaseTypeId = network == null ? databaseTypeId : network.NetworkDatabases.FirstOrDefault()?.Database?.DatabaseType?.Id;
            // Check if there isn't any database type ID provided or if the database type ID couldn't be inferred.
            if (string.IsNullOrEmpty(databaseTypeId))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A type is required for creating an analysis.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Try to get the database type with the provided ID.
            var databaseType = _context.DatabaseTypes.FirstOrDefault(item => item.Id == databaseTypeId);
            // Check if there wasn't any database type found.
            if (databaseType == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No type could be found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                IsGeneric = databaseType.Name == "Generic",
                NodeDatabases = _context.Databases
                    .Where(item => item.DatabaseType == databaseType)
                    .Where(item => item.IsPublic || item.DatabaseUsers.Any(item1 => item1.User == user))
                    .Where(item => item.DatabaseNodeFields.Any(item1 => item1.IsSearchable)),
                EdgeDatabases = _context.Databases
                    .Where(item => item.DatabaseType == databaseType)
                    .Where(item => item.IsPublic || item.DatabaseUsers.Any(item1 => item1.User == user))
                    .Where(item => item.DatabaseEdges.Any()),
                SeedNodeCollections = _context.NodeCollections
                    .Where(item => item.NodeCollectionDatabases.Any(item1 => item1.Database.DatabaseType == databaseType))
                    .Where(item => item.NodeCollectionDatabases.Any(item1 => item1.Database.IsPublic || item1.Database.DatabaseUsers.Any(item2 => item2.User == user)))
            };
            // Check if there weren't any node databases available.
            if (!View.NodeDatabases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A new network can't be created, as there are no node databases available.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Check if there weren't any edge databases available.
            if (!View.EdgeDatabases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A new network can't be created, as there are no edge databases available.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Define the input.
            Input = network == null ?
                new InputModel
                {
                    DatabaseTypeId = databaseType.Id,
                    SeedData = View.IsGeneric ? JsonSerializer.Serialize(Enumerable.Empty<ItemModel>()) : JsonSerializer.Serialize(Enumerable.Empty<string>()),
                    NodeDatabaseIds = View.IsGeneric ? View.NodeDatabases.Select(item => item.Id) : Enumerable.Empty<string>(),
                    EdgeDatabaseIds = View.IsGeneric ? View.EdgeDatabases.Select(item => item.Id) : Enumerable.Empty<string>()
                } :
                new InputModel
                {
                    DatabaseTypeId = databaseType.Id,
                    Name = network.Name,
                    Description = network.Description,
                    Algorithm = network.Algorithm,
                    SeedData = View.IsGeneric ?
                        JsonSerializer.Serialize(network.NetworkEdges
                            .Select(item => new ItemModel
                            {
                                SourceNode = item.Edge.EdgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Source)?.Node?.Name,
                                TargetNode = item.Edge.EdgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Target)?.Node?.Name
                            })
                            .Where(item => !string.IsNullOrEmpty(item.SourceNode) && !string.IsNullOrEmpty(item.TargetNode))) :
                        JsonSerializer.Serialize(network.NetworkNodes
                            .Where(item => item.Type == NetworkNodeType.Seed)
                            .Select(item => item.Node.Name)),
                    NodeDatabaseIds = network.NetworkDatabases
                        .Select(item => item.Database)
                        .Intersect(View.NodeDatabases)
                        .Select(item => item.Id),
                    EdgeDatabaseIds = network.NetworkDatabases
                        .Select(item => item.Database)
                        .Intersect(View.EdgeDatabases)
                        .Select(item => item.Id),
                    SeedNodeCollectionIds = network.NetworkNodeCollections
                        .Select(item => item.NodeCollection)
                        .Intersect(View.SeedNodeCollections)
                        .Select(item => item.Id)
                };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if the user does not exist.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: An error occured while trying to load the user data. If you are already logged in, please log out and try again.";
                // Redirect to the home page.
                return RedirectToPage("/Index");
            }
            // Check if there isn't any database type ID provided or if the database type ID couldn't be inferred.
            if (string.IsNullOrEmpty(Input.DatabaseTypeId))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A type is required for creating an analysis.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Try to get the database type with the provided ID.
            var databaseType = _context.DatabaseTypes.FirstOrDefault(item => item.Id == Input.DatabaseTypeId);
            // Check if there wasn't any database type found.
            if (databaseType == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No type could be found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                IsGeneric = databaseType.Name == "Generic",
                NodeDatabases = _context.Databases
                    .Where(item => item.DatabaseType == databaseType)
                    .Where(item => item.IsPublic || item.DatabaseUsers.Any(item1 => item1.User == user))
                    .Where(item => item.DatabaseNodeFields.Any(item1 => item1.IsSearchable)),
                EdgeDatabases = _context.Databases
                    .Where(item => item.DatabaseType == databaseType)
                    .Where(item => item.IsPublic || item.DatabaseUsers.Any(item1 => item1.User == user))
                    .Where(item => item.DatabaseEdges.Any()),
                SeedNodeCollections = _context.NodeCollections
                    .Where(item => item.NodeCollectionDatabases.Any(item1 => item1.Database.DatabaseType == databaseType))
                    .Where(item => item.NodeCollectionDatabases.Any(item1 => item1.Database.IsPublic || item1.Database.DatabaseUsers.Any(item2 => item2.User == user)))
            };
            // Check if there weren't any node databases available.
            if (!View.NodeDatabases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A new network can't be created, as there are no node databases available.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Check if there weren't any edge databases available.
            if (!View.EdgeDatabases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A new network can't be created, as there are no edge databases available.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Check if the provided model isn't valid.
            if (!ModelState.IsValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error has been encountered. Please check again the input fields.");
                // Redisplay the page.
                return Page();
            }
            // Check if the algorithm is not valid.
            if (View.IsGeneric ^ Input.Algorithm == NetworkAlgorithm.None)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The algorithm is not valid for the provided database type.");
                // Redisplay the page.
                return Page();
            }
            // Get the provided node database IDs.
            var nodeDatabaseIds = Input.NodeDatabaseIds ?? Enumerable.Empty<string>();
            // Check if there weren't any node database IDs provided.
            if (!nodeDatabaseIds.Any())
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "At least one node database must be selected.");
                // Redisplay the page.
                return Page();
            }
            // Try to get the node databases with the provided IDs.
            var nodeDatabases = View.NodeDatabases.Where(item => nodeDatabaseIds.Contains(item.Id));
            // Check if there weren't any node databases found.
            if (!nodeDatabases.Any())
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "No node databases could be found with the provided IDs.");
                // Redisplay the page.
                return Page();
            }
            // Get the provided edge database IDs.
            var edgeDatabaseIds = Input.EdgeDatabaseIds ?? Enumerable.Empty<string>();
            // Check if there weren't any edge database IDs provided.
            if (!edgeDatabaseIds.Any())
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "At least one edge database must be selected.");
                // Redisplay the page.
                return Page();
            }
            // Try to get the edge databases with the provided IDs.
            var edgeDatabases = View.EdgeDatabases.Where(item => edgeDatabaseIds.Contains(item.Id));
            // Check if there weren't any edge databases found.
            if (!edgeDatabases.Any())
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "No edge databases could be found with the provided IDs.");
                // Redisplay the page.
                return Page();
            }
            // Check if the network is generic.
            if (View.IsGeneric)
            {
                // Try to deserialize the seed data.
                if (!Input.SeedData.TryDeserializeJsonObject<IEnumerable<ItemModel>>(out var seedItems))
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "The provided seed data is not a valid JSON object of edges.");
                    // Redisplay the page.
                    return Page();
                }
                // Get the edges from the items.
                var seedEdges = seedItems
                    .Where(item => !string.IsNullOrEmpty(item.SourceNode) && !string.IsNullOrEmpty(item.TargetNode))
                    .Select(item => (item.SourceNode, item.TargetNode))
                    .Distinct();
                // Check if there haven't been any edges found.
                if (seedEdges == null || !seedEdges.Any())
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "No seed edges could be found with the provided seed data.");
                    // Redisplay the page.
                    return Page();
                }
                // Get a list of all the nodes in the edges.
                var seedNodes = seedEdges
                    .Select(item => item.SourceNode)
                    .Concat(seedEdges.Select(item => item.TargetNode))
                    .Distinct();
                // Define the new network.
                var network = new Network
                {
                    DateTimeCreated = DateTime.Now,
                    Name = Input.Name,
                    Description = Input.Description,
                    Algorithm = Input.Algorithm,
                    NetworkDatabases = nodeDatabases
                        .Concat(edgeDatabases)
                        .Distinct()
                        .Select(item => new NetworkDatabase
                        {
                            Database = item
                        })
                        .ToList(),
                    NetworkUsers = new List<NetworkUser>
                    {
                        new NetworkUser
                        {
                            User = user,
                            DateTimeCreated = DateTime.Now
                        }
                    }
                };
                // Define the related entities.
                network.NetworkNodes = seedNodes
                    .Select(item => new NetworkNode
                    {
                        Node = new Node
                        {
                            DateTimeCreated = DateTime.Now,
                            Name = item,
                            Description = $"This is an automatically generated node for the network \"{Input.Name}\".",
                            DatabaseNodes = nodeDatabases
                                .Select(item1 => new DatabaseNode
                                {
                                    Database = item1
                                })
                                .ToList(),
                            DatabaseNodeFieldNodes = nodeDatabases
                                .Select(item1 => item1.DatabaseNodeFields)
                                .SelectMany(item1 => item1)
                                .Select(item1 => new DatabaseNodeFieldNode
                                {
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
                            Name = $"{item.SourceNode} - {item.TargetNode}",
                            Description = $"This is an automatically generated edge for the network \"{Input.Name}\".",
                            DatabaseEdges = edgeDatabases
                                .Select(item1 => new DatabaseEdge
                                {
                                    Database = item1
                                })
                                .ToList(),
                            DatabaseEdgeFieldEdges = edgeDatabases
                                .Select(item1 => item1.DatabaseEdgeFields)
                                .SelectMany(item1 => item1)
                                .Select(item1 => new DatabaseEdgeFieldEdge
                                {
                                    DatabaseEdgeField = item1,
                                    Value = $"{item.SourceNode} - {item.TargetNode}"
                                })
                                .ToList(),
                            EdgeNodes = new List<EdgeNode>
                            {
                                new EdgeNode
                                {
                                    Node = network.NetworkNodes
                                        .FirstOrDefault(item1 => item1.Node.Name == item.SourceNode)
                                        ?.Node,
                                    Type = EdgeNodeType.Source
                                },
                                new EdgeNode
                                {
                                    Node = network.NetworkNodes
                                        .FirstOrDefault(item1 => item1.Node.Name == item.TargetNode)
                                        ?.Node,
                                    Type = EdgeNodeType.Target
                                }
                            }
                            .Where(item1 => item1.Node != null)
                            .ToList()
                        }
                    })
                    .ToList();
                // Mark the data for addition.
                _context.Networks.Add(network);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
            }
            else
            {
                // Try to deserialize the data.
                if (!Input.SeedData.TryDeserializeJsonObject<IEnumerable<string>>(out var seedItems))
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "The provided seed data is not a valid JSON object of nodes.");
                    // Redisplay the page.
                    return Page();
                }
                // Get the provided seed node collection IDs.
                var seedNodeCollectionIds = Input.SeedNodeCollectionIds ?? Enumerable.Empty<string>();
                // Try to get the seed node collections with the provided IDs.
                var seedNodeCollections = View.SeedNodeCollections
                    .Where(item => seedNodeCollectionIds.Contains(item.Id));
                // Get all of the nodes in the provided node databases that match the given data.
                var seedNodes = nodeDatabases
                    .Select(item => item.DatabaseNodeFields)
                    .SelectMany(item => item)
                    .Where(item => item.IsSearchable)
                    .Select(item => item.DatabaseNodeFieldNodes)
                    .SelectMany(item => item)
                    .Where(item => seedItems.Contains(item.Node.Id) || seedItems.Contains(item.Value))
                    .Select(item => item.Node)
                    .Concat(seedNodeCollections
                        .Select(item => item.NodeCollectionNodes)
                        .SelectMany(item => item)
                        .Select(item => item.Node))
                    .Distinct();
                // Check if there haven't been any seed nodes found.
                if (seedNodes == null || !seedNodes.Any())
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "No seed nodes could be found with the provided seed data.");
                    // Redisplay the page.
                    return Page();
                }
                // Get all of the edges in the provided edge databases that match the given data.
                var seedEdges = edgeDatabases
                    .Select(item => item.DatabaseEdges)
                    .SelectMany(item => item)
                    .Select(item => item.Edge);
                // Check if there haven't been any seed edges found.
                if (seedEdges == null || !seedEdges.Any())
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "No seed edges could be found with the provided seed data.");
                    // Redisplay the page.
                    return Page();
                }
                // Define the edges of the network.
                var edges = new List<Edge>();
                // Check which algorithm is selected.
                if (Input.Algorithm == NetworkAlgorithm.Neighbors)
                {
                    // Add all of the edges which contain the seed nodes.
                    edges.AddRange(seedEdges.Where(item => item.EdgeNodes.Any(item1 => seedNodes.Contains(item1.Node))));
                }
                else if (Input.Algorithm == NetworkAlgorithm.Gap0 || Input.Algorithm == NetworkAlgorithm.Gap1 || Input.Algorithm == NetworkAlgorithm.Gap2 || Input.Algorithm == NetworkAlgorithm.Gap3 || Input.Algorithm == NetworkAlgorithm.Gap4)
                {
                    // Get the gap value.
                    var gap = Input.Algorithm == NetworkAlgorithm.Gap0 ? 0 :
                        Input.Algorithm == NetworkAlgorithm.Gap1 ? 1 :
                        Input.Algorithm == NetworkAlgorithm.Gap2 ? 2 :
                        Input.Algorithm == NetworkAlgorithm.Gap3 ? 3 : 4;
                    // Define the list to store the edges.
                    var list = new List<List<Edge>>();
                    // For "gap" times, for all terminal nodes, add all possible edges.
                    for (int index = 0; index < gap + 1; index++)
                    {
                        // Get the terminal nodes (the seed nodes for the first iteration, the target nodes of all edges in the previous iteration for the subsequent iterations).
                        var terminalNodes = index == 0 ? seedNodes : list.Last()
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
                        list.Add(temporaryList);
                    }
                    // Define a variable to store, at each step, the nodes to keep.
                    var nodesToKeep = seedNodes.AsEnumerable();
                    // Starting from the right, mark all terminal nodes that are not seed nodes for removal.
                    for (int index = gap; index >= 0; index--)
                    {
                        // Remove from the list all edges that do not end in nodes to keep.
                        list.ElementAt(index)
                            .RemoveAll(item => item.EdgeNodes.Any(item1 => item1.Type == EdgeNodeType.Target && !nodesToKeep.Contains(item1.Node)));
                        // Update the nodes to keep to be the source nodes of the interactions of the current step together with the seed nodes.
                        nodesToKeep = list.ElementAt(index)
                            .Select(item => item.EdgeNodes.Where(item1 => item1.Type == EdgeNodeType.Source).Select(item1 => item1.Node))
                            .SelectMany(item => item)
                            .Concat(seedNodes)
                            .Distinct();
                    }
                    // Add all of the remaining edges.
                    edges.AddRange(list.SelectMany(item => item).Distinct());
                }
                else
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "The provided algorithm is not yet implemented. Please select another algorithm.");
                    // Redisplay the page.
                    return Page();
                }
                // Check if there haven't been any edges found.
                if (edges == null || !edges.Any())
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "No edges could be found with the provided data using the provided algorithm.");
                    // Redisplay the page.
                    return Page();
                }
                // Get all of the nodes used by the found edges.
                var nodes = edges
                    .Select(item => item.EdgeNodes)
                    .SelectMany(item => item)
                    .Select(item => item.Node)
                    .Distinct();
                // Define the new network.
                var network = new Network
                {
                    DateTimeCreated = DateTime.Now,
                    Name = Input.Name,
                    Description = Input.Description,
                    Algorithm = Input.Algorithm,
                    NetworkDatabases = nodeDatabases
                        .Concat(edgeDatabases)
                        .Distinct()
                        .Select(item => new NetworkDatabase
                        {
                            Database = item
                        })
                        .ToList(),
                    NetworkNodes = nodes
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
                        .ToList(),
                    NetworkEdges = edges
                        .Select(item => new NetworkEdge
                        {
                            Edge = item
                        })
                        .ToList(),
                    NetworkNodeCollections = seedNodeCollections
                        .Select(item => new NetworkNodeCollection
                        {
                            NodeCollection = item,
                            Type = NetworkNodeCollectionType.Seed
                        })
                        .ToList(),
                    NetworkUsers = new List<NetworkUser>
                    {
                        new NetworkUser
                        {
                            User = user,
                            DateTimeCreated = DateTime.Now
                        }
                    }
                };
                // Mark the data for addition.
                _context.Networks.Add(network);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
            }
            // Display a message.
            TempData["StatusMessage"] = $"Success: 1 network of type \"{databaseType.Name}\" created successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Content/Created/Networks/Index");
        }
    }
}
