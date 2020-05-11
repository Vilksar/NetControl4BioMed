using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a network user.
    /// </summary>
    public class NetworkUserInputModel
    {
        /// <summary>
        /// Represents the network ID of the network user.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string NetworkId { get; set; }

        /// <summary>
        /// Represents the user ID of the network user.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string UserId { get; set; }
    }
}
