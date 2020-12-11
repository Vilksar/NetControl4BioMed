using System.Collections.Generic;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a path.
    /// </summary>
    public class PathInputModel
    {
        /// <summary>
        /// Represents the ID of the path.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Represents the description of the edge.
        /// </summary>
        public ControlPathInputModel ControlPath { get; set; }

        /// <summary>
        /// Gets or sets the nodes which appear in the path.
        /// </summary>
        public IEnumerable<PathNodeInputModel> PathNodes { get; set; }

        /// <summary>
        /// Gets or sets the edges which appear in the path.
        /// </summary>
        public IEnumerable<PathEdgeInputModel> PathEdges { get; set; }
    }
}
