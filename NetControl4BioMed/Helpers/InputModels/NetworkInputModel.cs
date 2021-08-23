using System.Collections.Generic;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a network.
    /// </summary>
    public class NetworkInputModel
    {
        /// <summary>
        /// Represents the ID of the network.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Represents the name of the network.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Represents the description of the network.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Represents the public availability status of the network.
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Gets or sets the demonstration availability of the network.
        /// </summary>
        public bool IsDemonstration { get; set; }

        /// <summary>
        /// Represents the algorithm of the network.
        /// </summary>
        public string Algorithm { get; set; }

        /// <summary>
        /// Represents the data for generating the network.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Represents the network users of the network.
        /// </summary>
        public IEnumerable<NetworkUserInputModel> NetworkUsers { get; set; }

        /// <summary>
        /// Represents the network databases of the network.
        /// </summary>
        public IEnumerable<NetworkDatabaseInputModel> NetworkDatabases { get; set; }

        /// <summary>
        /// Represents the network proteins of the network.
        /// </summary>
        public IEnumerable<NetworkProteinInputModel> NetworkProteins { get; set; }

        /// <summary>
        /// Represents the network interactions of the network.
        /// </summary>
        public IEnumerable<NetworkInteractionInputModel> NetworkInteractions { get; set; }

        /// <summary>
        /// Represents the network protein collections of the network.
        /// </summary>
        public IEnumerable<NetworkProteinCollectionInputModel> NetworkProteinCollections { get; set; }

        /// <summary>
        /// Represents the analyses of the network.
        /// </summary>
        public IEnumerable<AnalysisInputModel> Analyses { get; set; }
    }
}
