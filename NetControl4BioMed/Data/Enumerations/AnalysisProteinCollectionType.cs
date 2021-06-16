using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Enumerations
{
    /// <summary>
    /// Represents the possible types of a protein collection used by an analysis.
    /// </summary>
    public enum AnalysisProteinCollectionType
    {
        /// <summary>
        /// Represents a collection with proteins used as source proteins.
        /// </summary>
        [Display(Name = "Source", Description = "The proteins in the collection are source proteins in the analysis.")]
        Source,

        /// <summary>
        /// Represents a collection with proteins used as target proteins.
        /// </summary>
        [Display(Name = "Target", Description = "The proteins in the collection are target proteins in the analysis.")]
        Target
    }
}
