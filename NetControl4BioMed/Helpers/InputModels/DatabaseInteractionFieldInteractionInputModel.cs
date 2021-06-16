namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a database interaction field interaction.
    /// </summary>
    public class DatabaseInteractionFieldInteractionInputModel
    {
        /// <summary>
        /// Represents the database interaction field of the database interaction field interaction.
        /// </summary>
        public DatabaseInteractionFieldInputModel DatabaseInteractionField { get; set; }

        /// <summary>
        /// Represents the interaction of the database interaction field interaction.
        /// </summary>
        public InteractionInputModel Interaction { get; set; }

        /// <summary>
        /// Represents the value of the database interaction field interaction.
        /// </summary>
        public string Value { get; set; }
    }
}
