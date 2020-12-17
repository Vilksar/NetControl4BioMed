namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a node collection type.
    /// </summary>
    public class NodeCollectionTypeInputModel
    {
        /// <summary>
        /// Represents the node collection of the node collection type.
        /// </summary>
        public NodeCollectionInputModel NodeCollection { get; set; }

        /// <summary>
        /// Represents the type of the node collection type.
        /// </summary>
        public string Type { get; set; }
    }
}
