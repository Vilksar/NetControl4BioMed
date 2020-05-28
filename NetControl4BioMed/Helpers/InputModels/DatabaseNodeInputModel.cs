namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a database node.
    /// </summary>
    public class DatabaseNodeInputModel
    {
        /// <summary>
        /// Represents the database of the database node.
        /// </summary>
        public DatabaseInputModel Database { get; set; }

        /// <summary>
        /// Represents the node of the database node.
        /// </summary>
        public NodeInputModel Node { get; set; }
    }
}
