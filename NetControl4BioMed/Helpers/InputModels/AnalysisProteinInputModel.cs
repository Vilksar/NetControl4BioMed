namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for an analysis protein.
    /// </summary>
    public class AnalysisProteinInputModel
    {
        /// <summary>
        /// Represents the analysis of the analysis protein.
        /// </summary>
        public AnalysisInputModel Analysis { get; set; }

        /// <summary>
        /// Represents the protein of the analysis protein.
        /// </summary>
        public ProteinInputModel Protein { get; set; }

        /// <summary>
        /// Represents the type of the analysis protein.
        /// </summary>
        public string Type { get; set; }
    }
}
