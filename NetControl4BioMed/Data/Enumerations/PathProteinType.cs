using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Enumerations
{
    /// <summary>
    /// Represents the possible types of a protein contained by a path in an analysis.
    /// </summary>
    public enum PathProteinType
    {
        /// <summary>
        /// Represents a protein of no particular type.
        /// </summary>
        [Display(Name = "None", Description = "The protein has no particular type in the path.")]
        None,

        /// <summary>
        /// Represents a source protein.
        /// </summary>
        [Display(Name = "Source", Description = "The protein is a source protein in the path.")]
        Source,

        /// <summary>
        /// Represents a target protein.
        /// </summary>
        [Display(Name = "Target", Description = "The protein is a target protein in the path.")]
        Target
    }
}
