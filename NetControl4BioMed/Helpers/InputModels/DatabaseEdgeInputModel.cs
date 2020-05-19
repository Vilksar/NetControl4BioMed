namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a database edge.
    /// </summary>
    public class DatabaseEdgeInputModel
    {
        /// <summary>
        /// Represents the database of the database edge.
        /// </summary>
        public DatabaseInputModel Database { get; set; }

        /// <summary>
        /// Represents the edge of the database edge.
        /// </summary>
        public EdgeInputModel Edge { get; set; }
    }
}
