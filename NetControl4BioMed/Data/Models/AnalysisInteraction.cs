using NetControl4BioMed.Data.Interfaces;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between an analysis and an interaction which it contains.
    /// </summary>
    public class AnalysisInteraction : IAnalysisDependent, IInteractionDependent
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
        /// Gets or sets the interaction ID of the relationship.
        /// </summary>
        public string InteractionId { get; set; }

        /// <summary>
        /// Gets or sets the interaction of the relationship.
        /// </summary>
        public Interaction Interaction { get; set; }
    }
}
