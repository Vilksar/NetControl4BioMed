using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Enumerations
{
    /// <summary>
    /// Represents the possible types of a node collection used by an analysis.
    /// </summary>
    public enum AnalysisNodeCollectionType
    {
        /// <summary>
        /// Represents a collection with nodes used as source (preferred) nodes.
        /// </summary>
        [Display(Name = "Source", Description = "The nodes in the collection are possible source nodes in the analysis.")]
        Source,

        /// <summary>
        /// Represents a collection with nodes used as target nodes.
        /// </summary>
        [Display(Name = "Target", Description = "The nodes in the collection are target nodes in the analysis.")]
        Target
    }
}
