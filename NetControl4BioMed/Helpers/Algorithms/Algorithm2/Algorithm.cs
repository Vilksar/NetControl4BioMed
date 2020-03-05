using MathNet.Numerics.LinearAlgebra;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Algorithms.Algorithm2
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
        public static async Task RunAsync(ApplicationDbContext context, Analysis analysis)
        {
            // Mark the analysis for updating.
            context.Update(analysis);
            // Update the analysis status and stats.
            analysis.ControlPaths = new List<ControlPath>();
            analysis.Status = AnalysisStatus.Initializing;
            analysis.DateTimeStarted = DateTime.Now;
            // Save the changes in the database.
            await context.SaveChangesAsync();
            // Reload it for a fresh start.
            await context.Entry(analysis).ReloadAsync();
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
                // Save the changes in the database.
                await context.SaveChangesAsync();
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
                // Save the changes in the database.
                await context.SaveChangesAsync();
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
                // Save the changes in the database.
                await context.SaveChangesAsync();
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
                // Save the changes in the database.
                await context.SaveChangesAsync();
                // End the function.
                return;
            }
            // Get the additional needed variables.
            var nodeIndex = nodes.Select((item, index) => (item, index)).ToDictionary(item => item.item, item => item.index);
            var nodeIsPreferred = nodes.ToDictionary(item => item, item => sources.Contains(item));
            var matrixA = GetMatrixA(nodeIndex, edges);
            var matrixC = GetMatrixC(nodeIndex, targets);
            var powersMatrixA = GetPowersMatrixA(matrixA, parameters.MaximumPathLength);
            var powersMatrixCA = GetPowersMatrixCA(matrixC, powersMatrixA);
            var targetAncestors = GetTargetAncestors(powersMatrixA, targets, nodeIndex);
            // Update the analysis status.
            analysis.Status = AnalysisStatus.Ongoing;
            // Save the changes in the database.
            await context.SaveChangesAsync();
            // Set up the first iteration.
            var random = new Random(parameters.RandomSeed);
            var currentIteration = analysis.CurrentIteration;
            var currentIterationWithoutImprovement = analysis.CurrentIterationWithoutImprovement;
            var maximumIterations = analysis.MaximumIterations;
            var maximumIterationsWithoutImprovement = analysis.MaximumIterationsWithoutImprovement;
            var bestSolutionSize = 0.0;
            // Initialize a new population.
            var population = new Population(nodeIndex, targets, targetAncestors, powersMatrixCA, nodeIsPreferred, parameters, random);
            // Run as long as the analysis exists and the final iteration hasn't been reached.
            while (analysis != null && analysis.Status == AnalysisStatus.Ongoing && currentIteration < maximumIterations && currentIterationWithoutImprovement < maximumIterationsWithoutImprovement)
            {
                // Move on to the next iterations.
                currentIteration += 1;
                currentIterationWithoutImprovement += 1;
                // Update the iteration count.
                analysis.CurrentIteration = currentIteration;
                analysis.CurrentIterationWithoutImprovement = currentIterationWithoutImprovement;
                // Save the changes to the analysis.
                await context.SaveChangesAsync();
                // Move on to the next population.
                population = new Population(population, nodeIndex, targets, targetAncestors, powersMatrixCA, nodeIsPreferred, parameters, random);
                // Get the best fitness of the current solution.
                var currentSolutionSize = population.GetMaximumFitness();
                // Check if the current solution is better than the previously obtained best solutions.
                if (bestSolutionSize < currentSolutionSize)
                {
                    // Update the best solution.
                    bestSolutionSize = currentSolutionSize;
                    // Reset the number of iterations.
                    currentIterationWithoutImprovement = 0;
                }
                // And reload it for the next iteration.
                await context.Entry(analysis).ReloadAsync();
            }
            // Check if the analysis doesn't exist anymore (if it has been deleted).
            if (analysis == null)
            {
                // End the function.
                return;
            }
            // Get the control paths.
            var controlPaths = population.GetControlPaths(nodeIndex, nodes, edges).Select(item => new ControlPath
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
            // Save the changes in the database.
            await context.SaveChangesAsync();
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
