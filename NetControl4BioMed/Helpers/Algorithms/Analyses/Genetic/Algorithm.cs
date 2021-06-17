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
            var analysisProteinIds = new List<string>();
            var analysisInteractionIds = new List<(string, string, string)>();
            var proteins = new List<string>();
            var interactions = new List<(string, string)>();
            var sources = new List<string>();
            var targets = new List<string>();
            var parameters = new Parameters();
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
                analysisProteinIds = context.AnalysisProteins
                    .Where(item => item.Analysis == analysis)
                    .Where(item => item.Type == AnalysisProteinType.None)
                    .Select(item => item.Protein.Id)
                    .ToList();
                analysisInteractionIds = context.AnalysisInteractions
                    .Where(item => item.Analysis == analysis)
                    .Select(item => item.Interaction)
                    .Select(item => new
                    {
                        Interaction = item.Id,
                        SourceProteinId = item.InteractionProteins
                            .Where(item1 => item1.Type == InteractionProteinType.Source)
                            .Select(item1 => item1.Protein.Id)
                            .FirstOrDefault(),
                        TargetProteinId = item.InteractionProteins
                            .Where(item1 => item1.Type == InteractionProteinType.Target)
                            .Select(item1 => item1.Protein.Id)
                            .FirstOrDefault()
                    })
                    .Where(item => !string.IsNullOrEmpty(item.SourceProteinId) && !string.IsNullOrEmpty(item.TargetProteinId))
                    .AsEnumerable()
                    .Select(item => (item.SourceProteinId, item.TargetProteinId, item.Interaction))
                    .Distinct()
                    .ToList();
                // Get the proteins, interactions, target proteins and source (preferred) proteins.
                proteins = analysisProteinIds
                    .ToList();
                interactions = analysisInteractionIds
                    .Select(item => (item.Item1, item.Item2))
                    .ToList();
                sources = context.AnalysisProteins
                    .Where(item => item.Analysis == analysis)
                    .Where(item => item.Type == AnalysisProteinType.Source)
                    .Select(item => item.Protein.Id)
                    .ToList();
                targets = context.AnalysisProteins
                    .Where(item => item.Analysis == analysis)
                    .Where(item => item.Type == AnalysisProteinType.Target)
                    .Select(item => item.Protein.Id)
                    .ToList();
                // Check if there is any protein in an interaction that does not appear in the list of proteins.
                if (interactions.Select(item => item.Item1).Concat(interactions.Select(item => item.Item2)).Distinct().Except(proteins).Any())
                {
                    // Update the analysis with an error message.
                    analysis.Log = analysis.AppendToLog("There are interactions which contain unknown proteins.");
                    // Update the analysis status.
                    analysis.Status = AnalysisStatus.Error;
                    // Update the analysis end time.
                    analysis.DateTimeEnded = DateTime.UtcNow;
                    // Update the analysis.
                    await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                    // End the function.
                    return;
                }
                // Check if there is any target protein that does not appear in the list of proteins.
                if (targets.Except(proteins).Any())
                {
                    // Update the analysis with an error message.
                    analysis.Log = analysis.AppendToLog("There are unknown target proteins.");
                    // Update the analysis status.
                    analysis.Status = AnalysisStatus.Error;
                    // Update the analysis end time.
                    analysis.DateTimeEnded = DateTime.UtcNow;
                    // Update the analysis.
                    await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                    // End the function.
                    return;
                }
                // Check if there is any source protein that does not appear in the list of proteins.
                if (sources.Except(proteins).Any())
                {
                    // Update the analysis with an error message.
                    analysis.Log = analysis.AppendToLog("There are unknown source proteins.");
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
                if (!analysis.Parameters.TryDeserializeJsonObject<Parameters>(out parameters))
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
                maximumIterations = analysis.MaximumIterations;
                maximumIterationsWithoutImprovement = analysis.MaximumIterationsWithoutImprovement;
            }
            // Get the additional needed variables.
            var proteinIndex = proteins.Select((item, index) => (item, index)).ToDictionary(item => item.item, item => item.index);
            var proteinIsPreferred = proteins.ToDictionary(item => item, item => sources.Contains(item));
            var matrixA = GetMatrixA(proteinIndex, interactions);
            var matrixC = GetMatrixC(proteinIndex, targets);
            var powersMatrixA = GetPowersMatrixA(matrixA, parameters.MaximumPathLength);
            var powersMatrixCA = GetPowersMatrixCA(matrixC, powersMatrixA);
            var targetAncestors = GetTargetAncestors(powersMatrixA, targets, proteinIndex);
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
                await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
            }
            // Define the required data.
            var analysisStillExists = true;
            var analysisStatus = AnalysisStatus.Ongoing;
            var currentIteration = 0;
            var currentIterationWithoutImprovement = 0;
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
                    await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
                }
            }, null, TimeSpan.FromSeconds(0.0), TimeSpan.FromSeconds(30.0)))
            {
                // Set up the first iteration.
                var random = new Random(parameters.RandomSeed);
                var bestSolutionSize = 0.0;
                // Initialize a new population.
                var population = new Population(proteinIndex, targets, targetAncestors, powersMatrixCA, proteinIsPreferred, parameters, random);
                // Run as long as the analysis exists and the final iteration hasn't been reached.
                while (analysisStillExists && analysisStatus == AnalysisStatus.Ongoing && currentIteration < maximumIterations && currentIterationWithoutImprovement < maximumIterationsWithoutImprovement && !token.IsCancellationRequested)
                {
                    // Move on to the next iterations.
                    currentIteration += 1;
                    currentIterationWithoutImprovement += 1;
                    // Move on to the next population.
                    population = new Population(population, proteinIndex, targets, targetAncestors, powersMatrixCA, proteinIsPreferred, parameters, random);
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
                controlPaths = population.GetControlPaths(proteinIndex, proteins, interactions).Select(item => new ControlPath
                {
                    Paths = item.Values.Select(item1 =>
                    {
                        // Get the proteins and interactions in the path.
                        var pathProteins = item1
                            .Select(item2 => analysisProteinIds.FirstOrDefault(item3 => item3 == item2))
                            .Where(item2 => item2 != null)
                            .Reverse()
                            .ToList();
                        var pathInteractions = item1
                            .Zip(item1.Skip(1), (item2, item3) => (item3.ToString(), item2.ToString()))
                            .Select(item2 => analysisInteractionIds.FirstOrDefault(item3 => item3.Item1 == item2.Item1 && item3.Item2 == item2.Item2))
                            .Select(item2 => item2.Item3)
                            .Where(item2 => !string.IsNullOrEmpty(item2))
                            .Reverse()
                            .ToList();
                        // Return the path.
                        return new Path
                        {
                            PathProteins = pathProteins.Select((item2, index) => new PathProtein { ProteinId = item2, Type = PathProteinType.None, Index = index })
                                .Append(new PathProtein { ProteinId = pathProteins.First(), Type = PathProteinType.Source, Index = -1 })
                                .Append(new PathProtein { ProteinId = pathProteins.Last(), Type = PathProteinType.Target, Index = pathProteins.Count() })
                                .ToList(),
                            PathInteractions = pathInteractions.Select((item2, index) => new PathInteraction { InteractionId = item2, Index = index }).ToList()
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
                await IEnumerableExtensions.EditAsync(analysis.Yield(), serviceProvider, token);
            }
            // End the function.
            return;
        }

        /// <summary>
        /// Computes the A matrix (corresponding to the adjacency matrix).
        /// </summary>
        /// <param name="proteinIndices">The dictionary containing, for each protein, its index in the protein list.</param>
        /// <param name="interactions">The interactions of the graph.</param>
        /// <returns>The A matrix (corresponding to the adjacency matrix).</returns>
        private static Matrix<double> GetMatrixA(Dictionary<string, int> proteinIndices, List<(string, string)> interactions)
        {
            // Initialize the adjacency matrix with zero.
            var matrixA = Matrix<double>.Build.DenseDiagonal(proteinIndices.Count(), proteinIndices.Count(), 0.0);
            // Go over each of the interactions.
            foreach (var interaction in interactions)
            {
                // Set to 1.0 the corresponding entry in the matrix (source proteins are on the columns, target proteins are on the rows).
                matrixA[proteinIndices[interaction.Item2], proteinIndices[interaction.Item1]] = 1.0;
            }
            // Return the matrix.
            return matrixA;
        }

        /// <summary>
        /// Computes the C matrix (corresponding to the target proteins).
        /// </summary>
        /// <param name="proteinIndex">The dictionary containing, for each protein, its index in the protein list.</param>
        /// <param name="targets">The target proteins for the algorithm.</param>
        /// <returns>The C matrix (corresponding to the target proteins).</returns>
        private static Matrix<double> GetMatrixC(Dictionary<string, int> proteinIndex, List<string> targets)
        {
            // Initialize the C matrix with zero.
            var matrixC = Matrix<double>.Build.Dense(targets.Count(), proteinIndex.Count());
            // Go over each target protein,
            for (int index = 0; index < targets.Count(); index++)
            {
                // Set to 1.0 the corresponding entry in the matrix.
                matrixC[index, proteinIndex[targets[index]]] = 1.0;
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
        /// <param name="matrixC">The matrix corresponding to the target proteins in the graph.</param>
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
        /// Computes, for every target protein, the list of proteins from which it can be reached.
        /// </summary>
        /// <param name="powersMatrixA">The list of powers of the adjacency matrix A.</param>
        /// <param name="targets">The target proteins for the algorithm.</param>
        /// <param name="proteinIndex">The dictionary containing, for each protein, its index in the protein list.</param>
        /// <returns>The list of proteins from which every target protein can be reached.</returns>
        private static Dictionary<string, List<string>> GetTargetAncestors(List<Matrix<double>> powersMatrixA, List<string> targets, Dictionary<string, int> proteinIndex)
        {
            // Initialize the path dictionary with an empty list for each target protein.
            var dictionary = targets.ToDictionary(item => item, item => new List<string>());
            // For every power of the adjacency matrix.
            for (int index1 = 0; index1 < powersMatrixA.Count(); index1++)
            {
                // For every target protein.
                for (int index2 = 0; index2 < targets.Count(); index2++)
                {
                    // Add to the target protein all of the proteins corresponding to the non-zero entries in the proper row of the matrix.
                    dictionary[targets[index2]].AddRange(powersMatrixA[index1]
                        .Row(proteinIndex[targets[index2]])
                        .Select((value, index) => value != 0 ? proteinIndex.FirstOrDefault(item => item.Value == index).Key : null)
                        .Where(item => !string.IsNullOrEmpty(item))
                        .ToList());
                }
            }
            // For each item in the dictionary.
            foreach (var item in dictionary.Keys.ToList())
            {
                // Remove all duplicate proteins.
                dictionary[item] = dictionary[item].Distinct().ToList();
            }
            // Return the dictionary.
            return dictionary;
        }
    }
}
