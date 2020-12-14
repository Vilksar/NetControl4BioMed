using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Services;
using NetControl4BioMed.Helpers.Tasks;
using NetControl4BioMed.Helpers.ViewModels;

namespace NetControl4BioMed.Pages.Administration
{
    [Authorize(Roles = "Administrator")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IReCaptchaChecker _reCaptchaChecker;

        public IndexModel(ApplicationDbContext context, IConfiguration configuration, IReCaptchaChecker reCaptchaChecker)
        {
            _context = context;
            _configuration = configuration;
            _reCaptchaChecker = reCaptchaChecker;
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public Dictionary<string, int?> AllItemCount { get; set; }

            public Dictionary<string, int?> DuplicateItemCount { get; set; }

            public Dictionary<string, int?> OrphanedItemCount { get; set; }

            public Dictionary<string, int?> InconsistentItemCount { get; set; }

            public string AnnouncementMessage { get; set; }
        }

        public IActionResult OnGet()
        {
            // Get the data from configuration.
            var allItemCount = _configuration
                .GetSection("Data")
                .GetSection("ItemCount")
                .GetSection("All")
                .GetChildren()
                .ToDictionary(item => item.Key, item => int.TryParse(item.Value, out var result) ? (int?)result : null);
            var duplicateItemCount = _configuration
                .GetSection("Data")
                .GetSection("ItemCount")
                .GetSection("Duplicate")
                .GetChildren()
                .ToDictionary(item => item.Key, item => int.TryParse(item.Value, out var result) ? (int?)result : null);
            var orphanedItemCount = _configuration
                .GetSection("Data")
                .GetSection("ItemCount")
                .GetSection("Orphaned")
                .GetChildren()
                .ToDictionary(item => item.Key, item => int.TryParse(item.Value, out var result) ? (int?)result : null);
            var inconsistentItemCount = _configuration
                .GetSection("Data")
                .GetSection("ItemCount")
                .GetSection("Inconsistent")
                .GetChildren()
                .ToDictionary(item => item.Key, item => int.TryParse(item.Value, out var result) ? (int?)result : null);
            var announcementMessage = _configuration["Data:AnnouncementMessage"];
            // Define the view.
            View = new ViewModel
            {
                AllItemCount = allItemCount,
                DuplicateItemCount = duplicateItemCount,
                OrphanedItemCount = orphanedItemCount,
                InconsistentItemCount = inconsistentItemCount,
                AnnouncementMessage = announcementMessage
            };
            // Return the page.
            return Page();
        }

        public IActionResult OnPostHangfire()
        {
            // Redirect to the Hangfire dashboard.
            return LocalRedirect("/Hangfire");
        }

        public IActionResult OnPostUpdateAnnouncementMessage(string announcementMessage)
        {
            // Update the announcement message.
            _configuration["Data:AnnouncementMessage"] = announcementMessage;
            // Display a message.
            TempData["StatusMessage"] = "Success: The announcement message has been successfully updated.";
            // Redirect to the page.
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostResetHangfireRecurrentJobsAsync()
        {
            // Use a new connection to the job storage.
            using (var connection = JobStorage.Current.GetConnection())
            {
                // Go over each recurring job in the storage.
                foreach (var recurringJob in StorageConnectionExtensions.GetRecurringJobs(connection))
                {
                    // Delete the recurring job.
                    RecurringJob.RemoveIfExists(recurringJob.Id);
                }
            }
            // Get the existing recurring background tasks.
            var backgroundTasks = _context.BackgroundTasks
                .Where(item => item.IsRecurring);
            // Mark the existing background tasks for deletion.
            _context.BackgroundTasks.RemoveRange(backgroundTasks);
            // Save the changes to the database.
            await _context.SaveChangesAsync();
            // Get the list of names of the recurring tasks.
            var taskNames = new List<string>
            {
                nameof(IRecurringTaskManager.CountAllItemsAsync),
                nameof(IRecurringTaskManager.CountDuplicateItemsAsync),
                nameof(IRecurringTaskManager.CountOrphanedItemsAsync),
                nameof(IRecurringTaskManager.CountInconsistentItemsAsync),
                nameof(IRecurringTaskManager.StopAnalysesAsync),
                nameof(IRecurringTaskManager.AlertUsersAsync),
                nameof(IRecurringTaskManager.DeleteUnconfirmedUsersAsync),
                nameof(IRecurringTaskManager.DeleteOrphanedItemsAsync),
                nameof(IRecurringTaskManager.DeleteNetworksAsync),
                nameof(IRecurringTaskManager.DeleteAnalysesAsync)
            };
            // Define a new recurring job manager.
            var recurringJobManager = new RecurringJobManager();
            // Go over each task name.
            foreach (var taskName in taskNames)
            {
                // Get the corresponding background tasks name.
                var backgroundTaskName = $"{nameof(IRecurringTaskManager)}.{taskName}";
                // Delete the existing corresponding recurring job.
                recurringJobManager.RemoveIfExists(backgroundTaskName);
                // Define the new background task.
                var backgroundTask = new BackgroundTask
                {
                    DateTimeCreated = DateTime.UtcNow,
                    Name = backgroundTaskName,
                    IsRecurring = true,
                    Data = JsonSerializer.Serialize(new RecurringTask
                    {
                        Scheme = HttpContext.Request.Scheme,
                        HostValue = HttpContext.Request.Host.Value
                    })
                };
                // Mark the background task for addition.
                _context.BackgroundTasks.Add(backgroundTask);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
                // Define the new recurring job.
                var job = new Job(typeof(IRecurringTaskManager), typeof(IRecurringTaskManager).GetMethod(taskName), new object[] { backgroundTask.Id, CancellationToken.None });
                // Create the new Hangfire recurring job.
                recurringJobManager.AddOrUpdate(backgroundTaskName, job, Cron.Daily());
            }
            // Display a message.
            TempData["StatusMessage"] = "Success: The database recurring jobs have been successfully reset. You can view more details on the Hangfire dashboard.";
            // Redirect to the page.
            return RedirectToPage();
        }

        public IActionResult OnPostDownload(IEnumerable<string> downloadItems)
        {
            // Check if there are no items provided.
            if (downloadItems == null || !downloadItems.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: There were no provided items to download.";
                // Redirect to the page.
                return RedirectToPage();
            }
            // Define the JSON serializer options for all of the returned files.
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            // Return the streamed file.
            return new FileCallbackResult(MediaTypeNames.Application.Zip, async (zipStream, _) =>
            {
                // Define a new ZIP archive.
                using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create);
                // Check the items to download.
                if (downloadItems.Contains("AllDatabaseTypes"))
                {
                    // Get the required data.
                    var data = _context.DatabaseTypes
                        .Select(item => new
                        {
                            Id = item.Id,
                            DateTimeCreated = item.DateTimeCreated,
                            Name = item.Name,
                            Description = item.Description,
                            Databases = item.Databases
                                .Select(item1 => new
                                {
                                    Id = item1.Id,
                                    Name = item1.Name
                                })
                        })
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-All-DatabaseTypes.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("AllDatabases"))
                {
                    // Get the required data.
                    var data = _context.Databases
                        .Select(item => new
                        {
                            Id = item.Id,
                            DateTimeCreated = item.DateTimeCreated,
                            Name = item.Name,
                            Description = item.Description,
                            DatabaseType = new
                            {
                                Id = item.DatabaseType.Id,
                                Name = item.DatabaseType.Name
                            },
                            DatabaseNodeFields = item.DatabaseNodeFields
                                .Select(item1 => new
                                {
                                    Id = item1.Id,
                                    Name = item1.Name
                                }),
                            DatabaseEdgeFields = item.DatabaseEdgeFields
                                .Select(item1 => new
                                {
                                    Id = item1.Id,
                                    Name = item1.Name
                                })
                        })
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-All-Databases.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("AllDatabaseNodeFields"))
                {
                    // Get the required data.
                    var data = _context.DatabaseNodeFields
                        .Select(item => new
                        {
                            Id = item.Id,
                            DateTimeCreated = item.DateTimeCreated,
                            Name = item.Name,
                            Description = item.Description,
                            IsSearchable = item.IsSearchable,
                            Url = item.Url,
                            Database = new
                            {
                                Id = item.Database.Id,
                                Name = item.Database.Name
                            },
                            DatabaseNodeFieldNodes = item.DatabaseNodeFieldNodes
                                .Select(item1 => new
                                {
                                    Id = item1.Node.Id,
                                    Name = item1.Node.Name,
                                    Value = item1.Value
                                })
                        })
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-All-DatabaseNodeFields.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("AllDatabaseEdgeFields"))
                {
                    // Get the required data.
                    var data = _context.DatabaseEdgeFields
                        .Select(item => new
                        {
                            Id = item.Id,
                            DateTimeCreated = item.DateTimeCreated,
                            Name = item.Name,
                            Description = item.Description,
                            Url = item.Url,
                            Database = new
                            {
                                Id = item.Database.Id,
                                Name = item.Database.Name
                            },
                            DatabaseEdgeFieldEdges = item.DatabaseEdgeFieldEdges
                                .Select(item1 => new
                                {
                                    Id = item1.Edge.Id,
                                    Name = item1.Edge.Name,
                                    Value = item1.Value
                                })
                        })
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-All-DatabaseEdgeFields.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("AllNodes"))
                {
                    // Get the required data.
                    var data = _context.Nodes
                        .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                        .Select(item => new
                        {
                            Id = item.Id,
                            DateTimeCreated = item.DateTimeCreated,
                            Name = item.Name,
                            Description = item.Description,
                            Databases = item.DatabaseNodes
                                .Select(item1 => new
                                {
                                    Id = item1.Database.Id,
                                    Name = item1.Database.Name
                                }),
                            DatabaseNodeFieldNodes = item.DatabaseNodeFieldNodes
                                .Select(item1 => new
                                {
                                    Id = item1.DatabaseNodeField.Id,
                                    Name = item1.DatabaseNodeField.Name,
                                    Value = item1.Value
                                }),
                            EdgeNodes = item.EdgeNodes
                                .Select(item1 => new
                                {
                                    Id = item1.Edge.Id,
                                    Name = item1.Edge.Name,
                                    Type = item1.Type.ToString()
                                })
                        })
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-All-Nodes.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("AllEdges"))
                {
                    // Get the required data.
                    var data = _context.Edges
                        .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                        .Select(item => new
                        {
                            Id = item.Id,
                            DateTimeCreated = item.DateTimeCreated,
                            Name = item.Name,
                            Description = item.Description,
                            Databases = item.DatabaseEdges
                                .Select(item1 => new
                                {
                                    Id = item1.Database.Id,
                                    Name = item1.Database.Name
                                }),
                            DatabaseEdgeFieldEdges = item.DatabaseEdgeFieldEdges
                                .Select(item1 => new
                                {
                                    Id = item1.DatabaseEdgeField.Id,
                                    Name = item1.DatabaseEdgeField.Name,
                                    Value = item1.Value
                                }),
                            EdgeNodes = item.EdgeNodes
                                .Select(item1 => new
                                {
                                    Id = item1.Node.Id,
                                    Name = item1.Node.Name,
                                    Type = item1.Type.GetDisplayName()
                                })
                        })
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-All-Edges.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("AllNodeCollections"))
                {
                    // Get the required data.
                    var data = _context.NodeCollections
                        .Select(item => new
                        {
                            Id = item.Id,
                            DateTimeCreated = item.DateTimeCreated,
                            Name = item.Name,
                            Description = item.Description,
                            NodeCollectionNodes = item.NodeCollectionNodes
                                .Select(item1 => new
                                {
                                    Id = item1.Node.Id,
                                    Name = item1.Node.Name
                                })
                        })
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-All-NodeCollections.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("AllNetworks"))
                {
                    // Get the required data.
                    var data = _context.Networks
                        .Select(item => new
                        {
                            Id = item.Id,
                            DateTimeCreated = item.DateTimeCreated,
                            Algorithm = item.Algorithm.GetDisplayName()
                        })
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-All-Networks.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("AllAnalyses"))
                {
                    // Get the required data.
                    var data = _context.Analyses
                        .Select(item => new
                        {
                            Id = item.Id,
                            DateTimeStarted = item.DateTimeStarted,
                            DateTimeEnded = item.DateTimeEnded,
                            Status = item.Status,
                            CurrentIteration = item.CurrentIteration,
                            CurrentIterationWithoutImprovement = item.CurrentIterationWithoutImprovement,
                            Algorithm = item.Algorithm.GetDisplayName(),
                            Parameters = item.Parameters
                        })
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-All-Analyses.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("AllSamples"))
                {
                    // Get the required data.
                    var data = _context.Samples
                        .Select(item => new
                        {
                            Id = item.Id,
                            DateTimeCreated = item.DateTimeCreated,
                            Name = item.Name,
                            Description = item.Description,
                            Type = item.Type.GetDisplayName(),
                            Data = item.Data
                        })
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-All-Samples.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("DuplicateDatabaseTypes"))
                {
                    // Get the duplicate keys.
                    var keys = _context.DatabaseTypes
                        .Where(item => item.Name != "Generic")
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => item.Key)
                        .ToList();
                    // Get the required data.
                    var data = _context.DatabaseTypes
                        .Where(item => item.Name != "Generic")
                        .Where(item => keys.Contains(item.Name))
                        .AsNoTracking()
                        .AsEnumerable()
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => new
                        {
                            Key = item.Key,
                            Values = item.Select(item1 => item1.Id)
                        });
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Duplicate-DatabaseTypes.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("DuplicateDatabases"))
                {
                    // Get the duplicate keys.
                    var keys = _context.Databases
                        .Where(item => item.DatabaseType.Name != "Generic")
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => item.Key)
                        .ToList();
                    // Get the required data.
                    var data = _context.Databases
                        .Where(item => item.DatabaseType.Name != "Generic")
                        .Where(item => keys.Contains(item.Name))
                        .AsNoTracking()
                        .AsEnumerable()
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => new
                        {
                            Key = item.Key,
                            Values = item.Select(item1 => item1.Id)
                        });
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Duplicate-Databases.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("DuplicateDatabaseNodeFields"))
                {
                    // Get the duplicate keys.
                    var keys = _context.DatabaseNodeFields
                        .Where(item => item.Database.DatabaseType.Name != "Generic")
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => item.Key)
                        .ToList();
                    // Get the required data.
                    var data = _context.DatabaseNodeFields
                        .Where(item => item.Database.DatabaseType.Name != "Generic")
                        .Where(item => keys.Contains(item.Name))
                        .AsNoTracking()
                        .AsEnumerable()
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => new
                        {
                            Key = item.Key,
                            Values = item.Select(item1 => item1.Id)
                        });
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Duplicate-DatabaseNodeFields.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("DuplicateDatabaseEdgeFields"))
                {
                    // Get the duplicate keys.
                    var keys = _context.DatabaseEdgeFields
                        .Where(item => item.Database.DatabaseType.Name != "Generic")
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => item.Key)
                        .ToList();
                    // Get the required data.
                    var data = _context.DatabaseEdgeFields
                        .Where(item => item.Database.DatabaseType.Name != "Generic")
                        .Where(item => keys.Contains(item.Name))
                        .AsNoTracking()
                        .AsEnumerable()
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => new
                        {
                            Key = item.Key,
                            Values = item.Select(item1 => item1.Id)
                        });
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Duplicate-DatabaseEdgeFields.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("DuplicateDatabaseNodeFieldNodes"))
                {
                    // Get the duplicate keys.
                    var keys = _context.DatabaseNodeFieldNodes
                        .Where(item => item.DatabaseNodeField.Database.DatabaseType.Name != "Generic")
                        .Where(item => item.DatabaseNodeField.IsSearchable)
                        .GroupBy(item => new { DatabaseNodeFieldId = item.DatabaseNodeFieldId, Value = item.Value })
                        .Where(item => item.Count() > 1)
                        .Select(item => item.Key)
                        .ToList();
                    // Get the duplicate values.
                    var values = keys
                        .Select(item => item.Value);
                    // Get the required data.
                    var data = _context.DatabaseNodeFieldNodes
                        .Where(item => item.DatabaseNodeField.Database.DatabaseType.Name != "Generic")
                        .Where(item => item.DatabaseNodeField.IsSearchable)
                        .Where(item => values.Contains(item.Value))
                        .AsNoTracking()
                        .AsEnumerable()
                        .Where(item => keys.Any(item1 => item1.DatabaseNodeFieldId == item.DatabaseNodeFieldId && item1.Value == item.Value))
                        .GroupBy(item => new { item.DatabaseNodeFieldId, item.Value })
                        .Where(item => item.Count() > 1)
                        .Select(item => new
                        {
                            Key = item.Key,
                            Values = item.Select(item1 => item1.NodeId)
                        });
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Duplicate-DatabaseNodeFieldNodes.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("DuplicateDatabaseEdgeFieldEdges"))
                {
                    // Get the duplicate keys.
                    var keys = _context.DatabaseEdgeFieldEdges
                        .Where(item => item.DatabaseEdgeField.Database.DatabaseType.Name != "Generic")
                        .Where(item => item.DatabaseEdgeField.IsSearchable)
                        .GroupBy(item => new { DatabaseEdgeFieldId = item.DatabaseEdgeFieldId, Value = item.Value })
                        .Where(item => item.Count() > 1)
                        .Select(item => item.Key)
                        .ToList();
                    // Get the duplicate values.
                    var values = keys
                        .Select(item => item.Value);
                    // Get the required data.
                    var data = _context.DatabaseEdgeFieldEdges
                        .Where(item => item.DatabaseEdgeField.Database.DatabaseType.Name != "Generic")
                        .Where(item => item.DatabaseEdgeField.IsSearchable)
                        .Where(item => values.Contains(item.Value))
                        .AsNoTracking()
                        .AsEnumerable()
                        .Where(item => keys.Any(item1 => item1.DatabaseEdgeFieldId == item.DatabaseEdgeFieldId && item1.Value == item.Value))
                        .GroupBy(item => new { DatabaseEdgeFieldId = item.DatabaseEdgeFieldId, Value = item.Value })
                        .Where(item => item.Count() > 1)
                        .Select(item => new
                        {
                            Key = item.Key,
                            Values = item.Select(item1 => item1.EdgeId)
                        });
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Duplicate-DatabaseEdgeFieldEdges.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("DuplicateNodes"))
                {
                    // Get the duplicate keys.
                    var keys = _context.Nodes
                        .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => item.Key)
                        .ToList();
                    // Get the required data.
                    var data = _context.Nodes
                        .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                        .Where(item => keys.Contains(item.Name))
                        .AsNoTracking()
                        .AsEnumerable()
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => new
                        {
                            Key = item.Key,
                            Values = item.Select(item1 => item1.Id)
                        });
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Duplicate-Nodes.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("DuplicateEdges"))
                {
                    // Get the duplicate keys.
                    var keys = _context.Edges
                        .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => item.Key)
                        .ToList();
                    // Get the required data.
                    var data = _context.Edges
                        .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                        .Where(item => keys.Contains(item.Name))
                        .AsNoTracking()
                        .AsEnumerable()
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => new
                        {
                            Key = item.Key,
                            Values = item.Select(item1 => item1.Id)
                        });
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Duplicate-Edges.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("DuplicateNodeCollections"))
                {
                    // Get the duplicate keys.
                    var keys = _context.NodeCollections
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => item.Key)
                        .ToList();
                    // Get the required data.
                    var data = _context.NodeCollections
                        .Where(item => keys.Contains(item.Name))
                        .AsNoTracking()
                        .AsEnumerable()
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => new
                        {
                            Key = item.Key,
                            Values = item.Select(item1 => item1.Id)
                        });
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Duplicate-NodeCollections.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("OrphanedNodes"))
                {
                    // Get the required data.
                    var data = _context.Nodes
                        .Where(item => !item.DatabaseNodeFieldNodes.Any())
                        .Select(item => item.Id)
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Orphaned-Nodes.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("OrphanedEdges"))
                {
                    // Get the required data.
                    var data = _context.Edges
                        .Where(item => !item.DatabaseEdges.Any() || item.EdgeNodes.Count() < 2)
                        .Select(item => item.Id)
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Orphaned-Edges.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("OrphanedNodeCollections"))
                {
                    // Get the required data.
                    var data = _context.NodeCollections
                        .Where(item => !item.NodeCollectionNodes.Any())
                        .Select(item => item.Id)
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Orphaned-NodeCollections.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("OrphanedNetworks"))
                {
                    // Get the required data.
                    var data = _context.Networks
                        .Where(item => !item.NetworkDatabases.Any() || !item.NetworkNodes.Any() || !item.NetworkEdges.Any() || !item.NetworkUsers.Any())
                        .Select(item => item.Id)
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Orphaned-Networks.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("OrphanedAnalyses"))
                {
                    // Get the required data.
                    var data = _context.Analyses
                        .Where(item => !item.AnalysisDatabases.Any() || !item.AnalysisNodes.Any() || !item.AnalysisEdges.Any() || !item.AnalysisNetworks.Any() || !item.AnalysisUsers.Any())
                        .Select(item => item.Id)
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Orphaned-Analyses.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("InconsistentNodes"))
                {
                    // Get the required data.
                    var data = _context.Nodes
                        .Where(item => item.DatabaseNodes.Select(item1 => item1.Database.DatabaseType).Distinct().Count() > 1)
                        .Select(item => item.Id)
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Inconsistent-Nodes.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("InconsistentEdges"))
                {
                    // Get the required data.
                    var data = _context.Edges
                        .Where(item => item.DatabaseEdges.Select(item1 => item1.Database.DatabaseType).Distinct().Count() > 1)
                        .Select(item => item.Id)
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Inconsistent-Edges.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("InconsistentNodeCollections"))
                {
                    // Get the required data.
                    var data = _context.NodeCollections
                        .Where(item => item.NodeCollectionNodes.Select(item1 => item1.Node.DatabaseNodes).SelectMany(item1 => item1).Select(item1 => item1.Database.DatabaseType).Distinct().Count() > 1)
                        .Select(item => item.Id)
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Inconsistent-NodeCollections.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("InconsistentNetworks"))
                {
                    // Get the required data.
                    var data = _context.Networks
                        .Where(item => item.NetworkDatabases.Select(item1 => item1.Database.DatabaseType).Distinct().Count() > 1)
                        .Select(item => item.Id)
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Inconsistent-Networks.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("InconsistentAnalyses"))
                {
                    // Get the required data.
                    var data = _context.Analyses
                        .Where(item => item.AnalysisDatabases.Select(item1 => item1.Database.DatabaseType).Distinct().Count() > 1)
                        .Select(item => item.Id)
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Inconsistent-Analyses.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
            })
            {
                FileDownloadName = $"NetControl4BioMed-Data-{DateTime.UtcNow:yyyyMMdd}.zip"
            };
        }

        public async Task<IActionResult> OnPostDeleteAsync(IEnumerable<string> deleteItems, string deleteConfirmation, string reCaptchaToken)
        {
            // Check if there are no items provided.
            if (deleteItems == null || !deleteItems.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: There were no provided items to delete.";
                // Redirect to the page.
                return RedirectToPage();
            }
            // Check if the confirmation is not valid.
            if (deleteConfirmation != $"I confirm that I want to delete {string.Join(" and ", deleteItems.Select(item => $"all {item}"))}!")
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The confirmation message was not valid for the selected items.";
                // Redirect to the page.
                return RedirectToPage();
            }
            // Check if the reCaptcha is valid.
            if (!await _reCaptchaChecker.IsValid(reCaptchaToken))
            {
                // Add an error to the model.
                TempData["StatusMessage"] = "Error: The reCaptcha verification failed.";
                // Redirect to the page.
                return RedirectToPage();
            }
            // Define the JSON serializer options.
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                IgnoreNullValues = true
            };
            // Check the items to delete.
            if (deleteItems.Contains("Nodes"))
            {
                // Define a new background task.
                var backgroundTask = new BackgroundTask
                {
                    DateTimeCreated = DateTime.UtcNow,
                    Name = $"{nameof(IAdministrationTaskManager)}.{nameof(IAdministrationTaskManager.DeleteAllNodesAsync)}",
                    IsRecurring = false,
                    Data = JsonSerializer.Serialize(new NodesTask(), jsonSerializerOptions)
                };
                // Mark the background task for addition.
                _context.BackgroundTasks.Add(backgroundTask);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
                // Create a new Hangfire background job.
                var jobId = BackgroundJob.Enqueue<IAdministrationTaskManager>(item => item.DeleteAllNodesAsync(backgroundTask.Id, CancellationToken.None));
            }
            // Check the items to delete.
            if (deleteItems.Contains("Edges"))
            {
                // Define a new background task.
                var backgroundTask = new BackgroundTask
                {
                    DateTimeCreated = DateTime.UtcNow,
                    Name = $"{nameof(IAdministrationTaskManager)}.{nameof(IAdministrationTaskManager.DeleteAllEdgesAsync)}",
                    IsRecurring = false,
                    Data = JsonSerializer.Serialize(new EdgesTask(), jsonSerializerOptions)
                };
                // Mark the background task for addition.
                _context.BackgroundTasks.Add(backgroundTask);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
                // Create a new Hangfire background job.
                var jobId = BackgroundJob.Enqueue<IAdministrationTaskManager>(item => item.DeleteAllEdgesAsync(backgroundTask.Id, CancellationToken.None));
            }
            // Check the items to delete.
            if (deleteItems.Contains("NodeCollections"))
            {
                // Define a new background task.
                var backgroundTask = new BackgroundTask
                {
                    DateTimeCreated = DateTime.UtcNow,
                    Name = $"{nameof(IAdministrationTaskManager)}.{nameof(IAdministrationTaskManager.DeleteAllNodeCollectionsAsync)}",
                    IsRecurring = false,
                    Data = JsonSerializer.Serialize(new NodeCollectionsTask(), jsonSerializerOptions)
                };
                // Mark the background task for addition.
                _context.BackgroundTasks.Add(backgroundTask);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
                // Create a new Hangfire background job.
                var jobId = BackgroundJob.Enqueue<IAdministrationTaskManager>(item => item.DeleteAllNodeCollectionsAsync(backgroundTask.Id, CancellationToken.None));
            }
            // Check the items to delete.
            if (deleteItems.Contains("Networks"))
            {
                // Define a new background task.
                var backgroundTask = new BackgroundTask
                {
                    DateTimeCreated = DateTime.UtcNow,
                    Name = $"{nameof(IAdministrationTaskManager)}.{nameof(IAdministrationTaskManager.DeleteAllNetworksAsync)}",
                    IsRecurring = false,
                    Data = JsonSerializer.Serialize(new NetworksTask(), jsonSerializerOptions)
                };
                // Mark the background task for addition.
                _context.BackgroundTasks.Add(backgroundTask);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
                // Create a new Hangfire background job.
                var jobId = BackgroundJob.Enqueue<IAdministrationTaskManager>(item => item.DeleteAllNetworksAsync(backgroundTask.Id, CancellationToken.None));
            }
            // Check the items to delete.
            if (deleteItems.Contains("Analyses"))
            {
                // Define a new background task.
                var backgroundTask = new BackgroundTask
                {
                    DateTimeCreated = DateTime.UtcNow,
                    Name = $"{nameof(IAdministrationTaskManager)}.{nameof(IAdministrationTaskManager.DeleteAllAnalysesAsync)}",
                    IsRecurring = false,
                    Data = JsonSerializer.Serialize(new AnalysesTask(), jsonSerializerOptions)
                };
                // Mark the background task for addition.
                _context.BackgroundTasks.Add(backgroundTask);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
                // Create a new Hangfire background job.
                var jobId = BackgroundJob.Enqueue<IAdministrationTaskManager>(item => item.DeleteAllAnalysesAsync(backgroundTask.Id, CancellationToken.None));
            }
            // Check the items to delete.
            if (deleteItems.Contains("Samples"))
            {
                // Define a new background task.
                var backgroundTask = new BackgroundTask
                {
                    DateTimeCreated = DateTime.UtcNow,
                    Name = $"{nameof(IAdministrationTaskManager)}.{nameof(IAdministrationTaskManager.DeleteAllSamplesAsync)}",
                    IsRecurring = false,
                    Data = JsonSerializer.Serialize(new AnalysesTask(), jsonSerializerOptions)
                };
                // Mark the background task for addition.
                _context.BackgroundTasks.Add(backgroundTask);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
                // Create a new Hangfire background job.
                var jobId = BackgroundJob.Enqueue<IAdministrationTaskManager>(item => item.DeleteAllSamplesAsync(backgroundTask.Id, CancellationToken.None));
            }
            // Display a message.
            TempData["StatusMessage"] = $"Success: A new background task was created to delete {string.Join(" and ", deleteItems.Select(item => $"all {item}"))}.";
            // Redirect to the page.
            return RedirectToPage();
        }
    }
}
