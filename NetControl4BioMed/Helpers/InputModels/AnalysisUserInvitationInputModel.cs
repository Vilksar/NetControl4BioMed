namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for an analysis user invitation.
    /// </summary>
    public class AnalysisUserInvitationInputModel
    {
        /// <summary>
        /// Represents the analysis ID of the analysis user invitation.
        /// </summary>
        public string AnalysisId { get; set; }

        /// <summary>
        /// Represents the e-mail of the analysis user invitation.
        /// </summary>
        public string Email { get; set; }
    }
}
