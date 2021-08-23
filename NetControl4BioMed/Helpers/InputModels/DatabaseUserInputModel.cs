namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a database user.
    /// </summary>
    public class DatabaseUserInputModel
    {
        /// <summary>
        /// Represents the database of the database user.
        /// </summary>
        public DatabaseInputModel Database { get; set; }

        /// <summary>
        /// Represents the user of the database user.
        /// </summary>
        public UserInputModel User { get; set; }

        /// <summary>
        /// Represents the e-mail of the database user.
        /// </summary>
        public string Email { get; set; }
    }
}
