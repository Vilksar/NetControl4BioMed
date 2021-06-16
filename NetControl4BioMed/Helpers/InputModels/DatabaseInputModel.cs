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
        /// Represents the database users of the database.
        /// </summary>
        public IEnumerable<DatabaseUserInputModel> DatabaseUsers { get; set; }

        /// <summary>
        /// Represents the database protein fields of the database.
        /// </summary>
        public IEnumerable<DatabaseProteinFieldInputModel> DatabaseProteinFields { get; set; }

        /// <summary>
        /// Represents the database interaction fields of the database.
        /// </summary>
        public IEnumerable<DatabaseInteractionFieldInputModel> DatabaseInteractionFields { get; set; }

        /// <summary>
        /// Represents the database proteins of the database.
        /// </summary>
        public IEnumerable<DatabaseProteinInputModel> DatabaseProteins { get; set; }

        /// <summary>
        /// Represents the database interactions of the database.
        /// </summary>
        public IEnumerable<DatabaseInteractionInputModel> DatabaseInteractions { get; set; }

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
