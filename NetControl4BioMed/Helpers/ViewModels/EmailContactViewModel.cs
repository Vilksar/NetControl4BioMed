namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the variables needed for a contact e-mail.
    /// </summary>
    public class EmailContactViewModel
    {
        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the e-mail of the user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the message sent by the user.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the URL to the home page of the application.
        /// </summary>
        public string ApplicationUrl { get; set; }
    }
}
