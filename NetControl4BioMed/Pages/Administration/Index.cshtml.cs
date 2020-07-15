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
            public int UserCount { get; set; }

            public int RoleCount { get; set; }

            public int DatabaseCount { get; set; }

            public int NodeCount { get; set; }

            public int EdgeCount { get; set; }

            public int NodeCollectionCount { get; set; }

            public Dictionary<string, Dictionary<string, int>> IssueCount { get; set; }

            public string AnnouncementMessage { get; set; }
        }

        public IActionResult OnGet()
        {
            // Get the data from configuration.
            var count = _configuration
                .GetSection("Data")
                .GetSection("ItemCount")
                .GetChildren()
                .ToDictionary(item => item.Key, item => int.TryParse(item.Value, out var result) ? result : -1);
            var issueCount = _configuration
                .GetSection("Data")
                .GetSection("IssueCount")
                .GetChildren()
                .Where(item => item.GetChildren().Any())
                .ToDictionary(item => item.Key, item => item.GetChildren().ToDictionary(item1 => item1.Key, item1 => int.TryParse(item1.Value, out var result) ? result : -1));
            var announcementMessage = _configuration["Data:AnnouncementMessage"];
            // Define the view.
            View = new ViewModel
            {
                UserCount = count.GetValueOrDefault("Users", -1),
                RoleCount = count.GetValueOrDefault("Roles", -1),
                DatabaseCount = count.GetValueOrDefault("Databases", -1),
                NodeCount = count.GetValueOrDefault("Nodes", -1),
                EdgeCount = count.GetValueOrDefault("Edges", -1),
                NodeCollectionCount = count.GetValueOrDefault("NodeCollections", -1),
                IssueCount = issueCount,
                AnnouncementMessage = announcementMessage
            };
            // Return the page.
            return Page();
        }

        public IActionResult OnPostResetIssueCount()
        {
            // Update the duplicate counts.
            _configuration["Data:IssueCount:Duplicate:DatabaseTypes"] = _context.DatabaseTypes
                .Where(item => item.Name != "Generic")
                .GroupBy(item => item.Name)
                .Where(item => item.Count() > 1)
                .Select(item => item.Key)
                .Count()
                .ToString();
            _configuration["Data:IssueCount:Duplicate:Databases"] = _context.Databases
                .Where(item => item.DatabaseType.Name != "Generic")
                .GroupBy(item => item.Name)
                .Where(item => item.Count() > 1)
                .Select(item => item.Key)
                .Count()
                .ToString();
            _configuration["Data:IssueCount:Duplicate:DatabaseNodeFields"] = _context.DatabaseNodeFields
                .Where(item => item.Database.DatabaseType.Name != "Generic")
                .GroupBy(item => item.Name)
                .Where(item => item.Count() > 1)
                .Select(item => item.Key)
                .Count()
                .ToString();
            _configuration["Data:IssueCount:Duplicate:DatabaseEdgeFields"] = _context.DatabaseEdgeFields
                .Where(item => item.Database.DatabaseType.Name != "Generic")
                .GroupBy(item => item.Name)
                .Where(item => item.Count() > 1)
                .Select(item => item.Key)
                .Count()
                .ToString();
            _configuration["Data:IssueCount:Duplicate:DatabaseNodeFieldNodes"] = _context.DatabaseNodeFieldNodes
                .Where(item => item.DatabaseNodeField.Database.DatabaseType.Name != "Generic")
                .Where(item => item.DatabaseNodeField.IsSearchable)
                .GroupBy(item => item.Value)
                .Where(item => item.Count() > 1)
                .Select(item => item.Key)
                .Count()
                .ToString();
            _configuration["Data:IssueCount:Duplicate:Nodes"] = _context.Nodes
                .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                .GroupBy(item => item.Name)
                .Where(item => item.Count() > 1)
                .Select(item => item.Key)
                .Count()
                .ToString();
            _configuration["Data:IssueCount:Duplicate:Edges"] = _context.Edges
                .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                .GroupBy(item => item.Name)
                .Where(item => item.Count() > 1)
                .Select(item => item.Key)
                .Count()
                .ToString();
            _configuration["Data:IssueCount:Duplicate:NodeCollections"] = _context.NodeCollections
                .GroupBy(item => item.Name)
                .Where(item => item.Count() > 1)
                .Select(item => item.Key)
                .Count()
                .ToString();
            // Update the orphaned counts.
            _configuration["Data:IssueCount:Orphaned:Nodes"] = _context.Nodes
                .Where(item => !item.DatabaseNodeFieldNodes.Any())
                .Count()
                .ToString();
            _configuration["Data:IssueCount:Orphaned:Edges"] = _context.Edges
                .Where(item => !item.DatabaseEdges.Any() || item.EdgeNodes.Count() < 2)
                .Count()
                .ToString();
            _configuration["Data:IssueCount:Orphaned:NodeCollections"] = _context.NodeCollections
                .Where(item => !item.NodeCollectionNodes.Any())
                .Count()
                .ToString();
            _configuration["Data:IssueCount:Orphaned:Networks"] = _context.Networks
                .Where(item => !item.NetworkDatabases.Any() || !item.NetworkNodes.Any() || !item.NetworkEdges.Any() || !item.NetworkUsers.Any())
                .Count()
                .ToString();
            _configuration["Data:IssueCount:Orphaned:Analyses"] = _context.Analyses
                .Where(item => !item.AnalysisDatabases.Any() || !item.AnalysisNodes.Any() || !item.AnalysisEdges.Any() || !item.AnalysisNetworks.Any() || !item.AnalysisUsers.Any())
                .Count()
                .ToString();
            // Update the inconsistent counts.
            _configuration["Data:IssueCount:Inconsistent:Nodes"] = _context.Nodes
                .Where(item => item.DatabaseNodes.Select(item1 => item1.Database.DatabaseType).Distinct().Count() > 1)
                .Count()
                .ToString();
            _configuration["Data:IssueCount:Inconsistent:Edges"] = _context.Edges
                .Where(item => item.DatabaseEdges.Select(item1 => item1.Database.DatabaseType).Distinct().Count() > 1)
                .Count()
                .ToString();
            _configuration["Data:IssueCount:Inconsistent:NodeCollections"] = _context.NodeCollections
                .Where(item => item.NodeCollectionNodes.Select(item1 => item1.Node.DatabaseNodes).SelectMany(item1 => item1).Select(item1 => item1.Database.DatabaseType).Distinct().Count() > 1)
                .Count()
                .ToString();
            _configuration["Data:IssueCount:Inconsistent:Networks"] = _context.Networks
                .Where(item => item.NetworkDatabases.Select(item1 => item1.Database.DatabaseType).Distinct().Count() > 1)
                .Count()
                .ToString();
            _configuration["Data:IssueCount:Inconsistent:Analyses"] = _context.Analyses
                .Where(item => item.AnalysisDatabases.Select(item1 => item1.Database.DatabaseType).Distinct().Count() > 1)
                .Count()
                .ToString();
            // Display a message.
            TempData["StatusMessage"] = "Success: The issue count has been successfully updated.";
            // Redirect to the page.
            return RedirectToPage();
        }

        public IActionResult OnPostResetItemCount()
        {
            // Update the counts.
            _configuration["Data:ItemCount:Users"] = _context.Users
                .Count()
                .ToString();
            _configuration["Data:ItemCount:Roles"] = _context.Roles
                .Count()
                .ToString();
            _configuration["Data:ItemCount:DatabaseTypes"] = _context.DatabaseTypes
                .Count()
                .ToString();
            _configuration["Data:ItemCount:Databases"] = _context.Databases
                .Count()
                .ToString();
            _configuration["Data:ItemCount:DatabaseNodeFields"] = _context.DatabaseNodeFields
                .Count()
                .ToString();
            _configuration["Data:ItemCount:DatabaseEdgeFields"] = _context.DatabaseEdgeFields
                .Count()
                .ToString();
            _configuration["Data:ItemCount:Nodes"] = _context.Nodes
                .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                .Count()
                .ToString();
            _configuration["Data:ItemCount:Edges"] = _context.Edges
                .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                .Count()
                .ToString();
            _configuration["Data:ItemCount:NodeCollections"] = _context.NodeCollections
                .Count()
                .ToString();
            _configuration["Data:ItemCount:Networks"] = _context.Networks
                .Count()
                .ToString();
            _configuration["Data:ItemCount:Analyses"] = _context.Analyses
                .Count()
                .ToString();
            // Display a message.
            TempData["StatusMessage"] = "Success: The item count has been successfully updated.";
            // Redirect to the page.
            return RedirectToPage();
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
            // Define the names of the recurring background tasks.
            var nameStopAnalyses = $"{nameof(IRecurringTaskManager)}.{nameof(IRecurringTaskManager.StopAnalyses)}";
            var nameAlertUsers = $"{nameof(IRecurringTaskManager)}.{nameof(IRecurringTaskManager.AlertUsers)}";
            var nameDeleteUsers = $"{nameof(IRecurringTaskManager)}.{nameof(IRecurringTaskManager.DeleteUsers)}";
            var nameDeleteNetworks = $"{nameof(IRecurringTaskManager)}.{nameof(IRecurringTaskManager.DeleteNetworks)}";
            var nameDeleteAnalyses = $"{nameof(IRecurringTaskManager)}.{nameof(IRecurringTaskManager.DeleteAnalyses)}";
            // Delete the existing corresponding recurring jobs.
            RecurringJob.RemoveIfExists(nameStopAnalyses);
            RecurringJob.RemoveIfExists(nameAlertUsers);
            RecurringJob.RemoveIfExists(nameDeleteUsers);
            RecurringJob.RemoveIfExists(nameDeleteNetworks);
            RecurringJob.RemoveIfExists(nameDeleteAnalyses);
            // Get the existing background tasks.
            var backgroundTasks = _context.BackgroundTasks
                .Where(item => item.Name == nameStopAnalyses || item.Name == nameAlertUsers || item.Name == nameDeleteUsers || item.Name == nameDeleteNetworks || item.Name == nameDeleteAnalyses);
            // Mark the existing background tasks for deletion.
            _context.BackgroundTasks.RemoveRange(backgroundTasks);
            // Save the changes to the database.
            await _context.SaveChangesAsync();
            // Define the new background tasks.
            var backgroundTaskStopAnalyses = new BackgroundTask
            {
                DateTimeCreated = DateTime.Now,
                Name = nameStopAnalyses,
                IsRecurring = true,
                Data = JsonSerializer.Serialize(new RecurringTask
                {
                    Scheme = HttpContext.Request.Scheme,
                    HostValue = HttpContext.Request.Host.Value,
                    DaysBeforeStop = ApplicationDbContext.DaysBeforeStop,
                    DaysBeforeAlert = ApplicationDbContext.DaysBeforeAlert,
                    DaysBeforeDelete = ApplicationDbContext.DaysBeforeDelete
                })
            };
            var backgroundTaskAlertUsers = new BackgroundTask
            {
                DateTimeCreated = DateTime.Now,
                Name = nameAlertUsers,
                IsRecurring = true,
                Data = JsonSerializer.Serialize(new RecurringTask
                {
                    Scheme = HttpContext.Request.Scheme,
                    HostValue = HttpContext.Request.Host.Value,
                    DaysBeforeStop = ApplicationDbContext.DaysBeforeStop,
                    DaysBeforeAlert = ApplicationDbContext.DaysBeforeAlert,
                    DaysBeforeDelete = ApplicationDbContext.DaysBeforeDelete
                })
            };
            var backgroundTaskDeleteUsers = new BackgroundTask
            {
                DateTimeCreated = DateTime.Now,
                Name = nameDeleteUsers,
                IsRecurring = true,
                Data = JsonSerializer.Serialize(new RecurringTask
                {
                    Scheme = HttpContext.Request.Scheme,
                    HostValue = HttpContext.Request.Host.Value,
                    DaysBeforeStop = ApplicationDbContext.DaysBeforeStop,
                    DaysBeforeAlert = ApplicationDbContext.DaysBeforeAlert,
                    DaysBeforeDelete = ApplicationDbContext.DaysBeforeDelete
                })
            };
            var backgroundTaskDeleteNetworks = new BackgroundTask
            {
                DateTimeCreated = DateTime.Now,
                Name = nameDeleteNetworks,
                IsRecurring = true,
                Data = JsonSerializer.Serialize(new RecurringTask
                {
                    Scheme = HttpContext.Request.Scheme,
                    HostValue = HttpContext.Request.Host.Value,
                    DaysBeforeStop = ApplicationDbContext.DaysBeforeStop,
                    DaysBeforeAlert = ApplicationDbContext.DaysBeforeAlert,
                    DaysBeforeDelete = ApplicationDbContext.DaysBeforeDelete
                })
            };
            var backgroundTaskDeleteAnalyses = new BackgroundTask
            {
                DateTimeCreated = DateTime.Now,
                Name = nameDeleteAnalyses,
                IsRecurring = true,
                Data = JsonSerializer.Serialize(new RecurringTask
                {
                    Scheme = HttpContext.Request.Scheme,
                    HostValue = HttpContext.Request.Host.Value,
                    DaysBeforeStop = ApplicationDbContext.DaysBeforeStop,
                    DaysBeforeAlert = ApplicationDbContext.DaysBeforeAlert,
                    DaysBeforeDelete = ApplicationDbContext.DaysBeforeDelete
                })
            };
            // Mark the background tasks for addition.
            _context.BackgroundTasks.Add(backgroundTaskStopAnalyses);
            _context.BackgroundTasks.Add(backgroundTaskAlertUsers);
            _context.BackgroundTasks.Add(backgroundTaskDeleteUsers);
            _context.BackgroundTasks.Add(backgroundTaskDeleteNetworks);
            _context.BackgroundTasks.Add(backgroundTaskDeleteAnalyses);
            // Save the changes to the database.
            await _context.SaveChangesAsync();
            // Create the new Hangfire daily recurring jobs.
            RecurringJob.AddOrUpdate<IRecurringTaskManager>(nameStopAnalyses, item => item.StopAnalyses(backgroundTaskStopAnalyses.Id, CancellationToken.None), Cron.Daily());
            RecurringJob.AddOrUpdate<IRecurringTaskManager>(nameAlertUsers, item => item.AlertUsers(backgroundTaskAlertUsers.Id, CancellationToken.None), Cron.Daily());
            RecurringJob.AddOrUpdate<IRecurringTaskManager>(nameDeleteUsers, item => item.DeleteUsers(backgroundTaskDeleteUsers.Id, CancellationToken.None), Cron.Daily());
            RecurringJob.AddOrUpdate<IRecurringTaskManager>(nameDeleteNetworks, item => item.DeleteNetworks(backgroundTaskDeleteNetworks.Id, CancellationToken.None), Cron.Daily());
            RecurringJob.AddOrUpdate<IRecurringTaskManager>(nameDeleteAnalyses, item => item.DeleteAnalyses(backgroundTaskDeleteAnalyses.Id, CancellationToken.None), Cron.Daily());
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
                if (downloadItems.Contains("DuplicateDatabaseTypes"))
                {
                    // Get the duplicate values.
                    var values = _context.DatabaseTypes
                        .Where(item => item.Name != "Generic")
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => item.Key)
                        .ToList();
                    // Get the required data.
                    var data = _context.DatabaseTypes
                        .Where(item => item.Name != "Generic")
                        .Where(item => values.Contains(item.Name))
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
                        .AsNoTracking()
                        .AsEnumerable()
                        .GroupBy(item => item.Name)
                        .ToDictionary(item => item.Key, item => item.Select(item1 => item1));
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Duplicate-DatabaseTypes.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("DuplicateDatabases"))
                {
                    // Get the duplicate values.
                    var values = _context.Databases
                        .Where(item => item.DatabaseType.Name != "Generic")
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => item.Key)
                        .ToList();
                    // Get the required data.
                    var data = _context.Databases
                        .Where(item => item.DatabaseType.Name != "Generic")
                        .Where(item => values.Contains(item.Name))
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
                        .AsNoTracking()
                        .AsEnumerable()
                        .GroupBy(item => item.Name)
                        .ToDictionary(item => item.Key, item => item.Select(item1 => item1));
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Duplicate-Databases.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("DuplicateDatabaseNodeFields"))
                {
                    // Get the duplicate values.
                    var values = _context.DatabaseNodeFields
                        .Where(item => item.Database.DatabaseType.Name != "Generic")
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => item.Key)
                        .ToList();
                    // Get the required data.
                    var data = _context.DatabaseNodeFields
                        .Where(item => item.Database.DatabaseType.Name != "Generic")
                        .Where(item => values.Contains(item.Name))
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
                        .AsNoTracking()
                        .AsEnumerable()
                        .GroupBy(item => item.Name)
                        .ToDictionary(item => item.Key, item => item.Select(item1 => item1));
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Duplicate-DatabaseNodeFields.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("DuplicateDatabaseEdgeFields"))
                {
                    // Get the duplicate values.
                    var values = _context.DatabaseEdgeFields
                        .Where(item => item.Database.DatabaseType.Name != "Generic")
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => item.Key)
                        .ToList();
                    // Get the required data.
                    var data = _context.DatabaseEdgeFields
                        .Where(item => item.Database.DatabaseType.Name != "Generic")
                        .Where(item => values.Contains(item.Name))
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
                        .AsNoTracking()
                        .AsEnumerable()
                        .GroupBy(item => item.Name)
                        .ToDictionary(item => item.Key, item => item.Select(item1 => item1));
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Duplicate-DatabaseEdgeFields.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("DuplicateDatabaseNodeFieldNodes"))
                {
                    // Get the duplicate values.
                    var values = _context.DatabaseNodeFieldNodes
                        .Where(item => item.DatabaseNodeField.Database.DatabaseType.Name != "Generic")
                        .Where(item => item.DatabaseNodeField.IsSearchable)
                        .GroupBy(item => item.Value)
                        .Where(item => item.Count() > 1)
                        .Select(item => item.Key)
                        .ToList();
                    // Get the required data.
                    var data = _context.DatabaseNodeFieldNodes
                        .Where(item => item.DatabaseNodeField.Database.DatabaseType.Name != "Generic")
                        .Where(item => item.DatabaseNodeField.IsSearchable)
                        .Where(item => values.Contains(item.Value))
                        .Select(item => new
                        {
                            DatabaseNodeField = new
                            {
                                Id = item.DatabaseNodeField.Id,
                                Name = item.DatabaseNodeField.Name
                            },
                            Node = new
                            {
                                Id = item.Node.Id,
                                Name = item.Node.Name
                            },
                            Value = item.Value
                        })
                        .AsNoTracking()
                        .AsEnumerable()
                        .GroupBy(item => item.Value)
                        .ToDictionary(item => item.Key, item => item.Select(item1 => item1));
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Duplicate-DatabaseNodeFieldNodes.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("DuplicateNodes"))
                {
                    // Get the duplicate values.
                    var values = _context.Nodes
                        .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => item.Key)
                        .ToList();
                    // Get the required data.
                    var data = _context.Nodes
                        .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                        .Where(item => values.Contains(item.Name))
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
                        .AsNoTracking()
                        .AsEnumerable()
                        .GroupBy(item => item.Name)
                        .ToDictionary(item => item.Key, item => item.Select(item1 => item1));
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Duplicate-Nodes.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("DuplicateEdges"))
                {
                    // Get the duplicate values.
                    var values = _context.Edges
                        .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => item.Key)
                        .ToList();
                    // Get the required data.
                    var data = _context.Edges
                        .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                        .Where(item => values.Contains(item.Name))
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
                        .AsNoTracking()
                        .AsEnumerable()
                        .GroupBy(item => item.Name)
                        .ToDictionary(item => item.Key, item => item.Select(item1 => item1));
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Duplicate-Edges.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
                // Check the items to download.
                if (downloadItems.Contains("DuplicateNodeCollections"))
                {
                    // Get the duplicate values.
                    var values = _context.NodeCollections
                        .GroupBy(item => item.Name)
                        .Where(item => item.Count() > 1)
                        .Select(item => item.Key)
                        .ToList();
                    // Get the required data.
                    var data = _context.NodeCollections
                        .Where(item => values.Contains(item.Name))
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
                        .AsNoTracking()
                        .AsEnumerable()
                        .GroupBy(item => item.Name)
                        .ToDictionary(item => item.Key, item => item.Select(item1 => item1));
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
                        .Select(item => new
                        {
                            Id = item.Id
                        })
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
                        .Select(item => new
                        {
                            Id = item.Id
                        })
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
                        .Select(item => new
                        {
                            Id = item.Id
                        })
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
                        .Select(item => new
                        {
                            Id = item.Id
                        })
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
                        .Select(item => new
                        {
                            Id = item.Id
                        })
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
                        .Select(item => new
                        {
                            Id = item.Id
                        })
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
                        .Select(item => new
                        {
                            Id = item.Id
                        })
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
                        .Select(item => new
                        {
                            Id = item.Id
                        })
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
                        .Select(item => new
                        {
                            Id = item.Id
                        })
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
                        .Select(item => new
                        {
                            Id = item.Id
                        })
                        .AsNoTracking();
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-Inconsistent-Analyses.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
            })
            {
                FileDownloadName = $"NetControl4BioMed-Data-{DateTime.Now:yyyyMMdd}.zip"
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
            if (deleteConfirmation != $"I confirm that I want to delete the {string.Join(" and ", deleteItems)}!")
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
            // Check the items to delete.
            if (deleteItems.Contains("Nodes"))
            {
                // Get the items.
                var items = _context.Nodes
                    .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"));
                // Define a new background task.
                var task = new BackgroundTask
                {
                    DateTimeCreated = DateTime.Now,
                    Name = $"{nameof(IAdministrationTaskManager)}.{nameof(IAdministrationTaskManager.DeleteNodes)}",
                    IsRecurring = false,
                    Data = JsonSerializer.Serialize(new NodesTask
                    {
                        Items = items.Select(item => new NodeInputModel
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
                var jobId = BackgroundJob.Enqueue<IAdministrationTaskManager>(item => item.DeleteNodes(task.Id, CancellationToken.None));
            }
            // Check the items to delete.
            if (deleteItems.Contains("Edges"))
            {
                // Get the items.
                var items = _context.Edges
                    .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"));
                // Define a new background task.
                var task = new BackgroundTask
                {
                    DateTimeCreated = DateTime.Now,
                    Name = $"{nameof(IAdministrationTaskManager)}.{nameof(IAdministrationTaskManager.DeleteEdges)}",
                    IsRecurring = false,
                    Data = JsonSerializer.Serialize(new EdgesTask
                    {
                        Items = items.Select(item => new EdgeInputModel
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
                var jobId = BackgroundJob.Enqueue<IAdministrationTaskManager>(item => item.DeleteEdges(task.Id, CancellationToken.None));
            }
            // Check the items to delete.
            if (deleteItems.Contains("NodeCollections"))
            {
                // Get the items.
                var items = _context.NodeCollections
                    .AsQueryable();
                // Define a new background task.
                var task = new BackgroundTask
                {
                    DateTimeCreated = DateTime.Now,
                    Name = $"{nameof(IAdministrationTaskManager)}.{nameof(IAdministrationTaskManager.DeleteNodeCollections)}",
                    IsRecurring = false,
                    Data = JsonSerializer.Serialize(new NodeCollectionsTask
                    {
                        Items = items.Select(item => new NodeCollectionInputModel
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
            // Check the items to delete.
            if (deleteItems.Contains("Networks"))
            {
                // Get the items.
                var items = _context.Networks
                    .AsQueryable();
                // Define a new background task.
                var task = new BackgroundTask
                {
                    DateTimeCreated = DateTime.Now,
                    Name = $"{nameof(IAdministrationTaskManager)}.{nameof(IAdministrationTaskManager.DeleteNetworks)}",
                    IsRecurring = false,
                    Data = JsonSerializer.Serialize(new NetworksTask
                    {
                        Items = items.Select(item => new NetworkInputModel
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
                var jobId = BackgroundJob.Enqueue<IAdministrationTaskManager>(item => item.DeleteNetworks(task.Id, CancellationToken.None));
            }
            // Check the items to delete.
            if (deleteItems.Contains("Analyses"))
            {
                // Get the items.
                var items = _context.Analyses
                    .AsQueryable();
                // Define a new background task.
                var task = new BackgroundTask
                {
                    DateTimeCreated = DateTime.Now,
                    Name = $"{nameof(IAdministrationTaskManager)}.{nameof(IAdministrationTaskManager.DeleteAnalyses)}",
                    IsRecurring = false,
                    Data = JsonSerializer.Serialize(new AnalysesTask
                    {
                        Items = items.Select(item => new AnalysisInputModel
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
                var jobId = BackgroundJob.Enqueue<IAdministrationTaskManager>(item => item.DeleteAnalyses(task.Id, CancellationToken.None));
            }
            // Display a message.
            TempData["StatusMessage"] = $"Success: A new background task was created to delete {string.Join("and ", deleteItems.Select(item => $"all {item.ToLower()}"))}.";
            // Redirect to the page.
            return RedirectToPage();
        }
    }
}
