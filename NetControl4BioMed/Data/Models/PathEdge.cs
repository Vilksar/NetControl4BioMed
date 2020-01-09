using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between a path and an edge which it contains.
    /// </summary>
    public class PathEdge
    {
        /// <summary>
        /// Gets or sets the path ID of the relationship.
        /// </summary>
        public string PathId { get; set; }

        /// <summary>
        /// Gets or sets the path of the relationship.
        /// </summary>
        public Path Path { get; set; }

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
