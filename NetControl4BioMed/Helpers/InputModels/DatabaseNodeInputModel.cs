namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a database node.
    /// </summary>
    public class DatabaseNodeInputModel
    {
        /// <summary>
        /// Represents the database ID of the database node.
        /// </summary>
        public string DatabaseId { get; set; }

        /// <summary>
        /// Represents the node ID of the database node.
        /// </summary>
        public string NodeId { get; set; }
    }
}
