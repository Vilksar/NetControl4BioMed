using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a database.
    /// </summary>
    public class Database
    {
        /// <summary>
        /// Gets or sets the unique internal ID of the database.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the date when the database was created.
        /// </summary>
        public DateTime DateTimeCreated { get; set; }

        /// <summary>
        /// Gets or sets the name of the database.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the database.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the URL for the database.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the public availability of the database.
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Gets or sets the type ID of the database.
        /// </summary>
        public string DatabaseTypeId { get; set; }

        /// <summary>
        /// Gets or sets the type of the database.
        /// </summary>
        public DatabaseType DatabaseType { get; set; }

        /// <summary>
        /// Gets or sets the registered users who have access to the database, even when it is not public.
        /// </summary>
        public ICollection<DatabaseUser> DatabaseUsers { get; set; }

        /// <summary>
        /// Gets or sets the unregistered users that will have access to the database after regsitration, even when it is not public.
        /// </summary>
        public ICollection<DatabaseUserInvitation> DatabaseUserInvitations { get; set; }

        /// <summary>
        /// Gets or sets the node fields which appear in the database.
        /// </summary>
        public ICollection<DatabaseNodeField> DatabaseNodeFields { get; set; }

        /// <summary>
        /// Gets or sets the edge fields which appear in the database.
        /// </summary>
        public ICollection<DatabaseEdgeField> DatabaseEdgeFields { get; set; }

        /// <summary>
        /// Gets or sets the nodes which appear in the database.
        /// </summary>
        public ICollection<DatabaseNode> DatabaseNodes { get; set; }

        /// <summary>
        /// Gets or sets the edges which appear in the database.
        /// </summary>
        public ICollection<DatabaseEdge> DatabaseEdges { get; set; }

        /// <summary>
        /// Gets or sets the networks which use the database.
        /// </summary>
        public ICollection<NetworkDatabase> NetworkDatabases { get; set; }

        /// <summary>
        /// Gets or sets the analyses which use the database.
        /// </summary>
        public ICollection<AnalysisDatabase> AnalysisDatabases { get; set; }
    }
}
