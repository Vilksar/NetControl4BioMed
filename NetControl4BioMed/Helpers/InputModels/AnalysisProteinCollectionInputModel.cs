namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for an analysis protein collection.
    /// </summary>
    public class AnalysisProteinCollectionInputModel
    {
        /// <summary>
        /// Represents the analysis of the analysis protein collection.
        /// </summary>
        public AnalysisInputModel Analysis { get; set; }

        /// <summary>
        /// Represents the protein collection of the analysis protein collection.
        /// </summary>
        public ProteinCollectionInputModel ProteinCollection { get; set; }

        /// <summary>
        /// Represents the type of the analysis protein collection.
        /// </summary>
        public string Type { get; set; }
    }
}
