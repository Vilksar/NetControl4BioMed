using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between an analysis and a protein collection which it uses.
    /// </summary>
    public class AnalysisProteinCollection : IAnalysisDependent, IProteinCollectionDependent
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
        /// Gets or sets the protein collection ID of the relationship.
        /// </summary>
        public string ProteinCollectionId { get; set; }

        /// <summary>
        /// Gets or sets the protein collection of the relationship.
        /// </summary>
        public ProteinCollection ProteinCollection { get; set; }

        /// <summary>
        /// Gets or sets the type of the relationship.
        /// </summary>
        public AnalysisProteinCollectionType Type { get; set; }
    }
}
