namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a network node collection.
    /// </summary>
    public class NetworkNodeCollectionInputModel
    {
        /// <summary>
        /// Represents the network ID of the network node collection.
        /// </summary>
        public string NetworkId { get; set; }

        /// <summary>
        /// Represents the node collection ID of the network node collection.
        /// </summary>
        public string NodeCollectionId { get; set; }

        /// <summary>
        /// Represents the type of the network node collection.
        /// </summary>
        public string Type { get; set; }
    }
}
