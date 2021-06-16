using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Enumerations
{
    /// <summary>
    /// Represents the possible types of a protein collection.
    /// </summary>
    public enum ProteinCollectionType
    {
        /// <summary>
        /// Represents a protein collection that contains seed proteins.
        /// </summary>
        [Display(Name = "Seed", Description = "The protein collection contains seed proteins.")]
        Seed,

        /// <summary>
        /// Represents a protein collection that contains source proteins.
        /// </summary>
        [Display(Name = "Source", Description = "The protein collection contains source proteins.")]
        Source,

        /// <summary>
        /// Represents a protein collection that contains target proteins.
        /// </summary>
        [Display(Name = "Target", Description = "The protein collection contains target proteins.")]
        Target
    }
}
