using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for an analysis user invitation.
    /// </summary>
    public class AnalysisUserInvitationInputModel
    {
        /// <summary>
        /// Represents the analysis ID of the analysis user invitation.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string AnalysisId { get; set; }

        /// <summary>
        /// Represents the e-mail of the analysis user invitation.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string Email { get; set; }
    }
}
