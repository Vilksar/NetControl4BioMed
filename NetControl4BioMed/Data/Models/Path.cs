using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a path in a control path corresponding to an analysis.
    /// </summary>
    public class Path
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
        /// Gets or sets the nodes which appear in the path.
        /// </summary>
        public ICollection<PathNode> PathNodes { get; set; }

        /// <summary>
        /// Gets or sets the edges which appear in the path.
        /// </summary>
        public ICollection<PathEdge> PathEdges { get; set; }
    }
}
