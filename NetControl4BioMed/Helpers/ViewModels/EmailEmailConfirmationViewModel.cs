namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the variables needed for an address confirmation e-mail.
    /// </summary>
    public class EmailEmailConfirmationViewModel
    {
        /// <summary>
        /// Gets or sets the e-mail to be confirmed.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the encoded e-mail confirmation URL.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the URL to the home page of the application.
        /// </summary>
        public string ApplicationUrl { get; set; }
    }
}
