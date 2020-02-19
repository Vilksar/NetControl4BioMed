using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;

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
            public bool IssueDetected { get; set; }

            public int UserCount { get; set; }

            public int RoleCount { get; set; }

            public int DatabaseCount { get; set; }

            public int NodeCount { get; set; }

            public int EdgeCount { get; set; }

            public int NodeCollectionCount { get; set; }

            public IQueryable<string> DuplicateDatabaseTypes { get; set; }

            public IQueryable<string> DuplicateDatabases { get; set; }

            public IQueryable<string> DuplicateDatabaseNodeFields { get; set; }

            public IQueryable<string> DuplicateDatabaseEdgeFields { get; set; }

            public IQueryable<string> DuplicateDatabaseNodeFieldNodes { get; set; }

            public IQueryable<string> DuplicateNodes { get; set; }

            public IQueryable<string> DuplicateEdges { get; set; }

            public IQueryable<string> DuplicateNodeCollections { get; set; }

            public IQueryable<Node> OrphanedNodes { get; set; }

            public IQueryable<Edge> OrphanedEdges { get; set; }

            public IQueryable<NodeCollection> OrphanedNodeCollections { get; set; }

            public IQueryable<Network> OrphanedNetworks { get; set; }

            public IQueryable<Analysis> OrphanedAnalyses { get; set; }
        }

        public IActionResult OnGet()
        {
            // Define the view.
            View = new ViewModel
            {
                UserCount = _context.Users.Count(),
                RoleCount = _context.Roles.Count(),
                DatabaseCount = _context.Databases.Count(),
                NodeCount = _context.Nodes.Count(),
                EdgeCount = _context.Edges.Count(),
                NodeCollectionCount = _context.NodeCollections.Count(),
                DuplicateDatabaseTypes = _context.DatabaseTypes.GroupBy(item => item.Name).Where(item => item.Count() > 1).Select(item => item.Key),
                DuplicateDatabases = _context.Databases.GroupBy(item => item.Name).Where(item => item.Count() > 1).Select(item => item.Key),
                DuplicateDatabaseNodeFields = _context.DatabaseNodeFields.GroupBy(item => item.Name).Where(item => item.Count() > 1).Select(item => item.Key),
                DuplicateDatabaseEdgeFields = _context.DatabaseEdgeFields.GroupBy(item => item.Name).Where(item => item.Count() > 1).Select(item => item.Key),
                DuplicateDatabaseNodeFieldNodes = _context.DatabaseNodeFieldNodes.Where(item => item.DatabaseNodeField.IsSearchable).GroupBy(item => item.Value).Where(item => item.Count() > 1).Select(item => item.Key),
                DuplicateNodes = _context.Nodes.GroupBy(item => item.Name).Where(item => item.Count() > 1).Select(item => item.Key),
                DuplicateEdges = _context.Edges.GroupBy(item => item.Name).Where(item => item.Count() > 1).Select(item => item.Key),
                DuplicateNodeCollections = _context.NodeCollections.GroupBy(item => item.Name).Where(item => item.Count() > 1).Select(item => item.Key),
                OrphanedNodes = _context.Nodes.Where(item => !item.DatabaseNodeFieldNodes.Any()),
                OrphanedEdges = _context.Edges.Where(item => !item.DatabaseEdges.Any() || item.EdgeNodes.Count() < 2),
                OrphanedNodeCollections = _context.NodeCollections.Where(item => !item.NodeCollectionNodes.Any()),
                OrphanedNetworks = _context.Networks.Where(item => !item.NetworkDatabases.Any() || !item.NetworkNodes.Any() || !item.NetworkEdges.Any() || !item.NetworkUsers.Any()),
                OrphanedAnalyses = _context.Analyses.Where(item => !item.AnalysisDatabases.Any() || !item.AnalysisNodes.Any() || !item.AnalysisEdges.Any() || !item.AnalysisNetworks.Any() || !item.AnalysisUsers.Any())
            };
            // Check if there were any issues detected.
            View.IssueDetected = View.DuplicateDatabaseTypes.Any() || View.DuplicateDatabases.Any() || View.DuplicateDatabaseNodeFields.Any() || View.DuplicateDatabaseEdgeFields.Any() || View.DuplicateDatabaseNodeFieldNodes.Any() || View.DuplicateNodes.Any() || View.DuplicateEdges.Any() || View.DuplicateNodeCollections.Any() || View.OrphanedNodes.Any() || View.OrphanedEdges.Any() || View.OrphanedNodeCollections.Any() || View.OrphanedNetworks.Any() || View.OrphanedAnalyses.Any();
            // Return the page.
            return Page();
        }

        public IActionResult OnPostHangfire()
        {
            // Redirect to the Hangfire dashboard.
            return LocalRedirect("/Hangfire");
        }

        public async Task<IActionResult> OnPostDownloadAsync()
        {
            // Get the data to be written to the archive.
            var data = new
            {
                DatabaseTypes = _context.DatabaseTypes.Select(item => new
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
                }),
                Databases = _context.Databases.Select(item => new
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
                }),
                DatabaseNodeFields = _context.DatabaseNodeFields.Select(item => new
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
                }),
                DatabaseEdgeFields = _context.DatabaseEdgeFields.Select(item => new
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
                }),
                Nodes = _context.Nodes.Select(item => new
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
                }),
                Edges = _context.Edges.Select(item => new
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
                }),
                NodeCollections = _context.NodeCollections.Select(item => new
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
                })
            };
            // Define the JSON serializer options for all of the returned files.
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            // Define the stream of the file to return.
            var zipStream = new MemoryStream();
            // Define a new ZIP archive.
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                // Go over each of the items in the data to download.
                foreach (var property in data.GetType().GetProperties())
                {
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"NetControl4BioMed-{property.Name}.json", CompressionLevel.Fastest).Open();
                    // Write the data to the stream corresponding to the file.
                    await JsonSerializer.SerializeAsync(stream, property.GetValue(data), jsonSerializerOptions);
                }
            }
            // Reset the stream position.
            zipStream.Position = 0;
            // Return the archive file.
            return new FileStreamResult(zipStream, MediaTypeNames.Application.Zip) { FileDownloadName = "NetControl4BioMed-Data.zip" };
            //return File(zipStream.ToArray(), MediaTypeNames.Application.Zip, "NetControl4BioMed-Data.zip");
        }
    }
}
