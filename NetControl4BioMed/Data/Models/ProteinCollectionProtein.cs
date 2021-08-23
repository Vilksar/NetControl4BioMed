using NetControl4BioMed.Data.Interfaces;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between a protein collection and a protein which it contains.
    /// </summary>
    public class ProteinCollectionProtein : IProteinCollectionDependent, IProteinDependent
    {
        /// <summary>
        /// Gets or sets the protein collection ID of the relationship.
        /// </summary>
        public string ProteinCollectionId { get; set; }

        /// <summary>
        /// Gets or sets the protein collection of the relationship.
        /// </summary>
        public ProteinCollection ProteinCollection { get; set; }

        /// <summary>
        /// Gets or sets the protein ID of the relationship.
        /// </summary>
        public string ProteinId { get; set; }

        /// <summary>
        /// Gets or sets the protein of the relationship.
        /// </summary>
        public Protein Protein { get; set; }
    }
}
