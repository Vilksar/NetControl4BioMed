namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a database node field node.
    /// </summary>
    public class DatabaseNodeFieldNodeInputModel
    {
        /// <summary>
        /// Represents the database node field of the database node field node.
        /// </summary>
        public DatabaseNodeFieldInputModel DatabaseNodeField { get; set; }

        /// <summary>
        /// Represents the node of the database node field node.
        /// </summary>
        public NodeInputModel Node { get; set; }

        /// <summary>
        /// Represents the value of the database node field node.
        /// </summary>
        public string Value { get; set; }
    }
}
