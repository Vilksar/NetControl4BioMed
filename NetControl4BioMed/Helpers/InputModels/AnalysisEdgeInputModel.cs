namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for an analysis edge.
    /// </summary>
    public class AnalysisEdgeInputModel
    {
        /// <summary>
        /// Represents the analysis of the analysis edge.
        /// </summary>
        public AnalysisInputModel Analysis { get; set; }

        /// <summary>
        /// Represents the edge of the analysis edge.
        /// </summary>
        public EdgeInputModel Edge { get; set; }
    }
}
