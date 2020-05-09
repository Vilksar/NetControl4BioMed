using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for an analysis node collection.
    /// </summary>
    public class AnalysisNodeCollectionInputModel
    {
        /// <summary>
        /// Represents the analysis ID of the analysis node collection.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string AnalysisId { get; set; }

        /// <summary>
        /// Represents the node collection ID of the analysis node collection.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string NodeCollectionId { get; set; }

        /// <summary>
        /// Represents the type of the analysis node collection.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string Type { get; set; }
    }
}
