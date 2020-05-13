using System.Collections.Generic;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a network.
    /// </summary>
    public class NetworkInputModel
    {
        /// <summary>
        /// Represents the ID of the network.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Represents the database type ID of the network.
        /// </summary>
        public string DatabaseTypeId { get; set; }

        /// <summary>
        /// Represents the name of the network.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Represents the description of the network.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Represents the algorithm of the network.
        /// </summary>
        public string Algorithm { get; set; }

        /// <summary>
        /// Represents the seed data for generating the network.
        /// </summary>
        public string SeedData { get; set; }

        /// <summary>
        /// Represents the network databases of the network.
        /// </summary>
        public IEnumerable<NetworkDatabaseInputModel> NetworkDatabases { get; set; }

        /// <summary>
        /// Represents the network node collections of the network.
        /// </summary>
        public IEnumerable<NetworkNodeCollectionInputModel> NetworkNodeCollections { get; set; }
    }
}
