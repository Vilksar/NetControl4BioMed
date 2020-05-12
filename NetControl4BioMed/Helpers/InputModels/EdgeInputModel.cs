using System.Collections.Generic;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for an edge.
    /// </summary>
    public class EdgeInputModel
    {
        /// <summary>
        /// Represents the ID of the edge.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Represents the description of the edge.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Represents the edge nodes of the edge.
        /// </summary>
        public IEnumerable<EdgeNodeInputModel> EdgeNodes { get; set; }

        /// <summary>
        /// Represents the IDs of the databases to be assigned to the edge.
        /// </summary>
        public IEnumerable<DatabaseEdgeInputModel> DatabaseEdges { get; set; }

        /// <summary>
        /// Represents the database edge field edges of the edge.
        /// </summary>
        public IEnumerable<DatabaseEdgeFieldEdgeInputModel> DatabaseEdgeFieldEdges { get; set; }
    }
}
