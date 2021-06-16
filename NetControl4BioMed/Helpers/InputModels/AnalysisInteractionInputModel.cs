namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for an analysis interaction.
    /// </summary>
    public class AnalysisInteractionInputModel
    {
        /// <summary>
        /// Represents the analysis of the analysis interaction.
        /// </summary>
        public AnalysisInputModel Analysis { get; set; }

        /// <summary>
        /// Represents the interaction of the analysis interaction.
        /// </summary>
        public InteractionInputModel Interaction { get; set; }
    }
}
