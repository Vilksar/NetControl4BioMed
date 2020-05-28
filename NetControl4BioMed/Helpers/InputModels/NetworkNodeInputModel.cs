namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a network node.
    /// </summary>
    public class NetworkNodeInputModel
    {
        /// <summary>
        /// Represents the network of the network node.
        /// </summary>
        public NetworkInputModel Network { get; set; }

        /// <summary>
        /// Represents the node of the network node.
        /// </summary>
        public NodeInputModel Node { get; set; }

        /// <summary>
        /// Represents the type of the network node.
        /// </summary>
        public string Type { get; set; }
    }
}
