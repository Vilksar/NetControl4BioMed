using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Algorithms.Algorithm2
{
    /// <summary>
    /// Represents a population of chromosomes used by the analysis.
    /// </summary>
    public class Population
    {
        /// <summary>
        /// Represents the maximum number of parallel operations.
        /// </summary>
        private static readonly int DegreeOfParallelism = (int)Math.Floor((double)(Environment.ProcessorCount - 1) / 2) + 1;

        /// <summary>
        /// Represents the chromosomes in the population.
        /// </summary>
        public List<Chromosome> Chromosomes { get; set; }

        /// <summary>
        /// Constructor for an empty population.
        /// </summary>
        public Population()
        {
            // Assign the default value for each property.
            Chromosomes = new List<Chromosome>();
        }

        /// <summary>
        /// Constructor for the initial population.
        /// </summary>
        /// <param name="nodeIndex">The dictionary containing, for each node, its index in the node list.</param>
        /// <param name="targetNodes">The target nodes for the algorithm.</param>
        /// <param name="targetAncestors">The list containing, for each target nodes, the nodes from which it can be reached.</param>
        /// <param name="powersMatrixCA">The list containing the different powers of the matrix (CA, CA^2, CA^3, ... ).</param>
        /// <param name="random">The random seed.</param>
        public Population(Dictionary<string, int> nodeIndex, List<string> targetNodes, Dictionary<string, List<string>> targetAncestors, List<Matrix<double>> powersMatrixCA, Dictionary<string, bool> nodeIsPreferred, Parameters parameters, Random random)
        {
            // Initialize the list of chromosomes.
            Chromosomes = new List<Chromosome>();
            // Get the number of elements in each group and the minimum number of elements per group.
            var numberOfGroups = (int)Math.Ceiling((double)targetNodes.Count() / (double)parameters.RandomGenesPerChromosome);
            var chromosomesPerGroup = (int)Math.Floor((double)parameters.PopulationSize / (double)numberOfGroups);
            var genesPerGroup = (int)Math.Floor((double)targetNodes.Count() / (double)numberOfGroups);
            var numberOfChromosomeGroupsExtra = parameters.PopulationSize - chromosomesPerGroup * numberOfGroups;
            var numberOfGeneGroupsExtra = targetNodes.Count() - genesPerGroup * numberOfGroups;
            // Get the actual number of chromosomes in each group.
            var chromosomeGroups = new List<int>()
                .Concat(Enumerable.Range(0, numberOfChromosomeGroupsExtra).Select(item => chromosomesPerGroup + 1))
                .Concat(Enumerable.Range(numberOfChromosomeGroupsExtra, numberOfGroups - numberOfChromosomeGroupsExtra).Select(item => chromosomesPerGroup))
                .ToList();
            // Get the actual numer of genes in each group.
            var sum = 0;
            var geneGroups = new List<int> { 0 }
                .Concat(Enumerable.Range(0, numberOfGeneGroupsExtra).Select(item => genesPerGroup + 1))
                .Concat(Enumerable.Range(numberOfGeneGroupsExtra, numberOfGroups - numberOfGeneGroupsExtra).Select(item => genesPerGroup))
                .Select(item => sum += item)
                .ToList();
            // Define a new concurrent bag for chromosomes.
            var bag = new ConcurrentBag<Chromosome>();
            // Define a new thread-safe queue of random seeds, as well as a default random seed.
            var randomSeeds = new ConcurrentQueue<int>(Enumerable.Range(0, parameters.PopulationSize).Select(item => random.Next()));
            var defaultRandomSeed = random.Next();
            // Repeat for each group.
            Parallel.For(0, numberOfGroups, new ParallelOptions { MaxDegreeOfParallelism = DegreeOfParallelism }, index1 =>
            {
                // Get the lower and upper limits.
                var lowerLimit = geneGroups[index1];
                var upperLimit = geneGroups[index1 + 1];
                // Repeat for the number of elements in the group.
                Parallel.For(0, chromosomeGroups[index1], new ParallelOptions { MaxDegreeOfParallelism = DegreeOfParallelism }, index2 =>
                {
                    // Try to get a new random seed from the list of random random seeds.
                    if (!randomSeeds.TryDequeue(out var randomSeed))
                    {
                        // If no seed could be gotten, then assign to it the default value.
                        randomSeed = defaultRandomSeed;
                    }
                    // Define a local random variable for only this thread.
                    var localRandom = new Random(randomSeed);
                    // Add a new, initialized, chromosome.
                    bag.Add(new Chromosome(targetNodes).Initialize(nodeIndex, targetAncestors, powersMatrixCA, lowerLimit, upperLimit, localRandom));
                });
            });
            // Add all chromosomes to the current population.
            Chromosomes.AddRange(bag);
        }

        /// <summary>
        /// Constructor for a subsequent population.
        /// </summary>
        /// <param name="previousPopulation">The previous population.</param>
        /// <param name="nodeIndex">The dictionary containing, for each node, its index in the node list.</param>
        /// <param name="targetNodes">The target nodes for the algorithm.</param>
        /// <param name="targetAncestors">The list containing, for each target nodes, the nodes from which it can be reached.</param>
        /// <param name="powersMatrixCA">The list containing the different powers of the matrix (CA, CA^2, CA^3, ... ).</param>
        /// <param name="nodeIsPreferred">The dictionary containing, for each node, its preferred status.</param>
        /// <param name="parameters">The parameters of the algorithm.</param>
        /// <param name="random">The random seed.</param>
        public Population(Population previousPopulation, Dictionary<string, int> nodeIndex, List<string> targetNodes, Dictionary<string, List<string>> targetAncestors, List<Matrix<double>> powersMatrixCA, Dictionary<string, bool> nodeIsPreferred, Parameters parameters, Random random)
        {
            // Initialize the list of chromosomes.
            Chromosomes = new List<Chromosome>();
            // Get the combined fitness list of the population.
            var combinedFitnessList = previousPopulation.GetCombinedFitnessList();
            // Add the specified number of elite chromosomes from the previous population.
            Chromosomes.AddRange(previousPopulation.GetBestChromosomes().OrderBy(item => random.NextDouble()).Take((int)Math.Min((int)Math.Floor(parameters.PercentageElite * parameters.PopulationSize), parameters.PopulationSize)));
            // Define a new thread-safe queue of random seeds, as well as a default random seed.
            var randomSeeds = new ConcurrentQueue<int>(Enumerable.Range(0, parameters.PopulationSize).Select(item => random.Next()));
            var defaultRandomSeed = random.Next();
            // Define a new concurrent bag for chromosomes.
            var bag = new ConcurrentBag<Chromosome>();
            // Add the specified number of random chromosomes.
            Parallel.For(Chromosomes.Count(), (int)Math.Min(Chromosomes.Count() + (int)Math.Floor(parameters.PercentageRandom * parameters.PopulationSize), parameters.PopulationSize), new ParallelOptions { MaxDegreeOfParallelism = DegreeOfParallelism }, index =>
            {
                // Try to get a new random seed from the list of random random seeds.
                if (!randomSeeds.TryDequeue(out var randomSeed))
                {
                    // If no seed could be gotten, then assign to it the default value.
                    randomSeed = defaultRandomSeed;
                }
                // Define a local random variable for only this thread.
                var localRandom = new Random(randomSeed);
                // Get the lower and upper limits.
                var lowerLimit = localRandom.Next(targetAncestors.Count());
                var upperLimit = (lowerLimit + parameters.RandomGenesPerChromosome) % targetAncestors.Count();
                // Add a new, initialized, chromosome.
                bag.Add(new Chromosome(targetNodes).Initialize(nodeIndex, targetAncestors, powersMatrixCA, lowerLimit, upperLimit, localRandom));
            });
            // Add all chromosomes to the current population.
            Chromosomes.AddRange(bag);
            // Reset the concurrent bag for chromosomes.
            bag = new ConcurrentBag<Chromosome>();
            // Add new chromosomes.
            Parallel.For(Chromosomes.Count(), parameters.PopulationSize, new ParallelOptions { MaxDegreeOfParallelism = DegreeOfParallelism }, index =>
            {
                // Try to get a new random seed from the list of random random seeds.
                if (!randomSeeds.TryDequeue(out var randomSeed))
                {
                    // If no seed could be gotten, then assign to it the default value.
                    randomSeed = defaultRandomSeed;
                }
                // Define a local random variable for only this thread.
                var localRandom = new Random(randomSeed);
                // Get a new offspring of two random chromosomes.
                var offspring = previousPopulation.Select(combinedFitnessList, localRandom)
                    .Crossover(previousPopulation.Select(combinedFitnessList, localRandom), nodeIndex, powersMatrixCA, nodeIsPreferred, parameters.CrossoverType, localRandom)
                    .Mutate(nodeIndex, targetAncestors, powersMatrixCA, nodeIsPreferred, parameters.MutationType, parameters.ProbabilityMutation, localRandom);
                // Add the offspring to the concurrent bag.
                bag.Add(offspring);
            });
            // Add all chromosomes to the current population.
            Chromosomes.AddRange(bag);
        }

        /// <summary>
        /// Gets the fitness list of the population.
        /// </summary>
        /// <returns>The fitness list.</returns>
        public List<double> GetFitnessList()
        {
            // Return the fitness list.
            return Chromosomes.Select(item => item.GetFitness()).ToList();
        }

        /// <summary>
        /// Gets all of the control paths corresponding to the solution chromosomes.
        /// </summary>
        /// <returns></returns>
        public List<Dictionary<string, List<string>>> GetControlPaths(Dictionary<string, int> nodeIndex, List<string> nodes, List<(string, string)> edges)
        {
            // Get the best fitness of the population.
            var bestFitness = GetFitnessList().Max();
            // Get the solution chrosomes.
            var solutionChromosomes = new List<Chromosome>();
            // Go over all of the chromosomes with the best fitness.
            foreach (var chromosome in Chromosomes.Where(item => item.GetFitness() == bestFitness))
            {
                // Check if the current combination already exists in the list of solutions.
                if (!solutionChromosomes.Any(item => new HashSet<string>(item.GetUniqueControlNodes()).SetEquals(new HashSet<string>(chromosome.GetUniqueControlNodes()))))
                {
                    // If not, then add it.
                    solutionChromosomes.Add(chromosome);
                }
            }
            // Get the arrays required for the Floyd-Warshall algorithm.
            var (dist, next) = GetFloydWarshallMatrices(edges, nodes);
            // Get the chromosome control paths.
            return solutionChromosomes.Select(item => item.Genes.ToDictionary(item1 => item1.Key, item1 => GetPath(next, nodeIndex[item1.Value], nodeIndex[item1.Key], nodes).Reverse().ToList())).ToList();
        }

        /// <summary>
        /// Gets the combined fitness list of the population, for a Monte-Carlo style selection.
        /// </summary>
        /// <returns>The combined fitness list of the population.</returns>
        private List<double> GetCombinedFitnessList()
        {
            // Get the fitness of each chromosome.
            var fitness = GetFitnessList();
            // Get the total fitness.
            var totalFitness = fitness.Sum();
            // Define a variable to store the temporary sum.
            var sum = 0.0;
            // Return the combined fitness.
            return fitness.Select(item => sum += item).Select(item => item / totalFitness).ToList();
        }

        /// <summary>
        /// Selects a chromosome based on its fitness (the better the fitness, the more chances to be selected).
        /// </summary>
        /// <param name="combinedFitnessList">The combined fitness list for the population.</param>
        /// <param name="random">The random seed.</param>
        /// <returns>A random chromosome, selected based on its fitness.</returns>
        private Chromosome Select(List<double> combinedFitnessList, Random random)
        {
            // Generate a random value.
            var randomValue = random.NextDouble();
            // Find the index corresponding to the random value.
            var index = combinedFitnessList.FindIndex(item => randomValue <= item);
            // Return the chromosome at the specified index.
            return Chromosomes[index];
        }

        /// <summary>
        /// Returns all of the unique chromosomes with the highest fitness in the population (providing an unique combination of genes).
        /// </summary>
        /// <returns>The chromosomes that have the best fitness in the current population.</returns>
        private IEnumerable<Chromosome> GetBestChromosomes()
        {
            // Get the best fitness of the population.
            var bestFitness = GetFitnessList().Max();
            // Define the variable to return.
            var bestChromosomes = new List<Chromosome>();
            // Go over all of the chromosomes with the best fitness.
            foreach (var chromosome in Chromosomes.Where(item => item.GetFitness() == bestFitness))
            {
                // Check if the current combination already exists in the list of solutions.
                if (!bestChromosomes.Any(item => item.IsEqual(chromosome)))
                {
                    // If not, then add it.
                    bestChromosomes.Add(chromosome);
                }
            }
            // Return the solutions.
            return bestChromosomes;
        }

        /// <summary>
        /// Represents the Floyd-Warshall algorithm for finding the shortest path between two nodes in a graph.
        /// </summary>
        /// <param name="edges">The edges of the graph.</param>
        /// <param name="nodes">The nodes of the graph.</param>
        /// <returns>Returns a tuple of two-dimensional arrays that contain the data required to build the shortest path between any two nodes.</returns>
        /// <remarks>The implementation is the Wikipedia version found on https://en.wikipedia.org/wiki/Floyd%E2%80%93Warshall_algorithm, written in C#.</remarks>
        private static (int[,], int[,]) GetFloydWarshallMatrices(List<(string, string)> edges, List<string> nodes)
        {
            // Define the variables to be used.
            var numberOfNodes = nodes.Count();
            // Transpose the list of edges into a list of node indices.
            var edgesIndex = edges.Select(item => (nodes.IndexOf(item.Item1), nodes.IndexOf(item.Item2)));
            // Define the matrices to return.
            var dist = new int[numberOfNodes, numberOfNodes];
            var next = new int[numberOfNodes, numberOfNodes];
            // Initialize the matrices to return.
            for (int index1 = 0; index1 < numberOfNodes; index1++)
            {
                for (int index2 = 0; index2 < numberOfNodes; index2++)
                {
                    dist[index1, index2] = int.MaxValue;
                    next[index1, index2] = -1;
                }
            }
            // Go over each edge and update the corresponding entry in matrices.
            foreach (var edge in edgesIndex)
            {
                dist[edge.Item1, edge.Item2] = 1;
                next[edge.Item1, edge.Item2] = edge.Item2;
            }
            // Go over each node and mark the distance from itself to itself as 0.
            for (int index = 0; index < numberOfNodes; index++)
            {
                dist[index, index] = 0;
                next[index, index] = index;
            }
            // Actual Floyd-Warshall implementation.
            for (int k = 0; k < numberOfNodes; k++)
            {
                for (int i = 0; i < numberOfNodes; i++)
                {
                    for (int j = 0; j < numberOfNodes; j++)
                    {
                        // Try to simulate the behavior of adding "inifinity".
                        var sum = int.MaxValue;
                        if (dist[i, k] != int.MaxValue && dist[k, j] != int.MaxValue)
                        {
                            sum = dist[i, k] + dist[k, j];
                        }
                        // Compare it with the current minimum value.
                        if (dist[i, j] > sum)
                        {
                            dist[i, j] = sum;
                            next[i, j] = next[i, k];
                        }
                    }
                }
            }
            // And return the two matrices.
            return (dist, next);
        }

        /// <summary>
        /// Returns the shortest path between two points.
        /// </summary>
        /// <param name="next">The array containing the next node in a path, required by the algorithm.</param>
        /// <param name="nodes">The nodes of the graph.</param>
        /// <param name="sourceIndex">The index of source node of the path.</param>
        /// <param name="targetIndex">The index of the target node of the path.</param>
        /// <returns>Returns an ordered list of the nodes in the path, from source to target.</returns>
        private static IEnumerable<string> GetPath(int[,] next, int sourceIndex, int targetIndex, List<string> nodes)
        {
            // Check if there is a path from the source to the target.
            if (next[sourceIndex, targetIndex] == -1)
            {
                return Enumerable.Empty<string>();
            }
            // Add the source to the path.
            var path = new List<string>() { nodes[sourceIndex] };
            // Recreate the path.
            while (sourceIndex != targetIndex)
            {
                sourceIndex = next[sourceIndex, targetIndex];
                path.Add(nodes[sourceIndex]);
            }
            // And return it.
            return path;
        }
    }
}
