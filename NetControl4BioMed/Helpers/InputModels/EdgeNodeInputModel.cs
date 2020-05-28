namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for an edge node.
    /// </summary>
    public class EdgeNodeInputModel
    {
        /// <summary>
        /// Represents the edge of the edge node.
        /// </summary>
        public EdgeInputModel Edge { get; set; }

        /// <summary>
        /// Represents the node of the edge node.
        /// </summary>
        public NodeInputModel Node { get; set; }

        /// <summary>
        /// Represents the type of the edge node.
        /// </summary>
        public string Type { get; set; }
    }
}
