using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the model for updating a node collection read as JSON.
    /// </summary>
    public class DataUpdateNodeCollectionViewModel
    {
        /// <summary>
        /// Represents the ID of the node collection.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Represents the name of the node collection.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Represents the description of the node collection.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Represents the IDs of the databases to be assigned to the node collection.
        /// </summary>
        public IEnumerable<string> DatabaseIds { get; set; }

        /// <summary>
        /// Represents the IDs of the nodes to be assigned to the node collection.
        /// </summary>
        public IEnumerable<string> NodeIds { get; set; }

        /// <summary>
        /// Represents the default value.
        /// </summary>
        public static DataUpdateNodeCollectionViewModel Default = new DataUpdateNodeCollectionViewModel
        {
            Id = "Node collection ID",
            Name = "Node collection name",
            Description = "Node collection description",
            DatabaseIds = new List<string> { "Database ID" },
            NodeIds = new List<string> { "Node ID" }
        };
    }
}
