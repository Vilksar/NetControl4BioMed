using System;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents a log entry for a network or an analysis.
    /// </summary>
    public class LogEntryViewModel
    {
        /// <summary>
        /// Gets or sets the date and time of the log entry.
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Gets or sets the message of the log entry.
        /// </summary>
        public string Message { get; set; }
    }
}
