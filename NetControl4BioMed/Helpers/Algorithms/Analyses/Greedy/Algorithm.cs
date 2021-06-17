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

namespace NetControl4BioMed.Helpers.Algorithms.Analyses.Greedy
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
            // Update the parameters.
            var heuristics = JsonSerializer.Deserialize<List<List<string>>>(parameters.Heuristics)
                .TakeWhile(item => !item.Contains("Z"))
                .Select(item => item.Distinct().ToList())
                .Append(new List<string> { "Z" })
                .ToList();
            parameters.Heuristics = JsonSerializer.Serialize(heuristics);
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
                // Update the analysis status and parameters.
                analysis.Parameters = JsonSerializer.Serialize(parameters);
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
                var bestSolutionSize = targets.Count() + 1;
                var bestControlPaths = new List<Dictionary<string, List<string>>>();
                // Run as long as the analysis exists and the final iteration hasn't been reached.
                while (analysisStillExists && analysisStatus == AnalysisStatus.Ongoing && currentIteration < maximumIterations && currentIterationWithoutImprovement < maximumIterationsWithoutImprovement && !token.IsCancellationRequested)
                {
                    // Move on to the next iterations.
                    currentIteration += 1;
                    currentIterationWithoutImprovement += 1;
                    // Define a variable to store the control paths.
                    var controlPath = new Dictionary<string, List<string>>();
                    // Set up the control path to start from the target proteins.
                    foreach (var protein in targets)
                    {
                        controlPath[protein] = new List<string> { protein };
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
                        // Get the target proteins to keep. If it is the first check of the current iteration, we have no kept proteins, so the current targets are simply the targets. This is a part of the "repeat" optimization.
                        var keptTargetProteins = GetControllingProteins(controlPath)
                            .Where(item => item.Value.Count() > 1)
                            .Select(item => item.Value)
                            .SelectMany(item => item)
                            .ToHashSet();
                        // Go over each of the paths corresponding to the target proteins to reset.
                        foreach (var item in controlPath.Keys.Except(keptTargetProteins).ToList())
                        {
                            // Reset the control path.
                            controlPath[item] = new List<string>() { item };
                        }
                        // Get the new targets.
                        currentTargets = currentTargets
                            .Except(keptTargetProteins)
                            .ToList();
                        // Run until there are no current targets or we reached the maximum path length.
                        while (currentTargets.Any() && currentPathLength < parameters.MaximumPathLength)
                        {
                            // Set all of the current targets as unmatched.
                            var unmatchedProteins = currentTargets.ToList();
                            // Set all proteins in the network as available to match.
                            var availableProteins = proteins.ToList();
                            // If it is the first check of the current iteration, there are no kept proteins, so the left proteins and interactions remain unchanged. Otherwise, remove from the left proteins the corresponding proteins in the current step in the control paths for the kept proteins. This is a part of the "repeat" optimization.
                            availableProteins = availableProteins
                                .Except(controlPath
                                    .Where(item => keptTargetProteins.Contains(item.Key))
                                    .Select(item => item.Value)
                                    .Where(item => currentPathLength + 1 < item.Count())
                                    .Select(item => item[currentPathLength + 1]))
                                .ToList();
                            // Define a variable to store the matched interactions of the matching.
                            var matchedInteractions = new List<(string, string)>();
                            // Go over each heuristic set.
                            foreach (var heuristicSet in heuristics)
                            {
                                // Get the left proteins, right proteins, and interactions of the current matching.
                                var leftProteins = availableProteins.ToList();
                                var rightProteins = unmatchedProteins.ToList();
                                var currentInteractions = GetSingleHeuristicInteractions(leftProteins, rightProteins, interactions, heuristicSet, controlPath, sources);
                                var matchingInteractions = GetMaximumMatching(leftProteins, rightProteins, currentInteractions, random);
                                // Add the matched interactions to the list.
                                matchedInteractions.AddRange(matchingInteractions);
                                // Update the remaining proteins after the matching.
                                availableProteins.RemoveAll(item => matchingInteractions.Any(item1 => item1.Item1 == item));
                                unmatchedProteins.RemoveAll(item => matchingInteractions.Any(item1 => item1.Item2 == item));
                            }
                            // Update the current targets to the current matched interaction source proteins, and the control path.
                            currentTargets = matchedInteractions.Select(item => item.Item1).ToList();
                            // Get the dictionary which stores, for each target, the corresponding new matched target to add to the path.
                            var currentTargetDictionary = controlPath
                                .Where(item => item.Value.Count() == currentPathLength + 1)
                                .ToDictionary(item => item.Key, item => matchedInteractions.Where(item1 => item1.Item2 == item.Value.Last()).Select(item1 => item1.Item1).FirstOrDefault())
                                .Where(item => item.Value != null);
                            // Go over all entries in dictionary.
                            foreach (var entry in currentTargetDictionary)
                            {
                                // Update the control path with the first protein of the corresponding matched interaction.
                                controlPath[entry.Key].Add(entry.Value);
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
                        // Get the controlling proteins for the path.
                        var controllingProteins = GetControllingProteins(controlPath).Keys
                            .ToHashSet();
                        // Go over each path in the control path.
                        foreach (var key in controlPath.Keys.ToList())
                        {
                            // Get the first index of any control protein.
                            var index = controlPath[key].FindIndex(item => controllingProteins.Contains(item));
                            // Check if the index doesn't correspond to the last element in the list.
                            if (index < controlPath[key].Count() - 1)
                            {
                                // Cut the path up to the first index of any control protein.
                                controlPath[key] = controlPath[key].Take(index + 1).ToList();
                                // Mark the cut as performed.
                                pathCutsPerformed = true;
                            }
                        }
                    } while (pathCutsPerformed);
                    // Compute the result.
                    var controlProteins = GetControllingProteins(controlPath).Keys.ToList();
                    // Check if the current solution is better than the previously obtained best solutions.
                    if (controlProteins.Count() < bestSolutionSize)
                    {
                        // Update the best solution.
                        bestSolutionSize = controlProteins.Count();
                        // Reset the number of iterations.
                        currentIterationWithoutImprovement = 0;
                        // Reset the best control paths.
                        bestControlPaths.RemoveAll(item => true);
                    }
                    // Check if the current solution is as good as the previously obtained best solutions.
                    if (controlProteins.Count() == bestSolutionSize)
                    {
                        // Check if none of the previous solutions has the same proteins.
                        if (!bestControlPaths.Any(item => !item.Keys.Except(controlPath.Keys).Any() && !controlPath.Keys.Except(item.Keys).Any()))
                        {
                            // Update the best control paths.
                            bestControlPaths.Add(controlPath);
                        }
                    }
                }
                // Get the control paths.
                controlPaths = bestControlPaths.Select(item => new ControlPath
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
        /// Returns a dictionary containing, for each control protein, the target proteins that it controls.
        /// </summary>
        /// <param name="controlPath">The control path of the current iteration.</param>
        /// <returns>Returns the dictionary containing, for each control protein, the target proteins that it controls.</returns>
        private static Dictionary<string, IEnumerable<string>> GetControllingProteins(Dictionary<string, List<string>> controlPath)
        {
            // Get the pairs of corresponding control proteins and target proteins for each path in the control path.
            var pairs = controlPath
                .Select(item => new
                {
                    TargetProtein = item.Key,
                    ControlProtein = item.Value.Last()
                });
            // Return, for each control protein, the target proteins that it controls.
            return pairs
                .Select(item => item.ControlProtein)
                .Distinct()
                .ToDictionary(item => item, item => pairs.Where(item1 => item1.ControlProtein == item).Select(item => item.TargetProtein));
        }

        /// <summary>
        /// Returns the interactions defined by the current heuristic.
        /// </summary>
        /// <param name="leftProteins">Represents the left proteins of the bipartite graph.</param>
        /// <param name="rightProteins">Represents the right proteins of the bipartite graph.</param>
        /// <param name="interactions">Represents the interactions of the bipartite graph.</param>
        /// <param name="heuristicSet">Represents the heuristic set to be used in the search.</param>
        /// <param name="controlPath">Represents the current control path.</param>
        /// <param name="sources">Represents the source (drug-target) proteins.</param>
        /// <returns></returns>
        private static List<(string, string)> GetSingleHeuristicInteractions(List<string> leftProteins, List<string> rightProteins, List<(string, string)> interactions, List<string> heuristicSet, Dictionary<string, List<string>> controlPath, List<string> sources)
        {
            // Define the variable to return.
            var heuristicInteractions = new List<(string, string)>();
            // Get all already identified control proteins, if needed.
            var currentLength = controlPath.Max(item => item.Value.Count());
            var controlProteins = heuristicSet.Contains("C") ?
                controlPath.Where(item => item.Value.Count() < currentLength)
                    .Select(item => item.Value.Last())
                    .Distinct()
                    .AsEnumerable() :
                Enumerable.Empty<string>();
            // Get all previously seen proteins in the control paths, if needed.
            var seenProteins = heuristicSet.Contains("A") || heuristicSet.Contains("D") || heuristicSet.Contains("E") ?
                controlPath.Select(item => item.Value)
                    .SelectMany(item => item)
                    .Distinct()
                    .AsEnumerable() :
                Enumerable.Empty<string>();
            // Get all previously seen source proteins in the control paths, if needed.
            var seenSourceProteins = heuristicSet.Contains("A") ?
                seenProteins.Intersect(sources) :
                Enumerable.Empty<string>();
            // Get all interactions starting from a previously seen source protein and ending in target proteins, if needed.
            var seenSourceInteractions = heuristicSet.Contains("A") ?
                interactions.Where(item => rightProteins.Contains(item.Item2) && seenSourceProteins.Contains(item.Item1)) :
                Enumerable.Empty<(string, string)>();
            // Get all interactions starting from a source protein and ending in target proteins, if needed.
            var sourceInteractions = heuristicSet.Contains("B") ?
                interactions.Where(item => rightProteins.Contains(item.Item2) && sources.Contains(item.Item1)) :
                Enumerable.Empty<(string, string)>();
            // Get all interactions starting from an already identified control protein and ending in target proteins, if needed.
            var controlInteractions = heuristicSet.Contains("C") ?
                interactions.Where(item => rightProteins.Contains(item.Item2) && controlProteins.Contains(item.Item1)) :
                Enumerable.Empty<(string, string)>();
            // Get all interactions starting from a previously seen protein, if needed.
            var seenInteractions = heuristicSet.Contains("D") ?
                interactions.Where(item => rightProteins.Contains(item.Item2) && seenProteins.Contains(item.Item1)) :
                Enumerable.Empty<(string, string)>();
            // Get all interactions not starting from a protein in the current control path, if needed (to avoid loops).
            var differentInteractions = heuristicSet.Contains("E") ?
                interactions.Where(item => rightProteins.Contains(item.Item2) && !controlPath.Where(item1 => item1.Value.Last() == item.Item2).Any(item1 => item1.Value.Contains(item.Item1))) :
                Enumerable.Empty<(string, string)>();
            // Get all possible interactions.
            var allInteractions = interactions.Where(item => rightProteins.Contains(item.Item2));
            // Go over all heuristic items.
            foreach (var heuristic in heuristicSet)
            {
                // Check the current heuristic item.
                switch (heuristic)
                {
                    case "A":
                        // Add all interactions from previously seen source proteins.
                        heuristicInteractions.AddRange(seenSourceInteractions);
                        // End the switch.
                        break;
                    case "B":
                        // Add all interactions from source proteins.
                        heuristicInteractions.AddRange(sourceInteractions);
                        // End the switch.
                        break;
                    case "C":
                        // Add all interactions from already identified controlling proteins.
                        heuristicInteractions.AddRange(controlInteractions);
                        // End the switch.
                        break;
                    case "D":
                        // Add all interactions from previously seen proteins.
                        heuristicInteractions.AddRange(seenInteractions);
                        // End the switch.
                        break;
                    case "E":
                        // Add all interactions not starting from a protein in the current control path (to avoid loops).
                        heuristicInteractions.AddRange(differentInteractions);
                        // End the switch.
                        break;
                    case "Z":
                        // Add all possible interactions.
                        heuristicInteractions.AddRange(allInteractions);
                        // End the switch.
                        break;
                    default:
                        // End the switch.
                        break;
                }
            }
            // Return all interactions that start in a left protein.
            return heuristicInteractions.Where(item => leftProteins.Contains(item.Item1)).ToList();
        }

        /// <summary>
        /// Represents the Hopcroft-Karp algorithm for maximum matching in a bipartite graph.
        /// </summary>
        /// <param name="leftProteins">Represents the left proteins of the bipartite graph.</param>
        /// <param name="rightProteins">Represents the right proteins of the bipartite graph.</param>
        /// <param name="interactions">Represents the interactions of the bipartite graph.</param>
        /// <param name="rand">Represents the random variable for choosing randomly a maximum matching.</param>
        /// <returns>Returns the list of interactions corresponding to a maximum matching for the bipartite graph.</returns>
        /// <remarks>The implementation is a slightly modified version from the one found on https://en.wikipedia.org/wiki/Hopcroft–Karp_algorithm, written in C#.</remarks>
        private static List<(string, string)> GetMaximumMatching(List<string> leftProteins, List<string> rightProteins, List<(string, string)> interactions, Random rand)
        {
            // The Wikipedia algorithm uses considers the left proteins as U, and the right ones as V. But, as the unmatched proteins are considered, in order, from the left side of the bipartite graph, the obtained matching would not be truly random, especially on the first step. That is why I perform here a simple switch, by inter-changing the lists U and V (left and right side proteins), and using the opposite direction interactions, in order to obtained a random maximum matching.
            var U = rightProteins;
            var V = leftProteins;
            var oppositeInteractions = interactions.Select((item) => (item.Item2, item.Item1)).ToList();
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
            while (BreadthFirstSearch(U, PairU, PairV, dist, oppositeInteractions, rand))
            {
                foreach (var u in U)
                {
                    if (PairU[u] == "null")
                    {
                        if (DepthFirstSearch(u, PairU, PairV, dist, oppositeInteractions, rand))
                        {
                            matching++;
                        }
                    }
                }
            }
            // Instead of the number of performed matchings, we will return the actual interactions corresponding to these matchings.
            // Because in the beginning of the function we switched the direction of the interactions, now we switch back the direction of
            // the matched interactions, to fit in with the rest of the program.
            var matchedInteractions = new List<(string, string)>();
            foreach (var item in PairU)
            {
                // We will return only interactions corresponding to the matching, that is interactions with both proteins not null.
                if (item.Value != "null")
                {
                    // Here we perform the switch back to the original direction. Otherwise we would return the pair (key, value).
                    matchedInteractions.Add((item.Value, item.Key));
                }
            }
            return matchedInteractions;
        }

        /// <summary>
        /// The breadth-first search of the Hopcroft-Karp maximum matching algorithm.
        /// </summary>
        /// <param name="U">The left proteins of the bipartite graph.</param>
        /// <param name="PairU">Dictionary containing a matching from the proteins on the left to proteins on the right.</param>
        /// <param name="PairV">Dictionary containing a matching from the proteins on the right to proteins on the left.</param>
        /// <param name="dist"></param>
        /// <param name="interactions">List of interactions in the bipartite graph.</param>
        /// <param name="rand">The random variable for choosing randomly a maximum matching.</param>
        /// <returns></returns>
        private static bool BreadthFirstSearch(List<string> U, Dictionary<string, string> PairU, Dictionary<string, string> PairV, Dictionary<string, int> dist, List<(string, string)> interactions, Random rand)
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
                    var adj = interactions.Where((interaction) => interaction.Item1 == u).Select((interaction) => interaction.Item2).OrderBy((item) => rand.Next());
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
        /// <param name="u">A protein from the left part of the bipartite graph.</param>
        /// <param name="PairU">Dictionary containing a matching from the proteins on the left to proteins on the right.</param>
        /// <param name="PairV">Dictionary containing a matching from the proteins on the right to proteins on the left.</param>
        /// <param name="dist"></param>
        /// <param name="interactions">List of interactions in the bipartite graph.</param>
        /// <param name="rand">The random variable for choosing randomly a maximum matching.</param>
        /// <returns></returns>
        private static bool DepthFirstSearch(string u, Dictionary<string, string> PairU, Dictionary<string, string> PairV, Dictionary<string, int> dist, List<(string, string)> interactions, Random rand)
        {
            if (u != "null")
            {
                var adj = interactions.Where((interaction) => interaction.Item1 == u).Select((interaction) => interaction.Item2).OrderBy((item) => rand.Next());
                foreach (var v in adj)
                {
                    if (dist[PairV[v]] == dist[u] + 1)
                    {
                        if (DepthFirstSearch(PairV[v], PairU, PairV, dist, interactions, rand))
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
