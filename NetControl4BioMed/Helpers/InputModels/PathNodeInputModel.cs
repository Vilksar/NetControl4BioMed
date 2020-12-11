namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a path node.
    /// </summary>
    public class PathNodeInputModel
    {
        /// <summary>
        /// Represents the network of the path node.
        /// </summary>
        public PathInputModel Path { get; set; }

        /// <summary>
        /// Represents the node of the path node.
        /// </summary>
        public NodeInputModel Node { get; set; }

        /// <summary>
        /// Represents the type of the path node.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the index of the path node.
        /// </summary>
        public int Index { get; set; }
    }
}
