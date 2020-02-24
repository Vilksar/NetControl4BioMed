using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between a node collection and a database which it uses.
    /// </summary>
    public class NodeCollectionDatabase
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
        /// Gets or sets the database ID of the relationship.
        /// </summary>
        public string DatabaseId { get; set; }

        /// <summary>
        /// Gets or sets the database of the relationship.
        /// </summary>
        public Database Database { get; set; }
    }
}
