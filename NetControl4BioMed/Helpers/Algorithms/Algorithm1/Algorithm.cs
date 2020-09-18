using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Algorithms.Algorithm1
{
    /// <summary>
    /// Defines the algorithm and several functions for it.
    /// </summary>
    public static class Algorithm
    {
        /// <summary>
        /// Runs the algorithm on the network with the provided details, using the given parameters.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="analysis">The analysis which to run using the algorithm.</param>
        public static async Task Run(Analysis analysis, IServiceProvider serviceProvider, CancellationToken token)
        {
            // Use a new context instance.
            using var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            // Update the analysis status and stats.
            analysis.ControlPaths = new List<ControlPath>();
            analysis.Status = AnalysisStatus.Initializing;
            analysis.DateTimeStarted = DateTime.UtcNow;
            // Update the analysis.
            await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
            // Reload the analysis for a fresh start.
            await context.Entry(analysis).ReloadAsync();
            // Get the nodes, edges, target nodes and source (preferred) nodes.
            var nodes = context.AnalysisNodes
                .Where(item => item.Analysis == analysis)
                .Where(item => item.Type == AnalysisNodeType.None)
                .Select(item => item.Node.Id)
                .ToList();
            var edges = context.AnalysisEdges
                .Where(item => item.Analysis == analysis)
                .Select(item => item.Edge)
                .Select(item => new
                {
                    SourceNodeId = item.EdgeNodes
                        .Where(item1 => item1.Type == EdgeNodeType.Source)
                        .Select(item1 => item1.Node.Id)
                        .FirstOrDefault(),
                    TargetNodeId = item.EdgeNodes
                        .Where(item1 => item1.Type == EdgeNodeType.Target)
                        .Select(item1 => item1.Node.Id)
                        .FirstOrDefault()
                })
                .Where(item => !string.IsNullOrEmpty(item.SourceNodeId) && !string.IsNullOrEmpty(item.TargetNodeId))
                .AsEnumerable()
                .Select(item => (item.SourceNodeId, item.TargetNodeId))
                .Distinct()
                .ToList();
            var sources = context.AnalysisNodes
                .Where(item => item.Analysis == analysis)
                .Where(item => item.Type == AnalysisNodeType.Source)
                .Select(item => item.Node.Id)
                .ToList();
            var targets = context.AnalysisNodes
                .Where(item => item.Analysis == analysis)
                .Where(item => item.Type == AnalysisNodeType.Target)
                .Select(item => item.Node.Id)
                .ToList();
            // Check if there is any node in an edge that does not appear in the list of nodes.
            if (edges.Select(item => item.Item1).Concat(edges.Select(item => item.Item2)).Distinct().Except(nodes).Any())
            {
                // Update the analysis with an error message.
                analysis.Log = analysis.AppendToLog("There are edges which contain unknown nodes.");
                // Update the analysis status.
                analysis.Status = AnalysisStatus.Error;
                // Update the analysis end time.
                analysis.DateTimeEnded = DateTime.UtcNow;
                // Update the analysis.
                await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                // End the function.
                return;
            }
            // Check if there is any target node that does not appear in the list of nodes.
            if (targets.Except(nodes).Any())
            {
                // Update the analysis with an error message.
                analysis.Log = analysis.AppendToLog("There are unknown target nodes.");
                // Update the analysis status.
                analysis.Status = AnalysisStatus.Error;
                // Update the analysis end time.
                analysis.DateTimeEnded = DateTime.UtcNow;
                // Update the analysis.
                await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                // End the function.
                return;
            }
            // Check if there is any source node that does not appear in the list of nodes.
            if (sources.Except(nodes).Any())
            {
                // Update the analysis with an error message.
                analysis.Log = analysis.AppendToLog("There are unknown source nodes.");
                // Update the analysis status.
                analysis.Status = AnalysisStatus.Error;
                // Update the analysis end time.
                analysis.DateTimeEnded = DateTime.UtcNow;
                // Update the analysis.
                await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                // End the function.
                return;
            }
            // Try to get the parameters for the algorithm.
            if (!analysis.Parameters.TryDeserializeJsonObject<Parameters>(out var parameters))
            {
                // Update the analysis with an error message.
                analysis.Log = analysis.AppendToLog("The parameters are not valid for the algorithm.");
                // Update the analysis status.
                analysis.Status = AnalysisStatus.Error;
                // Update the analysis end time.
                analysis.DateTimeEnded = DateTime.UtcNow;
                // Update the analysis.
                await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                // End the function.
                return;
            }
            // Set up the first iteration.
            var random = new Random(parameters.RandomSeed);
            var currentIteration = analysis.CurrentIteration;
            var currentIterationWithoutImprovement = analysis.CurrentIterationWithoutImprovement;
            var maximumIterations = analysis.MaximumIterations;
            var maximumIterationsWithoutImprovement = analysis.MaximumIterationsWithoutImprovement;
            var bestSolutionSize = targets.Count() + 1;
            var bestControlPaths = new List<Dictionary<string, List<string>>>();
            // Update the parameters.
            var heuristics = JsonSerializer.Deserialize<List<List<string>>>(parameters.Heuristics)
                .TakeWhile(item => !item.Contains("Z"))
                .Select(item => item.Distinct().ToList())
                .Append(new List<string> { "Z" })
                .ToList();
            parameters.Heuristics = JsonSerializer.Serialize(heuristics);
            // Update the analysis status and parameters.
            analysis.Parameters = JsonSerializer.Serialize(parameters);
            analysis.Status = AnalysisStatus.Ongoing;
            // Update the analysis.
            await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
            // Run as long as the analysis exists and the final iteration hasn't been reached.
            while (analysis != null && analysis.Status == AnalysisStatus.Ongoing && currentIteration < maximumIterations && currentIterationWithoutImprovement < maximumIterationsWithoutImprovement && !token.IsCancellationRequested)
            {
                // Move on to the next iterations.
                currentIteration += 1;
                currentIterationWithoutImprovement += 1;
                // Update the iteration count.
                analysis.CurrentIteration = currentIteration;
                analysis.CurrentIterationWithoutImprovement = currentIterationWithoutImprovement;
                // Update the analysis.
                await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                // Define a variable to store the control paths.
                var controlPath = new Dictionary<string, List<string>>();
                // Set up the control path to start from the target nodes.
                foreach (var node in targets)
                {
                    controlPath[node] = new List<string> { node };
                }
                // Start from no repeats.
                var currentRepeat = 0;
                // Repeat as long as we haven't reached the limit.
                while (currentRepeat < parameters.Repeats)
                {
                    // Set the current targets as the initial targets.
                    var currentTargets = new List<string>(targets);
                    // Set the current path length to 0.
                    var currentPathLength = 0;
                    // Get the target nodes to keep. If it is the first check of the current iteration, we have no kept nodes, so the current targets are simply the targets. This is a part of the "repeat" optimization.
                    var keptTargetNodes = GetControllingNodes(controlPath)
                        .Where(item => item.Value.Count() > 1)
                        .Select(item => item.Value)
                        .SelectMany(item => item)
                        .ToHashSet();
                    // Go over each of the paths corresponding to the target nodes to reset.
                    foreach (var item in controlPath.Keys.Except(keptTargetNodes).ToList())
                    {
                        // Reset the control path.
                        controlPath[item] = new List<string>() { item };
                    }
                    // Get the new targets.
                    currentTargets = currentTargets
                        .Except(keptTargetNodes)
                        .ToList();
                    // Run until there are no current targets or we reached the maximum path length.
                    while (currentTargets.Any() && currentPathLength < parameters.MaximumPathLength)
                    {
                        // Set all of the current targets as unmatched.
                        var unmatchedNodes = currentTargets.ToList();
                        // Set all nodes in the network as available to match.
                        var availableNodes = nodes.ToList();
                        // If it is the first check of the current iteration, there are no kept nodes, so the left nodes and edges remain unchanged. Otherwise, remove from the left nodes the corresponding nodes in the current step in the control paths for the kept nodes. This is a part of the "repeat" optimization.
                        availableNodes = availableNodes
                            .Except(controlPath
                                .Where(item => keptTargetNodes.Contains(item.Key))
                                .Select(item => item.Value)
                                .Where(item => currentPathLength + 1 < item.Count())
                                .Select(item => item[currentPathLength + 1]))
                            .ToList();
                        // Define a variable to store the matched edges of the matching.
                        var matchedEdges = new List<(string, string)>();
                        // Go over each heuristic set.
                        foreach (var heuristicSet in heuristics)
                        {
                            // Get the left nodes, right nodes, and edges of the current matching.
                            var leftNodes = availableNodes.ToList();
                            var rightNodes = unmatchedNodes.ToList();
                            var currentEdges = GetSingleHeuristicEdges(leftNodes, rightNodes, edges, heuristicSet, controlPath, sources);
                            var matchingEdges = GetMaximumMatching(leftNodes, rightNodes, currentEdges, random);
                            // Add the matched edges to the list.
                            matchedEdges.AddRange(matchingEdges);
                            availableNodes.RemoveAll(item => matchingEdges.Any(item1 => item1.Item1 == item));
                            unmatchedNodes.RemoveAll(item => matchingEdges.Any(item1 => item1.Item2 == item));
                        }
                        // Update the current targets to the current matched edge source nodes, and the control path.
                        currentTargets = matchedEdges.Select(item => item.Item1).Distinct().ToList();
                        // Create a temporary dictionary to store, for each target, the corresponding current target.
                        var currentTargetDictionary = controlPath.ToDictionary(item => item.Key, item => item.Value.Last());
                        // Go over all of the matched edges.
                        foreach (var edge in matchedEdges)
                        {
                            // Get the keys of all paths to update, as the ones starting with the target node of the edge.
                            var keys = currentTargetDictionary
                                .Where(item => item.Value == edge.Item2)
                                .Select(item => item.Key);
                            // Go over all of the keys.
                            foreach (var key in keys)
                            {
                                // Add the source node of the edge to the corresponding path.
                                controlPath[key].Add(edge.Item1);
                            }
                        }
                        // Update the current path length.
                        currentPathLength++;
                    }
                    // Update the current repeat count.
                    currentRepeat++;
                }
                // Define a variable to store if any path cuts have been performed. This is a part of the "cut-to-driven" optimization.
                var pathCutsPerformed = false;
                // Repeat until there are no more cuts.
                do
                {
                    // Reset the cut paths status.
                    pathCutsPerformed = false;
                    // Get the controlling nodes for the path.
                    var controllingNodes = GetControllingNodes(controlPath).Keys
                        .ToHashSet();
                    // Go over each path in the control path.
                    foreach (var key in controlPath.Keys.ToList())
                    {
                        // Get the first index of any control node.
                        var index = controlPath[key].FindIndex(item => controllingNodes.Contains(item));
                        // Check if the index doesn't correspond to the last element in the list.
                        if (index < controlPath[key].Count() - 1)
                        {
                            // Cut the path up to the first index of any control node.
                            controlPath[key] = controlPath[key].Take(index + 1).ToList();
                            // Mark the cut as performed.
                            pathCutsPerformed = true;
                        }
                    }
                } while (pathCutsPerformed);
                // Compute the result.
                var controlNodes = GetControllingNodes(controlPath).Keys.ToList();
                // Check if the current solution is better than the previously obtained best solutions.
                if (controlNodes.Count() < bestSolutionSize)
                {
                    // Update the best solution.
                    bestSolutionSize = controlNodes.Count();
                    // Reset the number of iterations.
                    currentIterationWithoutImprovement = 0;
                    // Reset the best control paths.
                    bestControlPaths.RemoveAll(item => true);
                }
                // Check if the current solution is as good as the previously obtained best solutions.
                if (controlNodes.Count() == bestSolutionSize)
                {
                    // Check if none of the previous solutions has the same nodes.
                    if (!bestControlPaths.Any(item => !item.Keys.Except(controlPath.Keys).Any() && !controlPath.Keys.Except(item.Keys).Any()))
                    {
                        // Update the best control paths.
                        bestControlPaths.Add(controlPath);
                    }
                }
                // Reload it for the next iteration.
                await context.Entry(analysis).ReloadAsync();
            }
            // Check if the analysis doesn't exist anymore (if it has been deleted).
            if (analysis == null)
            {
                // End the function.
                return;
            }
            // Get the required data.
            var analysisNodes = context.AnalysisNodes
                .Where(item => item.Analysis == analysis)
                .Where(item => item.Type == AnalysisNodeType.None)
                .Select(item => item.Node)
                .ToList();
            var analysisEdges = context.AnalysisEdges
                .Where(item => item.Analysis == analysis)
                .Select(item => item.Edge)
                .Select(item => new
                {
                    Edge = item,
                    SourceNodeId = item.EdgeNodes
                        .Where(item1 => item1.Type == EdgeNodeType.Source)
                        .Select(item1 => item1.Node.Id)
                        .FirstOrDefault(),
                    TargetNodeId = item.EdgeNodes
                        .Where(item1 => item1.Type == EdgeNodeType.Target)
                        .Select(item1 => item1.Node.Id)
                        .FirstOrDefault()
                })
                .Where(item => !string.IsNullOrEmpty(item.SourceNodeId) && !string.IsNullOrEmpty(item.TargetNodeId))
                .ToList();
            // Get the control paths.
            var controlPaths = bestControlPaths.Select(item => new ControlPath
            {
                Paths = item.Values.Select(item1 =>
                {
                    // Get the nodes and edges in the path.
                    var pathNodes = item1
                        .Select(item2 => analysisNodes.FirstOrDefault(item3 => item3.Id == item2))
                        .Where(item2 => item2 != null)
                        .Reverse()
                        .ToList();
                    var pathEdges = item1
                        .Zip(item1.Skip(1), (item2, item3) => (item3.ToString(), item2.ToString()))
                        .Select(item2 => analysisEdges.FirstOrDefault(item3 => item3.SourceNodeId == item2.Item1 && item3.TargetNodeId == item2.Item2))
                        .Where(item2 => item2 != null)
                        .Select(item2 => item2.Edge)
                        .Reverse()
                        .ToList();
                    // Return the path.
                    return new Path
                    {
                        PathNodes = pathNodes.Select((item2, index) => new PathNode { NodeId = item2.Id, Node = item2, Type = PathNodeType.None, Index = index })
                            .Append(new PathNode { NodeId = pathNodes.First().Id, Node = pathNodes.First(), Type = PathNodeType.Source, Index = -1 })
                            .Append(new PathNode { NodeId = pathNodes.Last().Id, Node = pathNodes.Last(), Type = PathNodeType.Target, Index = pathNodes.Count() + 1 })
                            .ToList(),
                        PathEdges = pathEdges.Select((item2, index) => new PathEdge { EdgeId = item2.Id, Edge = item2, Index = index }).ToList()
                    };
                }).ToList()
            }).ToList();
            // Update the analysis.
            analysis.ControlPaths = controlPaths;
            analysis.Status = currentIteration < maximumIterations && currentIterationWithoutImprovement < maximumIterationsWithoutImprovement ? AnalysisStatus.Stopped : AnalysisStatus.Completed;
            analysis.DateTimeEnded = DateTime.UtcNow;
            analysis.Log = analysis.AppendToLog($"The analysis has ended with the status \"{analysis.Status.GetDisplayName()}\".");
            // Update the analysis.
            await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
            // End the function.
            return;
        }

        /// <summary>
        /// Returns a dictionary containing, for each control node, the target nodes that it controls.
        /// </summary>
        /// <param name="controlPath">The control path of the current iteration.</param>
        /// <returns>Returns the dictionary containing, for each control node, the target nodes that it controls.</returns>
        private static Dictionary<string, IEnumerable<string>> GetControllingNodes(Dictionary<string, List<string>> controlPath)
        {
            // Get the pairs of corresponding control nodes and target nodes for each path in the control path.
            var pairs = controlPath
                .Select(item => new
                {
                    TargetNode = item.Key,
                    ControlNode = item.Value.Last()
                });
            // Return, for each control node, the target nodes that it controls.
            return pairs
                .Select(item => item.ControlNode)
                .Distinct()
                .ToDictionary(item => item, item => pairs.Where(item1 => item1.ControlNode == item).Select(item => item.TargetNode));
        }

        /// <summary>
        /// Returns the edges defined by the current heuristic.
        /// </summary>
        /// <param name="leftNodes">Represents the left nodes of the bipartite graph.</param>
        /// <param name="rightNodes">Represents the right nodes of the bipartite graph.</param>
        /// <param name="edges">Represents the edges of the bipartite graph.</param>
        /// <param name="heuristicSet">Represents the heuristic set to be used in the search.</param>
        /// <param name="controlPath">Represents the current control path.</param>
        /// <param name="sources">Represents the source (drug-target) nodes.</param>
        /// <returns></returns>
        private static List<(string, string)> GetSingleHeuristicEdges(List<string> leftNodes, List<string> rightNodes, List<(string, string)> edges, List<string> heuristicSet, Dictionary<string, List<string>> controlPath, List<string> sources)
        {
            // Define the variable to return.
            var heuristicEdges = new List<(string, string)>();
            // Get all edges in the current control path, if needed.
            var currentEdges = heuristicSet.Contains("A") || heuristicSet.Contains("C") || heuristicSet.Contains("E") ?
                controlPath.Select(item => item.Value.Zip(item.Value.Skip(1), (item1, item2) => (item2.ToString(), item1.ToString())))
                    .SelectMany(item => item)
                    .Distinct():
                Enumerable.Empty<(string, string)>();
            // Get all already identified control nodes, if needed.
            var currentLength = controlPath.Max(item => item.Value.Count());
            var controlNodes = heuristicSet.Contains("C") || heuristicSet.Contains("D") ?
                controlPath.Where(item => item.Value.Count() < currentLength)
                    .Select(item => item.Value.Last())
                    .AsEnumerable() :
                Enumerable.Empty<string>();
            // Get all previously seen nodes in the control paths, if needed.
            var currentNodes = heuristicSet.Contains("E") || heuristicSet.Contains("F") ?
                controlPath.Select(item => item.Value)
                    .SelectMany(item => item)
                    .Distinct() :
                Enumerable.Empty<string>();
            // Get all edges starting from a source node and ending in target nodes, if needed.
            var sourceEdges = heuristicSet.Contains("A") || heuristicSet.Contains("B") ?
                edges.Where(item => rightNodes.Contains(item.Item2) && sources.Contains(item.Item1)) :
                Enumerable.Empty<(string, string)>();
            // Get all edges starting from an already identified control node and ending in target nodes, if needed.
            var controlEdges = heuristicSet.Contains("C") || heuristicSet.Contains("D") ?
                edges.Where(item => rightNodes.Contains(item.Item2) && controlNodes.Contains(item.Item1)) :
                Enumerable.Empty<(string, string)>();
            // Get all edges starting from a previously seen node, if needed.
            var seenEdges = heuristicSet.Contains("E") || heuristicSet.Contains("F") ?
                edges.Where(item => rightNodes.Contains(item.Item2) && currentNodes.Contains(item.Item1)) :
                Enumerable.Empty<(string, string)>();
            // Get all edges not starting from a node in the current control path, if needed (to avoid loops).
            var differentEdges = heuristicSet.Contains("G") ?
                edges.Where(item => rightNodes.Contains(item.Item2) && !controlPath.Where(item1 => item1.Value.Last() == item.Item2).Any(item1 => item1.Value.Contains(item.Item1))) :
                Enumerable.Empty<(string, string)>();
            // Get all possible edges.
            var allEdges = edges.Where(item => rightNodes.Contains(item.Item2));
            // Go over all heuristic items.
            foreach (var heuristic in heuristicSet)
            {
                // Check the current heuristic item.
                switch (heuristic)
                {
                    case "A":
                        // Add previously seen edges from source nodes.
                        heuristicEdges.AddRange(currentEdges.Intersect(sourceEdges));
                        // End the switch.
                        break;
                    case "B":
                        // Add all edges from source nodes.
                        heuristicEdges.AddRange(sourceEdges);
                        // End the switch.
                        break;
                    case "C":
                        // Add previously seen edges from already identified control nodes.
                        heuristicEdges.AddRange(currentEdges.Intersect(controlEdges));
                        // End the switch.
                        break;
                    case "D":
                        // Add all edges from already identified control nodes.
                        heuristicEdges.AddRange(controlEdges);
                        // End the switch.
                        break;
                    case "E":
                        // Add previously seen edges.
                        heuristicEdges.AddRange(currentEdges.Intersect(seenEdges));
                        // End the switch.
                        break;
                    case "F":
                        // Add all edges from previously seen nodes.
                        heuristicEdges.AddRange(seenEdges);
                        // End the switch.
                        break;
                    case "G":
                        // Add all edges not starting from a node in the current control path (to avoid loops).
                        heuristicEdges.AddRange(differentEdges);
                        // End the switch.
                        break;
                    case "Z":
                        // Add all possible edges.
                        heuristicEdges.AddRange(allEdges);
                        // End the switch.
                        break;
                    default:
                        // End the switch.
                        break;
                }
            }
            // Return all edges that start in a left node.
            return heuristicEdges.Where(item => leftNodes.Contains(item.Item1)).ToList();
        }

        /// <summary>
        /// Represents the Hopcroft-Karp algorithm for maximum matching in a bipartite graph.
        /// </summary>
        /// <param name="leftNodes">Represents the left nodes of the bipartite graph.</param>
        /// <param name="rightNodes">Represents the right nodes of the bipartite graph.</param>
        /// <param name="edges">Represents the edges of the bipartite graph.</param>
        /// <param name="rand">Represents the random variable for choosing randomly a maximum matching.</param>
        /// <returns>Returns the list of edges corresponding to a maximum matching for the bipartite graph.</returns>
        /// <remarks>The implementation is a slightly modified version from the one found on https://en.wikipedia.org/wiki/Hopcroft–Karp_algorithm, written in C#.</remarks>
        private static List<(string, string)> GetMaximumMatching(List<string> leftNodes, List<string> rightNodes, List<(string, string)> edges, Random rand)
        {
            // The Wikipedia algorithm uses considers the left nodes as U, and the right ones as V. But, as the unmatched nodes are considered, in order, from the left side of the bipartite graph, the obtained matching would not be truly random, especially on the first step. That is why I perform here a simple switch, by inter-changing the lists U and V (left and right side nodes), and using the opposite direction edges, in order to obtained a random maximum matching.
            var U = rightNodes;
            var V = leftNodes;
            var oppositeEdges = edges.Select((item) => (item.Item2, item.Item1)).ToList();
            // The actual algorithm starts from here.
            var PairU = new Dictionary<string, string>();
            var PairV = new Dictionary<string, string>();
            var dist = new Dictionary<string, int>();
            foreach (var u in U)
            {
                PairU[u] = "null";
            }
            foreach (var v in V)
            {
                PairV[v] = "null";
            }
            var matching = 0;
            while (BreadthFirstSearch(U, PairU, PairV, dist, oppositeEdges, rand))
            {
                foreach (var u in U)
                {
                    if (PairU[u] == "null")
                    {
                        if (DepthFirstSearch(u, PairU, PairV, dist, oppositeEdges, rand))
                        {
                            matching++;
                        }
                    }
                }
            }
            // Instead of the number of performed matchings, we will return the actual edges corresponding to these matchings.
            // Because in the beginning of the function we switched the direction of the edges, now we switch back the direction of
            // the matched edges, to fit in with the rest of the program.
            var matchedEdges = new List<(string, string)>();
            foreach (var item in PairU)
            {
                // We will return only edges corresponding to the matching, that is edges with both nodes not null.
                if (item.Value != "null")
                {
                    // Here we perform the switch back to the original direction. Otherwise we would return the pair (key, value).
                    matchedEdges.Add((item.Value, item.Key));
                }
            }
            return matchedEdges;
        }

        /// <summary>
        /// The breadth-first search of the Hopcroft-Karp maximum matching algorithm.
        /// </summary>
        /// <param name="U">The left nodes of the bipartite graph.</param>
        /// <param name="PairU">Dictionary containing a matching from the nodes on the left to nodes on the right.</param>
        /// <param name="PairV">Dictionary containing a matching from the nodes on the right to nodes on the left.</param>
        /// <param name="dist"></param>
        /// <param name="edges">List of edges in the bipartite graph.</param>
        /// <param name="rand">The random variable for choosing randomly a maximum matching.</param>
        /// <returns></returns>
        private static bool BreadthFirstSearch(List<string> U, Dictionary<string, string> PairU, Dictionary<string, string> PairV, Dictionary<string, int> dist, List<(string, string)> edges, Random rand)
        {
            var queue = new Queue<string>();
            foreach (var u in U)
            {
                if (PairU[u] == "null")
                {
                    dist[u] = 0;
                    queue.Enqueue(u);
                }
                else
                {
                    dist[u] = int.MaxValue;
                }
            }
            dist["null"] = int.MaxValue;
            while (queue.Any())
            {
                var u = queue.Dequeue();
                if (dist[u] < dist["null"])
                {
                    var adj = edges.Where((edge) => edge.Item1 == u).Select((edge) => edge.Item2).OrderBy((item) => rand.Next());
                    foreach (var v in adj)
                    {
                        if (dist[PairV[v]] == int.MaxValue)
                        {
                            dist[PairV[v]] = dist[u] + 1;
                            queue.Enqueue(PairV[v]);
                        }
                    }
                }
            }
            return dist["null"] != int.MaxValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="u">A node from the left part of the bipartite graph.</param>
        /// <param name="PairU">Dictionary containing a matching from the nodes on the left to nodes on the right.</param>
        /// <param name="PairV">Dictionary containing a matching from the nodes on the right to nodes on the left.</param>
        /// <param name="dist"></param>
        /// <param name="edges">List of edges in the bipartite graph.</param>
        /// <param name="rand">The random variable for choosing randomly a maximum matching.</param>
        /// <returns></returns>
        private static bool DepthFirstSearch(string u, Dictionary<string, string> PairU, Dictionary<string, string> PairV, Dictionary<string, int> dist, List<(string, string)> edges, Random rand)
        {
            if (u != "null")
            {
                var adj = edges.Where((edge) => edge.Item1 == u).Select((edge) => edge.Item2).OrderBy((item) => rand.Next());
                foreach (var v in adj)
                {
                    if (dist[PairV[v]] == dist[u] + 1)
                    {
                        if (DepthFirstSearch(PairV[v], PairU, PairV, dist, edges, rand))
                        {
                            PairV[v] = u;
                            PairU[u] = v;
                            return true;
                        }
                    }
                }
                dist[u] = int.MaxValue;
                return false;
            }
            return true;
        }
    }
}
