using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Enumerations
{
    /// <summary>
    /// Represents the possible statuses of a network.
    /// </summary>
    public enum NetworkStatus
    {
        /// <summary>
        /// Represents a network which has encountered an error.
        /// </summary>
        [Display(Name = "Error", Description = "The network has encountered an error.")]
        Error,

        /// <summary>
        /// Represents a network which has been defined.
        /// </summary>
        [Display(Name = "Defined", Description = "The network has been defined.")]
        Defined,

        /// <summary>
        /// Represents a network which has been generated.
        /// </summary>
        [Display(Name = "Generated", Description = "The network has been generated.")]
        Generated
    }
}
