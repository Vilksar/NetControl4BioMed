using NetControl4BioMed.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a path in a control path corresponding to an analysis.
    /// </summary>
    public class Path : IControlPathDependent
    {
        /// <summary>
        /// Gets or sets the unique internal ID of the path.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the control path ID of the path.
        /// </summary>
        public string ControlPathId { get; set; }

        /// <summary>
        /// Gets or sets the control path of the path.
        /// </summary>
        public ControlPath ControlPath { get; set; }

        /// <summary>
        /// Gets or sets the proteins which appear in the path.
        /// </summary>
        public ICollection<PathProtein> PathProteins { get; set; }

        /// <summary>
        /// Gets or sets the interactions which appear in the path.
        /// </summary>
        public ICollection<PathInteraction> PathInteractions { get; set; }
    }
}
