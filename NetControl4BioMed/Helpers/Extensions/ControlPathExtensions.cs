using Microsoft.AspNetCore.Routing;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Extensions
{
    /// <summary>
    /// Provides extensions for the control paths.
    /// </summary>
    public static class ControlPathExtensions
    {
        /// <summary>
        /// Gets the Cytoscape view model corresponding to the provided control path.
        /// </summary>
        /// <param name="controlPath">The current control path.</param>
        /// <param name="linkGenerator">Represents the link generator.</param>
        /// <returns>Returns the Cytoscape view model corresponding to the provided control path.</returns>
        public static CytoscapeViewModel GetCytoscapeViewModel(this ControlPath controlPath, LinkGenerator linkGenerator, ApplicationDbContext context)
        {
            // Get the default values.
            var emptyEnumerable = Enumerable.Empty<string>();
            var interactionType = context.ControlPaths
                .Where(item => item == controlPath)
                .Select(item => item.Analysis.AnalysisDatabases)
                .SelectMany(item => item)
                .Select(item => item.Database.DatabaseType.Name.ToLower())
                .FirstOrDefault();
            var isGeneric = interactionType == "generic";
            var controlClasses = new List<string> { "control" };
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
                                .Concat(controlNodes.Contains(item.Id) ? controlClasses : emptyEnumerable)
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
