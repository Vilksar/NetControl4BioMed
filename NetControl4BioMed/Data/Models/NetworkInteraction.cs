using NetControl4BioMed.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between a network and an interaction which it contains.
    /// </summary>
    public class NetworkInteraction : INetworkDependent, IInteractionDependent
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
        /// Gets or sets the interaction ID of the relationship.
        /// </summary>
        public string InteractionId { get; set; }

        /// <summary>
        /// Gets or sets the interaction of the relationship.
        /// </summary>
        public Interaction Interaction { get; set; }
    }
}
