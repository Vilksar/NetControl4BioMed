using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.ViewModels;

namespace NetControl4BioMed.Pages.Administration.Data.Edges
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
            // Define the view.
            View = new ViewModel
            {
                JsonModel = JsonSerializer.Serialize(new List<DataUpdateEdgeViewModel> { DataUpdateEdgeViewModel.Default }, new JsonSerializerOptions { WriteIndented = true })
            };
            // Check if there are any IDs provided.
            ids ??= Enumerable.Empty<string>();
            // Get the database items based on the provided IDs.
            var edges = _context.Edges
                .Where(item => !item.DatabaseEdges.Any(item => item.Database.DatabaseType.Name == "Generic"))
                .Where(item => ids.Contains(item.Id))
                .Include(item => item.EdgeNodes)
                    .ThenInclude(item => item.Node)
                .Include(item => item.DatabaseEdges)
                    .ThenInclude(item => item.Database)
                .Include(item => item.DatabaseEdgeFieldEdges);
            // Get the items for the view.
            var items = edges.Select(item =>
                new DataUpdateEdgeViewModel
                {
                    Id = item.Id,
                    Description = item.Description,
                    DatabaseIds = item.DatabaseEdges.Select(item1 => item1.Database.Id),
                    Nodes = item.EdgeNodes.Select(item1 => new DataUpdateEdgeViewModel.NodeModel { Id = item1.Node.Id, Type = item1.Type.ToString() }),
                    Fields = item.DatabaseEdgeFieldEdges.Select(item1 => new DataUpdateEdgeViewModel.FieldModel { Key = item1.DatabaseEdgeField.Id, Value = item1.Value })
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

        public IActionResult OnPost()
        {
            // Define the view.
            View = new ViewModel
            {
                JsonModel = JsonSerializer.Serialize(new List<DataUpdateEdgeViewModel> { DataUpdateEdgeViewModel.Default }, new JsonSerializerOptions { WriteIndented = true })
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
            if (!Input.Data.TryDeserializeJsonObject<IEnumerable<DataUpdateEdgeViewModel>>(out var items))
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The provided data is not a valid JSON object.");
                // Redisplay the page.
                return Page();
            }
            // Check if any of the items has any null values.
            if (items.Any(item => item.Id == null || item.Description == null || item.DatabaseIds == null || item.Nodes == null || item.Nodes.Any(item1 => item1.Id == null || item1.Type == null) || item.Fields == null || item.Fields.Any(item1 => item1.Key == null || item1.Value == null)))
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
                // Save the number of items.
                itemCount = items.Count();
                // Create a new Hangfire background task.
                var jobId = BackgroundJob.Enqueue<IDatabaseDataManager>(item => item.CreateEdges(items, CancellationToken.None));
            }
            // Check if the items should be edited.
            else if (Input.Type == "Edit")
            {
                // Keep only the valid items.
                items = items
                    .Where(item => !string.IsNullOrEmpty(item.Id) && item.Nodes != null && item.Nodes.Any() && ((item.DatabaseIds != null && item.DatabaseIds.Any()) || (item.Fields != null && item.Fields.Any())));
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
                // Create a new Hangfire background task.
                var jobId = BackgroundJob.Enqueue<IDatabaseDataManager>(item => item.UpdateEdges(items, CancellationToken.None));
            }
            // Check if the items should be deleted.
            else if (Input.Type == "Delete")
            {
                // Keep only the valid items.
                items = items
                    .Where(item => !string.IsNullOrEmpty(item.Id));
                // Get the list of IDs from the provided items.
                var itemIds = items.Select(item => item.Id);
                // Get the IDs of the edges from the non-generic databases that have the given IDs.
                var ids = _context.Edges
                    .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => itemIds.Contains(item.Id))
                    .Select(item => item.Id);
                // Save the number of nodes found.
                itemCount = items.Count();
                // Create a new Hangfire background task.
                var jobId = BackgroundJob.Enqueue<IDatabaseDataManager>(item => item.DeleteEdges(ids, CancellationToken.None));
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
            TempData["StatusMessage"] = $"Success: The background task for updating the data ({itemCount.ToString()} item{(itemCount != 1 ? "s" : string.Empty)} of type \"{Input.Type}\") has been created and scheduled successfully. You can view the progress on the Hangfire dashboard.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Data/Edges/Index");
        }
    }
}
