namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a database protein.
    /// </summary>
    public class DatabaseProteinInputModel
    {
        /// <summary>
        /// Represents the database of the database protein.
        /// </summary>
        public DatabaseInputModel Database { get; set; }

        /// <summary>
        /// Represents the protein of the database protein.
        /// </summary>
        public ProteinInputModel Protein { get; set; }
    }
}
