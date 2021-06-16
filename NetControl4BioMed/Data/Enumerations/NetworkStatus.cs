using System.ComponentModel.DataAnnotations;

namespace NetControl4BioMed.Data.Enumerations
{
    /// <summary>
    /// Represents the possible statuses of a network.
    /// </summary>
    public enum NetworkStatus
    {
        /// <summary>
        /// Represents a network which has encountered an error.
        /// </summary>
        [Display(Name = "Error", Description = "The network has encountered an error.")]
        Error,

        /// <summary>
        /// Represents a network which has been defined.
        /// </summary>
        [Display(Name = "Defined", Description = "The network has been defined.")]
        Defined,

        /// <summary>
        /// Represents a network which is currently generating.
        /// </summary>
        [Display(Name = "Generating", Description = "The network is currently generating.")]
        Generating,

        /// <summary>
        /// Represents a network which has been generated successfully.
        /// </summary>
        [Display(Name = "Completed", Description = "The network has been generated successfully.")]
        Completed
    }
}
