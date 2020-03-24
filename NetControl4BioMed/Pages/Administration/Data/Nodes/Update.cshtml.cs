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

namespace NetControl4BioMed.Pages.Administration.Data.Nodes
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

            public IEnumerable<FieldModel> Fields { get; set; }

            public class FieldModel
            {
                public string Key { get; set; }

                public string Value { get; set; }
            }

            public static ItemModel Default = new ItemModel
            {
                Id = "Node ID",
                Description = "Node description",
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
            var nodes = _context.Nodes
                .Where(item => !item.DatabaseNodes.Any(item => item.Database.DatabaseType.Name == "Generic"))
                .Where(item => ids.Contains(item.Id));
            // Get the items for the view.
            var items = nodes.Select(item =>
                new ItemModel
                {
                    Id = item.Id,
                    Description = item.Description,
                    Fields = item.DatabaseNodeFieldNodes.Select(item1 => new ItemModel.FieldModel { Key = item1.DatabaseNodeField.Id, Value = item1.Value })
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
            if (items.Any(item => item.Id == null || item.Description == null || item.Fields == null || item.Fields.Any(item1 => item1.Key == null || item1.Value == null)))
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
                // Keep only the valid items.
                items = items
                    .Where(item => item.Fields != null && item.Fields.Any());
                // Check if there weren't any valid items found.
                if (items == null || !items.Any())
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "No valid items could be found with the provided data.");
                    // Redisplay the page.
                    return Page();
                }
                // Get the manually provided IDs of all the items that are to be created.
                var itemNodeIds = items
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id);
                // Check if any of the manually provided IDs are repeating in the list.
                if (itemNodeIds.Distinct().Count() != itemNodeIds.Count())
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "One or more of the manually provided IDs are duplicated.");
                    // Redisplay the page.
                    return Page();
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
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "No database node fields could be found in the database with the provided IDs.");
                    // Redisplay the page.
                    return Page();
                }
                // Get the valid database node field IDs.
                var validItemNodeFieldIds = nodeFields
                    .Select(item => item.Id);
                // Save the nodes to add.
                var nodes = new List<Node>();
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
                // Save the number of nodes.
                itemCount = nodes.Count();
                // Mark the nodes for addition.
                _context.Nodes.AddRange(nodes);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
            }
            // Check if the items should be edited.
            else if (Input.Type == "Edit")
            {
                // Keep only the valid items.
                items = items
                    .Where(item => !string.IsNullOrEmpty(item.Id) && item.Fields != null && item.Fields.Any());
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
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "No nodes could be found in the database with the provided IDs.");
                    // Redisplay the page.
                    return Page();
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
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "No database node fields could be found in the database with the provided IDs.");
                    // Redisplay the page.
                    return Page();
                }
                // Get the valid database node field IDs.
                var validItemNodeFieldIds = nodeFields
                    .Select(item => item.Id);
                // Save the nodes to update.
                var nodesToUpdate = new List<Node>();
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
                // Save the number of nodes.
                itemCount = nodesToUpdate.Count();
                // Mark the nodes for updating.
                _context.Nodes.UpdateRange(nodesToUpdate);
                // Get the networks and analyses that contain the nodes.
                var networks = _context.Networks.Where(item => item.NetworkNodes.Any(item1 => nodesToUpdate.Contains(item1.Node)));
                var analyses = _context.Analyses.Where(item => item.AnalysisNodes.Any(item1 => nodesToUpdate.Contains(item1.Node)));
                // Mark the items for deletion.
                _context.Analyses.RemoveRange(analyses);
                _context.Networks.RemoveRange(networks);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
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
                // Mark the edges for updating.
                _context.Edges.UpdateRange(edges);
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
                // Get the nodes from the non-generic databases that have the given IDs.
                var nodes = _context.Nodes
                    .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => itemIds.Contains(item.Id));
                // Save the number of nodes found.
                itemCount = nodes.Count();
                // Get the edges, networks and analyses that contain the nodes.
                var edges = _context.Edges.Where(item => item.EdgeNodes.Any(item1 => nodes.Contains(item1.Node)));
                var networks = _context.Networks.Where(item => item.NetworkNodes.Any(item1 => nodes.Contains(item1.Node)));
                var analyses = _context.Analyses.Where(item => item.AnalysisNodes.Any(item1 => nodes.Contains(item1.Node)));
                // Mark the items for deletion.
                _context.Analyses.RemoveRange(analyses);
                _context.Networks.RemoveRange(networks);
                _context.Edges.RemoveRange(edges);
                _context.Nodes.RemoveRange(nodes);
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
            return RedirectToPage("/Administration/Data/Nodes/Index");
        }
    }
}
