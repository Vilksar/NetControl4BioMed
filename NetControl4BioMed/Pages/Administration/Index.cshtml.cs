using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Services;
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
            public bool AnyDuplicate { get; set; }

            public bool AnyOrphaned { get; set; }

            public bool AnyInconsistent { get; set; }

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
            // Check if corresponding configuration section doesn't exist.
            if (!_configuration.GetSection("Data").GetSection("IssueCount").Exists())
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
            }
            // Define the view.
            View = new ViewModel
            {
                UserCount = _context.Users
                    .Count(),
                RoleCount = _context.Roles
                    .Count(),
                DatabaseCount = _context.Databases
                    .Count(),
                NodeCount = _context.Nodes
                    .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Count(),
                EdgeCount = _context.Edges
                    .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Count(),
                NodeCollectionCount = _context.NodeCollections
                    .Count(),
                IssueCount = _configuration
                    .GetSection("Data")
                    .GetSection("IssueCount")
                    .GetChildren()
                    .ToDictionary(item => item.Key, item => item.GetChildren().ToDictionary(item1 => item1.Key, item1 => int.TryParse(item1.Value, out var result) ? result : -1)),
                AnnouncementMessage = _configuration["Data:AnnouncementMessage"]
            };
            // Return the page.
            return Page();
        }

        public IActionResult OnPostResetIssueCount()
        {
            // Reset the configuration data.
            _configuration["Data:IssueCount"] = null;
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

        public IActionResult OnPostResetHangfireRecurrentJobs()
        {
            // Delete any existing recurring tasks of cleaning the database.
            RecurringJob.RemoveIfExists(nameof(IHangfireRecurringJobRunner));
            // Define the view model for the recurring task of cleaning the database.
            var viewModel = new HangfireRecurringCleanerViewModel
            {
                Scheme = HttpContext.Request.Scheme,
                HostValue = HttpContext.Request.Host.Value
            };
            // Create a daily recurring Hangfire task of cleaning the database.
            RecurringJob.AddOrUpdate<IHangfireRecurringJobRunner>(nameof(IHangfireRecurringJobRunner), item => item.Run(viewModel), Cron.Daily());
            // Display a message.
            TempData["StatusMessage"] = "Success: The Hangfire recurrent jobs have been successfully reset. You can view more details on the Hangfire dasboard.";
            // Redirect to the page.
            return RedirectToPage();
        }

        public IActionResult OnPostDownload(string type, IEnumerable<string> items)
        {
            // Define the JSON serializer options for all of the returned files.
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            // Check if there is no type provided.
            if (string.IsNullOrEmpty(type))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: There was no provided download type.";
                // Redirect to the page.
                return RedirectToPage();
            }
            // Check if there are no items provided.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: There were no provided items to download.";
                // Redirect to the page.
                return RedirectToPage();
            }
            // Check the type of the download.
            if (type == "All")
            {
                // Return the streamed file.
                return new FileCallbackResult(MediaTypeNames.Application.Zip, async (zipStream, _) =>
                {
                    // Define a new ZIP archive.
                    using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create);
                    // Create an entry for the database types.
                    if (items.Contains("DatabaseTypes"))
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
                            });
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-DatabaseTypes.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                    // Create an entry for the databases.
                    if (items.Contains("Databases"))
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
                                    }),
                                DatabaseNodes = item.DatabaseNodes
                                    .Select(item1 => new
                                    {
                                        Id = item1.Node.Id,
                                        Name = item1.Node.Name
                                    }),
                                DatabaseEdges = item.DatabaseEdges
                                    .Select(item1 => new
                                    {
                                        Id = item1.Edge.Id,
                                        Name = item1.Edge.Name
                                    })
                            });
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-Databases.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                    // Create an entry for the database node fields.
                    if (items.Contains("DatabaseNodeFields"))
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
                            });
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-DatabaseNodeFields.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                    // Create an entry for the database edge fields.
                    if (items.Contains("DatabaseEdgeFields"))
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
                            });
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-DatabaseEdgeFields.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                    // Create an entry for the nodes.
                    if (items.Contains("Nodes"))
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
                            });
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-Nodes.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                    // Create an entry for the edges.
                    if (items.Contains("Edges"))
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
                                        Type = item1.Type.ToString()
                                    })
                            });
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-Edges.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                    // Create an entry for the node collections.
                    if (items.Contains("NodeCollections"))
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
                            });
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-NodeCollections.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                    // Create an entry for the networks.
                    if (items.Contains("Networks"))
                    {
                        // Get the required data.
                        var data = _context.Networks
                            .Select(item => new
                            {
                                Id = item.Id,
                                DateTimeCreated = item.DateTimeCreated,
                                Algorithm = item.Algorithm.GetDisplayName()
                            });
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-Networks.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                    // Create an entry for the analyses.
                    if (items.Contains("Analyses"))
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
                            });
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-Analyses.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                })
                {
                    FileDownloadName = $"NetControl4BioMed-AllData.zip"
                };
            }
            // Check the type of the download.
            if (type == "Duplicate")
            {
                // Return the streamed file.
                return new FileCallbackResult(MediaTypeNames.Application.Zip, async (zipStream, _) =>
                {
                    // Define a new ZIP archive.
                    using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create);
                    // Create an entry for the database types.
                    if (items.Contains("DatabaseTypes"))
                    {
                        // Get the duplicate values.
                        var values = _context.DatabaseTypes
                            .Where(item => item.Name != "Generic")
                            .GroupBy(item => item.Name)
                            .Where(item => item.Count() > 1)
                            .Select(item => item.Key);
                        // Get the required data.
                        var data = _context.DatabaseTypes
                            .Where(item => item.Name != "Generic")
                            .Where(item => values.Contains(item.Name))
                            .Select(item => new
                            {
                                Id = item.Id,
                                DateTimeCreated = item.DateTimeCreated,
                                Name = item.Name,
                                Description = item.Description
                            })
                            .AsEnumerable()
                            .GroupBy(item => item.Name)
                            .ToDictionary(item => item.Key, item => item.Select(item1 => item1));
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-DatabaseTypes.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                    // Create an entry for the databases.
                    if (items.Contains("Databases"))
                    {
                        // Get the duplicate values.
                        var values = _context.Databases
                            .Where(item => item.DatabaseType.Name != "Generic")
                            .GroupBy(item => item.Name)
                            .Where(item => item.Count() > 1)
                            .Select(item => item.Key);
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
                                }
                            })
                            .AsEnumerable()
                            .GroupBy(item => item.Name)
                            .ToDictionary(item => item.Key, item => item.Select(item1 => item1));
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-Databases.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                    // Create an entry for the database node fields.
                    if (items.Contains("DatabaseNodeFields"))
                    {
                        // Get the duplicate values.
                        var values = _context.DatabaseNodeFields
                            .Where(item => item.Database.DatabaseType.Name != "Generic")
                            .GroupBy(item => item.Name)
                            .Where(item => item.Count() > 1)
                            .Select(item => item.Key);
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
                                }
                            })
                            .AsEnumerable()
                            .GroupBy(item => item.Name)
                            .ToDictionary(item => item.Key, item => item.Select(item1 => item1));
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-DatabaseNodeFields.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                    // Create an entry for the database edge fields.
                    if (items.Contains("DatabaseEdgeFields"))
                    {
                        // Get the duplicate values.
                        var values = _context.DatabaseEdgeFields
                            .Where(item => item.Database.DatabaseType.Name != "Generic")
                            .GroupBy(item => item.Name)
                            .Where(item => item.Count() > 1)
                            .Select(item => item.Key);
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
                                }
                            })
                            .AsEnumerable()
                            .GroupBy(item => item.Name)
                            .ToDictionary(item => item.Key, item => item.Select(item1 => item1));
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-DatabaseEdgeFields.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                    // Create an entry for the nodes.
                    if (items.Contains("Nodes"))
                    {
                        // Get the duplicate values.
                        var values = _context.Nodes
                            .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                            .GroupBy(item => item.Name)
                            .Where(item => item.Count() > 1)
                            .Select(item => item.Key);
                        // Get the required data.
                        var data = _context.Nodes
                            .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                            .Where(item => values.Contains(item.Name))
                            .Select(item => new
                            {
                                Id = item.Id,
                                DateTimeCreated = item.DateTimeCreated,
                                Name = item.Name,
                                Description = item.Description
                            })
                            .AsEnumerable()
                            .GroupBy(item => item.Name)
                            .ToDictionary(item => item.Key, item => item.Select(item1 => item1));
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-Nodes.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                    // Create an entry for the nodes (matching on database node field node values).
                    if (items.Contains("Nodes"))
                    {
                        // Get the duplicate values.
                        var values = _context.DatabaseNodeFieldNodes
                            .Where(item => item.DatabaseNodeField.Database.DatabaseType.Name != "Generic")
                            .Where(item => item.DatabaseNodeField.IsSearchable)
                            .GroupBy(item => item.Value)
                            .Where(item => item.Count() > 1)
                            .Select(item => item.Key);
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
                            .AsEnumerable()
                            .GroupBy(item => item.Value)
                            .ToDictionary(item => item.Key, item => item.Select(item1 => item1));
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-DatabaseNodeFieldNodes.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                    // Create an entry for the edges.
                    if (items.Contains("Edges"))
                    {
                        // Get the duplicate values.
                        var values = _context.Edges
                            .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                            .GroupBy(item => item.Name)
                            .Where(item => item.Count() > 1)
                            .Select(item => item.Key);
                        // Get the required data.
                        var data = _context.Edges
                            .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                            .Where(item => values.Contains(item.Name))
                            .Select(item => new
                            {
                                Id = item.Id,
                                DateTimeCreated = item.DateTimeCreated,
                                Name = item.Name,
                                Description = item.Description
                            })
                            .AsEnumerable()
                            .GroupBy(item => item.Name)
                            .ToDictionary(item => item.Key, item => item.Select(item1 => item1));
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-Edges.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                    // Create an entry for the node collections.
                    if (items.Contains("NodeCollections"))
                    {
                        // Get the duplicate values.
                        var values = _context.NodeCollections
                            .GroupBy(item => item.Name)
                            .Where(item => item.Count() > 1)
                            .Select(item => item.Key);
                        // Get the required data.
                        var data = _context.NodeCollections
                            .Where(item => values.Contains(item.Name))
                            .Select(item => new
                            {
                                Id = item.Id,
                                DateTimeCreated = item.DateTimeCreated,
                                Name = item.Name,
                                Description = item.Description
                            })
                            .AsEnumerable()
                            .GroupBy(item => item.Name)
                            .ToDictionary(item => item.Key, item => item.Select(item1 => item1));
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-NodeCollections.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                })
                {
                    FileDownloadName = $"NetControl4BioMed-DuplicateData.zip"
                };
            }
            // Check the type of the download.
            if (type == "Orphaned")
            {
                // Return the streamed file.
                return new FileCallbackResult(MediaTypeNames.Application.Zip, async (zipStream, _) =>
                {
                    // Define a new ZIP archive.
                    using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create);
                    // Create an entry for the nodes.
                    if (items.Contains("Nodes"))
                    {
                        // Get the required data.
                        var data = _context.Nodes
                            .Where(item => !item.DatabaseNodeFieldNodes.Any())
                            .Select(item => new
                            {
                                Id = item.Id
                            });
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-Nodes.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                    // Create an entry for the edges.
                    if (items.Contains("Edges"))
                    {
                        // Get the required data.
                        var data = _context.Edges
                            .Where(item => !item.DatabaseEdges.Any() || item.EdgeNodes.Count() < 2)
                            .Select(item => new
                            {
                                Id = item.Id
                            });
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-Edges.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                    // Create an entry for the node collections.
                    if (items.Contains("NodeCollections"))
                    {
                        // Get the required data.
                        var data = _context.NodeCollections
                            .Where(item => !item.NodeCollectionNodes.Any())
                            .Select(item => new
                            {
                                Id = item.Id
                            });
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-NodeCollections.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                    // Create an entry for the networks.
                    if (items.Contains("Networks"))
                    {
                        // Get the required data.
                        var data = _context.Networks
                            .Where(item => !item.NetworkDatabases.Any() || !item.NetworkNodes.Any() || !item.NetworkEdges.Any() || !item.NetworkUsers.Any())
                            .Select(item => new
                            {
                                Id = item.Id
                            });
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-Networks.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                    // Create an entry for the analyses.
                    if (items.Contains("Analyses"))
                    {
                        // Get the required data.
                        var data = _context.Analyses
                            .Where(item => !item.AnalysisDatabases.Any() || !item.AnalysisNodes.Any() || !item.AnalysisEdges.Any() || !item.AnalysisNetworks.Any() || !item.AnalysisUsers.Any())
                            .Select(item => new
                            {
                                Id = item.Id
                            });
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-Analyses.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                })
                {
                    FileDownloadName = $"NetControl4BioMed-OrphanedData.zip"
                };
            }
            // Check the type of the download.
            if (type == "Inconsistent")
            {
                // Return the streamed file.
                return new FileCallbackResult(MediaTypeNames.Application.Zip, async (zipStream, _) =>
                {
                    // Define a new ZIP archive.
                    using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create);
                    // Create an entry for the nodes.
                    if (items.Contains("Nodes"))
                    {
                        // Get the required data.
                        var data = _context.Nodes
                            .Where(item => item.DatabaseNodes.Select(item1 => item1.Database.DatabaseType).Distinct().Count() > 1)
                            .Select(item => new
                            {
                                Id = item.Id
                            });
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-Nodes.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                    // Create an entry for the edges.
                    if (items.Contains("Edges"))
                    {
                        // Get the required data.
                        var data = _context.Edges
                            .Where(item => item.DatabaseEdges.Select(item1 => item1.Database.DatabaseType).Distinct().Count() > 1)
                            .Select(item => new
                            {
                                Id = item.Id
                            });
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-Edges.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                    // Create an entry for the node collections.
                    if (items.Contains("NodeCollections"))
                    {
                        // Get the required data.
                        var data = _context.NodeCollections
                            .Where(item => item.NodeCollectionNodes.Select(item1 => item1.Node.DatabaseNodes).SelectMany(item1 => item1).Select(item1 => item1.Database.DatabaseType).Distinct().Count() > 1)
                            .Select(item => new
                            {
                                Id = item.Id
                            });
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-NodeCollections.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                    // Create an entry for the networks
                    if (items.Contains("Networks"))
                    {
                        // Get the required data.
                        var data = _context.Networks
                            .Where(item => item.NetworkDatabases.Select(item1 => item1.Database.DatabaseType).Distinct().Count() > 1)
                            .Select(item => new
                            {
                                Id = item.Id
                            });
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-Networks.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                    // Create an entry for the analyses.
                    if (items.Contains("Analyses"))
                    {
                        // Get the required data.
                        var data = _context.Analyses
                            .Where(item => item.AnalysisDatabases.Select(item1 => item1.Database.DatabaseType).Distinct().Count() > 1)
                            .Select(item => new
                            {
                                Id = item.Id
                            });
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"NetControl4BioMed-Analyses.json", CompressionLevel.Fastest).Open();
                        // Write the data to the stream corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                })
                {
                    FileDownloadName = $"NetControl4BioMed-InconsistentData.zip"
                };
            }
            // Display a message.
            TempData["StatusMessage"] = "Error: The provided download type is not valid.";
            // Redirect to the page.
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string items, string confirmation, string reCaptchaToken)
        {
            // Check if there is any missing parameter.
            if (string.IsNullOrEmpty(items))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: There were no provided items to delete.";
                // Redirect to the page.
                return RedirectToPage();
            }
            // Check if the confirmation is not valid.
            if (confirmation != $"I confirm that I want to delete the {items}!")
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
                TempData["StatusMessage"] = "The reCaptcha verification failed.";
                // Redirect to the page.
                return RedirectToPage();
            }
            // Check the items to delete.
            if (items == "Nodes")
            {
                // Get the items to delete.
                var nodes = _context.Nodes
                    .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"));
                // Save the number of items found.
                var nodeCount = nodes.Count();
                // Get the related entities that use the items.
                var edges = _context.Edges
                    .Where(item => item.EdgeNodes.Any(item1 => nodes.Contains(item1.Node)));
                var networks = _context.Networks
                    .Where(item => item.NetworkNodes.Any(item1 => nodes.Contains(item1.Node)));
                var analyses = _context.Analyses
                    .Where(item => item.AnalysisNodes.Any(item1 => nodes.Contains(item1.Node)));
                // Mark the items for deletion.
                _context.Analyses.RemoveRange(analyses);
                _context.Networks.RemoveRange(networks);
                _context.Edges.RemoveRange(edges);
                _context.Nodes.RemoveRange(nodes);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
                // Display a message.
                TempData["StatusMessage"] = $"Success: {nodeCount.ToString()} node{(nodeCount != 1 ? "s" : string.Empty)} deleted successfully.";
                // Redirect to the page.
                return RedirectToPage();
            }
            // Check the items to delete.
            if (items == "Edges")
            {
                // Get the items to delete.
                var edges = _context.Edges
                    .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"));
                // Save the number of items found.
                var edgeCount = edges.Count();
                // Get the related entities that use the items.
                var networks = _context.Networks
                    .Where(item => item.NetworkEdges.Any(item1 => edges.Contains(item1.Edge)));
                var analyses = _context.Analyses
                    .Where(item => item.AnalysisEdges.Any(item1 => edges.Contains(item1.Edge)));
                // Mark the items for deletion.
                _context.Analyses.RemoveRange(analyses);
                _context.Networks.RemoveRange(networks);
                _context.Edges.RemoveRange(edges);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
                // Display a message.
                TempData["StatusMessage"] = $"Success: {edgeCount.ToString()} edge{(edgeCount != 1 ? "s" : string.Empty)} deleted successfully.";
                // Redirect to the page.
                return RedirectToPage();
            }
            // Check the items to delete.
            if (items == "NodeCollections")
            {
                // Get the items to delete.
                var nodeCollections = _context.NodeCollections
                    .AsQueryable();
                // Save the number of items found.
                var nodeCollectionCount = nodeCollections.Count();
                // Get the related entities that use the items.
                var networks = _context.Networks.Where(item => item.NetworkNodeCollections.Any(item1 => nodeCollections.Contains(item1.NodeCollection)));
                var analyses = _context.Analyses.Where(item => item.AnalysisNodeCollections.Any(item1 => nodeCollections.Contains(item1.NodeCollection)) || item.AnalysisNetworks.Any(item1 => networks.Contains(item1.Network)));
                // Mark the items for deletion.
                _context.Analyses.RemoveRange(analyses);
                _context.Networks.RemoveRange(networks);
                _context.NodeCollections.RemoveRange(nodeCollections);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
                // Display a message.
                TempData["StatusMessage"] = $"Success: {nodeCollectionCount.ToString()} node collection{(nodeCollectionCount != 1 ? "s" : string.Empty)} deleted successfully.";
                // Redirect to the page.
                return RedirectToPage();
            }
            // Check the items to delete.
            if (items == "Networks")
            {
                // Get the items to delete.
                var networks = _context.Networks
                    .AsQueryable();
                // Save the number of items found.
                var networkCount = networks.Count();
                // Get the related entities that use the items.
                var analyses = _context.Analyses.Where(item => item.AnalysisNetworks.Any(item1 => networks.Contains(item1.Network)));
                // Get the generic entities among them.
                var genericNetworks = networks.Where(item => item.NetworkDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"));
                var genericNodes = _context.Nodes.Where(item => item.NetworkNodes.Any(item1 => genericNetworks.Contains(item1.Network)));
                var genericEdges = _context.Edges.Where(item => item.NetworkEdges.Any(item1 => genericNetworks.Contains(item1.Network)) || item.EdgeNodes.Any(item1 => genericNodes.Contains(item1.Node)));
                // Mark the items for deletion.
                _context.Analyses.RemoveRange(analyses);
                _context.Networks.RemoveRange(networks);
                _context.Edges.RemoveRange(genericEdges);
                _context.Nodes.RemoveRange(genericNodes);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
                // Display a message.
                TempData["StatusMessage"] = $"Success: {networkCount.ToString()} network{(networkCount != 1 ? "s" : string.Empty)} deleted successfully.";
                // Redirect to the page.
                return RedirectToPage();
            }
            // Check the items to delete.
            if (items == "Analyses")
            {
                // Get the items to delete.
                var analyses = _context.Analyses
                    .AsQueryable();
                // Save the number of items found.
                var analysisCount = analyses.Count();
                // Mark the items for deletion.
                _context.Analyses.RemoveRange(analyses);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
                // Display a message.
                TempData["StatusMessage"] = $"Success: {analysisCount.ToString()} analys{(analysisCount != 1 ? "e" : "i")}s deleted successfully.";
                // Redirect to the page.
                return RedirectToPage();
            }
            // Display a message.
            TempData["StatusMessage"] = "Error: There were no items to delete.";
            // Redirect to the page.
            return RedirectToPage();
        }
    }
}
