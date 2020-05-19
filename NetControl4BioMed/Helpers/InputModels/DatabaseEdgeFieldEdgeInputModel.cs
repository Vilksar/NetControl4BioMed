namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a database edge field edge.
    /// </summary>
    public class DatabaseEdgeFieldEdgeInputModel
    {
        /// <summary>
        /// Represents the database edge field of the database edge field edge.
        /// </summary>
        public DatabaseEdgeFieldInputModel DatabaseEdgeField { get; set; }

        /// <summary>
        /// Represents the edge of the database edge field edge.
        /// </summary>
        public EdgeInputModel Edge { get; set; }

        /// <summary>
        /// Represents the value of the database edge field edge.
        /// </summary>
        public string Value { get; set; }
    }
}
