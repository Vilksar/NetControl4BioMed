using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for an analysis user.
    /// </summary>
    public class AnalysisUserInputModel
    {
        /// <summary>
        /// Represents the analysis ID of the analysis user.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string AnalysisId { get; set; }

        /// <summary>
        /// Represents the user ID of the analysis user.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string UserId { get; set; }
    }
}
