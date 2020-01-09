using NetControl4BioMed.Data.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between an analysis and a node which it contains.
    /// </summary>
    public class AnalysisNode
    {
        /// <summary>
        /// Gets or sets the analysis ID of the relationship.
        /// </summary>
        public string AnalysisId { get; set; }

        /// <summary>
        /// Gets or sets the analysis of the relationship.
        /// </summary>
        public Analysis Analysis { get; set; }

        /// <summary>
        /// Gets or sets the node ID of the relationship.
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// Gets or sets the node of the relationship.
        /// </summary>
        public Node Node { get; set; }

        /// <summary>
        /// Gets or sets the type of the relationship.
        /// </summary>
        public AnalysisNodeType Type { get; set; }
    }
}
