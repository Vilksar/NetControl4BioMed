using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for an analysis network.
    /// </summary>
    public class AnalysisNetworkInputModel
    {
        /// <summary>
        /// Represents the analysis ID of the analysis network.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string AnalysisId { get; set; }

        /// <summary>
        /// Represents the network ID of the analysis network.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string NetworkId { get; set; }
    }
}
