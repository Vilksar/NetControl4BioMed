namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a network edge.
    /// </summary>
    public class NetworkEdgeInputModel
    {
        /// <summary>
        /// Represents the network of the network edge.
        /// </summary>
        public NetworkInputModel Network { get; set; }

        /// <summary>
        /// Represents the edge of the network edge.
        /// </summary>
        public EdgeInputModel Edge { get; set; }
    }
}
