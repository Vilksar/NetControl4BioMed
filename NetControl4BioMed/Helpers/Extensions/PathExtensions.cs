using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Interfaces;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Extensions
{
    /// <summary>
    /// Provides extensions for the paths.
    /// </summary>
    public static class PathExtensions
    {
        /// <summary>
        /// Represents the maximum size for which visualization will be enabled.
        /// </summary>
        public readonly static int MaximumSizeForVisualization = 500;

        /// <summary>
        /// Deletes the related entities of the corresponding paths.
        /// </summary>
        /// <typeparam name="T">The type of the related entities.</typeparam>
        /// <param name="itemIds">The IDs of the items whose entities should be deleted.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public static async Task DeleteRelatedEntitiesAsync<T>(IEnumerable<string> itemIds, IServiceProvider serviceProvider, CancellationToken token) where T : class, IPathDependent
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
                    .Where(item => itemIds.Contains(item.Path.Id))
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
                        .Where(item => itemIds.Contains(item.Path.Id))
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
        /// Gets the Cytoscape view model corresponding to the provided path.
        /// </summary>
        /// <param name="path">The current path.</param>
        /// <param name="linkGenerator">Represents the link generator.</param>
        /// <returns>Returns the Cytoscape view model corresponding to the provided path.</returns>
        public static CytoscapeViewModel GetCytoscapeViewModel(this Path path, HttpContext httpContext, LinkGenerator linkGenerator, ApplicationDbContext context)
        {
            // Get the default values.
            var emptyEnumerable = Enumerable.Empty<string>();
            var interactionType = context.Paths
                .Where(item => item == path)
                .Select(item => item.ControlPath.Analysis.AnalysisDatabases)
                .SelectMany(item => item)
                .Select(item => item.Database.DatabaseType.Name)
                .FirstOrDefault();
            var isGeneric = interactionType == "Generic";
            var controlClasses = new List<string> { "control" };
            // Get the control data.
            var analysis = context.Paths
                .Where(item => item == path)
                .Select(item => item.ControlPath.Analysis)
                .FirstOrDefault();
            var controlNodes = context.Paths
                .Where(item => item == path)
                .Select(item => item.PathNodes)
                .SelectMany(item => item)
                .Where(item => item.Type == PathNodeType.Source)
                .Select(item => item.Node.Id)
                .Where(item => !string.IsNullOrEmpty(item))
                .ToHashSet();
            var controlEdges = context.Paths
                .Where(item => item == path)
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
                        .Where(item => item.Path == path)
                        .Where(item => item.Type == PathNodeType.None)
                        .Select(item => item.Node)
                        .Select(item => new
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Alias = item.DatabaseNodeFieldNodes
                                .Where(item1 => item1.DatabaseNodeField.IsSearchable)
                                .Select(item1 => item1.Value),
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
                                Href = isGeneric ? string.Empty : linkGenerator.GetUriByPage(httpContext, "/Content/Data/Nodes/Details", handler: null, values: new { id = item.Id }),
                                Alias = item.Alias
                            },
                            Classes = item.Classes
                                .Concat(controlNodes.Contains(item.Id) ? controlClasses : emptyEnumerable)
                        }),
                    Edges = context.PathEdges
                        .Where(item => item.Path == path)
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
                                Interaction = interactionType
                            },
                            Classes = controlEdges.Contains(item.Id) ? controlClasses : emptyEnumerable
                        })
                },
                Layout = CytoscapeViewModel.DefaultLayout,
                Styles = CytoscapeViewModel.DefaultStyles.Concat(CytoscapeViewModel.DefaultAnalysisStyles).Concat(CytoscapeViewModel.DefaultControlPathStyles)
            };
        }
    }
}
