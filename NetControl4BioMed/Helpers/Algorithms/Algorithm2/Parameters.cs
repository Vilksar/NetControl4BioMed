using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Algorithms.Algorithm2
{
    /// <summary>
    /// Represents the model of the parameters used by the algorithm.
    /// </summary>
    public class Parameters : IValidatableObject
    {
        /// <summary>
        /// Gets or sets the random seed to be used throughout the algorithm.
        /// </summary>
        [Display(Name = "Random seed", Description = "The random seed to be used throughout the algorithm.")]
        [Range(0, int.MaxValue, ErrorMessage = "The value must be a positive integer.")]
        [Required(ErrorMessage = "This field is required.")]
        public int RandomSeed { get; set; } = new Random().Next();

        /// <summary>
        /// Gets or sets the maximum length of any path between a source node and a target node.
        /// </summary>
        [Display(Name = "Maximum path length", Description = "The maximum length of any path between a source node and a target node.")]
        [Range(0, 25, ErrorMessage = "The value must be between {1} and {2}.")]
        [Required(ErrorMessage = "This field is required.")]
        public int MaximumPathLength { get; set; } = 15;

        /// <summary>
        /// Gets or sets the number of chromosomes in each population.
        /// </summary>
        [Display(Name = "Population size", Description = "The number of chromosomes in each population.")]
        [Range(2, 150, ErrorMessage = "The value must be between {1} and {2}.")]
        [Required(ErrorMessage = "This field is required.")]
        public int PopulationSize { get; set; } = 80;

        /// <summary>
        /// Gets or sets the maximum number of genes whose value can be simultaneously randomly generated.
        /// </summary>
        [Display(Name = "Random genes per chromosome", Description = "The maximum number of genes whose value can be simultaneously randomly generated.")]
        [Range(0, 30, ErrorMessage = "The value must be between {1} and {2}.")]
        [Required(ErrorMessage = "This field is required.")]
        public int RandomGenesPerChromosome { get; set; } = 25;

        /// <summary>
        /// Gets or sets the percentage of a population which is composed of randomly generated chromosomes.
        /// </summary>
        [Display(Name = "Percentage random", Description = "The percentage of a population which is composed of randomly generated chromosomes.")]
        [Range(0.0, 1.0, ErrorMessage = "The value must be between {1} and {2}.")]
        [Required(ErrorMessage = "This field is required.")]
        public double PercentageRandom { get; set; } = 0.25;

        /// <summary>
        /// Gets or sets the percentage of a population which is composed of the elite chromosomes of the previous population.
        /// </summary>
        [Display(Name = "Percentage elite", Description = "The percentage of a population which is composed of the elite chromosomes of the previous population.")]
        [Range(0.0, 1.0, ErrorMessage = "The value must be between {1} and {2}.")]
        [Required(ErrorMessage = "This field is required.")]
        public double PercentageElite { get; set; } = 0.25;

        /// <summary>
        /// Gets or sets the probability of mutation for each gene of a chromosome.
        /// </summary>
        [Display(Name = "Percentage elite", Description = "The probability of mutation for each gene of a chromosome.")]
        [Range(0.0, 1.0, ErrorMessage = "The value must be between {1} and {2}.")]
        [Required(ErrorMessage = "This field is required.")]
        public double ProbabilityMutation { get; set; } = 0.01;

        /// <summary>
        /// Gets or sets the crossover algorithm to be used.
        /// </summary>
        [Display(Name = "Crossover type", Description = "The crossover algorithm to be used.")]
        [Required(ErrorMessage = "This field is required.")]
        public CrossoverType CrossoverType { get; set; } = CrossoverType.WeightedRandom;

        /// <summary>
        /// Gets or sets the mutation algorithm to be used.
        /// </summary>
        [Display(Name = "Mutation type", Description = "The mutation algorithm to be used.")]
        [Required(ErrorMessage = "This field is required.")]
        public MutationType MutationType { get; set; } = MutationType.WeightedRandomAncestor;

        /// <summary>
        /// Checks if the parameters are valid.
        /// </summary>
        /// <param name="validationContext">Represents the validation context.</param>
        /// <returns>Returns a list with the validation errors.</returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Don't return any errors.
            yield break;
        }
    }
}
