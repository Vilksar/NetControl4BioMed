﻿using DocumentFormat.OpenXml;
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
using System.Text.Json.Serialization;
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
        public static FileCyjsViewModel GetCytoscapeViewModel(this Analysis analysis, HttpContext httpContext, LinkGenerator linkGenerator, ApplicationDbContext context)
        {
            // Return the view model.
            return new FileCyjsViewModel
            {
                Elements = new FileCyjsViewModel.CyjsElements
                {
                    Nodes = context.AnalysisProteins
                        .Where(item => item.Analysis == analysis)
                        .Where(item => item.Type == AnalysisProteinType.None)
                        .Select(item => item.Protein)
                        .Select(item => new
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Classes = item.AnalysisProteins
                                .Where(item1 => item1.Analysis == analysis)
                                .Select(item1 => item1.Type.ToString().ToLower())
                        })
                        .AsEnumerable()
                        .Select(item => new FileCyjsViewModel.CyjsElements.CyjsNode
                        {
                            Data = new FileCyjsViewModel.CyjsElements.CyjsNode.CyjsNodeData
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Href = linkGenerator.GetUriByPage(httpContext, $"/AvailableData/Data/Proteins/Details", handler: null, values: new { id = item.Id })
                            },
                            Classes = item.Classes
                        }),
                    Edges = context.AnalysisInteractions
                        .Where(item => item.Analysis == analysis)
                        .Select(item => item.Interaction)
                        .Select(item => new
                        {
                            Id = item.Id,
                            Name = item.Name,
                            SourceProteinId = item.InteractionProteins
                                .Where(item1 => item1.Type == InteractionProteinType.Source)
                                .Select(item1 => item1.Protein)
                                .Where(item1 => item1 != null)
                                .Select(item1 => item1.Id)
                                .FirstOrDefault(),
                            TargetProteinId = item.InteractionProteins
                                .Where(item1 => item1.Type == InteractionProteinType.Target)
                                .Select(item1 => item1.Protein)
                                .Where(item1 => item1 != null)
                                .Select(item1 => item1.Id)
                                .FirstOrDefault()
                        })
                        .AsEnumerable()
                        .Select(item => new FileCyjsViewModel.CyjsElements.CyjsEdge
                        {
                            Data = new FileCyjsViewModel.CyjsElements.CyjsEdge.CyjsEdgeData
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Href = linkGenerator.GetUriByPage(httpContext, $"/AvailableData/Data/Interactions/Details", handler: null, values: new { id = item.Id }),
                                Source = item.SourceProteinId,
                                Target = item.TargetProteinId
                            }
                        })
                },
                Layout = FileCyjsViewModel.DefaultLayout,
                Styles = FileCyjsViewModel.DefaultStyles.Concat(FileCyjsViewModel.DefaultAnalysisStyles)
            };
        }

        /// <summary>
        /// Gets the content of an overview text file to download corresponding to the provided analyses.
        /// </summary>
        /// <param name="analysisIds">The IDs of the current analyses.</param>
        /// <param name="stream">The stream to which to write to.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="scheme">The HTTP context scheme.</param>
        /// <param name="host">The HTTP context host.</param>
        /// <returns></returns>
        public static async Task WriteToStreamOverviewTextFileContent(IEnumerable<string> analysisIds, Stream stream, IServiceProvider serviceProvider, string scheme, HostString host)
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
            var data = string.Concat("The following analyses have been downloaded:\n\n", string.Join("\n", context.Analyses
                .Where(item => analysisIds.Contains(item.Id))
                .Select(item => new
                {
                    Id = item.Id,
                    Name = item.Name
                })
                .AsEnumerable()
                .Select(item => $"{item.Name} - {linkGenerator.GetUriByPage($"/CreatedData/Analyses/Details/Index", handler: null, values: new { id = item.Id }, scheme: scheme, host: host)}")));
            // Write the data to the stream.
            await streamWriter.WriteAsync(data);
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
            // Get the required data.
            var data = string.Join("\n", context.AnalysisInteractions
                .Where(item => item.Analysis == analysis)
                .Select(item => item.Interaction)
                .Select(item => new
                {
                    SourceProteinName = item.InteractionProteins
                        .Where(item1 => item1.Type == InteractionProteinType.Source)
                        .Select(item1 => item1.Protein)
                        .Where(item1 => item1 != null)
                        .Select(item1 => item1.Name)
                        .FirstOrDefault(),
                    TargetProteinName = item.InteractionProteins
                        .Where(item1 => item1.Type == InteractionProteinType.Target)
                        .Select(item1 => item1.Protein)
                        .Where(item1 => item1 != null)
                        .Select(item1 => item1.Name)
                        .FirstOrDefault()
                })
                .Where(item => !string.IsNullOrEmpty(item.SourceProteinName) && !string.IsNullOrEmpty(item.TargetProteinName))
                .Select(item => $"{item.SourceProteinName};{item.TargetProteinName}"));
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
            // Get the required data.
            var data = string.Join("\n", context.AnalysisInteractions
                .Where(item => item.Analysis == analysis)
                .Select(item => item.Interaction)
                .Select(item => new
                {
                    SourceProteinName = item.InteractionProteins
                        .Where(item1 => item1.Type == InteractionProteinType.Source)
                        .Select(item1 => item1.Protein)
                        .Where(item1 => item1 != null)
                        .Select(item1 => item1.Name)
                        .FirstOrDefault(),
                    TargetProteinName = item.InteractionProteins
                        .Where(item1 => item1.Type == InteractionProteinType.Target)
                        .Select(item1 => item1.Protein)
                        .Where(item1 => item1 != null)
                        .Select(item1 => item1.Name)
                        .FirstOrDefault()
                })
                .Where(item => !string.IsNullOrEmpty(item.SourceProteinName) && !string.IsNullOrEmpty(item.TargetProteinName))
                .Select(item => $"{item.SourceProteinName}\tinteracts with\t{item.TargetProteinName}"));
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
                Type = "Analysis",
                Id = analysis.Id,
                Name = analysis.Name,
                Description = analysis.Description,
                IsPublic = analysis.IsPublic,
                Algorithm = analysis.Algorithm.ToString(),
                NetworkData = context.Networks
                    .Where(item => item.Analyses.Contains(analysis))
                    .Select(item => new
                    {
                        Id = item.Id,
                        Name = item.Name
                    })
                    .FirstOrDefault(),
                SourceProteinData = context.AnalysisProteins
                    .Where(item => item.Analysis == analysis)
                    .Where(item => item.Type == AnalysisProteinType.Source)
                    .Select(item => item.Protein.Name),
                SourceProteinCollectionData = context.AnalysisProteinCollections
                    .Where(item => item.Analysis == analysis)
                    .Where(item => item.Type == AnalysisProteinCollectionType.Source)
                    .Select(item => item.ProteinCollection.Id),
                TargetProteinData = context.AnalysisProteins
                    .Where(item => item.Analysis == analysis)
                    .Where(item => item.Type == AnalysisProteinType.Target)
                    .Select(item => item.Protein.Name),
                TargetProteinCollectionData = context.AnalysisProteinCollections
                    .Where(item => item.Analysis == analysis)
                    .Where(item => item.Type == AnalysisProteinCollectionType.Target)
                    .Select(item => item.ProteinCollection.Id),
                MaximumIterations = analysis.MaximumIterations,
                MaximumIterationsWithoutImprovement = analysis.MaximumIterationsWithoutImprovement,
                AlgorithmParameters = analysis.Parameters
            };
            // Write the data corresponding to the file.
            await JsonSerializer.SerializeAsync(stream, data, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
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
            // Return the view model.
            var data = new FileCyjsViewModel
            {
                Data = new FileCyjsViewModel.CyjsData
                {
                    Id = analysis.Id,
                    Name = analysis.Name,
                    Description = analysis.Description
                },
                Elements = new FileCyjsViewModel.CyjsElements
                {
                    Nodes = context.AnalysisProteins
                        .Where(item => item.Analysis == analysis)
                        .Where(item => item.Type == AnalysisProteinType.None)
                        .Select(item => item.Protein)
                        .Select(item => new
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Classes = item.AnalysisProteins
                                .Where(item1 => item1.Analysis == analysis)
                                .Select(item => item.Type.ToString().ToLower())
                        })
                        .AsEnumerable()
                        .Select(item => new FileCyjsViewModel.CyjsElements.CyjsNode
                        {
                            Data = new FileCyjsViewModel.CyjsElements.CyjsNode.CyjsNodeData
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Type = string.Join(",", item.Classes.OrderBy(item => item))
                            }
                        }),
                    Edges = context.AnalysisInteractions
                        .Where(item => item.Analysis == analysis)
                        .Select(item => item.Interaction)
                        .Select(item => new
                        {
                            Id = item.Id,
                            Name = item.Name,
                            SourceProteinId = item.InteractionProteins
                                .Where(item1 => item1.Type == InteractionProteinType.Source)
                                .Select(item1 => item1.Protein)
                                .Where(item1 => item1 != null)
                                .Select(item1 => item1.Id)
                                .FirstOrDefault(),
                            TargetProteinId = item.InteractionProteins
                                .Where(item1 => item1.Type == InteractionProteinType.Target)
                                .Select(item1 => item1.Protein)
                                .Where(item1 => item1 != null)
                                .Select(item1 => item1.Id)
                                .FirstOrDefault()
                        })
                        .AsEnumerable()
                        .Select(item => new FileCyjsViewModel.CyjsElements.CyjsEdge
                        {
                            Data = new FileCyjsViewModel.CyjsElements.CyjsEdge.CyjsEdgeData
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Source = item.SourceProteinId,
                                Target = item.TargetProteinId
                            }
                        })
                }
            };
            // Write the data corresponding to the file.
            await JsonSerializer.SerializeAsync(stream, data, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
        }

        /// <summary>
        /// Gets the content of a JSON file to download corresponding to the provided network.
        /// </summary>
        /// <param name="analysis">The current analysis.</param>
        /// <param name="stream">The stream to which to write to.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <returns>The content of the JSON file corresponding to the provided network.</returns>
        public static async Task WriteToStreamCxFileContent(this Analysis analysis, Stream stream, IServiceProvider serviceProvider)
        {
            // Use a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Get the related protein data.
            var proteinList = context.AnalysisProteins
                .Where(item => item.Analysis == analysis)
                .Where(item => item.Type == AnalysisProteinType.None)
                .Select(item => item.Protein)
                .OrderBy(item => item.Id)
                .Select(item => new
                {
                    Id = item.Id,
                    Name = item.Name,
                    Types = item.AnalysisProteins
                        .Where(item1 => item1.Analysis == analysis)
                        .Select(item => item.Type.ToString().ToLower())
                })
                .AsEnumerable()
                .Select((item, index) => new
                {
                    Id = item.Id,
                    Name = item.Name,
                    Index = index + 1,
                    Types = item.Types
                });
            var proteinListCount = proteinList.Count();
            // Get the related interaction data.
            var interactionList = context.AnalysisInteractions
                .Where(item => item.Analysis == analysis)
                .Select(item => item.Interaction)
                .OrderBy(item => item.Id)
                .Select(item => new
                {
                    Id = item.Id,
                    Name = item.Name,
                    SourceProteinId = item.InteractionProteins
                        .Where(item1 => item1.Type == InteractionProteinType.Source)
                        .Select(item1 => item1.Protein)
                        .Where(item1 => item1 != null)
                        .Select(item1 => item1.Id)
                        .FirstOrDefault(),
                    TargetProteinId = item.InteractionProteins
                        .Where(item1 => item1.Type == InteractionProteinType.Target)
                        .Select(item1 => item1.Protein)
                        .Where(item1 => item1 != null)
                        .Select(item1 => item1.Id)
                        .FirstOrDefault()
                })
                .Where(item => !string.IsNullOrEmpty(item.SourceProteinId) && !string.IsNullOrEmpty(item.TargetProteinId))
                .AsEnumerable()
                .Select((item, index) => new
                {
                    Id = item.Id,
                    Name = item.Name,
                    Index = index + proteinListCount + 1,
                    SourceProteinId = item.SourceProteinId,
                    TargetProteinId = item.TargetProteinId
                });
            // Get the related additional data.
            var proteinIdDictionary = proteinList.ToDictionary(item => item.Id, item => item);
            // Get the required data.
            var data = new List<FileCxViewModel.CxBaseObject>
            {
                new FileCxViewModel.CxBaseObject
                {
                    NetworkAttributes = new List<FileCxViewModel.CxBaseObject.CxNetworkAttribute>
                    {
                        new FileCxViewModel.CxBaseObject.CxNetworkAttribute
                        {
                            Name = "name",
                            Value = analysis.Name
                        },
                        new FileCxViewModel.CxBaseObject.CxNetworkAttribute
                        {
                            Name = "description",
                            Value = analysis.Description
                        },
                        new FileCxViewModel.CxBaseObject.CxNetworkAttribute
                        {
                            Name = "source",
                            Value = "NetControl4BioMed"
                        }
                    }
                },
                new FileCxViewModel.CxBaseObject
                {
                    Nodes = proteinList
                        .Select(item => new FileCxViewModel.CxBaseObject.CxNode
                        {
                            Id = item.Index,
                            Name = item.Name
                        })
                },
                new FileCxViewModel.CxBaseObject
                {
                    NodeAttributes = proteinList
                        .Select(item => new FileCxViewModel.CxBaseObject.CxNodeAttribute
                        {
                            Id = item.Index,
                            Name = "type",
                            Value = string.Join(",", item.Types.OrderBy(item => item))
                        })
                },
                new FileCxViewModel.CxBaseObject
                {
                    Edges = interactionList
                        .Select(item => new
                        {
                            Id = item.Index,
                            Source = proteinIdDictionary.GetValueOrDefault(item.SourceProteinId),
                            Target = proteinIdDictionary.GetValueOrDefault(item.TargetProteinId)
                        })
                        .Where(item => item.Source != null && item.Target != null)
                        .Select(item => new FileCxViewModel.CxBaseObject.CxEdge
                        {
                            Id = item.Id,
                            Source = item.Source.Index,
                            Target = item.Target.Index,
                            Type = "interacts with"
                        })
                        .Where(item => item.Source != 0 && item.Target != 0)
                },
                new FileCxViewModel.CxBaseObject
                {
                    EdgeAttributes = interactionList
                        .Select(item => new
                        {
                            Id = item.Index,
                            Source = proteinIdDictionary.GetValueOrDefault(item.SourceProteinId),
                            Target = proteinIdDictionary.GetValueOrDefault(item.TargetProteinId)
                        })
                        .Where(item => item.Source != null && item.Target != null)
                        .Select(item => new FileCxViewModel.CxBaseObject.CxEdgeAttribute
                        {
                            Id = item.Id,
                            Name = "name",
                            Value = $"{item.Source.Name} (interacts with) {item.Target.Name}"
                        })
                },
                new FileCxViewModel.CxBaseObject
                {
                    CytoscapeTableColumns = FileCxViewModel.DefaultCytoscapeTableColumns
                },
                new FileCxViewModel.CxBaseObject
                {
                    Status = new List<FileCxViewModel.CxBaseObject.CxStatus>
                    {
                        new FileCxViewModel.CxBaseObject.CxStatus
                        {
                            ErrorMessage = string.Empty,
                            IsSuccessful = true
                        }
                    }
                }
            };
            // Update the meta data.
            FileCxViewModel.AddMetaData(data);
            // Write the data corresponding to the file.
            await JsonSerializer.SerializeAsync(stream, data, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
        }

        /// <summary>
        /// Gets the content of an Excel file to download corresponding to the provided analysis.
        /// </summary>
        /// <param name="analysis">The current analysis.</param>
        /// <param name="user">The current user.</param>
        /// <param name="stream">The stream to which to write to.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <returns>The content of the Excel file corresponding to the provided analysis.</returns>
        public static async Task WriteToStreamXlsxFileContent(this Analysis analysis, User user, Stream stream, IServiceProvider serviceProvider)
        {
            // Use a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Get the required data.
            var databaseProteinFields = context.AnalysisDatabases
                .Where(item => item.Analysis == analysis)
                .Where(item => item.Type == AnalysisDatabaseType.Protein)
                .Where(item => item.Database.IsPublic || (user != null && item.Database.DatabaseUsers.Any(item => item.Email == user.Email)))
                .Select(item => item.Database.DatabaseProteinFields)
                .SelectMany(item => item)
                .ToList();
            var databaseInteractionFields = context.AnalysisDatabases
                .Where(item => item.Analysis == analysis)
                .Where(item => item.Type == AnalysisDatabaseType.Interaction)
                .Where(item => item.Database.IsPublic || (user != null && item.Database.DatabaseUsers.Any(item => item.Email == user.Email)))
                .Select(item => item.Database.DatabaseInteractionFields)
                .SelectMany(item => item)
                .ToList();
            // Get the required variables.
            var parameterDictionary = new Dictionary<string, string>();
            // Switch on the algorithm.
            switch (analysis.Algorithm)
            {
                case AnalysisAlgorithm.Greedy:
                    // Try to deserialize the parameters.
                    if (analysis.Parameters.TryDeserializeJsonObject<Algorithms.Analyses.Greedy.Parameters>(out var greedyParameters))
                    {
                        // Update the parameter dictionary.
                        parameterDictionary = typeof(Algorithms.Analyses.Greedy.Parameters).GetProperties().ToDictionary(item => item.Name, item => item.GetValue(greedyParameters).ToString());
                    }
                    // Break.
                    break;
                case AnalysisAlgorithm.Genetic:
                    // Try to deserialize the parameters.
                    if (analysis.Parameters.TryDeserializeJsonObject<Algorithms.Analyses.Genetic.Parameters>(out var geneticParameters))
                    {
                        // Update the parameter dictionary.
                        parameterDictionary = typeof(Algorithms.Analyses.Genetic.Parameters).GetProperties().ToDictionary(item => item.Name, item => item.GetValue(geneticParameters).ToString());
                    }
                    // Break.
                    break;
                default:
                    // Break.
                    break;
            }
            // Define the rows in the first sheet.
            var worksheet1Rows = new List<List<string>>
            {
                new List<string>
                {
                    "Internal ID",
                    analysis.Id
                },
                new List<string>
                {
                    "Date created",
                    analysis.DateTimeCreated.ToString()
                },
                new List<string>
                {
                    "Date started",
                    analysis.DateTimeStarted?.ToString() ?? string.Empty
                },
                new List<string>
                {
                    "Date ended",
                    analysis.DateTimeEnded?.ToString() ?? string.Empty
                },
                new List<string>
                {
                    "Name",
                    analysis.Name
                },
                new List<string>
                {
                    "Description",
                    analysis.Description
                },
                new List<string>
                {
                    "Algorithm",
                    analysis.Algorithm.ToString()
                },
                new List<string>
                {
                    nameof(analysis.MaximumIterations),
                    analysis.MaximumIterations.ToString()
                },
                new List<string>
                {
                    nameof(analysis.MaximumIterationsWithoutImprovement),
                    analysis.MaximumIterationsWithoutImprovement.ToString()
                }
            }.Concat(parameterDictionary
                .Select(item => new List<string>
                {
                    item.Key,
                    item.Value
                }));
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
                .Where(item => item.Database.IsPublic || (user != null && item.Database.DatabaseUsers.Any(item => item.Email == user.Email)))
                .Select(item => item.Database)
                .Select(item => new
                {
                    Id = item.Id,
                    Name = item.Name,
                    Types = item.AnalysisDatabases
                        .Where(item1 => item1.Analysis == analysis)
                        .Select(item => item.Type.ToString().ToLower())
                })
                .AsEnumerable()
                .Select(item => new List<string>
                {
                    item.Id,
                    item.Name,
                    string.Join(",", item.Types)
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
                    "Type"
                }.Concat(databaseProteinFields
                    .Select(item => item.Name)
                    .ToList())
                .ToList()
            }
            .Concat(context.AnalysisProteins
                .Where(item => item.Analysis == analysis)
                .Where(item => item.Type == AnalysisProteinType.None)
                .Select(item => item.Protein)
                .Select(item => new
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Values = item.DatabaseProteinFieldProteins
                        .Select(item1 => new
                        {
                            DatabaseProteinFieldId = item1.DatabaseProteinField.Id,
                            Value = item1.Value
                        }),
                    Types = item.AnalysisProteins
                        .Where(item1 => item1.Analysis == analysis)
                        .Select(item => item.Type.ToString().ToLower())
                })
                .AsEnumerable()
                .Select(item => new List<string>
                    {
                        item.Id,
                        item.Name,
                        string.Join(",", item.Types)
                    }.Concat(databaseProteinFields
                        .Select(item1 => item.Values.FirstOrDefault(item2 => item2.DatabaseProteinFieldId == item1.Id))
                        .Select(item1 => item1 == null ? string.Empty : item1.Value)
                        .ToList())))
            .ToList();
            // Define the rows in the fourth sheet.
            var worksheet4Rows = new List<List<string>>
            {
                new List<string>
                {
                    "Internal ID",
                    $"Source protein ID",
                    $"Source protein name",
                    $"Target protein ID",
                    $"Target protein name"
                }.Concat(databaseInteractionFields
                    .Select(item => item.Name)
                    .ToList())
                .ToList()
            }
            .Concat(context.AnalysisInteractions
                .Where(item => item.Analysis == analysis)
                .Select(item => item.Interaction)
                .Select(item => new
                {
                    Id = item.Id,
                    SourceProtein = item.InteractionProteins
                        .Where(item1 => item1.Type == InteractionProteinType.Source)
                        .Select(item1 => item1.Protein)
                        .FirstOrDefault(),
                    TargetProtein = item.InteractionProteins
                        .Where(item1 => item1.Type == InteractionProteinType.Target)
                        .Select(item1 => item1.Protein)
                        .FirstOrDefault(),
                    Values = item.DatabaseInteractionFieldInteractions
                        .Select(item1 => new
                        {
                            DatabaseInteractionFieldId = item1.DatabaseInteractionField.Id,
                            Value = item1.Value
                        })
                })
                .Where(item => item.SourceProtein != null && item.TargetProtein != null)
                .AsEnumerable()
                .Select(item => new List<string>
                {
                    item.Id,
                    item.SourceProtein.Id,
                    item.SourceProtein.Name,
                    item.TargetProtein.Id,
                    item.TargetProtein.Name
                }.Concat(databaseInteractionFields
                    .Select(item1 => item.Values.FirstOrDefault(item2 => item2.DatabaseInteractionFieldId == item1.Id))
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
            var worksheet2 = new Sheet { Id = workbookPart.GetIdOfPart(worksheet2Part), SheetId = 2, Name = "Databases" };
            worksheet2Part.Worksheet = new Worksheet(worksheet2Data);
            worksheet2Data.Append(worksheet2Rows.Select(item => new Row(item.Select(item1 => new Cell { DataType = CellValues.String, CellValue = new CellValue(item1) }))));
            worksheets.Append(worksheet2);
            // Define the third worksheet.
            var worksheet3Part = workbookPart.AddNewPart<WorksheetPart>();
            var worksheet3Data = new SheetData();
            var worksheet3 = new Sheet { Id = workbookPart.GetIdOfPart(worksheet3Part), SheetId = 3, Name = $"Proteins" };
            worksheet3Part.Worksheet = new Worksheet(worksheet3Data);
            worksheet3Data.Append(worksheet3Rows.Select(item => new Row(item.Select(item1 => new Cell { DataType = CellValues.String, CellValue = new CellValue(item1) }))));
            worksheets.Append(worksheet3);
            // Define the fourth worksheet.
            var worksheet4Part = workbookPart.AddNewPart<WorksheetPart>();
            var worksheet4Data = new SheetData();
            var worksheet4 = new Sheet { Id = workbookPart.GetIdOfPart(worksheet4Part), SheetId = 4, Name = $"Interactions" };
            worksheet4Part.Worksheet = new Worksheet(worksheet4Data);
            worksheet4Data.Append(worksheet4Rows.Select(item => new Row(item.Select(item1 => new Cell { DataType = CellValues.String, CellValue = new CellValue(item1) }))));
            worksheets.Append(worksheet4);
            // Close the document.
            document.Close();
            // Reset the stream position.
            fileStream.Position = 0;
            // Copy it to the archive stream.
            await fileStream.CopyToAsync(stream);
        }
    }
}
