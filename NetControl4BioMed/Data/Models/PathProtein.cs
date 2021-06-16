using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between a path and a protein which it contains.
    /// </summary>
    public class PathProtein : IPathDependent, IProteinDependent
    {
        /// <summary>
        /// Gets or sets the path ID of the relationship.
        /// </summary>
        public string PathId { get; set; }

        /// <summary>
        /// Gets or sets the path of the relationship.
        /// </summary>
        public Path Path { get; set; }

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
        public PathProteinType Type { get; set; }

        /// <summary>
        /// Gets or sets the index of the relationship.
        /// </summary>
        public int Index { get; set; }
    }
}
