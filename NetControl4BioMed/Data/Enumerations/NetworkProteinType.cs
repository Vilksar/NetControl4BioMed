using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Enumerations
{
    /// <summary>
    /// Represents the possible types of a protein contained by a network.
    /// </summary>
    public enum NetworkProteinType
    {
        /// <summary>
        /// Represents a protein of no particular type.
        /// </summary>
        [Display(Name = "None", Description = "The protein has no particular type in the network.")]
        None,

        /// <summary>
        /// Represents a seed protein.
        /// </summary>
        [Display(Name = "Seed", Description = "The protein is a seed protein in the network.")]
        Seed
    }
}
