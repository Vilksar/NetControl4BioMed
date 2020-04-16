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
            // Check if the issue count needs to be reset.
            if (!bool.TryParse(_configuration["Data:IssueCount:Reset"], out var reset) || reset)
            {
                // Update the reset status.
                _configuration["Data:IssueCount:Reset"] = false.ToString();
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
            // Update the reset status.
            _configuration["Data:IssueCount:Reset"] = true.ToString();
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
                        });
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
                        });
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
                        });
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
                        });
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
                                    Type = item1.Type.ToString()
                                })
                        });
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
                        });
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
                        });
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
                        });
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
                        });
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
                        });
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
                        });
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
                        });
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
                        });
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
                        });
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
                        });
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
                        });
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
                        });
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
                        });
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
                TempData["StatusMessage"] = "The reCaptcha verification failed.";
                // Redirect to the page.
                return RedirectToPage();
            }
            // Define a list to store the number of items found.
            var itemList = new List<string>();
            // Check the items to delete.
            if (deleteItems.Contains("Nodes"))
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
                // Save a message.
                itemList.Add($"{nodeCount.ToString()} node{(nodeCount != 1 ? "s" : string.Empty)}");
            }
            // Check the items to delete.
            if (deleteItems.Contains("Edges"))
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
                // Save a message.
                itemList.Add($"{edgeCount.ToString()} edge{(edgeCount != 1 ? "s" : string.Empty)}");
            }
            // Check the items to delete.
            if (deleteItems.Contains("NodeCollections"))
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
                // Save a message.
                itemList.Add($"{nodeCollectionCount.ToString()} node collection{(nodeCollectionCount != 1 ? "s" : string.Empty)}");
            }
            // Check the items to delete.
            if (deleteItems.Contains("Networks"))
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
                // Save a message.
                itemList.Add($"{networkCount.ToString()} network{(networkCount != 1 ? "s" : string.Empty)}");
            }
            // Check the items to delete.
            if (deleteItems.Contains("Analyses"))
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
                itemList.Add($"{analysisCount.ToString()} analys{(analysisCount != 1 ? "e" : "i")}s");
            }
            // Check if there weren't any items found.
            if (!itemList.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: There were no items to delete.";
                // Redirect to the page.
                return RedirectToPage();
            }
            // Display a message.
            TempData["StatusMessage"] = $"Success: {string.Join(" and ", itemList)} deleted successfully.";
            // Redirect to the page.
            return RedirectToPage();
        }
    }
}
