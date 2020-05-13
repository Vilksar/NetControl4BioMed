namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a user.
    /// </summary>
    public class UserInputModel
    {
        /// <summary>
        /// Represents the ID of the user.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Represents the e-mail of the user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Represents the type of the user.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Represents the data of the user.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Represents the e-mail confirmation status of the user.
        /// </summary>
        public bool EmailConfirmed { get; set; }
    }
}
