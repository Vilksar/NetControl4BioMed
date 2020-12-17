using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Enumerations
{
    /// <summary>
    /// Represents the possible types of a sample.
    /// </summary>
    public enum SampleType
    {
        /// <summary>
        /// Represents a sample that contains seed nodes.
        /// </summary>
        [Display(Name = "Seed nodes", Description = "The sample contains seed nodes.")]
        SeedNodes,

        /// <summary>
        /// Represents a sample that contains seed edges.
        /// </summary>
        [Display(Name = "Seed edges", Description = "The sample contains seed edges.")]
        SeedEdges,

        /// <summary>
        /// Represents a sample that contains source nodes.
        /// </summary>
        [Display(Name = "Source nodes", Description = "The sample contains source nodes.")]
        SourceNodes,

        /// <summary>
        /// Represents a sample that contains target nodes.
        /// </summary>
        [Display(Name = "Target nodes", Description = "The sample contains target nodes.")]
        TargetNodes
    }
}
