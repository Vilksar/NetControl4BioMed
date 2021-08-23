using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Interfaces;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between a network and a protein collection which it uses.
    /// </summary>
    public class NetworkProteinCollection : INetworkDependent, IProteinCollectionDependent
    {
        /// <summary>
        /// Gets or sets the network ID of the relationship.
        /// </summary>
        public string NetworkId { get; set; }

        /// <summary>
        /// Gets or sets the network of the relationship.
        /// </summary>
        public Network Network { get; set; }

        /// <summary>
        /// Gets or sets the protein collection ID of the relationship.
        /// </summary>
        public string ProteinCollectionId { get; set; }

        /// <summary>
        /// Gets or sets the protein collection of the relationship.
        /// </summary>
        public ProteinCollection ProteinCollection { get; set; }

        /// <summary>
        /// Gets or sets the type of the relationship.
        /// </summary>
        public NetworkProteinCollectionType Type { get; set; }
    }
}
