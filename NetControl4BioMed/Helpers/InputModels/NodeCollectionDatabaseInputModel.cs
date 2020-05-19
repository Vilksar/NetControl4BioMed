namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a node collection database.
    /// </summary>
    public class NodeCollectionDatabaseInputModel
    {
        /// <summary>
        /// Represents the node collection of the node collection database.
        /// </summary>
        public NodeCollectionInputModel NodeCollection { get; set; }

        /// <summary>
        /// Represents the database of the node collection database.
        /// </summary>
        public DatabaseInputModel Database { get; set; }
    }
}
