using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a database edge field.
    /// </summary>
    public class DatabaseEdgeFieldInputModel
    {
        /// <summary>
        /// Represents the ID of the database edge field.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string Id { get; set; }

        /// <summary>
        /// Represents the name of the database edge field.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string Name { get; set; }

        /// <summary>
        /// Represents the description of the database edge field.
        /// </summary>
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        /// <summary>
        /// Represents the URL of the database edge field.
        /// </summary>
        [DataType(DataType.Url)]
        public string Url { get; set; }

        /// <summary>
        /// Represents the search availability status of the database edge field.
        /// </summary>
        [DataType(DataType.Text)]
        public bool IsSearchable { get; set; }

        /// <summary>
        /// Represents the database ID of the database edge field.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string DatabaseId { get; set; }
    }
}
