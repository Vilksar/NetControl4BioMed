namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a network protein collection.
    /// </summary>
    public class NetworkProteinCollectionInputModel
    {
        /// <summary>
        /// Represents the network of the network protein collection.
        /// </summary>
        public NetworkInputModel Network { get; set; }

        /// <summary>
        /// Represents the protein collection of the network protein collection.
        /// </summary>
        public ProteinCollectionInputModel ProteinCollection { get; set; }

        /// <summary>
        /// Represents the type of the network protein collection.
        /// </summary>
        public string Type { get; set; }
    }
}
