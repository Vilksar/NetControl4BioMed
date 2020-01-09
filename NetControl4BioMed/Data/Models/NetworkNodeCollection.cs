using NetControl4BioMed.Data.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between a network and a node collection which it uses.
    /// </summary>
    public class NetworkNodeCollection
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
        /// Gets or sets the node collection ID of the relationship.
        /// </summary>
        public string NodeCollectionId { get; set; }

        /// <summary>
        /// Gets or sets the node collection of the relationship.
        /// </summary>
        public NodeCollection NodeCollection { get; set; }

        /// <summary>
        /// Gets or sets the type of the relationship.
        /// </summary>
        public NetworkNodeCollectionType Type { get; set; }
    }
}
