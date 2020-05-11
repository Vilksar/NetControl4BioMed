using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a network user invitation.
    /// </summary>
    public class NetworkUserInvitationInputModel
    {
        /// <summary>
        /// Represents the network ID of the network user invitation.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string NetworkId { get; set; }

        /// <summary>
        /// Represents the e-mail of the network user invitation.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string Email { get; set; }
    }
}
