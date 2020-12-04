using MathNet.Numerics.LinearAlgebra;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Algorithms.Analyses.Genetic
{
    /// <summary>
    /// Defines the algorithm and several functions for it.
    /// </summary>
    public static class Algorithm
    {
        /// <summary>
        /// Runs the algorithm on the analysis with the provided details, using the given parameters.
        /// </summary>
        /// <param name="analysisId">The ID of the analysis which to run using the algorithm.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        public static async Task Run(string analysisId, IServiceProvider serviceProvider, CancellationToken token)
        {
            // Define the required data.
            var analysisNodeIds = new List<string>();
            var analysisEdgeIds = new List<(string, string, string)>();
            var nodes = new List<string>();
            var edges = new List<(string, string)>();
            var sources = new List<string>();
            var targets = new List<string>();
            var parameters = new Parameters();
            var currentIteration = 0;
            var currentIterationWithoutImprovement = 0;
            var maximumIterations = 100;
            var maximumIterationsWithoutImprovement = 25;
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Reload the network.
                var analysis = context.Analyses
                    .FirstOrDefault(item => item.Id == analysisId);
                // Check if there was no item found.
                if (analysis == null)
                {
                    // Return.
                    return;
                }
                // Get the required data.
                analysisNodeIds = context.AnalysisNodes
                    .Where(item => item.Analysis == analysis)
                    .Where(item => item.Type == AnalysisNodeType.None)
                    .Select(item => item.Node.Id)
                    .ToList();
                analysisEdgeIds = context.AnalysisEdges
                    .Where(item => item.Analysis == analysis)
                    .Select(item => item.Edge)
                    .Select(item => new
                    {
                        Edge = item.Id,
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
                    .Select(item => (item.SourceNodeId, item.TargetNodeId, item.Edge))
                    .Distinct()
                    .ToList();
                // Get the nodes, edges, target nodes and source (preferred) nodes.
                nodes = analysisNodeIds
                    .ToList();
                edges = analysisEdgeIds
                    .Select(item => (item.Item1, item.Item2))
                    .ToList();
                sources = context.AnalysisNodes
                    .Where(item => item.Analysis == analysis)
                    .Where(item => item.Type == AnalysisNodeType.Source)
                    .Select(item => item.Node.Id)
                    .ToList();
                targets = context.AnalysisNodes
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
                    await IEnumerableExtensions.EditAsync(analysis.Yield(), context, token);
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
                    await IEnumerableExtensions.EditAsync(analysis.Yield(), context, token);
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
                    await IEnumerableExtensions.EditAsync(analysis.Yield(), context, token);
                    // End the function.
                    return;
                }
                // Try to get the parameters for the algorithm.
                if (!analysis.Parameters.TryDeserializeJsonObject<Parameters>(out parameters))
                {
                    // Update the analysis with an error message.
                    analysis.Log = analysis.AppendToLog("The parameters are not valid for the algorithm.");
                    // Update the analysis status.
                    analysis.Status = AnalysisStatus.Error;
                    // Update the analysis end time.
                    analysis.DateTimeEnded = DateTime.UtcNow;
                    // Update the analysis.
                    await IEnumerableExtensions.EditAsync(analysis.Yield(), context, token);
                    // End the function.
                    return;
                }
                // Set up the first iteration.
                currentIteration = analysis.CurrentIteration;
                currentIterationWithoutImprovement = analysis.CurrentIterationWithoutImprovement;
                maximumIterations = analysis.MaximumIterations;
                maximumIterationsWithoutImprovement = analysis.MaximumIterationsWithoutImprovement;
            }
            // Get the additional needed variables.
            var nodeIndex = nodes.Select((item, index) => (item, index)).ToDictionary(item => item.item, item => item.index);
            var nodeIsPreferred = nodes.ToDictionary(item => item, item => sources.Contains(item));
            var matrixA = GetMatrixA(nodeIndex, edges);
            var matrixC = GetMatrixC(nodeIndex, targets);
            var powersMatrixA = GetPowersMatrixA(matrixA, parameters.MaximumPathLength);
            var powersMatrixCA = GetPowersMatrixCA(matrixC, powersMatrixA);
            var targetAncestors = GetTargetAncestors(powersMatrixA, targets, nodeIndex);
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Reload the network.
                var analysis = context.Analyses
                    .FirstOrDefault(item => item.Id == analysisId);
                // Check if there was no item found.
                if (analysis == null)
                {
                    // Return.
                    return;
                }
                // Update the analysis status.
                analysis.Status = AnalysisStatus.Ongoing;
                // Add a message to the log.
                analysis.Log = analysis.AppendToLog("The analysis is now running.");
                // Update the analysis.
                await IEnumerableExtensions.EditAsync(analysis.Yield(), context, token);
            }
            // Define the required data.
            var analysisStillExists = true;
            var analysisStatus = AnalysisStatus.Ongoing;
            var controlPaths = new List<ControlPath>();
            // Use a new timer to display the progress.
            using (new Timer(async (state) =>
            {
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Reload the network.
                    var analysis = context.Analyses
                        .FirstOrDefault(item => item.Id == analysisId);
                    // Update the loop variables.
                    analysisStillExists = analysis != null;
                    // Check if there was no item found.
                    if (analysis == null)
                    {
                        // Return.
                        return;
                    }
                    // Update the loop variables.
                    analysisStatus = analysis.Status;
                    // Update the iteration count.
                    analysis.CurrentIteration = currentIteration;
                    analysis.CurrentIterationWithoutImprovement = currentIterationWithoutImprovement;
                    // Update the analysis.
                    await IEnumerableExtensions.EditAsync(analysis.Yield(), context, token);
                }
            }, null, TimeSpan.FromSeconds(0.0), TimeSpan.FromSeconds(30.0)))
            {
                // Set up the first iteration.
                var random = new Random(parameters.RandomSeed);
                var bestSolutionSize = 0.0;
                // Initialize a new population.
                var population = new Population(nodeIndex, targets, targetAncestors, powersMatrixCA, nodeIsPreferred, parameters, random);
                // Run as long as the analysis exists and the final iteration hasn't been reached.
                while (analysisStillExists && analysisStatus == AnalysisStatus.Ongoing && currentIteration < maximumIterations && currentIterationWithoutImprovement < maximumIterationsWithoutImprovement && !token.IsCancellationRequested)
                {
                    // Move on to the next iterations.
                    currentIteration += 1;
                    currentIterationWithoutImprovement += 1;
                    // Move on to the next population.
                    population = new Population(population, nodeIndex, targets, targetAncestors, powersMatrixCA, nodeIsPreferred, parameters, random);
                    // Get the best fitness of the current solution.
                    var currentSolutionSize = population.GetFitnessList().Max();
                    // Check if the current solution is better than the previously obtained best solutions.
                    if (bestSolutionSize < currentSolutionSize)
                    {
                        // Update the best solution.
                        bestSolutionSize = currentSolutionSize;
                        // Reset the number of iterations.
                        currentIterationWithoutImprovement = 0;
                    }
                }
                // Get the control paths.
                controlPaths = population.GetControlPaths(nodeIndex, nodes, edges).Select(item => new ControlPath
                {
                    Paths = item.Values.Select(item1 =>
                    {
                        // Get the nodes and edges in the path.
                        var pathNodes = item1
                            .Select(item2 => analysisNodeIds.FirstOrDefault(item3 => item3 == item2))
                            .Where(item2 => item2 != null)
                            .Reverse()
                            .ToList();
                        var pathEdges = item1
                            .Zip(item1.Skip(1), (item2, item3) => (item3.ToString(), item2.ToString()))
                            .Select(item2 => analysisEdgeIds.FirstOrDefault(item3 => item3.Item1 == item2.Item1 && item3.Item2 == item2.Item2))
                            .Select(item2 => item2.Item3)
                            .Where(item2 => !string.IsNullOrEmpty(item2))
                            .Reverse()
                            .ToList();
                        // Return the path.
                        return new Path
                        {
                            PathNodes = pathNodes.Select((item2, index) => new PathNode { NodeId = item2, Type = PathNodeType.None, Index = index })
                                .Append(new PathNode { NodeId = pathNodes.First(), Type = PathNodeType.Source, Index = -1 })
                                .Append(new PathNode { NodeId = pathNodes.Last(), Type = PathNodeType.Target, Index = pathNodes.Count() })
                                .ToList(),
                            PathEdges = pathEdges.Select((item2, index) => new PathEdge { EdgeId = item2, Index = index }).ToList()
                        };
                    }).ToList()
                }).ToList();
            }
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Reload the network.
                var analysis = context.Analyses
                    .FirstOrDefault(item => item.Id == analysisId);
                // Update the loop variables.
                analysisStillExists = analysis != null;
                // Check if there was no item found.
                if (analysis == null)
                {
                    // Return.
                    return;
                }
                // Update the analysis.
                analysis.CurrentIteration = currentIteration;
                analysis.CurrentIterationWithoutImprovement = currentIterationWithoutImprovement;
                analysis.ControlPaths = controlPaths;
                // Update the analysis.
                await IEnumerableExtensions.EditAsync(analysis.Yield(), context, token);
            }
            // End the function.
            return;
        }

        /// <summary>
        /// Computes the A matrix (corresponding to the adjacency matrix).
        /// </summary>
        /// <param name="nodeIndices">The dictionary containing, for each node, its index in the node list.</param>
        /// <param name="edges">The edges of the graph.</param>
        /// <returns>The A matrix (corresponding to the adjacency matrix).</returns>
        private static Matrix<double> GetMatrixA(Dictionary<string, int> nodeIndices, List<(string, string)> edges)
        {
            // Initialize the adjacency matrix with zero.
            var matrixA = Matrix<double>.Build.DenseDiagonal(nodeIndices.Count(), nodeIndices.Count(), 0.0);
            // Go over each of the edges.
            foreach (var edge in edges)
            {
                // Set to 1.0 the corresponding entry in the matrix (source nodes are on the columns, target nodes are on the rows).
                matrixA[nodeIndices[edge.Item2], nodeIndices[edge.Item1]] = 1.0;
            }
            // Return the matrix.
            return matrixA;
        }

        /// <summary>
        /// Computes the C matrix (corresponding to the target nodes).
        /// </summary>
        /// <param name="nodeIndex">The dictionary containing, for each node, its index in the node list.</param>
        /// <param name="targets">The target nodes for the algorithm.</param>
        /// <returns>The C matrix (corresponding to the target nodes).</returns>
        private static Matrix<double> GetMatrixC(Dictionary<string, int> nodeIndex, List<string> targets)
        {
            // Initialize the C matrix with zero.
            var matrixC = Matrix<double>.Build.Dense(targets.Count(), nodeIndex.Count());
            // Go over each target node,
            for (int index = 0; index < targets.Count(); index++)
            {
                // Set to 1.0 the corresponding entry in the matrix.
                matrixC[index, nodeIndex[targets[index]]] = 1.0;
            }
            // And we return the matrix.
            return matrixC;
        }

        /// <summary>
        /// Computes the powers of the adjacency matrix A, up to a given maximum power.
        /// </summary>
        /// <param name="matrixA">The adjacency matrix of the graph.</param>
        /// <param name="maximumPathLength">The maximum path length for control in the graph.</param>
        /// <returns>The powers of the adjacency matrix A, up to a given maximum power.</returns>
        private static List<Matrix<double>> GetPowersMatrixA(Matrix<double> matrixA, int maximumPathLength)
        {
            // Initialize a matrix list with the identity matrix.
            var powers = new List<Matrix<double>>(maximumPathLength + 1)
            {
                Matrix<double>.Build.DenseIdentity(matrixA.RowCount)
            };
            // Up to the maximum power, starting from the first element.
            for (int index = 1; index < maximumPathLength + 1; index++)
            {
                // Multiply the previous element with the matrix itself.
                powers.Add(matrixA.Multiply(powers[index - 1]));
            }
            // Return the list.
            return powers;
        }

        /// <summary>
        /// Computes the powers of the combination between the target matrix C and the adjacency matrix A.
        /// </summary>
        /// <param name="matrixC">The matrix corresponding to the target nodes in the graph.</param>
        /// <param name="powersMatrixA">The list of powers of the adjacency matrix A.</param>
        /// <returns>The powers of the combination between the target matrix C and the adjacency matrix A.</returns>
        private static List<Matrix<double>> GetPowersMatrixCA(Matrix<double> matrixC, List<Matrix<double>> powersMatrixA)
        {
            // Initialize a new empty list.
            var powers = new List<Matrix<double>>(powersMatrixA.Count());
            // Go over each power of the adjacency matrix.
            foreach (var power in powersMatrixA)
            {
                // Left-multiply with the target matrix C.
                powers.Add(matrixC.Multiply(power));
            }
            // Return the list.
            return powers;
        }

        /// <summary>
        /// Computes, for every target node, the list of nodes from which it can be reached.
        /// </summary>
        /// <param name="powersMatrixA">The list of powers of the adjacency matrix A.</param>
        /// <param name="targets">The target nodes for the algorithm.</param>
        /// <param name="nodeIndex">The dictionary containing, for each node, its index in the node list.</param>
        /// <returns>The list of nodes from which every target node can be reached.</returns>
        private static Dictionary<string, List<string>> GetTargetAncestors(List<Matrix<double>> powersMatrixA, List<string> targets, Dictionary<string, int> nodeIndex)
        {
            // Initialize the path dictionary with an empty list for each target node.
            var dictionary = targets.ToDictionary(item => item, item => new List<string>());
            // For every power of the adjacency matrix.
            for (int index1 = 0; index1 < powersMatrixA.Count(); index1++)
            {
                // For every target node.
                for (int index2 = 0; index2 < targets.Count(); index2++)
                {
                    // Add to the target node all of the nodes corresponding to the non-zero entries in the proper row of the matrix.
                    dictionary[targets[index2]].AddRange(powersMatrixA[index1]
                        .Row(nodeIndex[targets[index2]])
                        .Select((value, index) => value != 0 ? nodeIndex.FirstOrDefault(item => item.Value == index).Key : null)
                        .Where(item => !string.IsNullOrEmpty(item))
                        .ToList());
                }
            }
            // For each item in the dictionary.
            foreach (var item in dictionary.Keys.ToList())
            {
                // Remove all duplicate nodes.
                dictionary[item] = dictionary[item].Distinct().ToList();
            }
            // Return the dictionary.
            return dictionary;
        }
    }
}
