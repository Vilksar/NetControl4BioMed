using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a database user.
    /// </summary>
    public class DatabaseUserInputModel
    {
        /// <summary>
        /// Represents the database ID of the database user.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string DatabaseId { get; set; }

        /// <summary>
        /// Represents the user ID of the database user.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string UserId { get; set; }
    }
}
