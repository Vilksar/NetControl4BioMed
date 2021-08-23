using System;
using System.Collections.Generic;

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
        /// Gets or sets the registered users who have access to the database, even when it is not public.
        /// </summary>
        public ICollection<DatabaseUser> DatabaseUsers { get; set; }

        /// <summary>
        /// Gets or sets the protein fields which appear in the database.
        /// </summary>
        public ICollection<DatabaseProteinField> DatabaseProteinFields { get; set; }

        /// <summary>
        /// Gets or sets the interaction fields which appear in the database.
        /// </summary>
        public ICollection<DatabaseInteractionField> DatabaseInteractionFields { get; set; }

        /// <summary>
        /// Gets or sets the proteins which appear in the database.
        /// </summary>
        public ICollection<DatabaseProtein> DatabaseProteins { get; set; }

        /// <summary>
        /// Gets or sets the interactions which appear in the database.
        /// </summary>
        public ICollection<DatabaseInteraction> DatabaseInteractions { get; set; }

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
