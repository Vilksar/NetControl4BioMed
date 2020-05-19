namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a database user invitation.
    /// </summary>
    public class DatabaseUserInvitationInputModel
    {
        /// <summary>
        /// Represents the database of the database user invitation.
        /// </summary>
        public DatabaseInputModel Database { get; set; }

        /// <summary>
        /// Represents the e-mail of the database user invitation.
        /// </summary>
        public string Email { get; set; }
    }
}
