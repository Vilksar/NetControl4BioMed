using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a database edge.
    /// </summary>
    public class DatabaseEdgeInputModel
    {
        /// <summary>
        /// Represents the database ID of the database edge.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string DatabaseId { get; set; }

        /// <summary>
        /// Represents the edge ID of the database edge.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string EdgeId { get; set; }
    }
}
