using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetControl4BioMed.Data.Enumerations;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between a network and a database which it uses.
    /// </summary>
    public class NetworkDatabase
    {
        /// <summary>
        /// Gets or sets the network ID of the relationship.
        /// </summary>
        public string NetworkId { get; set; }

        /// <summary>
        /// Gets or sets the network of the relationship.
        /// </summary>
        public Network Network { get; set; }

        /// <summary>
        /// Gets or sets the database ID of the relationship.
        /// </summary>
        public string DatabaseId { get; set; }

        /// <summary>
        /// Gets or sets the database of the relationship.
        /// </summary>
        public Database Database { get; set; }

        /// <summary>
        /// Gets or sets the type of the relationship.
        /// </summary>
        public NetworkDatabaseType Type { get; set; }
    }
}
