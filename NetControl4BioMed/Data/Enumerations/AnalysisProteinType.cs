using System.ComponentModel.DataAnnotations;

namespace NetControl4BioMed.Data.Enumerations
{
    /// <summary>
    /// Represents the possible types of a proteins contained by an analysis.
    /// </summary>
    public enum AnalysisProteinType
    {
        /// <summary>
        /// Represents a protein of no particular type.
        /// </summary>
        [Display(Name = "None", Description = "The protein has no particular type in the analysis.")]
        None,

        /// <summary>
        /// Represents a source protein.
        /// </summary>
        [Display(Name = "Source", Description = "The protein is a source protein in the analysis.")]
        Source,

        /// <summary>
        /// Represents a target protein.
        /// </summary>
        [Display(Name = "Target", Description = "The protein is a target protein in the analysis.")]
        Target
    }
}
