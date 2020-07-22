using Microsoft.AspNetCore.Routing;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
        public static List<string> GetLog(this Analysis analysis)
        {
            // Return the list of log entries.
            return JsonSerializer.Deserialize<List<string>>(analysis.Log);
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
            return JsonSerializer.Serialize(analysis.GetLog().Append($"{DateTime.Now}: {message}"));
        }

        /// <summary>
        /// Gets the Cytoscape view model corresponding to the provided analysis.
        /// </summary>
        /// <param name="analysis">The current analysis.</param>
        /// <param name="linkGenerator">The link generator.</param>
        /// <returns>The Cytoscape view model corresponding to the provided network.</returns>
        public static CytoscapeViewModel GetCytoscapeViewModel(this Analysis analysis, LinkGenerator linkGenerator, ApplicationDbContext context)
        {
            // Get the default values.
            var emptyEnumerable = Enumerable.Empty<string>();
            var interactionType = context.AnalysisDatabases
                .Select(item => item.Database.DatabaseType.Name.ToLower())
                .FirstOrDefault();
            var isGeneric = interactionType == "generic";
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
                                Href = isGeneric ? string.Empty : linkGenerator.GetPathByPage(page: "/Content/Data/Nodes/Details", values: new { id = item.Id }),
                                Alias = item.Alias
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
                                Source = item.SourceNodeId,
                                Target = item.TargetNodeId,
                                Interaction = interactionType
                            },
                            Classes = emptyEnumerable
                        })
                },
                Layout = CytoscapeViewModel.DefaultLayout,
                Styles = CytoscapeViewModel.DefaultStyles.Concat(CytoscapeViewModel.DefaultAnalysisStyles)
            };
        }
    }
}
