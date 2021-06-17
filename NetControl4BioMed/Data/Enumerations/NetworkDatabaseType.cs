using System.ComponentModel.DataAnnotations;

namespace NetControl4BioMed.Data.Enumerations
{
    /// <summary>
    /// Represents the possible types of a database used by a network.
    /// </summary>
    public enum NetworkDatabaseType
    {
        /// <summary>
        /// Represents a database whose proteins are used by the network.
        /// </summary>
        [Display(Name = "Protein", Description = "The protein data in the database is used by the network.")]
        Protein,

        /// <summary>
        /// Represents a database whose interactions are used by the network.
        /// </summary>
        [Display(Name = "Interaction", Description = "The interaction data in the database is used by the network.")]
        Interaction
    }
}
