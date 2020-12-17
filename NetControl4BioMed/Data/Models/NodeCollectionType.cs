using NetControl4BioMed.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnumerationNodeCollectionType = NetControl4BioMed.Data.Enumerations.NodeCollectionType;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between a node collection and a type.
    /// </summary>
    public class NodeCollectionType : INodeCollectionDependent
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
        /// Gets or sets the type of the relationship.
        /// </summary>
        public EnumerationNodeCollectionType Type { get; set; }
    }
}
