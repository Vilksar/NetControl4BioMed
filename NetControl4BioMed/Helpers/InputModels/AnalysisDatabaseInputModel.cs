namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for an analysis database.
    /// </summary>
    public class AnalysisDatabaseInputModel
    {
        /// <summary>
        /// Represents the analysis of the analysis database.
        /// </summary>
        public AnalysisInputModel Analysis { get; set; }

        /// <summary>
        /// Represents the database of the analysis database.
        /// </summary>
        public DatabaseInputModel Database { get; set; }

        /// <summary>
        /// Represents the type of the analysis database.
        /// </summary>
        public string Type { get; set; }
    }
}
