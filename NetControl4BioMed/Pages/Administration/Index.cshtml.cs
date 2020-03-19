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
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Services;
using NetControl4BioMed.Helpers.ViewModels;

namespace NetControl4BioMed.Pages.Administration
{
    [Authorize(Roles = "Administrator")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public int UserCount { get; set; }

            public int RoleCount { get; set; }

            public int DatabaseCount { get; set; }

            public int NodeCount { get; set; }

            public int EdgeCount { get; set; }

            public int GenericNodeCount { get; set; }

            public int GenericEdgeCount { get; set; }

            public int NodeCollectionCount { get; set; }

            public bool DuplicateDetected { get; set; }

            public IQueryable<string> DuplicateDatabaseTypes { get; set; }

            public IQueryable<string> DuplicateDatabases { get; set; }

            public IQueryable<string> DuplicateDatabaseNodeFields { get; set; }

            public IQueryable<string> DuplicateDatabaseEdgeFields { get; set; }

            public IQueryable<string> DuplicateDatabaseNodeFieldNodes { get; set; }

            public IQueryable<string> DuplicateNodes { get; set; }

            public IQueryable<string> DuplicateEdges { get; set; }

            public IQueryable<string> DuplicateNodeCollections { get; set; }

            public bool OrphanDetected { get; set; }

            public IQueryable<Node> OrphanedNodes { get; set; }

            public IQueryable<Edge> OrphanedEdges { get; set; }

            public IQueryable<NodeCollection> OrphanedNodeCollections { get; set; }

            public IQueryable<Network> OrphanedNetworks { get; set; }

            public IQueryable<Analysis> OrphanedAnalyses { get; set; }

            public bool InconsistencyDetected { get; set; }

            public IQueryable<Node> InconsistentNodes { get; set; }

            public IQueryable<Edge> InconsistentEdges { get; set; }

            public IQueryable<NodeCollection> InconsistentNodeCollections { get; set; }

            public IQueryable<Network> InconsistentNetworks { get; set; }

            public IQueryable<Analysis> InconsistentAnalyses { get; set; }
        }

