﻿using System.ComponentModel.DataAnnotations;

namespace NetControl4BioMed.Helpers.Algorithms.Analyses.Genetic
{
    /// <summary>
    /// Represents the possible crossover algorithms for an analysis.
    /// </summary>
    public enum CrossoverType
    {
        /// <summary>
        /// Represents the default crossover algorithm.
        /// </summary>
        [Display(Name = "Weighted random parent")]
        WeightedRandom,

        /// <summary>
        /// Represents the default crossover algorithm that is twice more likely to choose preferred nodes.
        /// </summary>
        [Display(Name = "Weighted random preferred parent")]
        WeightedRandomPreferred,

        /// <summary>
        /// Represents the default crossover algorithm that always chooses the dominant gene.
        /// </summary>
        [Display(Name = "Dominant parent")]
        Dominant,

        /// <summary>
        /// Represents the default crossover algorithm that always chooses preferred nodes whenever possible.
        /// </summary>
        [Display(Name = "Dominant preferred parent")]
        DominantPreferred
    }
}
