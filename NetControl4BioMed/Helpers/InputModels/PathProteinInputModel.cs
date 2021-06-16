namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a path protein.
    /// </summary>
    public class PathProteinInputModel
    {
        /// <summary>
        /// Represents the network of the path protein.
        /// </summary>
        public PathInputModel Path { get; set; }

        /// <summary>
        /// Represents the protein of the path protein.
        /// </summary>
        public ProteinInputModel Protein { get; set; }

        /// <summary>
        /// Represents the type of the path protein.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the index of the path protein.
        /// </summary>
        public int Index { get; set; }
    }
}
