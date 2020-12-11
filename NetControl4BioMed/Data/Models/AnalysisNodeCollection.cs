using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between an analysis and a node collection which it uses.
    /// </summary>
    public class AnalysisNodeCollection : IAnalysisDependent
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
        /// Gets or sets the node collection ID of the relationship.
        /// </summary>
        public string NodeCollectionId { get; set; }

        /// <summary>
        /// Gets or sets the node collection of the relationship.
        /// </summary>
        public NodeCollection NodeCollection { get; set; }

        /// <summary>
        /// Gets or sets the type of the relationship.
        /// </summary>
        public AnalysisNodeCollectionType Type { get; set; }
    }
}
