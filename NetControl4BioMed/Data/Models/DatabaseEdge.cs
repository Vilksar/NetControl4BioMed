using NetControl4BioMed.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between a database and an edge which it contains.
    /// </summary>
    public class DatabaseEdge : IEdgeDependent
    {
        /// <summary>
        /// Gets or sets the database ID of the relationship.
        /// </summary>
        public string DatabaseId { get; set; }

        /// <summary>
        /// Gets or sets the database of the relationship.
        /// </summary>
        public Database Database { get; set; }

        /// <summary>
        /// Gets or sets the edge ID of the relationship.
        /// </summary>
        public string EdgeId { get; set; }

        /// <summary>
        /// Gets or sets the edge of the relationship.
        /// </summary>
        public Edge Edge { get; set; }
    }
}
