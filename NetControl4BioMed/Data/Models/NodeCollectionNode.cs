using NetControl4BioMed.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between a node collection and a node which it contains.
    /// </summary>
    public class NodeCollectionNode : INodeCollectionDependent
    {
        /// <summary>
        /// Gets or sets the node collection ID of the relationship.
        /// </summary>
        public string NodeCollectionId { get; set; }

        /// <summary>
        /// Gets or sets the node collection of the relationship.
        /// </summary>
        public NodeCollection NodeCollection { get; set; }

        /// <summary>
        /// Gets or sets the node ID of the relationship.
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// Gets or sets the node of the relationship.
        /// </summary>
        public Node Node { get; set; }
    }
}
