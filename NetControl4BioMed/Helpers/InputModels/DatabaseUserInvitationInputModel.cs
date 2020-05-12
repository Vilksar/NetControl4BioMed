namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a database user invitation.
    /// </summary>
    public class DatabaseUserInvitationInputModel
    {
        /// <summary>
        /// Represents the database ID of the database user invitation.
        /// </summary>
        public string DatabaseId { get; set; }

        /// <summary>
        /// Represents the e-mail of the database user invitation.
        /// </summary>
        public string Email { get; set; }
    }
}
