namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a protein collection protein.
    /// </summary>
    public class ProteinCollectionProteinInputModel
    {
        /// <summary>
        /// Represents the protein collection of the protein collection protein.
        /// </summary>
        public ProteinCollectionInputModel ProteinCollection { get; set; }

        /// <summary>
        /// Represents the protein of the protein collection protein.
        /// </summary>
        public ProteinInputModel Protein { get; set; }
    }
}
