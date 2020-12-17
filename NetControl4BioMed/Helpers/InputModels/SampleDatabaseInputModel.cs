namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a sample database.
    /// </summary>
    public class SampleDatabaseInputModel
    {
        /// <summary>
        /// Represents the sample of the sample database.
        /// </summary>
        public SampleInputModel Sample { get; set; }

        /// <summary>
        /// Represents the database of the sample database.
        /// </summary>
        public DatabaseInputModel Database { get; set; }
    }
}
