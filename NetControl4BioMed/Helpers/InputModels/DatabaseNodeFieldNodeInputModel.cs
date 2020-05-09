using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a database node field node.
    /// </summary>
    public class DatabaseNodeFieldNodeInputModel
    {
        /// <summary>
        /// Represents the database node field ID of the database node field node.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string DatabaseNodeFieldId { get; set; }

        /// <summary>
        /// Represents the node ID of the database node field node.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string NodeId { get; set; }

        /// <summary>
        /// Represents the value of the database node field node.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string Value { get; set; }
    }
}
