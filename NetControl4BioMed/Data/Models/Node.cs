using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a node.
    /// </summary>
    public class Node
    {
        /// <summary>
        /// Gets or sets the unique internal ID of the node.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the date when the node has been created.
        /// </summary>
        public DateTime DateTimeCreated { get; set; }

        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the node.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the edges which contain the node.
        /// </summary>
        public ICollection<EdgeNode> EdgeNodes { get; set; }

        /// <summary>
        /// Gets or sets the networks which contain the node.
        /// </summary>
        public ICollection<NetworkNode> NetworkNodes { get; set; }

        /// <summary>
        /// Gets or sets the analyses which contain the node.
        /// </summary>
        public ICollection<AnalysisNode> AnalysisNodes { get; set; }

        /// <summary>
        /// Gets or sets the paths which contain the node.
        /// </summary>
        public ICollection<PathNode> PathNodes { get; set; }

        /// <summary>
        /// Gets or sets the node collections which contain the node.
        /// </summary>
        public ICollection<NodeCollectionNode> NodeCollectionNodes { get; set; }

        /// <summary>
        /// Gets or sets the node database fields which have a value corresponding to the node.
        /// </summary>
        public ICollection<DatabaseNodeFieldNode> DatabaseNodeFieldNodes { get; set; }
    }
}
