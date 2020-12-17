using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Enumerations
{
    /// <summary>
    /// Represents the possible types of a node collection.
    /// </summary>
    public enum NodeCollectionType
    {
        /// <summary>
        /// Represents a node collection that contains seed nodes.
        /// </summary>
        [Display(Name = "Seed", Description = "The node collection contains seed nodes.")]
        Seed,

        /// <summary>
        /// Represents a node collection that contains source nodes.
        /// </summary>
        [Display(Name = "Source", Description = "The node collection contains source nodes.")]
        Source,

        /// <summary>
        /// Represents a node collection that contains target nodes.
        /// </summary>
        [Display(Name = "Target", Description = "The node collection contains target nodes.")]
        Target
    }
}
