namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for an analysis node.
    /// </summary>
    public class AnalysisNodeInputModel
    {
        /// <summary>
        /// Represents the analysis of the analysis node.
        /// </summary>
        public AnalysisInputModel Analysis { get; set; }

        /// <summary>
        /// Represents the node of the analysis node.
        /// </summary>
        public NodeInputModel Node { get; set; }

        /// <summary>
        /// Represents the type of the analysis node.
        /// </summary>
        public string Type { get; set; }
    }
}
