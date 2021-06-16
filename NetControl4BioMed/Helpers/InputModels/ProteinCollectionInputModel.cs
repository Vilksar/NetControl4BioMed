using System.Collections.Generic;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a protein collection.
    /// </summary>
    public class ProteinCollectionInputModel
    {
        /// <summary>
        /// Represents the ID of the protein collection.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Represents the name of the protein collection.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Represents the description of the protein collection.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Represents the protein collection types of the protein collection.
        /// </summary>
        public IEnumerable<ProteinCollectionTypeInputModel> ProteinCollectionTypes { get; set; }

        /// <summary>
        /// Represents the protein collection proteins of the protein collection.
        /// </summary>
        public IEnumerable<ProteinCollectionProteinInputModel> ProteinCollectionProteins { get; set; }

        /// <summary>
        /// Represents the network protein collections of the protein collection.
        /// </summary>
        public IEnumerable<NetworkProteinCollectionInputModel> NetworkProteinCollections { get; set; }

        /// <summary>
        /// Represents the analysis protein collections of the protein collection.
        /// </summary>
        public IEnumerable<AnalysisProteinCollectionInputModel> AnalysisProteinCollections { get; set; }
    }
}
