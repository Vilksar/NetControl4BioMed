namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for an interaction protein.
    /// </summary>
    public class InteractionProteinInputModel
    {
        /// <summary>
        /// Represents the interaction of the interaction protein.
        /// </summary>
        public InteractionInputModel Interaction { get; set; }

        /// <summary>
        /// Represents the protein of the interaction protein.
        /// </summary>
        public ProteinInputModel Protein { get; set; }

        /// <summary>
        /// Represents the type of the interaction protein.
        /// </summary>
        public string Type { get; set; }
    }
}
