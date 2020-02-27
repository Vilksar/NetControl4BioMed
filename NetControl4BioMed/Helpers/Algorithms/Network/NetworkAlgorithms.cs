using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Algorithms.Network
{
    /// <summary>
    /// Provides static methods and algorithms for a network.
    /// </summary>
    public static class NetworkAlgorithms
    {
        /// <summary>
        /// Gets the edges of a network, based on the provided seed nodes and seed edges and using the specified algorithm.
        /// </summary>
        /// <param name="seedNodes">The seed nodes from which to generate the network.</param>
        /// <param name="seedEdges">The seed edges from which to generate the network.</param>
        /// <param name="algorithm">The generation algorithm.</param>
        /// <returns>The edges of the network.</returns>
        public static IEnumerable<Edge> GetEdgesFromSeed(IEnumerable<Node> seedNodes, IEnumerable<Edge> seedEdges, NetworkAlgorithm algorithm)
        {
            // Check which algorithm is selected.
            if (algorithm == NetworkAlgorithm.Neighbors)
            {
                // Return all of the edges which contain the seed nodes.
                return seedEdges.Where(item => item.EdgeNodes.Any(item1 => seedNodes.Contains(item1.Node)));
            }
            else if (algorithm == NetworkAlgorithm.Gap0 || algorithm == NetworkAlgorithm.Gap1 || algorithm == NetworkAlgorithm.Gap2 || algorithm == NetworkAlgorithm.Gap3 || algorithm == NetworkAlgorithm.Gap4)
            {
                // Get the gap value.
                var gap = algorithm == NetworkAlgorithm.Gap0 ? 0 :
                    algorithm == NetworkAlgorithm.Gap1 ? 1 :
                    algorithm == NetworkAlgorithm.Gap2 ? 2 :
                    algorithm == NetworkAlgorithm.Gap3 ? 3 :
                    algorithm == NetworkAlgorithm.Gap4 ? 4 : -1;
                // Check if the gap value is not valid.
                if (gap == -1)
                {
                    // Return null.
                    return null;
                }
                // Define the list to store the edges.
                var list = new List<List<Edge>>();
                // For "gap" times, for all terminal nodes, add all possible edges.
                for (int index = 0; index < gap + 1; index++)
                {
                    // Get the terminal nodes (the seed nodes for the first iteration, the target nodes of all edges in the previous iteration for the subsequent iterations).
                    var terminalNodes = index == 0 ? seedNodes : list.Last()
                        .Select(item => item.EdgeNodes
                            .Where(item => item.Type == EdgeNodeType.Target)
                            .Select(item => item.Node))
                        .SelectMany(item => item);
                    // Get all edges that start in the terminal nodes.
                    var temporaryList = seedEdges
                        .Where(item => item.EdgeNodes
                            .Any(item1 => item1.Type == EdgeNodeType.Source && terminalNodes.Contains(item1.Node)))
                        .ToList();
                    // Add them to the list.
                    list.Add(temporaryList);
                }
                // Define a variable to store, at each step, the nodes to keep.
                var nodesToKeep = seedNodes.AsEnumerable();
                // Starting from the right, mark all terminal nodes that are not seed nodes for removal.
                for (int index = gap; index >= 0; index--)
                {
                    // Remove from the list all edges that do not end in nodes to keep.
                    list.ElementAt(index)
                        .RemoveAll(item => item.EdgeNodes.Any(item1 => item1.Type == EdgeNodeType.Target && !nodesToKeep.Contains(item1.Node)));
                    // Update the nodes to keep to be the source nodes of the interactions of the current step together with the seed nodes.
                    nodesToKeep = list.ElementAt(index)
                        .Select(item => item.EdgeNodes.Where(item1 => item1.Type == EdgeNodeType.Source).Select(item1 => item1.Node))
                        .SelectMany(item => item)
                        .Concat(seedNodes)
                        .Distinct();
                }
                // Return all of the remaining edges.
                return list
                    .SelectMany(item => item)
                    .Distinct();
            }
            // Return null.
            return null;
        }
    }
}
