using NetControl4BioMed.Data.Interfaces;
using System.Collections.Generic;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a control path corresponding to an analysis.
    /// </summary>
    public class ControlPath : IAnalysisDependent
    {
        /// <summary>
        /// Gets or sets the unique internal ID of the control path.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the analysis ID of the control path.
        /// </summary>
        public string AnalysisId { get; set; }

        /// <summary>
        /// Gets or sets the analysis of the control path.
        /// </summary>
        public Analysis Analysis { get; set; }

        /// <summary>
        /// Gets or sets the paths which appear in the control path.
        /// </summary>
        public ICollection<Path> Paths { get; set; }
    }
}
