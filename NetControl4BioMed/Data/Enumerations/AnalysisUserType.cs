using System.ComponentModel.DataAnnotations;

namespace NetControl4BioMed.Data.Enumerations
{
    /// <summary>
    /// Represents the possible types of a user having access to an analysis.
    /// </summary>
    public enum AnalysisUserType
    {
        /// <summary>
        /// Represents a user of no particular type in the analysis.
        /// </summary>
        [Display(Name = "None", Description = "The user has no particular type in the analysis.")]
        None,

        /// <summary>
        /// Represents a user owning the analysis.
        /// </summary>
        [Display(Name = "Owner", Description = "The user is owner of the analysis.")]
        Owner
    }
}
