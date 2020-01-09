using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Enumerations
{
    /// <summary>
    /// Represents the possible types of a node contained by a network.
    /// </summary>
    public enum NetworkNodeType
    {
        /// <summary>
        /// Represents a node of no particular type.
        /// </summary>
        [Display(Name = "None", Description = "The node has no particular type in the network.")]
        None,

        /// <summary>
        /// Represents a seed node.
        /// </summary>
        [Display(Name = "Seed", Description = "The node is a seed node in the network.")]
        Seed
    }
}
