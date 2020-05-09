using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a node.
    /// </summary>
    public class NodeInputModel
    {
        /// <summary>
        /// Represents the ID of the node.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string Id { get; set; }

        /// <summary>
        /// Represents the description of the node.
        /// </summary>
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        /// <summary>
        /// Represents the database node field nodes of the node.
        /// </summary>
        public IEnumerable<DatabaseNodeFieldNodeInputModel> DatabaseNodeFieldNodes { get; set; }
    }
}
