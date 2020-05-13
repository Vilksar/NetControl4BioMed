namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a network user invitation.
    /// </summary>
    public class NetworkUserInvitationInputModel
    {
        /// <summary>
        /// Represents the network ID of the network user invitation.
        /// </summary>
        public string NetworkId { get; set; }

        /// <summary>
        /// Represents the e-mail of the network user invitation.
        /// </summary>
        public string Email { get; set; }
    }
}
