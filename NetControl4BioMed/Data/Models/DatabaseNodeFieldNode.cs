using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between a database node field and a node with a corresponding value.
    /// </summary>
    public class DatabaseNodeFieldNode
    {
        /// <summary>
        /// Gets or sets the database node field ID of the relationship.
        /// </summary>
        public string DatabaseNodeFieldId { get; set; }

        /// <summary>
        /// Gets or sets the database node field of the relationship.
        /// </summary>
        public DatabaseNodeField DatabaseNodeField { get; set; }

        /// <summary>
        /// Gets or sets the node ID of the relationship.
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// Gets or sets the node of the relationship.
        /// </summary>
        public Node Node { get; set; }

        /// <summary>
        /// Gets or sets the value of the relationship.
        /// </summary>
        public string Value { get; set; }
    }
}
