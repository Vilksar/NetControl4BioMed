using Microsoft.AspNetCore.Routing;
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
        public static CytoscapeViewModel GetCytoscapeViewModel(this ControlPath controlPath, LinkGenerator linkGenerator)
        {
            // Get the control data.
            var controlNodes = controlPath.Paths.Select(item => item.PathNodes).SelectMany(item => item).Where(item => item.Type == PathNodeType.Source).Select(item => item.Node);
            var controlEdges = controlPath.Paths.Select(item => item.PathEdges).SelectMany(item => item).Select(item => item.Edge);
            // Get the default values.
            var interactionType = controlPath.Analysis.AnalysisDatabases.FirstOrDefault()?.Database.DatabaseType.Name.ToLower();
            var isGeneric = interactionType == "generic";
            var controlClasses = new List<string> { "control" };
            // Return the view model.
            return new CytoscapeViewModel
            {
                Elements = new CytoscapeViewModel.CytoscapeElements
                {
                    Nodes = controlPath.Analysis.AnalysisNodes
                        .Where(item => item.Type == AnalysisNodeType.None)
                        .Select(item => item.Node)
                        .Select(item => new CytoscapeViewModel.CytoscapeElements.CytoscapeNode
                        {
                            Data = new CytoscapeViewModel.CytoscapeElements.CytoscapeNode.CytoscapeNodeData
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Href = isGeneric ? string.Empty : linkGenerator.GetPathByPage(page: "/Content/Data/Nodes/Details", values: new { id = item.Id }),
                                Alias = item.DatabaseNodeFieldNodes
                                    .Where(item1 => item1.DatabaseNodeField.IsSearchable)
                                    .Select(item1 => item1.Value)
                            },
                            Classes = item.AnalysisNodes
                                .Where(item1 => item1.Analysis == controlPath.Analysis)
                                .Select(item1 => item1.Type.ToString().ToLower())
                                .Concat(controlNodes.Contains(item) ? controlClasses : Enumerable.Empty<string>())
                        }),
                    Edges = controlPath.Analysis.AnalysisEdges
                        .Select(item => item.Edge)
                        .Select(item => new CytoscapeViewModel.CytoscapeElements.CytoscapeEdge
                        {
                            Data = new CytoscapeViewModel.CytoscapeElements.CytoscapeEdge.CytoscapeEdgeData
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Source = item.EdgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Source)?.Node.Id,
                                Target = item.EdgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Target)?.Node.Id,
                                Interaction = interactionType
                            },
                            Classes = controlEdges.Contains(item) ? controlClasses : Enumerable.Empty<string>()
                        })
                },
                Layout = CytoscapeViewModel.DefaultLayout,
                Styles = CytoscapeViewModel.DefaultStyles.Concat(CytoscapeViewModel.DefaultAnalysisStyles).Concat(CytoscapeViewModel.DefaultControlPathStyles)
            };
        }
    }
}
