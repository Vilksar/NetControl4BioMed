using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between an interaction and a protein which it contains.
    /// </summary>
    public class InteractionProtein : IInteractionDependent, IProteinDependent
    {
        /// <summary>
        /// Gets or sets the interaction ID of the relationship.
        /// </summary>
        public string InteractionId { get; set; }

        /// <summary>
        /// Gets or sets the interaction of the relationship.
        /// </summary>
        public Interaction Interaction { get; set; }

        /// <summary>
        /// Gets or sets the protein ID of the relationship.
        /// </summary>
        public string ProteinId { get; set; }

        /// <summary>
        /// Gets or sets the protein of the relationship.
        /// </summary>
        public Protein Protein { get; set; }

        /// <summary>
        /// Gets or sets the type of the relationship.
        /// </summary>
        public InteractionProteinType Type { get; set; }
    }
}
