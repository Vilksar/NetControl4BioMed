using Microsoft.AspNetCore.Routing;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NetControl4BioMed.Helpers.Extensions
{
    /// <summary>
    /// Provides extensions for the networks.
    /// </summary>
    public static class NetworkExtensions
    {
        /// <summary>
        /// Gets the Cytoscape view model corresponding to the provided network.
        /// </summary>
        /// <param name="network">The current network.</param>
        /// <param name="linkGenerator">Represents the link generator.</param>
        /// <returns>Returns the Cytoscape view model corresponding to the provided network.</returns>
        public static CytoscapeViewModel GetCytoscapeViewModel(this Network network, LinkGenerator linkGenerator)
        {
            // Get the default values.
            var interactionType = network.NetworkDatabases.FirstOrDefault().Database.DatabaseType.Name.ToLower();
            var isGeneric = interactionType == "generic";
            // Return the view model.
            return new CytoscapeViewModel
            {
                Elements = new CytoscapeViewModel.CytoscapeElements
                {
                    Nodes = network.NetworkNodes
                        .Where(item => item.Type == NetworkNodeType.None)
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
                            Classes = item.NetworkNodes
                                .Where(item1 => item1.Network == network)
                                .Select(item => item.Type.ToString().ToLower())
                        }),
                    Edges = network.NetworkEdges
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
                            Classes = Enumerable.Empty<string>()
                        })
                },
                Layout = CytoscapeViewModel.DefaultLayout,
                Styles = CytoscapeViewModel.DefaultStyles.Concat(CytoscapeViewModel.DefaultNetworkStyles)
            };
        }
    }
}
