using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.ViewModels
{
    /// <summary>
    /// Represents a entry in the analysis log (used instead of Tuple&lt;DateTime, string&gt;).
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Represents the start time of the interval.
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Represents the end time of the interval.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Initializes a new default instance of the class.
        /// </summary>
        public LogEntry()
        {
            // Assign the default value for each property.
            DateTime = DateTime.Now;
            Message = null;
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="message">The message of the log entry.</param>
        public LogEntry(string message)
        {
            // Assign the value for each property.
            DateTime = DateTime.Now;
            Message = message;
        }
    }
}
