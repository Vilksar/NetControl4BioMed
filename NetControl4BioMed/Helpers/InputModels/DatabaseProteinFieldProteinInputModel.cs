namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a database protein field protein.
    /// </summary>
    public class DatabaseProteinFieldProteinInputModel
    {
        /// <summary>
        /// Represents the database protein field of the database protein field protein.
        /// </summary>
        public DatabaseProteinFieldInputModel DatabaseProteinField { get; set; }

        /// <summary>
        /// Represents the protein of the database protein field protein.
        /// </summary>
        public ProteinInputModel Protein { get; set; }

        /// <summary>
        /// Represents the value of the database protein field protein.
        /// </summary>
        public string Value { get; set; }
    }
}
