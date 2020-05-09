using NetControl4BioMed.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a node collection.
    /// </summary>
    public class NodeCollectionInputModel
    {
        /// <summary>
        /// Represents the ID of the node collection.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string Id { get; set; }

        /// <summary>
        /// Represents the name of the node collection.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string Name { get; set; }

        /// <summary>
        /// Represents the description of the node collection.
        /// </summary>
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        /// <summary>
        /// Represents the node collection databases of the node collection.
        /// </summary>
        public IEnumerable<NodeCollectionDatabaseInputModel> NodeCollectionDatabases { get; set; }

        /// <summary>
        /// Represents the node collection nodes of the node collection.
        /// </summary>
        public IEnumerable<NodeCollectionNodeInputModel> NodeCollectionNodes { get; set; }
    }
}
