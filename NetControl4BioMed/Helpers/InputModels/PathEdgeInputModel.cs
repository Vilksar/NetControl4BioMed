namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a path edge.
    /// </summary>
    public class PathEdgeInputModel
    {
        /// <summary>
        /// Represents the network of the path edge.
        /// </summary>
        public PathInputModel Path { get; set; }

        /// <summary>
        /// Represents the edge of the path edge.
        /// </summary>
        public EdgeInputModel Edge { get; set; }

        /// <summary>
        /// Gets or sets the index of the path edge.
        /// </summary>
        public int Index { get; set; }
    }
}
