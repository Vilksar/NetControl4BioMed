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
        /// <param name="context">The database context of the analysis.</param>
        /// <param name="analysis">The analysis which to run using the algorithm.</param>
        public static void Run(Analysis analysis, ApplicationDbContext context, CancellationToken token)
        {
            // Update the analysis status and stats.
            analysis.ControlPaths = new List<ControlPath>();
            analysis.Status = AnalysisStatus.Initializing;
            analysis.DateTimeStarted = DateTime.Now;
            // Update the analysis.
            IEnumerableExtensions.Edit(analysis.Yield(), context, token);
            // Reload the analysis for a fresh start.
            Task.Run(() => context.Entry(analysis).ReloadAsync()).Wait();
            // Get the nodes, edges, target nodes and source (preferred) nodes.
            var nodes = analysis.AnalysisNodes
                .Where(item => item.Type == AnalysisNodeType.None)
                .Select(item => item.Node.Id)
                .ToList();
            var edges = analysis.AnalysisEdges
                .Select(item => (item.Edge.EdgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Source), item.Edge.EdgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Target)))
                .Where(item => item.Item1 != null && item.Item2 != null)
                .Select(item => (item.Item1.Node.Id, item.Item2.Node.Id))
                .ToList();
            var targets = analysis.AnalysisNodes
                .Where(item => item.Type == AnalysisNodeType.Target)
                .Select(item => item.Node.Id)
                .ToList();
            var sources = analysis.AnalysisNodes
                .Where(item => item.Type == AnalysisNodeType.Source)
                .Select(item => item.Node.Id)
                .ToList();
            // Check if there is any node in an edge that does not appear in the list of nodes.
            if (edges.Select(item => item.Item1).Concat(edges.Select(item => item.Item2)).Except(nodes).Any())
            {
                // Update the analysis with an error message.
                analysis.Log = analysis.AppendToLog("There are edges which contain unknown nodes.");
                // Update the analysis status.
                analysis.Status = AnalysisStatus.Error;
                // Update the analysis end time.
                analysis.DateTimeEnded = DateTime.Now;
                // Update the analysis.
                IEnumerableExtensions.Edit(analysis.Yield(), context, token);
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
                analysis.DateTimeEnded = DateTime.Now;
                // Update the analysis.
                IEnumerableExtensions.Edit(analysis.Yield(), context, token);
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
                analysis.DateTimeEnded = DateTime.Now;
                // Update the analysis.
                IEnumerableExtensions.Edit(analysis.Yield(), context, token);
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
                analysis.DateTimeEnded = DateTime.Now;
                // Update the analysis.
                IEnumerableExtensions.Edit(analysis.Yield(), context, token);
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
            var heuristics = JsonSerializer.Deserialize<List<List<string>>>(parameters.Heuristics).TakeWhile(item => !item.Contains("Z")).Select(item => item.Distinct().ToList()).Append(new List<string> { "Z" }).ToList();
            parameters.Heuristics = JsonSerializer.Serialize(heuristics);
            // Update the analysis status and parameters.
            analysis.Parameters = JsonSerializer.Serialize(parameters);
            analysis.Status = AnalysisStatus.Ongoing;
            // Update the analysis.
            IEnumerableExtensions.Edit(analysis.Yield(), context, token);
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
                IEnumerableExtensions.Edit(analysis.Yield(), context, token);
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
                    // Get the target nodes to keep. The optimization part for the "repeats" starts here. If it is the first check of the current iteration, we have no kept nodes, so the current targets are simply the targets.
                    var keptNodes = GetControllingNodes(controlPath).Where(item => item.Value.Count() > 1).Select(item => item.Value).SelectMany(item => item);
                    // Go over each of the paths corresponding to the target nodes to reset.
                    foreach (var item in controlPath.Keys.Except(keptNodes).ToList())
                    {
                        // Reset the control path.
                        controlPath[item] = new List<string>() { item };
                    }
                    // Get the new targets.
                    currentTargets = currentTargets.Except(keptNodes).ToList();
                    // Run until there are no current targets or we reached the maximum path length.
                    while (currentTargets.Any() && currentPathLength < parameters.MaximumPathLength)
                    {
                        // Set all of the current targets as unmatched.
                        var unmatched = new List<string>(); unmatched.AddRange(currentTargets);
                        // Set all nodes in the network as free.
                        var free = new List<string>(); free.AddRange(nodes);
                        // If it is the first check of the current iteration, there are no kept nodes, so the left nodes and edges remain unchanged. Otherwise, remove from the left nodes the corresponding nodes in the current step in the control paths for the kept nodes. The optimization part for the "repeat" begins here.
                        foreach (var item in keptNodes)
                        {
                            if (currentPathLength + 1 < controlPath[item].Count)
                            {
                                var leftNode = controlPath[item][currentPathLength + 1];
                                free.Remove(leftNode);
                            }
                        }
                        // We determine a maximal matching by computing several maximum matchings, in steps, for each heuristic.
                        var matchedEdges = new List<(string, string)>();
                        foreach (var heuristic in heuristics)
                        {
                            // The left nodes, right nodes, and edges of the current matching.
                            var left = new List<string>(); left.AddRange(free);
                            var right = new List<string>(); right.AddRange(unmatched);
                            var currentEdges = new List<(string, string)>();
                            currentEdges = GetSingleHeuristicEdges(left, right, edges, heuristic, controlPath, sources);
                            var matchingEdges = GetMaximumMatching(left, right, currentEdges, random);
                            // For all matching edges, add them to the maximal matchings, and remove their corresponding nodes from free and unmatched lists.
                            foreach (var edge in matchingEdges)
                            {
                                matchedEdges.Add(edge);
                                free.Remove(edge.Item1);
                                unmatched.Remove(edge.Item2);
                            }
                        }
                        // Update the current targets to the current matched edge source nodes, and the control path.
                        currentTargets = matchedEdges.Select((edge) => edge.Item1).Distinct().ToList();
                        // Go over all of the matched edges.
                        foreach (var item in matchedEdges)
                        {
                            // Go over all of the paths in the control path that start from the target node of the edge.
                            foreach (var item2 in controlPath.Where(item3 => item3.Value.Last() == item.Item2))
                            {
                                // Add the source node of the edge to the path.
                                item2.Value.Add(item.Item1);
                            }
                        }
                        // Update the current path length.
                        currentPathLength++;
                    }
                    // Update the current repeat count.
                    currentRepeat++;
                }
                // The optimization part for the "cut to driven" parameter begins here.
                var stop = false;
                while (!stop)
                {
                    stop = true;
                    foreach (var item1 in controlPath)
                    {
                        var controllingNode = item1.Value.Last();
                        foreach (var item2 in controlPath)
                        {
                            var firstIndex = item2.Value.IndexOf(controllingNode);
                            if (firstIndex != -1 && firstIndex != item2.Value.Count - 1)
                            {
                                item2.Value.RemoveRange(firstIndex, item2.Value.Count - 1 - firstIndex);
                                stop = false;
                            }
                        }
                    }
                }
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
                    // Update the best control paths.
                    bestControlPaths.Add(controlPath);
                }
                // Check if the current solution is as good as the previously obtained best solutions.
                if (controlNodes.Count() == bestSolutionSize)
                {
                    // Check if none of the previous solutions has the same nodes.
                    if (!bestControlPaths.Any(item => item.Keys.All(key => controlPath.ContainsKey(key) && !item[key].Except(controlPath[key]).Any() && !controlPath[key].Except(item[key]).Any())))
                    {
                        // Update the best control paths.
                        bestControlPaths.Add(controlPath);
                    }
                }
                // Reload it for the next iteration.
                Task.Run(() => context.Entry(analysis).ReloadAsync()).Wait();
            }
            // Check if the analysis doesn't exist anymore (if it has been deleted).
            if (analysis == null)
            {
                // End the function.
                return;
            }
            // Get the control paths.
            var controlPaths = bestControlPaths.Select(item => new ControlPath
            {
                Paths = item.Values.Select(item1 =>
                {
                    // Get the nodes and edges in the path.
                    var pathNodes = item1
                        .Select(item2 => analysis.AnalysisNodes.First(item3 => item3.Node.Id == item2).Node)
                        .Where(item => item != null)
                        .ToList();
                    var pathEdges = item1
                        .Zip(item1.Skip(1), (item2, item3) => (item2.ToString(), item3.ToString()))
                        .Select(item2 => analysis.AnalysisEdges.First(item3 => item3.Edge.EdgeNodes.First(item4 => item4.Type == EdgeNodeType.Source).Node.Id == item2.Item2 && item3.Edge.EdgeNodes.First(item4 => item4.Type == EdgeNodeType.Target).Node.Id == item2.Item1).Edge)
                        .ToList();
                    // Return the path.
                    return new Path
                    {
                        PathNodes = pathNodes.Select(item2 => new PathNode { NodeId = item2.Id, Node = item2, Type = PathNodeType.None })
                            .Append(new PathNode { NodeId = pathNodes.First().Id, Node = pathNodes.First(), Type = PathNodeType.Target })
                            .Append(new PathNode { NodeId = pathNodes.Last().Id, Node = pathNodes.Last(), Type = PathNodeType.Source })
                            .ToList(),
                        PathEdges = pathEdges.Select(item2 => new PathEdge { EdgeId = item2.Id, Edge = item2 }).ToList()
                    };
                }).ToList()
            }).ToList();
            // Update the analysis.
            analysis.ControlPaths = controlPaths;
            analysis.Status = currentIteration < maximumIterations && currentIterationWithoutImprovement < maximumIterationsWithoutImprovement ? AnalysisStatus.Stopped : AnalysisStatus.Completed;
            analysis.DateTimeEnded = DateTime.Now;
            analysis.Log = analysis.AppendToLog($"The analysis has ended with the status \"{analysis.Status.GetDisplayName()}\".");
            // Update the analysis.
            IEnumerableExtensions.Edit(analysis.Yield(), context, token);
        }

        /// <summary>
        /// Returns a dictionary containing, for each control node, the target nodes that it controls.
        /// </summary>
        /// <param name="controlPath">The control path of the current iteration.</param>
        /// <returns>Returns the dictionary containing, for each control node, the target nodes that it controls.</returns>
        private static Dictionary<string, IEnumerable<string>> GetControllingNodes(Dictionary<string, List<string>> controlPath)
        {
            // Get the pairs of corresponding control nodes and target nodes for each path in the control path.
            var pairs = controlPath.Select(item => (item.Key.ToString(), item.Value.Last()));
            // Return, for each control node, the target nodes that it controls.
            return pairs.Select(item => item.Item1).ToDictionary(item => item, item => pairs.Where(item1 => item1.Item1 == item).Select(item => item.Item2));
        }

        /// <summary>
        /// Returns the edges defined by the current heuristic.
        /// </summary>
        /// <param name="leftNodes">Represents the left nodes of the bipartite graph.</param>
        /// <param name="rightNodes">Represents the right nodes of the bipartite graph.</param>
        /// <param name="edges">Represents the edges of the bipartite graph.</param>
        /// <param name="heuristic">Represents the heuristic to be used in the search.</param>
        /// <param name="controlPath">Represents the current control path.</param>
        /// <param name="sources">Represents the source (drug-target) nodes.</param>
        /// <returns></returns>
        private static List<(string, string)> GetSingleHeuristicEdges(List<string> leftNodes, List<string> rightNodes, List<(string, string)> edges, List<string> heuristic, Dictionary<string, List<string>> controlPath, List<string> sources)
        {
            // Define the variable to return.
            var heuristicEdges = new List<(string, string)>();
            // Get all edges in the current control path, if needed.
            var currentEdges = new List<(string, string)>();
            if (heuristic.Contains("A") || heuristic.Contains("C") || heuristic.Contains("E"))
            {
                foreach (var item in controlPath)
                {
                    for (int index = 0; index < item.Value.Count - 1; index++)
                    {
                        currentEdges.Add((item.Value[index + 1], item.Value[index]));
                    }
                }
                currentEdges = currentEdges.Distinct().ToList();
            }
            // Get all existing driven nodes, if needed.
            var drivenNodes = new List<string>();
            var currentLength = controlPath.Max((item) => item.Value.Count);
            if (heuristic.Contains("C") || heuristic.Contains("D"))
            {
                foreach (var item in controlPath)
                {
                    if (item.Value.Count < currentLength)
                    {
                        drivenNodes.Add(item.Value.Last());
                    }
                }
            }
            // Get all previously seen nodes in the control paths, if needed.
            var currentNodes = new List<string>();
            if (heuristic.Contains("E") || heuristic.Contains("F"))
            {
                foreach (var item in controlPath)
                {
                    foreach (var node in item.Value)
                    {
                        currentNodes.Add(node);
                    }
                }
                currentNodes = currentNodes.Distinct().ToList();
            }
            // Get all edges starting from a drug target node and ending in target nodes, if needed.
            var temporaryDrugEdges = new List<(string, string)>();
            if (sources != null && (heuristic.Contains("A") || heuristic.Contains("B")))
            {
                temporaryDrugEdges = edges.Where((edge) => rightNodes.Contains(edge.Item2) && sources.Contains(edge.Item1)).ToList();
            }
            // Get all edges starting from an already driven node and ending in target nodes, if needed.
            var temporaryDrivenEdges = new List<(string, string)>();
            if (heuristic.Contains("C") || heuristic.Contains("D"))
            {
                temporaryDrivenEdges = edges.Where((edge) => rightNodes.Contains(edge.Item2) && drivenNodes.Contains(edge.Item1)).ToList();
            }
            // Get all edges starting from a previously seen node in the control paths, if needed.
            var temporarySeenEdges = new List<(string, string)>();
            if (heuristic.Contains("E") || heuristic.Contains("F"))
            {
                temporarySeenEdges = edges.Where((edge) => rightNodes.Contains(edge.Item2) && currentNodes.Contains(edge.Item1)).ToList();
            }
            // Get all edges not starting from a node in the current control path for a node, if needed (to avoid loops).
            var temporaryControlEdges = new List<(string, string)>();
            if (heuristic.Contains("G"))
            {
                foreach (var target in rightNodes)
                {
                    // Get the nodes in the current control path.
                    var currentPathNodes = controlPath.First((item) => item.Value.Last() == target).Value.Distinct();
                    // Get all edges from nodes not in the current control path to the current target and add them to list.
                    temporaryControlEdges.AddRange(edges.Where((edge) => edge.Item2 == target && !currentPathNodes.Contains(edge.Item1)));
                }
            }
            // Get all possible edges.
            var allEdges = edges.Where((edge) => rightNodes.Contains(edge.Item2)).ToList();
            // For every target node.
            foreach (var target in rightNodes)
            {
                // We define a new list of edges which will end in the target node.
                var temporaryHeuristicEdges = new List<(string, string)>();
                // For all heuristic strings, separated by ";"
                foreach (var h in heuristic)
                {
                    // Add previously seen edges from drug-target nodes.
                    if (sources != null && h == "A")
                    {
                        temporaryHeuristicEdges.AddRange(currentEdges.Where((edge) => edge.Item2 == target).Intersect(temporaryDrugEdges.Where((edge) => edge.Item2 == target)));
                    }
                    // Add all edges from drug-target nodes.
                    else if (sources != null && h == "B")
                    {
                        temporaryHeuristicEdges.AddRange(temporaryDrugEdges.Where((edge) => edge.Item2 == target));
                    }
                    // Add previously seen edges from already driven nodes.
                    else if (h == "C")
                    {
                        temporaryHeuristicEdges.AddRange(currentEdges.Where((edge) => edge.Item2 == target).Intersect(temporaryDrivenEdges.Where((edge) => edge.Item2 == target)));
                    }
                    // Add all edges from already driven nodes.
                    else if (h == "D")
                    {
                        temporaryHeuristicEdges.AddRange(temporaryDrivenEdges.Where((edge) => edge.Item2 == target));
                    }
                    // Add previously seen edges from anywhere in the control path.
                    else if (h == "E")
                    {
                        temporaryHeuristicEdges.AddRange(currentEdges.Where((edge) => edge.Item2 == target).Intersect(temporarySeenEdges.Where((edge) => edge.Item2 == target)));
                    }
                    // Add all edges from previously seen nodes anywhere in the control path.
                    else if (h == "F")
                    {
                        temporaryHeuristicEdges.AddRange(temporarySeenEdges.Where((edge) => edge.Item2 == target));
                    }
                    // Add all edges from a node that has not appeared in the current control path (to preferably avoid loops).
                    else if (h == "G")
                    {
                        temporaryHeuristicEdges.AddRange(temporaryControlEdges.Where((edge) => edge.Item2 == target));
                    }
                    // Add all possible edges.
                    else if (h == "Z")
                    {
                        temporaryHeuristicEdges.AddRange(allEdges.Where((edge) => edge.Item2 == target));
                    }
                }
                // We add the heuristic edges of the current node to the list of edges.
                heuristicEdges.AddRange(temporaryHeuristicEdges);
            }
            // Remove all edges which don't start from one of the left nodes.
            heuristicEdges = heuristicEdges.Where((edge) => leftNodes.Contains(edge.Item1)).ToList();
            // We return all of the obtained edges.
            return heuristicEdges;
        }

        /// <summary>
        /// Represents the Hopcroft-Karp algorithm for maximum matching in a bipartite graph.
        /// The implementation is a slightly modified version from the one found on https://en.wikipedia.org/wiki/Hopcroft–Karp_algorithm.
        /// </summary>
        /// <param name="leftNodes">Represents the left nodes of the bipartite graph.</param>
        /// <param name="rightNodes">Represents the right nodes of the bipartite graph.</param>
        /// <param name="edges">Represents the edges of the bipartite graph.</param>
        /// <param name="rand">Represents the random variable for choosing randomly a maximum matching.</param>
        /// <returns>Returns the list of edges corresponding to a maximum matching for the bipartite graph.</returns>
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
