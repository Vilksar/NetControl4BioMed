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
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;

namespace NetControl4BioMed.Pages.Administration.Content.Edges
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

            public string Description { get; set; }

            public IEnumerable<string> DatabaseIds { get; set; }

            public IEnumerable<NodeModel> Nodes { get; set; }

            public class NodeModel
            {
                public string Id { get; set; }

                public string Type { get; set; }
            }

            public IEnumerable<FieldModel> Fields { get; set; }

            public class FieldModel
            {
                public string Key { get; set; }

                public string Value { get; set; }
            }

            public static ItemModel Default = new ItemModel
            {
                Id = "Edge ID",
                Description = "Edge description",
                DatabaseIds = new List<string> { "Database ID" },
                Nodes = new List<NodeModel>
                {
                    new NodeModel
                    {
                        Id = "Source node ID",
                        Type = "Source"
                    },
                    new NodeModel
                    {
                        Id = "Target node ID",
                        Type = "Target"
                    }
                },
                Fields = new List<FieldModel>
                {
                    new FieldModel
                    {
                        Key = "Field ID",
                        Value = "Value"
                    }
                }
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
            var edges = _context.Edges
                .Where(item => !item.DatabaseEdges.Any(item => item.Database.DatabaseType.Name == "Generic"))
                .Where(item => ids.Contains(item.Id));
            // Get the items for the view.
            var items = edges.Select(item =>
                new ItemModel
                {
                    Id = item.Id,
                    Description = item.Description,
                    Nodes = item.EdgeNodes.Select(item1 => new ItemModel.NodeModel { Id = item1.Node.Id, Type = item1.Type.ToString() }),
                    Fields = item.DatabaseEdgeFieldEdges.Select(item1 => new ItemModel.FieldModel { Key = item1.DatabaseEdgeField.Id, Value = item1.Value })
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
            // Save the number of items.
            var itemCount = 0;
            // Check if the items should be created.
            if (Input.Type == "Create")
            {
                // Keep only the valid items.
                items = items
                    .Where(item => item.Nodes != null && item.Nodes.Any() && ((item.DatabaseIds != null && item.DatabaseIds.Any()) || (item.Fields != null && item.Fields.Any())));
                // Check if there weren't any valid items found.
                if (items == null || !items.Any())
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "No valid items could be found with the provided data.");
                    // Redisplay the page.
                    return Page();
                }
                // Get the manually provided IDs of all the items that are to be created.
                var itemEdgeIds = items
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id);
                // Check if any of the manually provided IDs are repeating in the list.
                if (itemEdgeIds.Distinct().Count() != itemEdgeIds.Count())
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "One or more of the manually provided IDs are duplicated.");
                    // Redisplay the page.
                    return Page();
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
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "No nodes could be found in the database with the provided IDs.");
                    // Redisplay the page.
                    return Page();
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
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "No databases could be found in the database with the provided IDs.");
                    // Redisplay the page.
                    return Page();
                }
                // Get the valid database IDs.
                var validItemDatabaseIds = databases
                    .Select(item => item.Id);
                // Get the valid database node field IDs.
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
                        Name = string.Concat(edgeNodes.First(item1 => item1.Type == EdgeNodeType.Source).Node.Name, " -> ", edgeNodes.First(item1 => item1.Type == EdgeNodeType.Target).Node.Name),
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
                // Save the number of edges.
                itemCount = edges.Count();
                // Mark the nodes for addition.
                _context.Edges.AddRange(edges);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
            }
            // Check if the items should be edited.
            else if (Input.Type == "Edit")
            {
                // Keep only the valid items.
                items = items
                    .Where(item => !string.IsNullOrEmpty(item.Id) && item.Nodes != null && item.Nodes.Any());
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
                // Get the edges from the database that have the given IDs.
                var edges = _context.Edges
                    .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => itemIds.Contains(item.Id))
                    .Include(item => item.DatabaseEdgeFieldEdges)
                    .Include(item => item.DatabaseEdges)
                    .AsEnumerable();
                // Check if there weren't any edges found.
                if (edges == null || !edges.Any())
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "No edges could be found in the database with the provided IDs.");
                    // Redisplay the page.
                    return Page();
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
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "No nodes could be found in the database with the provided IDs.");
                    // Redisplay the page.
                    return Page();
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
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "No databases could be found in the database with the provided IDs.");
                    // Redisplay the page.
                    return Page();
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
                    edge.Name = string.Concat(edgeNodes.First(item1 => item1.Type == EdgeNodeType.Source).Node.Name, " -> ", edgeNodes.First(item1 => item1.Type == EdgeNodeType.Target).Node.Name);
                    edge.Description = item.Description;
                    edge.EdgeNodes = new List<EdgeNode> { edgeNodes.First(item1 => item1.Type == EdgeNodeType.Source), edgeNodes.First(item1 => item1.Type == EdgeNodeType.Target) };
                    edge.DatabaseEdgeFieldEdges = edgeFieldEdges.ToList();
                    edge.DatabaseEdges = databaseEdges.ToList().ToList();
                    // Add the edge to the list.
                    edgesToUpdate.Add(edge);
                }
                // Save the number of edges.
                itemCount = edgesToUpdate.Count();
                // Mark the edges for updating.
                _context.Edges.UpdateRange(edgesToUpdate);
                // Get the networks and analyses that contain the edges.
                var networks = _context.Networks.Where(item => item.NetworkEdges.Any(item1 => edgesToUpdate.Contains(item1.Edge)));
                var analyses = _context.Analyses.Where(item => item.AnalysisEdges.Any(item1 => edgesToUpdate.Contains(item1.Edge)));
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
                // Get the edges from the non-generic databases that have the given IDs.
                var edges = _context.Edges
                    .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => itemIds.Contains(item.Id));
                // Save the number of edges found.
                itemCount = edges.Count();
                // Get the networks and analyses that contain the edges.
                var networks = _context.Networks.Where(item => item.NetworkEdges.Any(item1 => edges.Contains(item1.Edge)));
                var analyses = _context.Analyses.Where(item => item.AnalysisEdges.Any(item1 => edges.Contains(item1.Edge)));
                // Mark the items for deletion.
                _context.Analyses.RemoveRange(analyses);
                _context.Networks.RemoveRange(networks);
                _context.Edges.RemoveRange(edges);
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
            return RedirectToPage("/Administration/Content/Nodes/Index");
        }
    }
}
