namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for an analysis user.
    /// </summary>
    public class AnalysisUserInputModel
    {
        /// <summary>
        /// Represents the analysis of the analysis user.
        /// </summary>
        public AnalysisInputModel Analysis { get; set; }

        /// <summary>
        /// Represents the user of the analysis user.
        /// </summary>
        public UserInputModel User { get; set; }

        /// <summary>
        /// Represents the e-mail of the analysis user.
        /// </summary>
        public string Email { get; set; }
    }
}
