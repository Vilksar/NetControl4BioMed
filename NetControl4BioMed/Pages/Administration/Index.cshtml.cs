using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Services;
using NetControl4BioMed.Helpers.Tasks;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

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

            public Dictionary<string, int?> PublicItemCount { get; set; }

            public Dictionary<string, int?> DuplicateItemCount { get; set; }

            public Dictionary<string, int?> OrphanedItemCount { get; set; }

            public string AnnouncementMessage { get; set; }

            public string DemonstrationControlPathId { get; set; }
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
            var publicItemCount = _configuration
                .GetSection("Data")
                .GetSection("ItemCount")
                .GetSection("Public")
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
            var announcementMessage = _configuration["Data:AnnouncementMessage"];
            var demonstrationControlPathId = _configuration["Data:Demonstration:ControlPathId"];
            // Define the view.
            View = new ViewModel
            {
                AllItemCount = allItemCount,
                PublicItemCount = publicItemCount,
                DuplicateItemCount = duplicateItemCount,
                OrphanedItemCount = orphanedItemCount,
                AnnouncementMessage = announcementMessage,
                DemonstrationControlPathId = demonstrationControlPathId
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

        public IActionResult OnPostUpdateDemonstrationIds(string demonstrationControlPathId)
        {
            // Check if the control path ID is missing.
            if (string.IsNullOrEmpty(demonstrationControlPathId))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No demonstration control path ID has been provided.";
                // Redirect to the page.
                return RedirectToPage();
            }
            // Try to get the corresponding control path.
            var controlPath = _context.ControlPaths
                .Include(item => item.Analysis)
                    .ThenInclude(item => item.Network)
                .Where(item => item.Id == demonstrationControlPathId)
                .AsNoTracking()
                .FirstOrDefault();
            // Check if the control path could not be found.
            if (controlPath == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No demonstration control path could be found with the provided ID.";
                // Redirect to the page.
                return RedirectToPage();
            }
            // Check if the analysis is not public and not demonstration.
            if (controlPath.Analysis == null || !controlPath.Analysis.IsPublic || !controlPath.Analysis.IsDemonstration)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The corresponding analysis doesn't exist, or is not public or demonstration.";
                // Redirect to the page.
                return RedirectToPage();
            }
            // Check if the network is not public and not demonstration.
            if (controlPath.Analysis.Network == null || !controlPath.Analysis.Network.IsPublic || !controlPath.Analysis.Network.IsDemonstration)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The corresponding network doesn't exist, or is not public or demonstration.";
                // Redirect to the page.
                return RedirectToPage();
            }
            // Update the demonstration item IDs.
            _configuration["Data:Demonstration:NetworkId"] = controlPath.Analysis.Network.Id;
            _configuration["Data:Demonstration:AnalysisId"] = controlPath.Analysis.Id;
            _configuration["Data:Demonstration:ControlPathId"] = controlPath.Id;
            // Display a message.
            TempData["StatusMessage"] = "Success: The demonstration data has been successfully updated.";
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
                nameof(IRecurringTaskManager.CountPublicItemsAsync),
                nameof(IRecurringTaskManager.CountDuplicateItemsAsync),
                nameof(IRecurringTaskManager.CountOrphanedItemsAsync),
                nameof(IRecurringTaskManager.StopAnalysesAsync),
                nameof(IRecurringTaskManager.ExtendTimeUntilDeleteDemonstrationItemsAsync),
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
                            DatabaseProteinFields = item.DatabaseProteinFields
                                .Select(item1 => new
                                {
                                    Id = item1.Id,
                                    Name = item1.Name
                                }),
                            DatabaseInteractionFields = item.DatabaseInteractionFields
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
                if (downloadItems.Contains("AllDatabaseProteinFields"))
                {
                    // Get the required data.
                    var data = _context.DatabaseProteinFields
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
                            DatabaseProteinFieldProteins = item.DatabaseProteinFieldProteins
                                .Select(item1 => new
                                {
                                    Id = item1.Protein.Id,
                                    Name = item1.Protein.Name,
                                    Value = item1.Value
                                })
                        })
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-All-DatabaseProteinFields.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("AllDatabaseInteractionFields"))
                {
                    // Get the required data.
                    var data = _context.DatabaseInteractionFields
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
                            DatabaseInteractionFieldInteractions = item.DatabaseInteractionFieldInteractions
                                .Select(item1 => new
                                {
                                    Id = item1.Interaction.Id,
                                    Name = item1.Interaction.Name,
                                    Value = item1.Value
                                })
                        })
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-All-DatabaseInteractionFields.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("AllProteins"))
                {
                    // Get the required data.
                    var data = _context.Proteins
                        .Where(item => item.DatabaseProteins.Any())
                        .Select(item => new
                        {
                            Id = item.Id,
                            DateTimeCreated = item.DateTimeCreated,
                            Name = item.Name,
                            Description = item.Description,
                            Databases = item.DatabaseProteins
                                .Select(item1 => new
                                {
                                    Id = item1.Database.Id,
                                    Name = item1.Database.Name
                                }),
                            DatabaseProteinFieldProteins = item.DatabaseProteinFieldProteins
                                .Select(item1 => new
                                {
                                    Id = item1.DatabaseProteinField.Id,
                                    Name = item1.DatabaseProteinField.Name,
                                    Value = item1.Value
                                }),
                            InteractionProteins = item.InteractionProteins
                                .Select(item1 => new
                                {
                                    Id = item1.Interaction.Id,
                                    Name = item1.Interaction.Name,
                                    Type = item1.Type.ToString()
                                })
                        })
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-All-Proteins.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("AllInteractions"))
                {
                    // Get the required data.
                    var data = _context.Interactions
                        .Where(item => item.DatabaseInteractions.Any())
                        .Select(item => new
                        {
                            Id = item.Id,
                            DateTimeCreated = item.DateTimeCreated,
                            Name = item.Name,
                            Description = item.Description,
                            Databases = item.DatabaseInteractions
                                .Select(item1 => new
                                {
                                    Id = item1.Database.Id,
                                    Name = item1.Database.Name
                                }),
                            DatabaseInteractionFieldInteractions = item.DatabaseInteractionFieldInteractions
                                .Select(item1 => new
                                {
                                    Id = item1.DatabaseInteractionField.Id,
                                    Name = item1.DatabaseInteractionField.Name,
                                    Value = item1.Value
                                }),
                            InteractionProteins = item.InteractionProteins
                                .Select(item1 => new
                                {
                                    Id = item1.Protein.Id,
                                    Name = item1.Protein.Name,
                                    Type = item1.Type.GetDisplayName()
                                })
                        })
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-All-Interactions.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("AllProteinCollections"))
                {
                    // Get the required data.
                    var data = _context.ProteinCollections
                        .Select(item => new
                        {
                            Id = item.Id,
                            DateTimeCreated = item.DateTimeCreated,
                            Name = item.Name,
                            Description = item.Description,
                            ProteinCollectionTypes = item.ProteinCollectionTypes
                                .Select(item => new
                                {
                                    Id = item.Type.ToString(),
                                    Name = item.Type.GetDisplayName(),
                                    Description = item.Type.GetDisplayDescription()
                                }),
                            ProteinCollectionProteins = item.ProteinCollectionProteins
                                .Select(item1 => new
                                {
                                    Id = item1.Protein.Id,
                                    Name = item1.Protein.Name
                                })
                        })
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-All-ProteinCollections.json", CompressionLevel.Fastest).Open();
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
                            IsPublic = item.IsPublic,
                            Status = item.Status,
                            IsDemonstration = item.IsDemonstration,
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
                            IsPublic = item.IsPublic,
                            IsDemonstration = item.IsDemonstration,
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
                if (downloadItems.Contains("DuplicateDatabases"))
                {
                    // Get the duplicate keys.
                    var keys = _context.Databases
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => item.Key)
                        .ToList();
                    // Get the required data.
                    var data = _context.Databases
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
                if (downloadItems.Contains("DuplicateDatabaseProteinFields"))
                {
                    // Get the duplicate keys.
                    var keys = _context.DatabaseProteinFields
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => item.Key)
                        .ToList();
                    // Get the required data.
                    var data = _context.DatabaseProteinFields
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
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Duplicate-DatabaseProteinFields.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("DuplicateDatabaseInteractionFields"))
                {
                    // Get the duplicate keys.
                    var keys = _context.DatabaseInteractionFields
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => item.Key)
                        .ToList();
                    // Get the required data.
                    var data = _context.DatabaseInteractionFields
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
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Duplicate-DatabaseInteractionFields.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("DuplicateDatabaseProteinFieldProteins"))
                {
                    // Get the duplicate keys.
                    var keys = _context.DatabaseProteinFieldProteins
                        .Where(item => item.DatabaseProteinField.IsSearchable)
                        .GroupBy(item => new { DatabaseProteinFieldId = item.DatabaseProteinFieldId, Value = item.Value })
                        .Where(item => item.Count() > 1)
                        .Select(item => item.Key)
                        .ToList();
                    // Get the duplicate values.
                    var values = keys
                        .Select(item => item.Value);
                    // Get the required data.
                    var data = _context.DatabaseProteinFieldProteins
                        .Where(item => item.DatabaseProteinField.IsSearchable)
                        .Where(item => values.Contains(item.Value))
                        .AsNoTracking()
                        .AsEnumerable()
                        .Where(item => keys.Any(item1 => item1.DatabaseProteinFieldId == item.DatabaseProteinFieldId && item1.Value == item.Value))
                        .GroupBy(item => new { item.DatabaseProteinFieldId, item.Value })
                        .Where(item => item.Count() > 1)
                        .Select(item => new
                        {
                            Key = item.Key,
                            Values = item.Select(item1 => item1.ProteinId)
                        });
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Duplicate-DatabaseProteinFieldProteins.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("DuplicateDatabaseInteractionFieldInteractions"))
                {
                    // Get the duplicate keys.
                    var keys = _context.DatabaseInteractionFieldInteractions
                        .Where(item => item.DatabaseInteractionField.IsSearchable)
                        .GroupBy(item => new { DatabaseInteractionFieldId = item.DatabaseInteractionFieldId, Value = item.Value })
                        .Where(item => item.Count() > 1)
                        .Select(item => item.Key)
                        .ToList();
                    // Get the duplicate values.
                    var values = keys
                        .Select(item => item.Value);
                    // Get the required data.
                    var data = _context.DatabaseInteractionFieldInteractions
                        .Where(item => item.DatabaseInteractionField.IsSearchable)
                        .Where(item => values.Contains(item.Value))
                        .AsNoTracking()
                        .AsEnumerable()
                        .Where(item => keys.Any(item1 => item1.DatabaseInteractionFieldId == item.DatabaseInteractionFieldId && item1.Value == item.Value))
                        .GroupBy(item => new { DatabaseInteractionFieldId = item.DatabaseInteractionFieldId, Value = item.Value })
                        .Where(item => item.Count() > 1)
                        .Select(item => new
                        {
                            Key = item.Key,
                            Values = item.Select(item1 => item1.InteractionId)
                        });
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Duplicate-DatabaseInteractionFieldInteractions.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("DuplicateProteins"))
                {
                    // Get the duplicate keys.
                    var keys = _context.Proteins
                        .Where(item => item.DatabaseProteins.Any())
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => item.Key)
                        .ToList();
                    // Get the required data.
                    var data = _context.Proteins
                        .Where(item => item.DatabaseProteins.Any())
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
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Duplicate-Proteins.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("DuplicateInteractions"))
                {
                    // Get the duplicate keys.
                    var keys = _context.Interactions
                        .Where(item => item.DatabaseInteractions.Any())
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => item.Key)
                        .ToList();
                    // Get the required data.
                    var data = _context.Interactions
                        .Where(item => item.DatabaseInteractions.Any())
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
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Duplicate-Interactions.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("DuplicateProteinCollections"))
                {
                    // Get the duplicate keys.
                    var keys = _context.ProteinCollections
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => item.Key)
                        .ToList();
                    // Get the required data.
                    var data = _context.ProteinCollections
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
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Duplicate-ProteinCollections.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("OrphanedProteins"))
                {
                    // Get the required data.
                    var data = _context.Proteins
                        .Where(item => !item.DatabaseProteins.Any() && !item.NetworkProteins.Any())
                        .Select(item => item.Id)
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Orphaned-Proteins.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("OrphanedInteractions"))
                {
                    // Get the required data.
                    var data = _context.Interactions
                        .Where(item => (!item.DatabaseInteractions.Any() && !item.NetworkInteractions.Any()) || item.InteractionProteins.Count() < 2)
                        .Select(item => item.Id)
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Orphaned-Interactions.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("OrphanedProteinCollections"))
                {
                    // Get the required data.
                    var data = _context.ProteinCollections
                        .Where(item => !item.ProteinCollectionProteins.Any())
                        .Select(item => item.Id)
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Orphaned-ProteinCollections.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("OrphanedNetworks"))
                {
                    // Get the required data.
                    var data = _context.Networks
                        .Where(item => !item.NetworkProteins.Any() || !item.NetworkInteractions.Any())
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
                        .Where(item => !item.AnalysisProteins.Any() || !item.AnalysisInteractions.Any() || item.Network == null)
                        .Select(item => item.Id)
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Orphaned-Analyses.json", CompressionLevel.Fastest).Open();
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
            if (deleteItems.Contains("Proteins"))
            {
                // Define a new background task.
                var backgroundTask = new BackgroundTask
                {
                    DateTimeCreated = DateTime.UtcNow,
                    Name = $"{nameof(IAdministrationTaskManager)}.{nameof(IAdministrationTaskManager.DeleteAllProteinsAsync)}",
                    IsRecurring = false,
                    Data = JsonSerializer.Serialize(new ProteinsTask(), jsonSerializerOptions)
                };
                // Mark the background task for addition.
                _context.BackgroundTasks.Add(backgroundTask);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
                // Create a new Hangfire background job.
                var jobId = BackgroundJob.Enqueue<IAdministrationTaskManager>(item => item.DeleteAllProteinsAsync(backgroundTask.Id, CancellationToken.None));
            }
            // Check the items to delete.
            if (deleteItems.Contains("Interactions"))
            {
                // Define a new background task.
                var backgroundTask = new BackgroundTask
                {
                    DateTimeCreated = DateTime.UtcNow,
                    Name = $"{nameof(IAdministrationTaskManager)}.{nameof(IAdministrationTaskManager.DeleteAllInteractionsAsync)}",
                    IsRecurring = false,
                    Data = JsonSerializer.Serialize(new InteractionsTask(), jsonSerializerOptions)
                };
                // Mark the background task for addition.
                _context.BackgroundTasks.Add(backgroundTask);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
                // Create a new Hangfire background job.
                var jobId = BackgroundJob.Enqueue<IAdministrationTaskManager>(item => item.DeleteAllInteractionsAsync(backgroundTask.Id, CancellationToken.None));
            }
            // Check the items to delete.
            if (deleteItems.Contains("ProteinCollections"))
            {
                // Define a new background task.
                var backgroundTask = new BackgroundTask
                {
                    DateTimeCreated = DateTime.UtcNow,
                    Name = $"{nameof(IAdministrationTaskManager)}.{nameof(IAdministrationTaskManager.DeleteAllProteinCollectionsAsync)}",
                    IsRecurring = false,
                    Data = JsonSerializer.Serialize(new ProteinCollectionsTask(), jsonSerializerOptions)
                };
                // Mark the background task for addition.
                _context.BackgroundTasks.Add(backgroundTask);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
                // Create a new Hangfire background job.
                var jobId = BackgroundJob.Enqueue<IAdministrationTaskManager>(item => item.DeleteAllProteinCollectionsAsync(backgroundTask.Id, CancellationToken.None));
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
            // Display a message.
            TempData["StatusMessage"] = $"Success: A new background task was created to delete {string.Join(" and ", deleteItems.Select(item => $"all {item}"))}.";
            // Redirect to the page.
            return RedirectToPage();
        }
    }
}
