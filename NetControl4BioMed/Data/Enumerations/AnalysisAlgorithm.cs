using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Enumerations
{
    /// <summary>
    /// Represents the possible algorithms used for running an analysis.
    /// </summary>
    public enum AnalysisAlgorithm
    {
        /// <summary>
        /// Represents the first algorithm (greedy).
        /// </summary>
        [Display(Name = "Greedy algorithm", Description = "A greedy algorithm.")]
        Algorithm1,

        /// <summary>
        /// Represents the second algorithm (genetic).
        /// </summary>
        [Display(Name = "Genetic algorithm", Description = "A genetic algorithm.")]
        Algorithm2
    }
}
