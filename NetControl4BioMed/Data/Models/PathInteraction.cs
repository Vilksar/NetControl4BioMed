using NetControl4BioMed.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between a path and an interaction which it contains.
    /// </summary>
    public class PathInteraction : IPathDependent, IInteractionDependent
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
        /// Gets or sets the interaction ID of the relationship.
        /// </summary>
        public string InteractionId { get; set; }

        /// <summary>
        /// Gets or sets the interaction of the relationship.
        /// </summary>
        public Interaction Interaction { get; set; }

        /// <summary>
        /// Gets or sets the index of the relationship.
        /// </summary>
        public int Index { get; set; }
    }
}
