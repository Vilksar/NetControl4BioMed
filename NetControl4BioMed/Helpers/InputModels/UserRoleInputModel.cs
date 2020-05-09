using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a user role.
    /// </summary>
    public class UserRoleInputModel
    {
        /// <summary>
        /// Represents the user ID of the user role.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string UserId { get; set; }

        /// <summary>
        /// Represents the role ID of the user role.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string RoleId { get; set; }
    }
}
