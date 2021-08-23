namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the variables needed for an e-mail sent to a user after they were added to an analysis.
    /// </summary>
    public class EmailWasAddedToAnalysisViewModel
    {
        /// <summary>
        /// Gets or sets the e-mail of the user that added a new user to the analysis.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the e-mail of the user added to the analysis.
        /// </summary>
        public string AddedByEmail { get; set; }

        /// <summary>
        /// Gets or sets the name of the analysis.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the URL to the analysis.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the URL to the home page of the application.
        /// </summary>
        public string ApplicationUrl { get; set; }
    }
}
