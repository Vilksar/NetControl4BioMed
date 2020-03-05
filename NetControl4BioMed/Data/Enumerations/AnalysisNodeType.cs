using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Enumerations
{
    /// <summary>
    /// Represents the possible types of a node contained by an analysis.
    /// </summary>
    public enum AnalysisNodeType
    {
        /// <summary>
        /// Represents a node of no particular type.
        /// </summary>
        [Display(Name = "None", Description = "The node has no particular type in the analysis.")]
        None,

        /// <summary>
        /// Represents a source (preferred) node.
        /// </summary>
        [Display(Name = "Source", Description = "The node is a possible source node in the analysis.")]
        Source,

        /// <summary>
        /// Represents a target node.
        /// </summary>
        [Display(Name = "Target", Description = "The node is a target node in the analysis.")]
        Target
    }
}
