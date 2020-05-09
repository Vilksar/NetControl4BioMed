using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a database type.
    /// </summary>
    public class DatabaseTypeInputModel
    {
        /// <summary>
        /// Represents the ID of the database type.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string Id { get; set; }

        /// <summary>
        /// Represents the name of the database type.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string Name { get; set; }

        /// <summary>
        /// Represents the description of the database type.
        /// </summary>
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
    }
}
