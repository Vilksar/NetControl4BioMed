using NetControl4BioMed.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between a protein collection and a type.
    /// </summary>
    public class ProteinCollectionType : IProteinCollectionDependent
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
        /// Gets or sets the type of the relationship.
        /// </summary>
        public Enumerations.ProteinCollectionType Type { get; set; }
    }
}
