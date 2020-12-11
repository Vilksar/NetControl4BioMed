using NetControl4BioMed.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between an analysis and an edge which it contains.
    /// </summary>
    public class AnalysisEdge : IAnalysisDependent
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
        /// Gets or sets the edge ID of the relationship.
        /// </summary>
        public string EdgeId { get; set; }

        /// <summary>
        /// Gets or sets the edge of the relationship.
        /// </summary>
        public Edge Edge { get; set; }
    }
}
