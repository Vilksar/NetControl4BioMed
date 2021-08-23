using System;
using System.Collections.Generic;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a protein.
    /// </summary>
    public class Protein
    {
        /// <summary>
        /// Gets or sets the unique internal ID of the protein.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the date when the protein has been created.
        /// </summary>
        public DateTime DateTimeCreated { get; set; }

        /// <summary>
        /// Gets or sets the name of the protein.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the protein.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the databases in which the protein appears.
        /// </summary>
        public ICollection<DatabaseProtein> DatabaseProteins { get; set; }

        /// <summary>
        /// Gets or sets the protein database fields which have a value corresponding to the protein.
        /// </summary>
        public ICollection<DatabaseProteinFieldProtein> DatabaseProteinFieldProteins { get; set; }

        /// <summary>
        /// Gets or sets the interactions which contain the protein.
        /// </summary>
        public ICollection<InteractionProtein> InteractionProteins { get; set; }

        /// <summary>
        /// Gets or sets the protein collections which contain the protein.
        /// </summary>
        public ICollection<ProteinCollectionProtein> ProteinCollectionProteins { get; set; }

        /// <summary>
        /// Gets or sets the networks which contain the protein.
        /// </summary>
        public ICollection<NetworkProtein> NetworkProteins { get; set; }

        /// <summary>
        /// Gets or sets the analyses which contain the protein.
        /// </summary>
        public ICollection<AnalysisProtein> AnalysisProteins { get; set; }

        /// <summary>
        /// Gets or sets the paths which contain the protein.
        /// </summary>
        public ICollection<PathProtein> PathProteins { get; set; }
    }
}
