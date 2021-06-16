using System.ComponentModel.DataAnnotations;

namespace NetControl4BioMed.Data.Enumerations
{
    /// <summary>
    /// Represents the possible types of a protein collection used by a network.
    /// </summary>
    public enum NetworkProteinCollectionType
    {
        /// <summary>
        /// Represents a collection with proteins used as seed proteins.
        /// </summary>
        [Display(Name = "Seed", Description = "The proteins in the collection are seed proteins in the network.")]
        Seed
    }
}
