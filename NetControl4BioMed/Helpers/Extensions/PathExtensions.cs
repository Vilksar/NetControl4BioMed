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
    /// Provides extensions for the paths.
    /// </summary>
    public static class PathExtensions
    {
        /// <summary>
        /// Gets the Cytoscape view model corresponding to the provided path.
        /// </summary>
        /// <param name="path">The current path.</param>
        /// <param name="linkGenerator">Represents the link generator.</param>
        /// <returns>Returns the Cytoscape view model corresponding to the provided path.</returns>
        public static CytoscapeViewModel GetCytoscapeViewModel(this Path path, LinkGenerator linkGenerator)
        {
            // Get the control data.
            var controlNodes = path.PathNodes.Where(item => item.Type == PathNodeType.Source).Select(item => item.Node);
            var controlEdges = path.PathEdges.Select(item => item.Edge);
            // Get the default values.
            var interactionType = path.ControlPath.Analysis.AnalysisDatabases.FirstOrDefault().Database.DatabaseType.Name.ToLower();
            var isGeneric = interactionType == "generic";
            var controlClasses = new List<string> { "control" };
            // Return the view model.
            return new CytoscapeViewModel
            {
                Elements = new CytoscapeViewModel.CytoscapeElements
                {
                    Nodes = path.PathNodes
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
                                .Where(item1 => item1.Analysis == path.ControlPath.Analysis)
                                .Select(item1 => item1.Type.ToString().ToLower())
                                .Concat(controlNodes.Contains(item) ? controlClasses : Enumerable.Empty<string>())
                        }),
                    Edges = path.PathEdges
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
                Styles = CytoscapeViewModel.DefaultStyles.Concat(CytoscapeViewModel.DefaultNetworkStyles)
            };
        }
    }
}
