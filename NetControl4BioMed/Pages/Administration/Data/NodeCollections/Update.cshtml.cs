using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;

namespace NetControl4BioMed.Pages.Administration.Data.NodeCollections
{
    [Authorize(Roles = "Administrator")]
    public class UpdateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public UpdateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            [RegularExpression("Create|Edit|Delete")]
            public string Type { get; set; }

            [DataType(DataType.MultilineText)]
            [Required(ErrorMessage = "This field is required.")]
            public string Data { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public string JsonModel { get; set; }
        }

        public class ItemModel
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public IEnumerable<string> DatabaseIds { get; set; }

            public IEnumerable<string> NodeIds { get; set; }

            public static ItemModel Default = new ItemModel
            {
                Id = "Node collection ID",
                Name = "Node collection name",
                Description = "Node collection description",
                DatabaseIds = new List<string> { "Database ID" },
                NodeIds = new List<string> { "Node ID" }
            };
        }

        public IActionResult OnGet(string type = null, IEnumerable<string> ids = null)
        {
            // Define the view.
            View = new ViewModel
            {
                JsonModel = JsonSerializer.Serialize(new List<ItemModel> { ItemModel.Default }, new JsonSerializerOptions { WriteIndented = true })
            };
            // Check if there are any IDs provided.
            ids ??= Enumerable.Empty<string>();
            // Get the database items based on the provided IDs.
            var nodeCollections = _context.NodeCollections
                .Where(item => ids.Contains(item.Id));
            // Get the items for the view.
            var items = nodeCollections.Select(item =>
                new ItemModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    DatabaseIds = item.NodeCollectionDatabases.Select(item1 => item1.Database.Id),
                    NodeIds = item.NodeCollectionNodes.Select(item1 => item1.Node.Id)
                });
            // Define the input.
            Input = new InputModel
            {
                Type = type,
                Data = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true })
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Define the view.
            View = new ViewModel
            {
                JsonModel = JsonSerializer.Serialize(new List<ItemModel> { ItemModel.Default }, new JsonSerializerOptions { WriteIndented = true })
            };
            // Check if the provided model isn't valid.
            if (!ModelState.IsValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error has been encountered. Please check again the input fields.");
                // Redisplay the page.
                return Page();
            }
            // Try to deserialize the data.
            if (!Input.Data.TryDeserializeJsonObject<IEnumerable<ItemModel>>(out var items))
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The provided data is not a valid JSON object.");
                // Redisplay the page.
                return Page();
            }
            // Check if any of the items has any null values.
            if (items.Any(item => item.Id == null || item.Name == null || item.Description == null || item.DatabaseIds == null || item.NodeIds == null))
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The provided JSON data can't contain any \"null\" values. Please replace them, eventually with an empty string.");
                // Redisplay the page.
                return Page();
            }
            // Save the number of items.
            var itemCount = 0;
            // Check if the items should be created.
            if (Input.Type == "Create")
            {
                // Check if there weren't any valid items found.
                if (items == null || !items.Any())
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "No valid items could be found with the provided data.");
                    // Redisplay the page.
                    return Page();
                }
                // Get the manually provided IDs of all the items that are to be created.
                var itemNodeCollectionIds = items
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id);
                // Check if any of the manually provided IDs are repeating in the list.
                if (itemNodeCollectionIds.Distinct().Count() != itemNodeCollectionIds.Count())
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "One or more of the manually provided IDs are duplicated.");
                    // Redisplay the page.
                    return Page();
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
                // Save the start time of the loop.
                var startTime = DateTime.Now;
                // Go over each of the items.
                foreach (var item in items)
                {
                    // Check if the loop has been running for more than the allowed time.
                    if ((DateTime.Now - startTime).TotalSeconds > 180)
                    {
                        // End the loop.
                        break;
                    }
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
                // Save the number of node collections.
                itemCount = nodeCollections.Count();
                // Mark the nodes for addition.
                _context.NodeCollections.AddRange(nodeCollections);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
            }
            // Check if the items should be edited.
            else if (Input.Type == "Edit")
            {
                // Keep only the valid items.
                items = items
                    .Where(item => !string.IsNullOrEmpty(item.Id));
                // Check if there weren't any valid items found.
                if (items == null || !items.Any())
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "No valid items could be found with the provided data.");
                    // Redisplay the page.
                    return Page();
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
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "No node collections could be found in the database with the provided IDs.");
                    // Redisplay the page.
                    return Page();
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
                // Save the start time of the loop.
                var startTime = DateTime.Now;
                // Go over each of the valid items.
                foreach (var item in items)
                {
                    // Check if the loop has been running for more than the allowed time.
                    if ((DateTime.Now - startTime).TotalSeconds > 180)
                    {
                        // End the loop.
                        break;
                    }
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
                // Save the number of node collections.
                itemCount = nodeCollectionsToUpdate.Count();
                // Mark the node collections for updating.
                _context.NodeCollections.UpdateRange(nodeCollectionsToUpdate);
                // Get the networks and analyses that use the node collections.
                var networks = _context.Networks.Where(item => item.NetworkNodeCollections.Any(item1 => nodeCollectionsToUpdate.Contains(item1.NodeCollection)));
                var analyses = _context.Analyses.Where(item => item.AnalysisNodeCollections.Any(item1 => nodeCollectionsToUpdate.Contains(item1.NodeCollection)));
                // Mark the items for deletion.
                _context.Analyses.RemoveRange(analyses);
                _context.Networks.RemoveRange(networks);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
            }
            // Check if the items should be deleted.
            else if (Input.Type == "Delete")
            {
                // Keep only the valid items.
                items = items
                    .Where(item => !string.IsNullOrEmpty(item.Id));
                // Get the list of IDs from the provided items.
                var itemIds = items.Select(item => item.Id);
                // Get the node collections that have the given IDs.
                var nodeCollections = _context.NodeCollections
                    .Where(item => itemIds.Contains(item.Id));
                // Save the number of node collections found.
                itemCount = nodeCollections.Count();
                // Get the networks and analyses that use the node collections.
                var networks = _context.Networks.Where(item => item.NetworkNodeCollections.Any(item1 => nodeCollections.Contains(item1.NodeCollection)));
                var analyses = _context.Analyses.Where(item => item.AnalysisNodeCollections.Any(item1 => nodeCollections.Contains(item1.NodeCollection)));
                // Mark the items for deletion.
                _context.Analyses.RemoveRange(analyses);
                _context.Networks.RemoveRange(networks);
                _context.NodeCollections.RemoveRange(nodeCollections);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
            }
            // Check if the type is not valid.
            else
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The provided type is not valid.");
                // Redisplay the page.
                return Page();
            }
            // Display a message.
            TempData["StatusMessage"] = $"Success: {itemCount.ToString()} item{(itemCount != 1 ? "s" : string.Empty)} of type \"{Input.Type}\" updated successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Data/NodeCollections/Index");
        }
    }
}
