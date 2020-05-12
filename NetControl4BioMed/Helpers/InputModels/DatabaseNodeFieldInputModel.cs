namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a database node field.
    /// </summary>
    public class DatabaseNodeFieldInputModel
    {
        /// <summary>
        /// Represents the ID of the database node field.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Represents the name of the database node field.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Represents the description of the database node field.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Represents the URL of the database node field.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Represents the search availability status of the database node field.
        /// </summary>
        public bool IsSearchable { get; set; }

        /// <summary>
        /// Represents the database ID of the database node field.
        /// </summary>
        public string DatabaseId { get; set; }
    }
}
