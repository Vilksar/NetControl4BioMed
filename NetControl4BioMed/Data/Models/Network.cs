using NetControl4BioMed.Data.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a network.
    /// </summary>
    public class Network
    {
        /// <summary>
        /// Gets or sets the unique internal ID of the network.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the date when the network has been created.
        /// </summary>
        public DateTime DateTimeCreated { get; set; }

        /// <summary>
        /// Gets or sets the name of the network.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the network.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the status the network.
        /// </summary>
        public NetworkStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the message log of the network.
        /// </summary>
        public string Log { get; set; }

        /// <summary>
        /// Gets or sets the algorithm used to generate the network.
        /// </summary>
        public NetworkAlgorithm Algorithm { get; set; }

        /// <summary>
        /// Gets or sets the data used to generate the network.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Gets or sets the users which have access to the network.
        /// </summary>
        public ICollection<NetworkUser> NetworkUsers { get; set; }

        /// <summary>
        /// Gets or sets the unregistered users that will have access to the network after registration.
        /// </summary>
        public ICollection<NetworkUserInvitation> NetworkUserInvitations { get; set; }

        /// <summary>
        /// Gets or sets the databases which are used by the network.
        /// </summary>
        public ICollection<NetworkDatabase> NetworkDatabases { get; set; }

        /// <summary>
        /// Gets or sets the nodes which appear in the network.
        /// </summary>
        public ICollection<NetworkNode> NetworkNodes { get; set; }

        /// <summary>
        /// Gets or sets the edges which appear in the network.
        /// </summary>
        public ICollection<NetworkEdge> NetworkEdges { get; set; }

        /// <summary>
        /// Gets or sets the node collections which are used by the network.
        /// </summary>
        public ICollection<NetworkNodeCollection> NetworkNodeCollections { get; set; }

        /// <summary>
        /// Gets or sets the analyses which use the network.
        /// </summary>
        public ICollection<AnalysisNetwork> AnalysisNetworks { get; set; }
    }
}
