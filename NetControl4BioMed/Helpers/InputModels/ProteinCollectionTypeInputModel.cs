namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the protein model for a protein collection type.
    /// </summary>
    public class ProteinCollectionTypeInputModel
    {
        /// <summary>
        /// Represents the protein collection of the protein collection type.
        /// </summary>
        public ProteinCollectionInputModel ProteinCollection { get; set; }

        /// <summary>
        /// Represents the type of the protein collection type.
        /// </summary>
        public string Type { get; set; }
    }
}
