namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a database user.
    /// </summary>
    public class DatabaseUserInputModel
    {
        /// <summary>
        /// Represents the database ID of the database user.
        /// </summary>
        public string DatabaseId { get; set; }

        /// <summary>
        /// Represents the user ID of the database user.
        /// </summary>
        public string UserId { get; set; }
    }
}
