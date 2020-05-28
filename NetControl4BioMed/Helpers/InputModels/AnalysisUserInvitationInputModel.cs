namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for an analysis user invitation.
    /// </summary>
    public class AnalysisUserInvitationInputModel
    {
        /// <summary>
        /// Represents the analysis of the analysis user invitation.
        /// </summary>
        public AnalysisInputModel Analysis { get; set; }

        /// <summary>
        /// Represents the e-mail of the analysis user invitation.
        /// </summary>
        public string Email { get; set; }
    }
}
