using System.Collections.Generic;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a database.
    /// </summary>
    public class DatabaseInputModel
    {
        /// <summary>
        /// Represents the ID of the database.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Represents the name of the database.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Represents the description of the database.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Represents the URL of the database.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Represents the public availability status of the database.
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Represents the database type of the database.
        /// </summary>
        public DatabaseTypeInputModel DatabaseType { get; set; }

        /// <summary>
        /// Represents the database users of the database.
        /// </summary>
        public IEnumerable<DatabaseUserInputModel> DatabaseUsers { get; set; }

        /// <summary>
        /// Represents the database user invitations of the database.
        /// </summary>
        public IEnumerable<DatabaseUserInvitationInputModel> DatabaseUserInvitations { get; set; }

        /// <summary>
        /// Represents the database node fields of the database.
        /// </summary>
        public IEnumerable<DatabaseNodeFieldInputModel> DatabaseNodeFields { get; set; }

        /// <summary>
        /// Represents the database edge fields of the database.
        /// </summary>
        public IEnumerable<DatabaseEdgeFieldInputModel> DatabaseEdgeFields { get; set; }

        /// <summary>
        /// Represents the database nodes of the database.
        /// </summary>
        public IEnumerable<DatabaseNodeInputModel> DatabaseNodes { get; set; }

        /// <summary>
        /// Represents the database edges of the database.
        /// </summary>
        public IEnumerable<DatabaseEdgeInputModel> DatabaseEdges { get; set; }

        /// <summary>
        /// Represents the node collection databases of the database.
        /// </summary>
        public IEnumerable<NodeCollectionDatabaseInputModel> NodeCollectionDatabases { get; set; }

        /// <summary>
        /// Represents the network databases of the database.
        /// </summary>
        public IEnumerable<NetworkDatabaseInputModel> NetworkDatabases { get; set; }

        /// <summary>
        /// Represents the analysis databases of the database.
        /// </summary>
        public IEnumerable<AnalysisDatabaseInputModel> AnalysisDatabases { get; set; }
    }
}
