using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between a database edge field and an edge with a corresponding value.
    /// </summary>
    public class DatabaseEdgeFieldEdge
    {
        /// <summary>
        /// Gets or sets the edge field ID of the relationship.
        /// </summary>
        public string DatabaseEdgeFieldId { get; set; }

        /// <summary>
        /// Gets or sets the database edge field of the relationship.
        /// </summary>
        public DatabaseEdgeField DatabaseEdgeField { get; set; }

        /// <summary>
        /// Gets or sets the edge ID of the relationship.
        /// </summary>
        public string EdgeId { get; set; }

        /// <summary>
        /// Gets or sets the edge of the relationship.
        /// </summary>
        public Edge Edge { get; set; }

        /// <summary>
        /// Gets or sets the value of the relationship.
        /// </summary>
        public string Value { get; set; }
    }
}
