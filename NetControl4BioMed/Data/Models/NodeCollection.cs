using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a node collection.
    /// </summary>
    public class NodeCollection
    {
        /// <summary>
        /// Gets or sets the unique internal ID of the node collection.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the date when the node collection was created.
        /// </summary>
        public DateTime DateTimeCreated { get; set; }

        /// <summary>
        /// Gets or sets the name of the node collection.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the node collection.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the analyses using the node collection.
        /// </summary>
        public ICollection<AnalysisNodeCollection> AnalysisNodeCollections { get; set; }

        /// <summary>
        /// Gets or sets the networks using the node collection.
        /// </summary>
        public ICollection<NetworkNodeCollection> NetworkNodeCollections { get; set; }

        /// <summary>
        /// Gets or sets the nodes which appear in the node collection.
        /// </summary>
        public ICollection<NodeCollectionNode> NodeCollectionNodes { get; set; }
    }
}
