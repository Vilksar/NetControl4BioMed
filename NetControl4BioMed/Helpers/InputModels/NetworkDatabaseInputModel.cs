using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a network database.
    /// </summary>
    public class NetworkDatabaseInputModel
    {
        /// <summary>
        /// Represents the network ID of the network database.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string NetworkId { get; set; }

        /// <summary>
        /// Represents the database ID of the network database.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string DatabaseId { get; set; }
    }
}
