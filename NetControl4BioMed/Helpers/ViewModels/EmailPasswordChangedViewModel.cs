﻿namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the variables for a password changed e-mail.
    /// </summary>
    public class EmailPasswordChangedViewModel
    {
        /// <summary>
        /// Gets or sets the user e-mail.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the URL to the account page.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the URL to the home page of the application.
        /// </summary>
        public string ApplicationUrl { get; set; }
    }
}
