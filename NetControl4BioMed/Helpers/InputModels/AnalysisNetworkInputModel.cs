namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for an analysis network.
    /// </summary>
    public class AnalysisNetworkInputModel
    {
        /// <summary>
        /// Represents the analysis of the analysis network.
        /// </summary>
        public AnalysisInputModel Analysis { get; set; }

        /// <summary>
        /// Represents the network ID of the analysis network.
        /// </summary>
        public NetworkInputModel Network { get; set; }
    }
}
