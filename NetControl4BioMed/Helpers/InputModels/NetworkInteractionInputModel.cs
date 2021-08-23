namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a network interaction.
    /// </summary>
    public class NetworkInteractionInputModel
    {
        /// <summary>
        /// Represents the network of the network interaction.
        /// </summary>
        public NetworkInputModel Network { get; set; }

        /// <summary>
        /// Represents the interaction of the network interaction.
        /// </summary>
        public InteractionInputModel Interaction { get; set; }
    }
}
