using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Enumerations
{
    /// <summary>
    /// Represents the possible types of a node contained by an edge.
    /// </summary>
    public enum EdgeNodeType
    {
        /// <summary>
        /// Represents a source node.
        /// </summary>
        [Display(Name = "Source", Description = "The node is a source node in the edge.")]
        Source,

        /// <summary>
        /// Represents a target node.
        /// </summary>
        [Display(Name = "Target", Description = "The node is a target node in the edge.")]
        Target
    }
}
