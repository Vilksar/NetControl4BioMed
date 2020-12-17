namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a sample type.
    /// </summary>
    public class SampleTypeInputModel
    {
        /// <summary>
        /// Represents the node collection of the sample type.
        /// </summary>
        public SampleInputModel Sample { get; set; }

        /// <summary>
        /// Represents the type of the sample type.
        /// </summary>
        public string Type { get; set; }
    }
}
