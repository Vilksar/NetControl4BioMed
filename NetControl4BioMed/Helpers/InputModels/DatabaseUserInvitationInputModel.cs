using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a database user invitation.
    /// </summary>
    public class DatabaseUserInvitationInputModel
    {
        /// <summary>
        /// Represents the database ID of the database user invitation.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string DatabaseId { get; set; }

        /// <summary>
        /// Represents the e-mail of the database user invitation.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string Email { get; set; }
    }
}
