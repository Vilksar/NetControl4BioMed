using NetControl4BioMed.Data.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a database edge field corresponding to a database.
    /// </summary>
    public class DatabaseEdgeField
    {
        /// <summary>
        /// Gets or sets the unique internal ID of the database edge field.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the date when the database edge field was created.
        /// </summary>
        public DateTime DateTimeCreated { get; set; }

        /// <summary>
        /// Gets or sets the ID of the database containing the database edge field.
        /// </summary>
        public string DatabaseId { get; set; }

        /// <summary>
        /// Gets or sets the database containing the database edge field.
        /// </summary>
        public Database Database { get; set; }

        /// <summary>
        /// Gets or sets the name of the database edge field.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the database edge field.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the type of the database edge field.
        /// </summary>
        public DatabaseEdgeFieldType Type { get; set; }

        /// <summary>
        /// Gets or sets the URL for the database edge field. The variable in the query will be replaced with the corresponding value.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the edges which have a value in the database edge field.
        /// </summary>
        public ICollection<DatabaseEdgeFieldEdge> DatabaseEdgeFieldEdges { get; set; }
    }
}
