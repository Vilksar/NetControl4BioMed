using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a database edge field edge.
    /// </summary>
    public class DatabaseEdgeFieldEdgeInputModel
    {
        /// <summary>
        /// Represents the database edge field ID of the database edge field edge.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string DatabaseEdgeFieldId { get; set; }

        /// <summary>
        /// Represents the edge ID of the database edge field edge.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string EdgeId { get; set; }

        /// <summary>
        /// Represents the value of the database edge field edge.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string Value { get; set; }
    }
}
