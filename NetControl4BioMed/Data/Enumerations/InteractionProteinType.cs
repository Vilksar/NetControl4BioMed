using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Enumerations
{
    /// <summary>
    /// Represents the possible types of a protein contained by an interaction.
    /// </summary>
    public enum InteractionProteinType
    {
        /// <summary>
        /// Represents a source protein.
        /// </summary>
        [Display(Name = "Source", Description = "The protein is a source protein in the interaction.")]
        Source,

        /// <summary>
        /// Represents a target protein.
        /// </summary>
        [Display(Name = "Target", Description = "The protein is a target protein in the interaction.")]
        Target
    }
}
