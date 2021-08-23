using System.Collections.Generic;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a database protein field.
    /// </summary>
    public class DatabaseProteinFieldInputModel
    {
        /// <summary>
        /// Represents the ID of the database protein field.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Represents the name of the database protein field.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Represents the description of the database protein field.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Represents the URL of the database protein field.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Represents the search availability status of the database protein field.
        /// </summary>
        public bool IsSearchable { get; set; }

        /// <summary>
        /// Represents the database of the database protein field.
        /// </summary>
        public DatabaseInputModel Database { get; set; }

        /// <summary>
        /// Represents the database protein field proteins of the database protein field.
        /// </summary>
        public IEnumerable<DatabaseProteinFieldProteinInputModel> DatabaseProteinFieldProteins { get; set; }
    }
}
