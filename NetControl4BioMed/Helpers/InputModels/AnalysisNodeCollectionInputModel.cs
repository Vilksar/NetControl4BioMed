namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for an analysis node collection.
    /// </summary>
    public class AnalysisNodeCollectionInputModel
    {
        /// <summary>
        /// Represents the analysis of the analysis node collection.
        /// </summary>
        public AnalysisInputModel Analysis { get; set; }

        /// <summary>
        /// Represents the node collection of the analysis node collection.
        /// </summary>
        public NodeCollectionInputModel NodeCollection { get; set; }

        /// <summary>
        /// Represents the type of the analysis node collection.
        /// </summary>
        public string Type { get; set; }
    }
}
