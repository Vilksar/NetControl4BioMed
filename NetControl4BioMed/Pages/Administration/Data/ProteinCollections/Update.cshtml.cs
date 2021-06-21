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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.Administration.Data.ProteinCollections
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

        public IActionResult OnGet(string type = null, IEnumerable<string> ids = null)
        {
            // Define the JSON serializer options.
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                IgnoreNullValues = true
            };
            // Define the view.
            View = new ViewModel
            {
                JsonModel = JsonSerializer.Serialize(new List<ProteinCollectionInputModel>
                {
                    new ProteinCollectionInputModel
                    {
                        Id = "ID",
                        Name = "Name",
                        Description = "Description",
                        ProteinCollectionTypes = new List<ProteinCollectionTypeInputModel>
                        {
                            new ProteinCollectionTypeInputModel
                            {
                                Type = "Type"
                            }
                        },
                        ProteinCollectionProteins = new List<ProteinCollectionProteinInputModel>
                        {
                            new ProteinCollectionProteinInputModel
                            {
                                Protein = new ProteinInputModel
                                {
                                    Id = "Protein ID"
                                }
                            }
                        }
                    }
                }, jsonSerializerOptions)
            };
            // Check if there are any IDs provided.
            ids ??= Enumerable.Empty<string>();
            // Get the database items based on the provided IDs.
            var proteinCollections = _context.ProteinCollections
                .Where(item => ids.Contains(item.Id));
            // Get the items for the view.
            var items = proteinCollections.Select(item => new ProteinCollectionInputModel
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                ProteinCollectionTypes = item.ProteinCollectionTypes.Select(item1 => new ProteinCollectionTypeInputModel
                {
                    Type = item1.Type.ToString()
                }),
                ProteinCollectionProteins = item.ProteinCollectionProteins.Select(item1 => new ProteinCollectionProteinInputModel
                {
                    Protein = new ProteinInputModel
                    {
                        Id = item1.Protein.Id
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
                WriteIndented = true,
                IgnoreNullValues = true
            };
            // Define the view.
            View = new ViewModel
            {
                JsonModel = JsonSerializer.Serialize(new List<ProteinCollectionInputModel>
                {
                    new ProteinCollectionInputModel
                    {
                        Id = "ID",
                        Name = "Name",
                        Description = "Description",
                        ProteinCollectionTypes = new List<ProteinCollectionTypeInputModel>
                        {
                            new ProteinCollectionTypeInputModel
                            {
                                Type = "Type"
                            }
                        },
                        ProteinCollectionProteins = new List<ProteinCollectionProteinInputModel>
                        {
                            new ProteinCollectionProteinInputModel
                            {
                                Protein = new ProteinInputModel
                                {
                                    Id = "Protein ID"
                                }
                            }
                        }
                    }
                }, jsonSerializerOptions)
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
            if (!Input.Data.TryDeserializeJsonObject<IEnumerable<ProteinCollectionInputModel>>(out var items) || items == null)
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
                var itemProteinCollectionIds = items
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id);
                // Check if any of the manually provided IDs are repeating in the list.
                if (itemProteinCollectionIds.Distinct().Count() != itemProteinCollectionIds.Count())
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
                    DateTimeCreated = DateTime.UtcNow,
                    Name = $"{nameof(IAdministrationTaskManager)}.{nameof(IAdministrationTaskManager.CreateProteinCollectionsAsync)}",
                    Data = JsonSerializer.Serialize(new ProteinCollectionsTask
                    {
                        Items = items
                    }, new JsonSerializerOptions { IgnoreNullValues = true })
                };
                // Mark the background task for addition.
                _context.BackgroundTasks.Add(task);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
                // Create a new Hangfire background job.
                var jobId = BackgroundJob.Enqueue<IAdministrationTaskManager>(item => item.CreateProteinCollectionsAsync(task.Id, CancellationToken.None));
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
                    DateTimeCreated = DateTime.UtcNow,
                    Name = $"{nameof(IAdministrationTaskManager)}.{nameof(IAdministrationTaskManager.EditProteinCollectionsAsync)}",
                    Data = JsonSerializer.Serialize(new ProteinCollectionsTask
                    {
                        Items = items
                    }, new JsonSerializerOptions { IgnoreNullValues = true })
                };
                // Mark the background task for addition.
                _context.BackgroundTasks.Add(task);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
                // Create a new Hangfire background job.
                var jobId = BackgroundJob.Enqueue<IAdministrationTaskManager>(item => item.EditProteinCollectionsAsync(task.Id, CancellationToken.None));
            }
            // Check if the items should be deleted.
            else if (Input.Type == "Delete")
            {
                // Keep only the valid items.
                items = items
                    .Where(item => !string.IsNullOrEmpty(item.Id));
                // Get the list of IDs from the provided items.
                var itemIds = items.Select(item => item.Id);
                // Get the protein collections that have the given IDs.
                var proteinCollections = _context.ProteinCollections
                    .Where(item => itemIds.Contains(item.Id));
                // Save the number of protein collections found.
                itemCount = proteinCollections.Count();
                // Define a new background task.
                var task = new BackgroundTask
                {
                    DateTimeCreated = DateTime.UtcNow,
                    Name = $"{nameof(IAdministrationTaskManager)}.{nameof(IAdministrationTaskManager.DeleteProteinCollectionsAsync)}",
                    Data = JsonSerializer.Serialize(new ProteinCollectionsTask
                    {
                        Items = proteinCollections.Select(item => new ProteinCollectionInputModel
                        {
                            Id = item.Id
                        })
                    }, new JsonSerializerOptions { IgnoreNullValues = true })
                };
                // Mark the background task for addition.
                _context.BackgroundTasks.Add(task);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
                // Create a new Hangfire background job.
                var jobId = BackgroundJob.Enqueue<IAdministrationTaskManager>(item => item.DeleteProteinCollectionsAsync(task.Id, CancellationToken.None));
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
            TempData["StatusMessage"] = $"Success: A new background task was created to {Input.Type.ToLower()} {itemCount} protein collection{(itemCount != 1 ? "s" : string.Empty)}.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Data/ProteinCollections/Index");
        }
    }
}
