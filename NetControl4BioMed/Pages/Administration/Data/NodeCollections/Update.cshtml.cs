using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office.CustomUI;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Tasks;
using NetControl4BioMed.Helpers.ViewModels;

namespace NetControl4BioMed.Pages.Administration.Data.NodeCollections
{
    [Authorize(Roles = "Administrator")]
    public class UpdateModel : PageModel
    {
        private static List<NodeCollectionInputModel> DefaultNodeCollectionInputModel { get; } = new List<NodeCollectionInputModel>
        {
            new NodeCollectionInputModel
            {
                Id = "ID",
                Name = "Name",
                Description = "Description",
                NodeCollectionDatabases = new List<NodeCollectionDatabaseInputModel>
                {
                    new NodeCollectionDatabaseInputModel
                    {
                        Database = new DatabaseInputModel
                        {
                            Id = "Database ID"
                        }
                    }
                },
                NodeCollectionNodes = new List<NodeCollectionNodeInputModel>
                {
                    new NodeCollectionNodeInputModel
                    {
                        Node = new NodeInputModel
                        {
                            Id = "Node ID"
                        }
                    }
                }
            }
        };

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

        public IActionResult OnGet(string type = null, IEnumerable<string> ids = null)
        {
            // Define the JSON serializer options.
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            // Define the view.
            View = new ViewModel
            {
                JsonModel = JsonSerializer.Serialize(DefaultNodeCollectionInputModel, jsonSerializerOptions)
            };
            // Check if there are any IDs provided.
            ids ??= Enumerable.Empty<string>();
            // Get the database items based on the provided IDs.
            var nodeCollections = _context.NodeCollections
                .Where(item => ids.Contains(item.Id));
            // Get the items for the view.
            var items = nodeCollections.Select(item => new NodeCollectionInputModel
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                NodeCollectionDatabases = item.NodeCollectionDatabases.Select(item1 => new NodeCollectionDatabaseInputModel
                {
                    Database = new DatabaseInputModel
                    {
                        Id = item1.Database.Id
                    }
                }),
                NodeCollectionNodes = item.NodeCollectionNodes.Select(item1 => new NodeCollectionNodeInputModel
                {
                    Node = new NodeInputModel
                    {
                        Id = item1.Node.Id
                    }
                })
            });
            // Define the input.
            Input = new InputModel
            {
                Type = type,
                Data = JsonSerializer.Serialize(items, jsonSerializerOptions)
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Define the JSON serializer options.
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            // Define the view.
            View = new ViewModel
            {
                JsonModel = JsonSerializer.Serialize(DefaultNodeCollectionInputModel, jsonSerializerOptions)
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
            if (!Input.Data.TryDeserializeJsonObject<IEnumerable<NodeCollectionInputModel>>(out var items) || items == null)
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
                // Save the number of items.
                itemCount = items.Count();
                // Define a new background task.
                var task = new BackgroundTask
                {
                    DateTimeCreated = DateTime.Now,
                    Name = $"{nameof(IAdministrationTaskManager)}.{nameof(IAdministrationTaskManager.CreateNodeCollections)}",
                    Data = JsonSerializer.Serialize(new NodeCollectionsTask
                    {
                        Items = items
                    })
                };
                // Mark the background task for addition.
                _context.BackgroundTasks.Add(task);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
                // Create a new Hangfire background job.
                var jobId = BackgroundJob.Enqueue<IAdministrationTaskManager>(item => item.CreateNodeCollections(task.Id, CancellationToken.None));
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
                // Save the number of items.
                itemCount = items.Count();
                // Define a new background task.
                var task = new BackgroundTask
                {
                    DateTimeCreated = DateTime.Now,
                    Name = $"{nameof(IAdministrationTaskManager)}.{nameof(IAdministrationTaskManager.EditNodeCollections)}",
                    Data = JsonSerializer.Serialize(new NodeCollectionsTask
                    {
                        Items = items
                    })
                };
                // Mark the background task for addition.
                _context.BackgroundTasks.Add(task);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
                // Create a new Hangfire background job.
                var jobId = BackgroundJob.Enqueue<IAdministrationTaskManager>(item => item.EditNodeCollections(task.Id, CancellationToken.None));
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
                // Define a new background task.
                var task = new BackgroundTask
                {
                    DateTimeCreated = DateTime.Now,
                    Name = $"{nameof(IAdministrationTaskManager)}.{nameof(IAdministrationTaskManager.DeleteNodeCollections)}",
                    Data = JsonSerializer.Serialize(new NodeCollectionsTask
                    {
                        Items = nodeCollections.Select(item => new NodeCollectionInputModel
                        {
                            Id = item.Id
                        })
                    })
                };
                // Mark the background task for addition.
                _context.BackgroundTasks.Add(task);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
                // Create a new Hangfire background job.
                var jobId = BackgroundJob.Enqueue<IAdministrationTaskManager>(item => item.DeleteNodeCollections(task.Id, CancellationToken.None));
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
            TempData["StatusMessage"] = $"Success: A new background task was created to {Input.Type.ToLower()} {itemCount} node collection{(itemCount != 1 ? "s" : string.Empty)}.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Data/NodeCollections/Index");
        }
    }
}
