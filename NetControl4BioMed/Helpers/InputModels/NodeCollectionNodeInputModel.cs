namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a node collection node.
    /// </summary>
    public class NodeCollectionNodeInputModel
    {
        /// <summary>
        /// Represents the node collection of the node collection node.
        /// </summary>
        public NodeCollectionInputModel NodeCollection { get; set; }

        /// <summary>
        /// Represents the node of the node collection node.
        /// </summary>
        public NodeInputModel Node { get; set; }
    }
}
