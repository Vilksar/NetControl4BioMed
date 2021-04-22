using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Tasks;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Extensions
{
    /// <summary>
    /// Provides extensions for the control paths.
    /// </summary>
    public static class ControlPathExtensions
    {
        /// <summary>
        /// Represents the maximum size for which visualization will be enabled.
        /// </summary>
        public readonly static int MaximumSizeForVisualization = 500;

        /// <summary>
        /// Deletes the dependent paths of the corresponding control paths.
        /// </summary>
        /// <param name="controlPathIds">The control paths whose entities should be deleted.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public static async Task DeleteDependentAnalysesAsync(IEnumerable<string> controlPathIds, IServiceProvider serviceProvider, CancellationToken token)
        {
            // Define a variable to store the total number of entities.
            var entityCount = 0;
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items in the current batch.
                entityCount = context.Paths
                    .Where(item => controlPathIds.Contains(item.ControlPath.Id))
                    .Count();
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)entityCount / ApplicationDbContext.BatchSize);
            // Go over each batch.
            for (int index = 0; index < count; index++)
            {
                // Check if the cancellation was requested.
                if (token.IsCancellationRequested)
                {
                    // Break.
                    break;
                }
                // Define the batch items.
                var batchItemInputs = new List<PathInputModel>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items in the current batch.
                    batchItemInputs = context.Paths
                        .Where(item => controlPathIds.Contains(item.ControlPath.Id))
                        .Select(item => new PathInputModel
                        {
                            Id = item.Id
                        })
                        .Take(ApplicationDbContext.BatchSize)
                        .ToList();
                    // Check if there were no items found.
                    if (batchItemInputs == null || !batchItemInputs.Any())
                    {
                        // Continue.
                        continue;
                    }
                }
                // Delete the items.
                await new PathsTask { Items = batchItemInputs }.DeleteAsync(serviceProvider, token);
            }
        }

        /// <summary>
        /// Gets the Cytoscape view model corresponding to the provided control path.
        /// </summary>
        /// <param name="controlPath">The current control path.</param>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="linkGenerator">The link generator.</param>
        /// <param name="context">The application database context.</param>
        /// <returns>Returns the Cytoscape view model corresponding to the provided control path.</returns>
        public static CytoscapeViewModel GetCytoscapeViewModel(this ControlPath controlPath, HttpContext httpContext, LinkGenerator linkGenerator, ApplicationDbContext context)
        {
            // Get the required values.
            var databaseTypeName = context.ControlPaths
                .Where(item => item == controlPath)
                .Select(item => item.Analysis.AnalysisDatabases)
                .SelectMany(item => item)
                .Select(item => item.Database.DatabaseType.Name)
                .First();
            // Get the control data.
            var analysis = context.ControlPaths
                .Where(item => item == controlPath)
                .Select(item => item.Analysis)
                .FirstOrDefault();
            var controlNodes = context.ControlPaths
                .Where(item => item == controlPath)
                .Select(item => item.Paths)
                .SelectMany(item => item)
                .Select(item => item.PathNodes)
                .SelectMany(item => item)
                .Where(item => item.Type == PathNodeType.Source)
                .Select(item => item.Node.Id)
                .Where(item => !string.IsNullOrEmpty(item))
                .ToHashSet();
            var controlEdges = context.ControlPaths
                .Where(item => item == controlPath)
                .Select(item => item.Paths)
                .SelectMany(item => item)
                .Select(item => item.PathEdges)
                .SelectMany(item => item)
                .Select(item => item.Edge.Id)
                .Where(item => !string.IsNullOrEmpty(item))
                .ToHashSet();
            // Return the view model.
            return new CytoscapeViewModel
            {
                Elements = new CytoscapeViewModel.CytoscapeElements
                {
                    Nodes = context.PathNodes
                        .Where(item => item.Path.ControlPath == controlPath)
                        .Where(item => item.Type == PathNodeType.None)
                        .Select(item => item.Node)
                        .Select(item => new
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Classes = item.AnalysisNodes
                                .Where(item1 => item1.Analysis == analysis)
                                .Select(item1 => item1.Type.ToString().ToLower())
                        })
                        .AsEnumerable()
                        .Select(item => new CytoscapeViewModel.CytoscapeElements.CytoscapeNode
                        {
                            Data = new CytoscapeViewModel.CytoscapeElements.CytoscapeNode.CytoscapeNodeData
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Href = linkGenerator.GetUriByPage(httpContext, $"/Content/DatabaseTypes/{databaseTypeName}/Data/Nodes/Details", handler: null, values: new { id = item.Id })
                            },
                            Classes = item.Classes.Concat(controlNodes.Contains(item.Id) ? new List<string> { "control" } : new List<string> { })
                        }),
                    Edges = context.PathEdges
                        .Where(item => item.Path.ControlPath == controlPath)
                        .Select(item => item.Edge)
                        .Select(item => new
                        {
                            Id = item.Id,
                            Name = item.Name,
                            SourceNodeId = item.EdgeNodes
                                .Where(item1 => item1.Type == EdgeNodeType.Source)
                                .Select(item1 => item1.Node)
                                .Where(item1 => item1 != null)
                                .Select(item1 => item1.Id)
                                .FirstOrDefault(),
                            TargetNodeId = item.EdgeNodes
                                .Where(item1 => item1.Type == EdgeNodeType.Target)
                                .Select(item1 => item1.Node)
                                .Where(item1 => item1 != null)
                                .Select(item1 => item1.Id)
                                .FirstOrDefault()
                        })
                        .AsEnumerable()
                        .Select(item => new CytoscapeViewModel.CytoscapeElements.CytoscapeEdge
                        {
                            Data = new CytoscapeViewModel.CytoscapeElements.CytoscapeEdge.CytoscapeEdgeData
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Href = linkGenerator.GetUriByPage(httpContext, $"/Content/DatabaseTypes/{databaseTypeName}/Data/Edges/Details", handler: null, values: new { id = item.Id }),
                                Source = item.SourceNodeId,
                                Target = item.TargetNodeId
                            },
                            Classes = controlEdges.Contains(item.Id) ? new List<string> { "control" } : new List<string> { }
                })
                },
                Layout = CytoscapeViewModel.DefaultLayout,
                Styles = CytoscapeViewModel.DefaultStyles.Concat(CytoscapeViewModel.DefaultAnalysisStyles).Concat(CytoscapeViewModel.DefaultControlPathStyles)
            };
        }

        /// <summary>
        /// Gets the content of an overview text file to download corresponding to the provided control paths.
        /// </summary>
        /// <param name="controlPathIds">The IDs of the current control paths.</param>
        /// <param name="stream">The stream to which to write to.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="scheme">The HTTP context scheme.</param>
        /// <param name="host">The HTTP context host.</param>
        /// <returns></returns>
        public static async Task WriteToStreamOverviewTextFileContent(IEnumerable<string> controlPathIds, Stream stream, IServiceProvider serviceProvider, string scheme, HostString host)
        {
            // Use a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Use a new link generator instance.
            var linkGenerator = scope.ServiceProvider.GetRequiredService<LinkGenerator>();
            // Define the stream writer for the file.
            using var streamWriter = new StreamWriter(stream);
            // Get the required data.
            var data = string.Concat("The following control paths have been downloaded:\n\n", string.Join("\n", context.ControlPaths
                .Where(item => controlPathIds.Contains(item.Id))
                .Select(item => new
                {
                    Id = item.Id,
                    AnalysisName = item.Analysis.Name,
                    DatabaseTypeName = item.Analysis.AnalysisDatabases
                        .Select(item1 => item1.Database.DatabaseType.Name)
                        .FirstOrDefault()
                })
                .AsEnumerable()
                .Select(item => $"{item.AnalysisName} - {linkGenerator.GetUriByPage($"/Content/DatabaseTypes/{item.DatabaseTypeName}/Created/Analyses/Details/Created/ControlPaths/Details/Index", handler: null, values: new { id = item.Id }, scheme: scheme, host: host)}")));
            // Write the data to the stream.
            await streamWriter.WriteAsync(data);
        }

        /// <summary>
        /// Gets the content of a text file to download corresponding to the provided control path.
        /// </summary>
        /// <param name="controlPath">The current control path.</param>
        /// <param name="stream">The stream to which to write to.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <returns>The content of the text file corresponding to the provided control path.</returns>
        public static async Task WriteToStreamTxtFileContent(this ControlPath controlPath, Stream stream, IServiceProvider serviceProvider)
        {
            // Use a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Define the stream writer for the file.
            using var streamWriter = new StreamWriter(stream);
            // Get the data for the file.
            var data = string.Join("\n", context.ControlPaths
                .Where(item => item == controlPath)
                .Select(item => item.Paths)
                .SelectMany(item => item)
                .Select(item => item.PathNodes
                    .Where(item1 => item1.Type == PathNodeType.None)
                    .OrderBy(item1 => item1.Index)
                    .Select(item1 => item1.Node.Name))
                .Select(item => string.Join(";", item)));
            // Write the data to the stream.
            await streamWriter.WriteAsync(data);
        }

        /// <summary>
        /// Gets the content of a SIF file to download corresponding to the provided control path.
        /// </summary>
        /// <param name="controlPath">The current control path.</param>
        /// <param name="stream">The stream to which to write to.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <returns>The content of the SIF file corresponding to the provided control path.</returns>
        public static async Task WriteToStreamSifFileContent(this ControlPath controlPath, Stream stream, IServiceProvider serviceProvider)
        {
            // Use a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Define the stream writer for the file.
            using var streamWriter = new StreamWriter(stream);
            // Get the data for the file.
            var data = string.Join("\n", context.Paths
                .Where(item => item.ControlPath == controlPath)
                .Select(item => item.PathEdges)
                .SelectMany(item => item)
                .Select(item => item.Edge)
                .Select(item => new
                {
                    SourceNodeName = item.EdgeNodes
                        .Where(item1 => item1.Type == EdgeNodeType.Source)
                        .Select(item1 => item1.Node)
                        .Where(item1 => item1 != null)
                        .Select(item1 => item1.Name)
                        .FirstOrDefault(),
                    TargetNodeName = item.EdgeNodes
                        .Where(item1 => item1.Type == EdgeNodeType.Target)
                        .Select(item1 => item1.Node)
                        .Where(item1 => item1 != null)
                        .Select(item1 => item1.Name)
                        .FirstOrDefault()
                })
                .Where(item => !string.IsNullOrEmpty(item.SourceNodeName) && !string.IsNullOrEmpty(item.TargetNodeName))
                .Select(item => $"{item.SourceNodeName}\tinteracts with\t{item.TargetNodeName}"));
            // Write the data to the stream.
            await streamWriter.WriteAsync(data);
        }

        /// <summary>
        /// Gets the content of a JSON file to download corresponding to the provided control path.
        /// </summary>
        /// <param name="controlPath">The current control path.</param>
        /// <param name="stream">The stream to which to write to.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <returns>The content of the JSON file corresponding to the provided control path.</returns>
        public static async Task WriteToStreamJsonFileContent(this ControlPath controlPath, Stream stream, IServiceProvider serviceProvider)
        {
            // Use a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Get the required data.
            var data = new
            {
                Id = controlPath.Id,
                Analysis = context.Analyses
                    .Where(item => item.ControlPaths.Any(item1 => item1 == controlPath))
                    .Select(item => new
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Description = item.Description,
                        Algorithm = item.Algorithm.ToString()
                    })
                    .FirstOrDefault(),
                UniqueControlNodes = context.Paths
                    .Where(item1 => item1.ControlPath == controlPath)
                    .Select(item1 => item1.PathNodes)
                    .SelectMany(item1 => item1)
                    .Where(item1 => item1.Type == PathNodeType.Source)
                    .Select(item1 => item1.Node)
                    .AsEnumerable()
                    .GroupBy(item1 => item1)
                    .Select(item1 => new
                    {
                        Id = item1.Key.Id,
                        Name = item1.Key.Name,
                        Count = item1.Count()
                    }),
                Paths = context.ControlPaths
                    .Where(item => item == controlPath)
                    .Select(item => item.Paths)
                    .SelectMany(item => item)
                    .Select(item => item.PathEdges
                        .OrderBy(item1 => item1.Index)
                        .Select(item1 => item1.Edge)
                        .Select(item1 => new
                        {
                            SourceNode = item1.EdgeNodes
                                .Where(item2 => item2.Type == EdgeNodeType.Source)
                                .Select(item2 => item2.Node)
                                .Where(item2 => item2 != null)
                                .FirstOrDefault(),
                            TargetNode = item1.EdgeNodes
                                .Where(item2 => item2.Type == EdgeNodeType.Target)
                                .Select(item2 => item2.Node)
                                .Where(item2 => item2 != null)
                                .FirstOrDefault()
                        })
                        .Where(item1 => item1.SourceNode != null && item1.TargetNode != null)
                        .AsEnumerable()
                        .Select(item1 => new
                        {
                            SourceNode = new
                            {
                                Id = item1.SourceNode.Id,
                                Name = item1.SourceNode.Name
                            },
                            TargetNode = new
                            {
                                Id = item1.TargetNode.Id,
                                Name = item1.TargetNode.Name
                            }
                        }))
            };
            // Write the data corresponding to the file.
            await JsonSerializer.SerializeAsync(stream, data, new JsonSerializerOptions { IgnoreNullValues = true });
        }

        /// <summary>
        /// Gets the content of a JSON file to download corresponding to the provided control path.
        /// </summary>
        /// <param name="controlPath">The current control path.</param>
        /// <param name="stream">The stream to which to write to.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <returns>The content of the JSON file corresponding to the provided control path.</returns>
        public static async Task WriteToStreamCyjsFileContent(this ControlPath controlPath, Stream stream, IServiceProvider serviceProvider)
        {
            // Use a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Get the control data.
            var analysis = context.ControlPaths
                .Where(item => item == controlPath)
                .Select(item => item.Analysis)
                .First();
            var controlNodes = context.ControlPaths
                .Where(item => item == controlPath)
                .Select(item => item.Paths)
                .SelectMany(item => item)
                .Select(item => item.PathNodes)
                .SelectMany(item => item)
                .Where(item => item.Type == PathNodeType.Source)
                .Select(item => item.Node.Id)
                .Where(item => !string.IsNullOrEmpty(item))
                .ToHashSet();
            var controlEdges = context.ControlPaths
                .Where(item => item == controlPath)
                .Select(item => item.Paths)
                .SelectMany(item => item)
                .Select(item => item.PathEdges)
                .SelectMany(item => item)
                .Select(item => item.Edge.Id)
                .Where(item => !string.IsNullOrEmpty(item))
                .ToHashSet();
            // Return the view model.
            var data = new CytoscapeViewModel
            {
                Data = new CytoscapeViewModel.CytoscapeData
                {
                    Id = controlPath.Id,
                    Name = $"{controlPath.Analysis.Name} - Control Path",
                    Description = controlPath.Analysis.Description
                },
                Elements = new CytoscapeViewModel.CytoscapeElements
                {
                    Nodes = context.PathNodes
                        .Where(item => item.Path.ControlPath == controlPath)
                        .Where(item => item.Type == PathNodeType.None)
                        .Select(item => item.Node)
                        .Select(item => new
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Classes = item.AnalysisNodes
                                .Where(item1 => item1.Analysis == analysis)
                                .Select(item1 => item1.Type.ToString().ToLower())
                        })
                        .AsEnumerable()
                        .Select(item => new CytoscapeViewModel.CytoscapeElements.CytoscapeNode
                        {
                            Data = new CytoscapeViewModel.CytoscapeElements.CytoscapeNode.CytoscapeNodeData
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Type = string.Join(",", item.Classes.Concat(controlNodes.Contains(item.Id) ? new List<string> { "control" } : new List<string> { }).OrderBy(item => item))
                            }
                        }),
                    Edges = context.PathEdges
                        .Where(item => item.Path.ControlPath == controlPath)
                        .Select(item => item.Edge)
                        .Select(item => new
                        {
                            Id = item.Id,
                            Name = item.Name,
                            SourceNodeId = item.EdgeNodes
                                .Where(item1 => item1.Type == EdgeNodeType.Source)
                                .Select(item1 => item1.Node)
                                .Where(item1 => item1 != null)
                                .Select(item1 => item1.Id)
                                .FirstOrDefault(),
                            TargetNodeId = item.EdgeNodes
                                .Where(item1 => item1.Type == EdgeNodeType.Target)
                                .Select(item1 => item1.Node)
                                .Where(item1 => item1 != null)
                                .Select(item1 => item1.Id)
                                .FirstOrDefault()
                        })
                        .AsEnumerable()
                        .Select(item => new CytoscapeViewModel.CytoscapeElements.CytoscapeEdge
                        {
                            Data = new CytoscapeViewModel.CytoscapeElements.CytoscapeEdge.CytoscapeEdgeData
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Source = item.SourceNodeId,
                                Target = item.TargetNodeId,
                                Type = string.Join(",", controlEdges.Contains(item.Id) ? new List<string> { "control" } : new List<string> { })
                            }
                        })
                    }
            };
            // Write the data corresponding to the file.
            await JsonSerializer.SerializeAsync(stream, data, new JsonSerializerOptions { IgnoreNullValues = true });
        }

        /// <summary>
        /// Gets the content of an Excel file to download corresponding to the provided control path.
        /// </summary>
        /// <param name="controlPath">The current control path.</param>
        /// <param name="stream">The stream to which to write to.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <returns>The content of the Excel file corresponding to the provided control path.</returns>
        public static async Task WriteToStreamXlsxFileContent(this ControlPath controlPath, Stream stream, IServiceProvider serviceProvider)
        {
            // Use a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Get the required data.
            var analysis = context.ControlPaths
                .Where(item => item == controlPath)
                .Select(item => item.Analysis)
                .FirstOrDefault();
            var databaseTypeName = context.AnalysisDatabases
                .Where(item => item.Analysis == analysis)
                .Select(item => item.Database.DatabaseType.Name)
                .FirstOrDefault();
            var databaseNodeFields = context.AnalysisDatabases
                .Where(item => item.Analysis == analysis)
                .Where(item => item.Type == AnalysisDatabaseType.Node)
                .Select(item => item.Database.DatabaseNodeFields)
                .SelectMany(item => item)
                .ToList();
            var databaseEdgeFields = context.AnalysisDatabases
                .Where(item => item.Analysis == analysis)
                .Where(item => item.Type == AnalysisDatabaseType.Edge)
                .Select(item => item.Database.DatabaseEdgeFields)
                .SelectMany(item => item)
                .ToList();
            // Get the required variables.
            var nodeTitle = databaseTypeName == "PPI" ? "Protein" : "Node";
            var edgeTitle = databaseTypeName == "PPI" ? "Interaction" : "Edge";
            // Define the rows in the first sheet.
            var worksheet1Rows = new List<List<string>>
            {
                new List<string>
                {
                    "Internal ID",
                    controlPath.Id
                },
                new List<string>
                {
                    "Analysis ID",
                    analysis?.Id ?? string.Empty
                },
                new List<string>
                {
                    "Analysis name",
                    analysis?.Name ?? string.Empty
                },
                new List<string>
                {
                    "Analysis description",
                    analysis?.Description ?? string.Empty
                },
                new List<string>
                {
                    "Analysis algorithm",
                    analysis?.Algorithm.ToString() ?? string.Empty
                }
            };
            // Define the rows in the second sheet.
            var worksheet2Rows = new List<List<string>>
            {
                new List<string>
                {
                    "Internal ID",
                    "Name",
                    "Type"
                }
            }.Concat(context.AnalysisDatabases
                .Where(item => item.Analysis == analysis)
                .Select(item => new
                {
                    DatabaseId = item.Database.Id,
                    DatabaseName = item.Database.Name,
                    Type = item.Type
                })
                .AsEnumerable()
                .Select(item => new List<string>
                {
                    item.DatabaseId,
                    item.DatabaseName,
                    item.Type == AnalysisDatabaseType.Node ? nodeTitle : item.Type == AnalysisDatabaseType.Edge ? edgeTitle : string.Empty
                })
                .ToList())
            .ToList();
            // Define the rows in the third sheet.
            var worksheet3Rows = new List<List<string>>
            {
                new List<string>
                {
                    "Internal ID",
                    "Name",
                    $"{nodeTitle}s"
                }.Concat(databaseNodeFields
                    .Select(item => item.Name)
                    .ToList())
                .ToList()
            }.Concat(context.PathNodes
                .Where(item => item.Path.ControlPath == controlPath)
                .Where(item => item.Type == PathNodeType.None)
                .Select(item => item.Node)
                .Select(item => new
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Values = item.DatabaseNodeFieldNodes
                        .Select(item1 => new
                        {
                            DatabaseNodeFieldId = item1.DatabaseNodeField.Id,
                            Value = item1.Value
                        }),
                    Types = item.PathNodes
                        .Where(item1 => item1.Path.ControlPath == controlPath)
                        .Select(item => item.Type.ToString().ToLower())
                })
                .AsEnumerable()
                .Select(item => new List<string>
                    {
                        item.Id,
                        item.Name,
                        string.Join(",", item.Types)
                    }.Concat(databaseNodeFields
                        .Select(item1 => item.Values.FirstOrDefault(item2 => item2.DatabaseNodeFieldId == item1.Id))
                        .Select(item1 => item1 == null ? string.Empty : item1.Value)
                        .ToList())))
            .ToList();
            // Define the rows in the fourth sheet.
            var worksheet4Rows = new List<List<string>>
            {
                new List<string>
                {
                    "Internal ID",
                    $"Source {nodeTitle.ToLower()} ID",
                    $"Source {nodeTitle.ToLower()} name",
                    $"Target {nodeTitle.ToLower()} ID",
                    $"Target {nodeTitle.ToLower()} name"
                }.Concat(databaseEdgeFields
                    .Select(item => item.Name)
                    .ToList())
                .ToList()
            }.Concat(context.PathEdges
                .Where(item => item.Path.ControlPath == controlPath)
                .Select(item => item.Edge)
                .Select(item => new
                {
                    Id = item.Id,
                    SourceNode = item.EdgeNodes
                        .Where(item1 => item1.Type == EdgeNodeType.Source)
                        .Select(item1 => item1.Node)
                        .FirstOrDefault(),
                    TargetNode = item.EdgeNodes
                        .Where(item1 => item1.Type == EdgeNodeType.Target)
                        .Select(item1 => item1.Node)
                        .FirstOrDefault(),
                    Values = item.DatabaseEdgeFieldEdges
                        .Select(item1 => new
                        {
                            DatabaseEdgeFieldId = item1.DatabaseEdgeField.Id,
                            Value = item1.Value
                        })
                })
                .Where(item => item.SourceNode != null && item.TargetNode != null)
                .AsEnumerable()
                .Select(item => new List<string>
                {
                    item.Id,
                    item.SourceNode.Id,
                    item.SourceNode.Name,
                    item.TargetNode.Id,
                    item.TargetNode.Name
                }.Concat(databaseEdgeFields
                    .Select(item1 => item.Values.FirstOrDefault(item2 => item2.DatabaseEdgeFieldId == item1.Id))
                    .Select(item1 => item1 == null ? string.Empty : item1.Value)
                    .ToList())))
            .ToList();
            // Define the rows in the fifth sheet.
            var worksheet5Rows = new List<List<string>>
            {
                new List<string>
                {
                    "Internal ID",
                    "Length",
                    $"Source {nodeTitle.ToLower()} name",
                    $"Target {nodeTitle.ToLower()} name",
                    $"{nodeTitle}s"
                }
            }.Concat(context.Paths
                .Where(item => item.ControlPath == controlPath)
                .Select(item => new
                {
                    Id = item.Id,
                    Length = item.PathEdges.Count(),
                    SourceNodeName = item.PathNodes
                        .Where(item1 => item1.Type == PathNodeType.Source)
                        .Select(item1 => item1.Node.Name)
                        .FirstOrDefault(),
                    TargetNodeName = item.PathNodes
                        .Where(item1 => item1.Type == PathNodeType.Target)
                        .Select(item1 => item1.Node.Name)
                        .FirstOrDefault(),
                    Nodes = item.PathNodes
                        .Where(item1 => item1.Type == PathNodeType.None)
                        .OrderBy(item1 => item1.Index)
                        .Select(item1 => item1.Node.Name)
                })
                .Where(item => !string.IsNullOrEmpty(item.SourceNodeName) && !string.IsNullOrEmpty(item.TargetNodeName))
                .AsEnumerable()
                .Select(item => new List<string>
                {
                    item.Id,
                    item.Length.ToString(),
                    item.SourceNodeName,
                    item.TargetNodeName,
                    string.Join(",", item.Nodes)
                })
                .ToList())
            .ToList();
            // Define the stream for the file.
            var fileStream = new MemoryStream();
            // Define the Excel file.
            using SpreadsheetDocument document = SpreadsheetDocument.Create(fileStream, SpreadsheetDocumentType.Workbook);
            // Definte a new workbook part.
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();
            var worksheets = workbookPart.Workbook.AppendChild(new Sheets());
            // Define the first worksheet.
            var worksheet1Part = workbookPart.AddNewPart<WorksheetPart>();
            var worksheet1Data = new SheetData();
            var worksheet1 = new Sheet { Id = workbookPart.GetIdOfPart(worksheet1Part), SheetId = 1, Name = "Details" };
            worksheet1Part.Worksheet = new Worksheet(worksheet1Data);
            worksheet1Data.Append(worksheet1Rows.Select(item => new Row(item.Select(item1 => new Cell { DataType = CellValues.String, CellValue = new CellValue(item1) }))));
            worksheets.Append(worksheet1);
            // Define the second worksheet.
            var worksheet2Part = workbookPart.AddNewPart<WorksheetPart>();
            var worksheet2Data = new SheetData();
            var worksheet2 = new Sheet { Id = workbookPart.GetIdOfPart(worksheet2Part), SheetId = 2, Name = "Databases" };
            worksheet2Part.Worksheet = new Worksheet(worksheet2Data);
            worksheet2Data.Append(worksheet2Rows.Select(item => new Row(item.Select(item1 => new Cell { DataType = CellValues.String, CellValue = new CellValue(item1) }))));
            worksheets.Append(worksheet2);
            // Define the third worksheet.
            var worksheet3Part = workbookPart.AddNewPart<WorksheetPart>();
            var worksheet3Data = new SheetData();
            var worksheet3 = new Sheet { Id = workbookPart.GetIdOfPart(worksheet3Part), SheetId = 3, Name = $"{nodeTitle}s" };
            worksheet3Part.Worksheet = new Worksheet(worksheet3Data);
            worksheet3Data.Append(worksheet3Rows.Select(item => new Row(item.Select(item1 => new Cell { DataType = CellValues.String, CellValue = new CellValue(item1) }))));
            worksheets.Append(worksheet3);
            // Define the fourth worksheet.
            var worksheet4Part = workbookPart.AddNewPart<WorksheetPart>();
            var worksheet4Data = new SheetData();
            var worksheet4 = new Sheet { Id = workbookPart.GetIdOfPart(worksheet4Part), SheetId = 4, Name = $"{edgeTitle}s" };
            worksheet4Part.Worksheet = new Worksheet(worksheet4Data);
            worksheet4Data.Append(worksheet4Rows.Select(item => new Row(item.Select(item1 => new Cell { DataType = CellValues.String, CellValue = new CellValue(item1) }))));
            worksheets.Append(worksheet4);
            // Define the fifth worksheet.
            var worksheet5Part = workbookPart.AddNewPart<WorksheetPart>();
            var worksheet5Data = new SheetData();
            var worksheet5 = new Sheet { Id = workbookPart.GetIdOfPart(worksheet5Part), SheetId = 5, Name = $"Paths" };
            worksheet5Part.Worksheet = new Worksheet(worksheet5Data);
            worksheet5Data.Append(worksheet5Rows.Select(item => new Row(item.Select(item1 => new Cell { DataType = CellValues.String, CellValue = new CellValue(item1) }))));
            worksheets.Append(worksheet5);
            // Close the document.
            document.Close();
            // Reset the stream position.
            fileStream.Position = 0;
            // Copy it to the archive stream.
            await fileStream.CopyToAsync(stream);
        }
    }
}
