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
        /// Represents the database type ID of the database.
        /// </summary>
        public string DatabaseTypeId { get; set; }
    }
}
