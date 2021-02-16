using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Interfaces;
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
    /// Provides extensions for the analyses.
    /// </summary>
    public static class AnalysisExtensions
    {
        /// <summary>
        /// Represents the maximum size for which visualization will be enabled.
        /// </summary>
        public readonly static int MaximumSizeForVisualization = 500;

        /// <summary>
        /// Gets the log of the analysis.
        /// </summary>
        /// <param name="analysis">The current analysis.</param>
        /// <returns>The log of the analysis.</returns>
        public static List<LogEntryViewModel> GetLog(this Analysis analysis)
        {
            // Return the list of log entries.
            return JsonSerializer.Deserialize<List<LogEntryViewModel>>(analysis.Log);
        }

        /// <summary>
        /// Appends to the list a new entry to the log of the analysis and returns the updated list.
        /// </summary>
        /// <param name="analysis">The current analysis.</param>
        /// <param name="message">The message to add as a new entry to the analysis log.</param>
        /// <returns>The updated log of the analysis.</returns>
        public static string AppendToLog(this Analysis analysis, string message)
        {
            // Return the log entries with the message appended.
            return JsonSerializer.Serialize(analysis.GetLog().Append(new LogEntryViewModel { DateTime = DateTime.UtcNow, Message = message }));
        }

        /// <summary>
        /// Deletes the related entities of the corresponding analyses.
        /// </summary>
        /// <typeparam name="T">The type of the related entities.</typeparam>
        /// <param name="itemIds">The IDs of the items whose entities should be deleted.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public static async Task DeleteRelatedEntitiesAsync<T>(IEnumerable<string> itemIds, IServiceProvider serviceProvider, CancellationToken token) where T : class, IAnalysisDependent
        {
            // Define a variable to store the total number of entities.
            var entityCount = 0;
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the corresponding database set.
                var set = context.Set<T>();
                // Get the items in the current batch.
                entityCount = set
                    .Where(item => itemIds.Contains(item.Analysis.Id))
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
                var batchItems = new List<T>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the corresponding database set.
                    var set = context.Set<T>();
                    // Get the items in the current batch.
                    batchItems = set
                        .Where(item => itemIds.Contains(item.Analysis.Id))
                        .Take(ApplicationDbContext.BatchSize)
                        .ToList();
                    // Check if there were no items found.
                    if (batchItems == null || !batchItems.Any())
                    {
                        // Continue.
                        continue;
                    }
                }
                // Delete the items.
                await IEnumerableExtensions.DeleteAsync(batchItems, serviceProvider, token);
            }
        }

        /// <summary>
        /// Deletes the dependent control paths of the corresponding analyses.
        /// </summary>
        /// <param name="analysisIds">The analyses whose entities should be deleted.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public static async Task DeleteDependentControlPathsAsync(IEnumerable<string> analysisIds, IServiceProvider serviceProvider, CancellationToken token)
        {
            // Define a variable to store the total number of entities.
            var entityCount = 0;
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items in the current batch.
                entityCount = context.ControlPaths
                    .Where(item => analysisIds.Contains(item.Analysis.Id))
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
                var batchItemInputs = new List<ControlPathInputModel>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items in the current batch.
                    batchItemInputs = context.ControlPaths
                        .Where(item => analysisIds.Contains(item.Analysis.Id))
                        .Select(item => new ControlPathInputModel
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
                await new ControlPathsTask { Items = batchItemInputs }.DeleteAsync(serviceProvider, token);
            }
        }

        /// <summary>
        /// Gets the Cytoscape view model corresponding to the provided analysis.
        /// </summary>
        /// <param name="analysis">The current analysis.</param>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="linkGenerator">The link generator.</param>
        /// <param name="context">The application database context.</param>
        /// <returns>The Cytoscape view model corresponding to the provided network.</returns>
        public static CytoscapeViewModel GetCytoscapeViewModel(this Analysis analysis, HttpContext httpContext, LinkGenerator linkGenerator, ApplicationDbContext context)
        {
            // Get the required values.
            var databaseTypeName = context.AnalysisDatabases
                .Where(item => item.Analysis == analysis)
                .Select(item => item.Database.DatabaseType.Name)
                .FirstOrDefault();
            // Return the view model.
            return new CytoscapeViewModel
            {
                Elements = new CytoscapeViewModel.CytoscapeElements
                {
                    Nodes = context.AnalysisNodes
                        .Where(item => item.Analysis == analysis)
                        .Where(item => item.Type == AnalysisNodeType.None)
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
                            Classes = item.Classes
                        }),
                    Edges = context.AnalysisEdges
                        .Where(item => item.Analysis == analysis)
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
                                Target = item.TargetNodeId,
                                Interaction = databaseTypeName.ToLower()
                            }
                        })
                },
                Layout = CytoscapeViewModel.DefaultLayout,
                Styles = CytoscapeViewModel.DefaultStyles.Concat(CytoscapeViewModel.DefaultAnalysisStyles)
            };
        }

        /// <summary>
        /// Gets the content of a text file to download corresponding to the provided analysis.
        /// </summary>
        /// <param name="analysis">The current analysis.</param>
        /// <param name="stream">The stream to which to write to.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <returns>The content of the text file corresponding to the provided analysis.</returns>
        public static async Task WriteToStreamTxtFileContent(this Analysis analysis, Stream stream, IServiceProvider serviceProvider)
        {
            // Use a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Define the stream writer for the file.
            using var streamWriter = new StreamWriter(stream);
            // Get the data for the file.
            var data = string.Join("\n", context.AnalysisEdges
                .Where(item => item.Analysis == analysis)
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
                .Select(item => $"{item.SourceNodeName};{item.TargetNodeName}"));
            // Write the data to the stream.
            await streamWriter.WriteAsync(data);
        }

        /// <summary>
        /// Gets the content of a SIF file to download corresponding to the provided analysis.
        /// </summary>
        /// <param name="analysis">The current analysis.</param>
        /// <param name="stream">The stream to which to write to.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <returns>The content of the SIF file corresponding to the provided analysis.</returns>
        public static async Task WriteToStreamSifFileContent(this Analysis analysis, Stream stream, IServiceProvider serviceProvider)
        {
            // Use a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Define the stream writer for the file.
            using var streamWriter = new StreamWriter(stream);
            // Get the required values.
            var databaseTypeName = context.AnalysisDatabases
                .Where(item => item.Analysis == analysis)
                .Select(item => item.Database.DatabaseType.Name)
                .First()
                .ToLower();
            // Get the data for the file.
            var data = string.Join("\n", context.AnalysisEdges
                .Where(item => item.Analysis == analysis)
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
                .Select(item => $"{item.SourceNodeName}\t{databaseTypeName}\t{item.TargetNodeName}"));
            // Write the data to the stream.
            await streamWriter.WriteAsync(data);
        }

        /// <summary>
        /// Gets the content of a JSON file to download corresponding to the provided analysis.
        /// </summary>
        /// <param name="analysis">The current analysis.</param>
        /// <param name="stream">The stream to which to write to.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <returns>The content of the JSON file corresponding to the provided analysis.</returns>
        public static async Task WriteToStreamJsonFileContent(this Analysis analysis, Stream stream, IServiceProvider serviceProvider)
        {
            // Use a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Get the required data.
            var data = new
            {
                Id = analysis.Id,
                Name = analysis.Name,
                Description = analysis.Description,
                Algorithm = analysis.Algorithm.GetDisplayName(),
                Nodes = context.AnalysisNodes
                    .Where(item => item.Analysis == analysis)
                    .Where(item => item.Type == AnalysisNodeType.None)
                    .Select(item => item.Node)
                    .Select(item => new
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Description = item.Description,
                        Values = item.DatabaseNodeFieldNodes
                            .Select(item1 => new
                            {
                                DatabaseId = item1.DatabaseNodeField.Database.Id,
                                DatabaseName = item1.DatabaseNodeField.Database.Name,
                                DatabaseNodeFieldId = item1.DatabaseNodeField.Id,
                                DatabaseNodeFieldName = item1.DatabaseNodeField.Name,
                                Value = item1.Value
                            })
                    }),
                Edges = context.AnalysisEdges
                    .Where(item => item.Analysis == analysis)
                    .Select(item => item.Edge)
                    .Select(item => new
                    {
                        Id = item.Id,
                        Description = item.Description,
                        Nodes = item.EdgeNodes
                            .Select(item1 => new
                            {
                                NodeId = item1.Node.Id,
                                NodeName = item1.Node.Name,
                                Type = item1.Type.GetDisplayName()
                            }),
                        Values = item.DatabaseEdgeFieldEdges
                            .Select(item1 => new
                            {
                                DatabaseId = item1.DatabaseEdgeField.Database.Id,
                                DatabaseName = item1.DatabaseEdgeField.Database.Name,
                                DatabaseEdgeFieldId = item1.DatabaseEdgeField.Id,
                                DatabaseEdgeFieldName = item1.DatabaseEdgeField.Name,
                                Value = item1.Value
                            })
                    })
            };
            // Write the data corresponding to the file.
            await JsonSerializer.SerializeAsync(stream, data, new JsonSerializerOptions { IgnoreNullValues = true });
        }

        /// <summary>
        /// Gets the content of a JSON file to download corresponding to the provided analysis.
        /// </summary>
        /// <param name="analysis">The current analysis.</param>
        /// <param name="stream">The stream to which to write to.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <returns>The content of the JSON file corresponding to the provided analysis.</returns>
        public static async Task WriteToStreamCyjsFileContent(this Analysis analysis, Stream stream, IServiceProvider serviceProvider)
        {
            // Use a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Get the required values.
            var databaseTypeName = context.AnalysisDatabases
                .Where(item => item.Analysis == analysis)
                .Select(item => item.Database.DatabaseType.Name)
                .First();
            // Return the view model.
            var data = new CytoscapeViewModel
            {
                Data = new CytoscapeViewModel.CytoscapeData
                {
                    Id = analysis.Id,
                    Name = analysis.Name,
                    Description = analysis.Description
                },
                Elements = new CytoscapeViewModel.CytoscapeElements
                {
                    Nodes = context.AnalysisNodes
                        .Where(item => item.Analysis == analysis)
                        .Where(item => item.Type == AnalysisNodeType.None)
                        .Select(item => item.Node)
                        .Select(item => new
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Classes = item.AnalysisNodes
                                .Where(item1 => item1.Analysis == analysis)
                                .Select(item => item.Type.ToString().ToLower())
                        })
                        .AsEnumerable()
                        .Select(item => new CytoscapeViewModel.CytoscapeElements.CytoscapeNode
                        {
                            Data = new CytoscapeViewModel.CytoscapeElements.CytoscapeNode.CytoscapeNodeData
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Types = string.Join(",", item.Classes.OrderBy(item => item))
                            }
                        }),
                    Edges = context.AnalysisEdges
                        .Where(item => item.Analysis == analysis)
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
                                Interaction = databaseTypeName.ToLower()
                            }
                        })
                }
            };
            // Write the data corresponding to the file.
            await JsonSerializer.SerializeAsync(stream, data, new JsonSerializerOptions { IgnoreNullValues = true });
        }

        /// <summary>
        /// Gets the content of an Excel file to download corresponding to the provided analysis.
        /// </summary>
        /// <param name="analysis">The current analysis.</param>
        /// <param name="stream">The stream to which to write to.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <returns>The content of the Excel file corresponding to the provided analysis.</returns>
        public static async Task WriteToStreamXlsxFileContent(this Analysis analysis, Stream stream, IServiceProvider serviceProvider)
        {
            // Use a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Get the required data.
            var databases = context.AnalysisDatabases
                .Where(item => item.Analysis == analysis)
                .Select(item => item.Database)
                .Where(item1 => item1.IsPublic);
            var databaseTypeName = databases
                .Select(item => item.DatabaseType.Name)
                .First();
            var databaseNodeFields = context.DatabaseNodeFields
                .Where(item => databases.Contains(item.Database))
                .Select(item => new
                {
                    Id = item.Id,
                    Name = item.Name,
                    DatabaseName = item.Database.Name
                })
                .ToList();
            var databaseEdgeFields = context.DatabaseEdgeFields
                .Where(item => databases.Contains(item.Database))
                .Select(item => new
                {
                    Id = item.Id,
                    Name = item.Name,
                    DatabaseName = item.Database.Name
                })
                .ToList();
            // Get the required variables.
            var nodeTitle = databaseTypeName == "PPI" ? "Protein" : "Node";
            var edgeTitle = databaseTypeName == "PPI" ? "Interaction" : "Edge";
            // Define the rows in the first sheet.
            var worksheet1Rows = new List<List<string>>
            {
                new List<string> { "Internal ID", "Date created", "Date started", "Date ended", "Name", "Description", "Algorithm" },
                new List<string> { analysis.Id, analysis.DateTimeCreated.ToString(), analysis.DateTimeStarted?.ToString(), analysis.DateTimeEnded?.ToString(), analysis.Name, analysis.Description, analysis.Algorithm.GetDisplayName() }
            };
            // Define the rows in the second sheet.
            var worksheet2Rows = new List<List<string>>
            {
                new List<string> { "Internal ID", "Name", "Type" }
                    .Concat(databaseNodeFields
                        .Select(item => $"{item.Name} ({item.DatabaseName})")
                        .ToList())
                    .ToList()
            }
            .Concat(context.AnalysisNodes
                .Where(item => item.Analysis == analysis)
                .Where(item => item.Type == AnalysisNodeType.None)
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
                    Types = item.AnalysisNodes
                        .Where(item1 => item1.Analysis == analysis)
                        .Select(item => item.Type.GetDisplayName().ToLower())
                })
                .AsEnumerable()
                .Select(item => new List<string> { item.Id, item.Name, string.Join(", ", item.Types) }
                    .Concat(databaseNodeFields
                        .Select(item1 => item.Values.FirstOrDefault(item2 => item2.DatabaseNodeFieldId == item1.Id))
                        .Select(item1 => item1 == null ? string.Empty : item1.Value)
                        .ToList())))
            .ToList();
            // Define the rows in the third sheet.
            var worksheet3Rows = new List<List<string>>
            {
                new List<string> { "Internal ID", $"Source {nodeTitle.ToLower()} ID", $"Source {nodeTitle.ToLower()} name", $"Target {nodeTitle.ToLower()} ID", $"Target {nodeTitle.ToLower()} name" }
                    .Concat(databaseEdgeFields
                        .Select(item => $"{item.Name} ({item.DatabaseName})")
                        .ToList())
                    .ToList()
            }
            .Concat(context.AnalysisEdges
                .Where(item => item.Analysis == analysis)
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
                .Select(item => new List<string> { item.Id, item.SourceNode.Id, item.SourceNode.Name, item.TargetNode.Id, item.TargetNode.Name }
                    .Concat(databaseEdgeFields
                        .Select(item1 => item.Values.FirstOrDefault(item2 => item2.DatabaseEdgeFieldId == item1.Id))
                        .Select(item1 => item1 == null ? string.Empty : item1.Value)
                        .ToList())))
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
            var worksheet2 = new Sheet { Id = workbookPart.GetIdOfPart(worksheet2Part), SheetId = 2, Name = $"{nodeTitle}s" };
            worksheet2Part.Worksheet = new Worksheet(worksheet2Data);
            worksheet2Data.Append(worksheet2Rows.Select(item => new Row(item.Select(item1 => new Cell { DataType = CellValues.String, CellValue = new CellValue(item1) }))));
            worksheets.Append(worksheet2);
            // Define the third worksheet.
            var worksheet3Part = workbookPart.AddNewPart<WorksheetPart>();
            var worksheet3Data = new SheetData();
            var worksheet3 = new Sheet { Id = workbookPart.GetIdOfPart(worksheet3Part), SheetId = 3, Name = $"{edgeTitle}s" };
            worksheet3Part.Worksheet = new Worksheet(worksheet3Data);
            worksheet3Data.Append(worksheet3Rows.Select(item => new Row(item.Select(item1 => new Cell { DataType = CellValues.String, CellValue = new CellValue(item1) }))));
            worksheets.Append(worksheet3);
            // Close the document.
            document.Close();
            // Reset the stream position.
            fileStream.Position = 0;
            // Copy it to the archive stream.
            await fileStream.CopyToAsync(stream);
        }
    }
}
