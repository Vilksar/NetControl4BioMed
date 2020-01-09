using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Enumerations
{
    /// <summary>
    /// Represents the possible types of a node contained by a path in an analysis.
    /// </summary>
    public enum PathNodeType
    {
        /// <summary>
        /// Represents a node of no particular type.
        /// </summary>
        [Display(Name = "None", Description = "The node has no particular type in the path.")]
        None,

        /// <summary>
        /// Represents a source node.
        /// </summary>
        [Display(Name = "Source", Description = "The node is a source node in the path.")]
        Source,

        /// <summary>
        /// Represents a target node.
        /// </summary>
        [Display(Name = "Target", Description = "The node is a target node in the path.")]
        Target
    }
}
