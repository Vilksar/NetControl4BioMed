namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a network node collection.
    /// </summary>
    public class NetworkNodeCollectionInputModel
    {
        /// <summary>
        /// Represents the network of the network node collection.
        /// </summary>
        public NetworkInputModel Network { get; set; }

        /// <summary>
        /// Represents the node collection of the network node collection.
        /// </summary>
        public NodeCollectionInputModel NodeCollection { get; set; }

        /// <summary>
        /// Represents the type of the network node collection.
        /// </summary>
        public string Type { get; set; }
    }
}
