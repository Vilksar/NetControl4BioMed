using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a possible type of database.
    /// </summary>
    public class DatabaseType
    {
        /// <summary>
        /// Gets or sets the unique internal ID of the database type.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the date when the database type was created.
        /// </summary>
        public DateTime DateTimeCreated { get; set; }

        /// <summary>
        /// Gets or sets the name of the database type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the database type.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the databases of this type.
        /// </summary>
        public ICollection<Database> Databases { get; set; }
    }
}
