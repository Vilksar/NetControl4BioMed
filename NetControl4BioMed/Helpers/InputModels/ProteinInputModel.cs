using System.Collections.Generic;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a protein.
    /// </summary>
    public class ProteinInputModel
    {
        /// <summary>
        /// Represents the ID of the protein.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Represents the name of the protein.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Represents the description of the protein.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Represents the database proteins of the protein.
        /// </summary>
        public IEnumerable<DatabaseProteinInputModel> DatabaseProteins { get; set; }

        /// <summary>
        /// Represents the database protein field proteins of the protein.
        /// </summary>
        public IEnumerable<DatabaseProteinFieldProteinInputModel> DatabaseProteinFieldProteins { get; set; }

        /// <summary>
        /// Represents the interaction proteins of the protein.
        /// </summary>
        public IEnumerable<InteractionProteinInputModel> InteractionProteins { get; set; }

        /// <summary>
        /// Represents the protein collection proteins of the protein.
        /// </summary>
        public IEnumerable<ProteinCollectionProteinInputModel> ProteinCollectionProteins { get; set; }

        /// <summary>
        /// Represents the network proteins of the protein.
        /// </summary>
        public IEnumerable<NetworkProteinInputModel> NetworkProteins { get; set; }

        /// <summary>
        /// Represents the analysis proteins of the protein.
        /// </summary>
        public IEnumerable<AnalysisProteinInputModel> AnalysisProteins { get; set; }
    }
}
