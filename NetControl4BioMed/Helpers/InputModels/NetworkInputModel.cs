using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string Id { get; set; }

        /// <summary>
        /// Represents the database type ID of the network.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string DatabaseTypeId { get; set; }

        /// <summary>
        /// Represents the name of the network.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string Name { get; set; }

        /// <summary>
        /// Represents the description of the network.
        /// </summary>
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        /// <summary>
        /// Represents the algorithm of the network.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string Algorithm { get; set; }

        /// <summary>
        /// Represents the seed data for generating the network.
        /// </summary>
        [DataType(DataType.MultilineText)]
        [Required(ErrorMessage = "This field is required.")]
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
