using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between a network and a node which it contains.
    /// </summary>
    public class NetworkNode : INetworkDependent
    {
        /// <summary>
        /// Gets or sets the network ID of the relationship.
        /// </summary>
        public string NetworkId { get; set; }

        /// <summary>
        /// Gets or sets the network of the relationship.
        /// </summary>
        public Network Network { get; set; }

        /// <summary>
        /// Gets or sets the node ID of the relationship.
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// Gets or sets the node of the relationship.
        /// </summary>
        public Node Node { get; set; }

        /// <summary>
        /// Gets or sets the type of the relationship.
        /// </summary>
        public NetworkNodeType Type { get; set; }
    }
}
