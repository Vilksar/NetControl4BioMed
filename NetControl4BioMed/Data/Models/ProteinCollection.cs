using System;
using System.Collections.Generic;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a protein collection.
    /// </summary>
    public class ProteinCollection
    {
        /// <summary>
        /// Gets or sets the unique internal ID of the protein collection.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the date when the protein collection was created.
        /// </summary>
        public DateTime DateTimeCreated { get; set; }

        /// <summary>
        /// Gets or sets the name of the protein collection.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the protein collection.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the types of the protein collection.
        /// </summary>
        public ICollection<ProteinCollectionType> ProteinCollectionTypes { get; set; }

        /// <summary>
        /// Gets or sets the proteins which appear in the protein collection.
        /// </summary>
        public ICollection<ProteinCollectionProtein> ProteinCollectionProteins { get; set; }

        /// <summary>
        /// Gets or sets the networks using the protein collection.
        /// </summary>
        public ICollection<NetworkProteinCollection> NetworkProteinCollections { get; set; }

        /// <summary>
        /// Gets or sets the analyses using the protein collection.
        /// </summary>
        public ICollection<AnalysisProteinCollection> AnalysisProteinCollections { get; set; }
    }
}
