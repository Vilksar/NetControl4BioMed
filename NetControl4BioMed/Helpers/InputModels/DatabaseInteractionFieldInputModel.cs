using System.Collections.Generic;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a database interaction field.
    /// </summary>
    public class DatabaseInteractionFieldInputModel
    {
        /// <summary>
        /// Represents the ID of the database interaction field.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Represents the name of the database interaction field.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Represents the description of the database interaction field.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Represents the URL of the database interaction field.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Represents the search availability status of the database interaction field.
        /// </summary>
        public bool IsSearchable { get; set; }

        /// <summary>
        /// Represents the database of the database interaction field.
        /// </summary>
        public DatabaseInputModel Database { get; set; }

        /// <summary>
        /// Represents the database interaction field interactions of the database interaction field.
        /// </summary>
        public IEnumerable<DatabaseInteractionFieldInteractionInputModel> DatabaseInteractionFieldInteractions { get; set; }
    }
}
