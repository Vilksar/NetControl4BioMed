using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a database.
    /// </summary>
    public class DatabaseInputModel
    {
        /// <summary>
        /// Represents the ID of the database.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string Id { get; set; }

        /// <summary>
        /// Represents the name of the database.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string Name { get; set; }

        /// <summary>
        /// Represents the description of the database.
        /// </summary>
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        /// <summary>
        /// Represents the URL of the database.
        /// </summary>
        [DataType(DataType.Url)]
        public string Url { get; set; }

        /// <summary>
        /// Represents the public availability status of the database.
        /// </summary>
        [DataType(DataType.Text)]
        public bool IsPublic { get; set; }

        /// <summary>
        /// Represents the database type ID of the database.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string DatabaseTypeId { get; set; }
    }
}
