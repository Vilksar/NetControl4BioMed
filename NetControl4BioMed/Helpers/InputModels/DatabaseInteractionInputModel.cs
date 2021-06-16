namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a database interaction.
    /// </summary>
    public class DatabaseInteractionInputModel
    {
        /// <summary>
        /// Represents the database of the database interaction.
        /// </summary>
        public DatabaseInputModel Database { get; set; }

        /// <summary>
        /// Represents the interaction of the database interaction.
        /// </summary>
        public InteractionInputModel Interaction { get; set; }
    }
}
