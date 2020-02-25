using NetControl4BioMed.Data.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a database node field corresponding to a database.
    /// </summary>
    public class DatabaseNodeField
    {
        /// <summary>
        /// Gets or sets the unique internal ID of the database node field.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the date when the database node field has been created.
        /// </summary>
        public DateTime DateTimeCreated { get; set; }

        /// <summary>
        /// Gets or sets the ID of the database containing the database node field.
        /// </summary>
        public string DatabaseId { get; set; }

        /// <summary>
        /// Gets or sets the database containing the database node field.
        /// </summary>
        public Database Database { get; set; }

        /// <summary>
        /// Gets or sets the name of the database node field.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the database node field.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the URL for the database node field. The variable in the query will be replaced with the corresponding value.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the automatic detection state for searches of the nodes in the database node field.
        /// </summary>
        public bool IsSearchable { get; set; }

        /// <summary>
        /// Gets or sets the nodes which have a value in the database node field.
        /// </summary>
        public ICollection<DatabaseNodeFieldNode> DatabaseNodeFieldNodes { get; set; }
    }
}
