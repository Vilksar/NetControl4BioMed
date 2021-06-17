namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a network database.
    /// </summary>
    public class NetworkDatabaseInputModel
    {
        /// <summary>
        /// Represents the network of the network database.
        /// </summary>
        public NetworkInputModel Network { get; set; }

        /// <summary>
        /// Represents the database of the network database.
        /// </summary>
        public DatabaseInputModel Database { get; set; }

        /// <summary>
        /// Represents the type of the network database.
        /// </summary>
        public string Type { get; set; }
    }
}
