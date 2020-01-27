using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.ViewModels
{
    /// <summary>
    /// Represents a date and time interval in which an analysis runs (used instead of Tuple&lt;DateTime?, DateTime?&gt;).
    /// </summary>
    public class DateTimeInterval
    {
        /// <summary>
        /// Represents the start time of the interval.
        /// </summary>
        public DateTime? DateTimeStarted { get; set; }

        /// <summary>
        /// Represents the end time of the interval.
        /// </summary>
        public DateTime? DateTimeEnded { get; set; }

        /// <summary>
        /// Initializes a new default instance of the class.
        /// </summary>
        public DateTimeInterval()
        {
            // Assign the default value for each property.
            DateTimeStarted = null;
            DateTimeEnded = null;
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="dateTimeStarted">The start time of the interval.</param>
        /// <param name="dateTimeEnded">The end time of the interval.</param>
        public DateTimeInterval(DateTime? dateTimeStarted, DateTime? dateTimeEnded)
        {
            // Assign the value for each property.
            DateTimeStarted = dateTimeStarted;
            DateTimeEnded = dateTimeEnded;
        }
    }
}
