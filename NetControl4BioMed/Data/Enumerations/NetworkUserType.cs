using System.ComponentModel.DataAnnotations;

namespace NetControl4BioMed.Data.Enumerations
{
    /// <summary>
    /// Represents the possible types of a user having access to a network.
    /// </summary>
    public enum NetworkUserType
    {
        /// <summary>
        /// Represents a user of no particular type in the network.
        /// </summary>
        [Display(Name = "None", Description = "The user has no particular type in the network.")]
        None,

        /// <summary>
        /// Represents a user owning the network.
        /// </summary>
        [Display(Name = "Owner", Description = "The user is owner of the network.")]
        Owner
    }
}
