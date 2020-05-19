using System.Collections.Generic;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a node.
    /// </summary>
    public class NodeInputModel
    {
        /// <summary>
        /// Represents the ID of the node.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Represents the description of the node.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Represents the database nodes of the node.
        /// </summary>
        public IEnumerable<DatabaseNodeInputModel> DatabaseNodes { get; set; }

        /// <summary>
        /// Represents the database node field nodes of the node.
        /// </summary>
        public IEnumerable<DatabaseNodeFieldNodeInputModel> DatabaseNodeFieldNodes { get; set; }

        /// <summary>
        /// Represents the edge nodes of the node.
        /// </summary>
        public IEnumerable<EdgeNodeInputModel> EdgeNodes { get; set; }

        /// <summary>
        /// Represents the node collection nodes of the node.
        /// </summary>
        public IEnumerable<NodeCollectionNodeInputModel> NodeCollectionNodes { get; set; }

        /// <summary>
        /// Represents the network nodes of the node.
        /// </summary>
        public IEnumerable<NetworkNodeInputModel> NetworkNodes { get; set; }

        /// <summary>
        /// Represents the analysis nodes of the node.
        /// </summary>
        public IEnumerable<AnalysisNodeInputModel> AnalysisNodes { get; set; }
    }
}
