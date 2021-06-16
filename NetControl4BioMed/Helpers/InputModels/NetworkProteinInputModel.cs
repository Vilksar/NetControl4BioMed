namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a network protein.
    /// </summary>
    public class NetworkProteinInputModel
    {
        /// <summary>
        /// Represents the network of the network protein.
        /// </summary>
        public NetworkInputModel Network { get; set; }

        /// <summary>
        /// Represents the protein of the network protein.
        /// </summary>
        public ProteinInputModel Protein { get; set; }

        /// <summary>
        /// Represents the type of the network protein.
        /// </summary>
        public string Type { get; set; }
    }
}
