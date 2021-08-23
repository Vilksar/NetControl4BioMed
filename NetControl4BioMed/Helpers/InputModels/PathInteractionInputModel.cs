namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a path interaction.
    /// </summary>
    public class PathInteractionInputModel
    {
        /// <summary>
        /// Represents the network of the path interaction.
        /// </summary>
        public PathInputModel Path { get; set; }

        /// <summary>
        /// Represents the interaction of the path interaction.
        /// </summary>
        public InteractionInputModel Interaction { get; set; }

        /// <summary>
        /// Gets or sets the index of the path interaction.
        /// </summary>
        public int Index { get; set; }
    }
}
