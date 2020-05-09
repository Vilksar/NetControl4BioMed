using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a node collection database.
    /// </summary>
    public class NodeCollectionDatabaseInputModel
    {
        /// <summary>
        /// Represents the node collection ID of the node collection database.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string NodeCollectionId { get; set; }

        /// <summary>
        /// Represents the database ID of the node collection database.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string DatabaseId { get; set; }
    }
}
