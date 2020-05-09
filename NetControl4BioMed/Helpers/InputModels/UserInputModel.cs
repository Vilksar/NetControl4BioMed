using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a user.
    /// </summary>
    public class UserInputModel
    {
        /// <summary>
        /// Represents the ID of the user.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string Id { get; set; }

        /// <summary>
        /// Represents the e-mail of the user.
        /// </summary>
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "This field is required.")]
        public string Email { get; set; }

        /// <summary>
        /// Represents the password of the user.
        /// </summary>
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "This field is required.")]
        [StringLength(100, ErrorMessage = "The password must be at least 6 characters long.", MinimumLength = 6)]
        public string Password { get; set; }

        /// <summary>
        /// Represents the password confirmation of the user.
        /// </summary>
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "This field is required.")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// Represents the e-mail confirmation status of the user.
        /// </summary>
        [Required(ErrorMessage = "This field is required.")]
        public bool EmailConfirmed { get; set; }
    }
}