        public IActionResult OnGet()
        {
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
                GenericNodeCount = _context.Nodes
                    .Where(item => item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Count(),
                GenericEdgeCount = _context.Edges
                    .Where(item => item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Count(),
                NodeCollectionCount = _context.NodeCollections
                    .Count(),
                DuplicateDatabaseTypes = _context.DatabaseTypes
                    .Where(item => item.Name != "Generic")
                    .GroupBy(item => item.Name)
                    .Where(item => item.Count() > 1)
                    .Select(item => item.Key),
                DuplicateDatabases = _context.Databases
                    .Where(item => item.DatabaseType.Name != "Generic")
                    .GroupBy(item => item.Name)
                    .Where(item => item.Count() > 1)
                    .Select(item => item.Key),
                DuplicateDatabaseNodeFields = _context.DatabaseNodeFields
                    .Where(item => item.Database.DatabaseType.Name != "Generic")
                    .GroupBy(item => item.Name)
                    .Where(item => item.Count() > 1)
                    .Select(item => item.Key),
                DuplicateDatabaseEdgeFields = _context.DatabaseEdgeFields
                    .Where(item => item.Database.DatabaseType.Name != "Generic")
                    .GroupBy(item => item.Name)
                    .Where(item => item.Count() > 1)
                    .Select(item => item.Key),
                DuplicateDatabaseNodeFieldNodes = _context.DatabaseNodeFieldNodes
                    .Where(item => item.DatabaseNodeField.Database.DatabaseType.Name != "Generic")
                    .Where(item => item.DatabaseNodeField.IsSearchable)
                    .GroupBy(item => item.Value)
                    .Where(item => item.Count() > 1)
                    .Select(item => item.Key),
                DuplicateNodes = _context.Nodes
                    .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .GroupBy(item => item.Name)
                    .Where(item => item.Count() > 1)
                    .Select(item => item.Key),
                DuplicateEdges = _context.Edges
                    .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .GroupBy(item => item.Name)
                    .Where(item => item.Count() > 1)
                    .Select(item => item.Key),
                DuplicateNodeCollections = _context.NodeCollections
                    .GroupBy(item => item.Name)
                    .Where(item => item.Count() > 1)
                    .Select(item => item.Key),
                OrphanedNodes = _context.Nodes
                    .Where(item => !item.DatabaseNodeFieldNodes.Any()),
                OrphanedEdges = _context.Edges
                    .Where(item => !item.DatabaseEdges.Any() || item.EdgeNodes.Count() < 2),
                OrphanedNodeCollections = _context.NodeCollections
                    .Where(item => !item.NodeCollectionNodes.Any()),
                OrphanedNetworks = _context.Networks
                    .Where(item => !item.NetworkDatabases.Any() || !item.NetworkNodes.Any() || !item.NetworkEdges.Any() || !item.NetworkUsers.Any()),
                OrphanedAnalyses = _context.Analyses
                    .Where(item => !item.AnalysisDatabases.Any() || !item.AnalysisNodes.Any() || !item.AnalysisEdges.Any() || !item.AnalysisNetworks.Any() || !item.AnalysisUsers.Any()),
                InconsistentNodes = _context.Nodes
                    .Where(item => item.DatabaseNodes.Select(item1 => item1.Database.DatabaseType).Distinct().Count() > 1),
                InconsistentEdges = _context.Edges
                    .Where(item => item.DatabaseEdges.Select(item1 => item1.Database.DatabaseType).Distinct().Count() > 1),
                InconsistentNodeCollections = _context.NodeCollections
                    .Where(item => item.NodeCollectionNodes.Select(item1 => item1.Node.DatabaseNodes).SelectMany(item1 => item1).Select(item1 => item1.Database.DatabaseType).Distinct().Count() > 1),
                InconsistentNetworks = _context.Networks
                    .Where(item => item.NetworkDatabases.Select(item1 => item1.Database.DatabaseType).Distinct().Count() > 1),
                InconsistentAnalyses = _context.Analyses
                    .Where(item => item.AnalysisDatabases.Select(item1 => item1.Database.DatabaseType).Distinct().Count() > 1)
            };
            // Check if there were any issues detected.
            View.DuplicateDetected = View.DuplicateDatabaseTypes.Any() || View.DuplicateDatabases.Any() || View.DuplicateDatabaseNodeFields.Any() || View.DuplicateDatabaseEdgeFields.Any() || View.DuplicateDatabaseNodeFieldNodes.Any() || View.DuplicateNodes.Any() || View.DuplicateEdges.Any() || View.DuplicateNodeCollections.Any();
            View.OrphanDetected = View.OrphanedNodes.Any() || View.OrphanedEdges.Any() || View.OrphanedNodeCollections.Any() || View.OrphanedNetworks.Any() || View.OrphanedAnalyses.Any();
            View.InconsistencyDetected = View.InconsistentNodes.Any() || View.InconsistentEdges.Any() || View.InconsistentNodeCollections.Any() || View.InconsistentNetworks.Any() || View.InconsistentAnalyses.Any();
            // Return the page.
            return Page();
        }

        public IActionResult OnPostHangfire()
        {
            // Redirect to the Hangfire dashboard.
            return LocalRedirect("/Hangfire");
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

        public IActionResult OnPostDownload()
        {
            // Return the streamed file.
            return new FileCallbackResult(MediaTypeNames.Application.Zip, async (zipStream, _) =>
            {
                // Define the JSON serializer options for all of the returned files.
                var jsonSerializerOptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                // Define a new ZIP archive.
                using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create);
                // Create an entry for the database types.
                if (true)
                {
                    // Get the required data.
                    var data = _context.DatabaseTypes.Select(item => new
                    {
                        Id = item.Id,
                        DateTimeCreated = item.DateTimeCreated,
                        Name = item.Name,
                        Description = item.Description,
                        Databases = item.Databases.Select(item1 => new
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
                if (true)
                {
                    // Get the required data.
                    var data = _context.Databases.Select(item => new
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
                        DatabaseNodeFields = item.DatabaseNodeFields.Select(item1 => new
                        {
                            Id = item1.Id,
                            Name = item1.Name
                        }),
                        DatabaseEdgeFields = item.DatabaseEdgeFields.Select(item1 => new
                        {
                            Id = item1.Id,
                            Name = item1.Name
                        }),
                        DatabaseNodes = item.DatabaseNodes.Select(item1 => new
                        {
                            Id = item1.Node.Id,
                            Name = item1.Node.Name
                        }),
                        DatabaseEdges = item.DatabaseEdges.Select(item1 => new
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
                if (true)
                {
                    // Get the required data.
                    var data = _context.DatabaseNodeFields.Select(item => new
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
                        DatabaseNodeFieldNodes = item.DatabaseNodeFieldNodes.Select(item1 => new
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
                if (true)
                {
                    // Get the required data.
                    var data = _context.DatabaseEdgeFields.Select(item => new
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
                        DatabaseEdgeFieldEdges = item.DatabaseEdgeFieldEdges.Select(item1 => new
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
                if (true)
                {
                    // Get the required data.
                    var data = _context.Nodes.Select(item => new
                    {
                        Id = item.Id,
                        DateTimeCreated = item.DateTimeCreated,
                        Name = item.Name,
                        Description = item.Description,
                        Databases = item.DatabaseNodes.Select(item1 => new
                        {
                            Id = item1.Database.Id,
                            Name = item1.Database.Name
                        }),
                        DatabaseNodeFieldNodes = item.DatabaseNodeFieldNodes.Select(item1 => new
                        {
                            Id = item1.DatabaseNodeField.Id,
                            Name = item1.DatabaseNodeField.Name,
                            Value = item1.Value
                        }),
                        EdgeNodes = item.EdgeNodes.Select(item1 => new
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
                if (true)
                {
                    // Get the required data.
                    var data = _context.Edges.Select(item => new
                    {
                        Id = item.Id,
                        DateTimeCreated = item.DateTimeCreated,
                        Name = item.Name,
                        Description = item.Description,
                        Databases = item.DatabaseEdges.Select(item1 => new
                        {
                            Id = item1.Database.Id,
                            Name = item1.Database.Name
                        }),
                        DatabaseEdgeFieldEdges = item.DatabaseEdgeFieldEdges.Select(item1 => new
                        {
                            Id = item1.DatabaseEdgeField.Id,
                            Name = item1.DatabaseEdgeField.Name,
                            Value = item1.Value
                        }),
                        EdgeNodes = item.EdgeNodes.Select(item1 => new
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
                if (true)
                {
                    // Get the required data.
                    var data = _context.NodeCollections.Select(item => new
                    {
                        Id = item.Id,
                        DateTimeCreated = item.DateTimeCreated,
                        Name = item.Name,
                        Description = item.Description,
                        NodeCollectionNodes = item.NodeCollectionNodes.Select(item1 => new
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
            })
            {
                FileDownloadName = $"NetControl4BioMed-Data.zip"
            };
        }
    }
}
