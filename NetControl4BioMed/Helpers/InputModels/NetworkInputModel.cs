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
        /// Represents the network user invitations of the network.
        /// </summary>
        public IEnumerable<NetworkUserInvitationInputModel> NetworkUserInvitations { get; set; }

        /// <summary>
        /// Represents the network databases of the network.
        /// </summary>
        public IEnumerable<NetworkDatabaseInputModel> NetworkDatabases { get; set; }

        /// <summary>
        /// Represents the network nodes of the network.
        /// </summary>
        public IEnumerable<NetworkNodeInputModel> NetworkNodes { get; set; }

        /// <summary>
        /// Represents the network edges of the network.
        /// </summary>
        public IEnumerable<NetworkEdgeInputModel> NetworkEdges { get; set; }

        /// <summary>
        /// Represents the network node collections of the network.
        /// </summary>
        public IEnumerable<NetworkNodeCollectionInputModel> NetworkNodeCollections { get; set; }

        /// <summary>
        /// Represents the analysis networks of the network.
        /// </summary>
        public IEnumerable<AnalysisNetworkInputModel> AnalysisNetworks { get; set; }
    }
}
