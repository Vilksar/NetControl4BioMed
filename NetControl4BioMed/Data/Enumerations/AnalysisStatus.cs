using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Enumerations
{
    /// <summary>
    /// Represents the possible statuses of an analysis.
    /// </summary>
    public enum AnalysisStatus
    {
        /// <summary>
        /// Represents an analysis which has encountered an error.
        /// </summary>
        [Display(Name = "Error", Description = "The analysis has encountered an error.")]
        Error,

        /// <summary>
        /// Represents an analysis which has been defined.
        /// </summary>
        [Display(Name = "Defined", Description = "The analysis has been defined.")]
        Defined,

        /// <summary>
        /// Represents an analysis which has been scheduled to start.
        /// </summary>
        [Display(Name = "Scheduled", Description = "The analysis has been scheduled to start.")]
        Scheduled,

        /// <summary>
        /// Represents an analysis which has been started and is currently initializing.
        /// </summary>
        [Display(Name = "Initializing", Description = "The analysis is currently initializing.")]
        Initializing,

        /// <summary>
        /// Represents an analysis which has been started and is still ongoing.
        /// </summary>
        [Display(Name = "Ongoing", Description = "The analysis is currently running.")]
        Ongoing,

        /// <summary>
        /// Represents an analysis which has been scheduled to stop as soon as possible.
        /// </summary>
        [Display(Name = "Stopping", Description = "The analysis has been scheduled to stop as soon as possible.")]
        Stopping,

        /// <summary>
        /// Represents an analysis which has been stopped.
        /// </summary>
        [Display(Name = "Stopped", Description = "The analysis has been stopped.")]
        Stopped,

        /// <summary>
        /// Represents an analysis which has completed successfully.
        /// </summary>
        [Display(Name = "Completed", Description = "The analysis has completed successfully.")]
        Completed
    }
}
