using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.ViewModels
{
    /// <summary>
    /// Represents a date and time interval in which an analysis runs (used instead of Tuple&lt;DateTime?, DateTime?&gt;).
    /// </summary>
    public class DateTimeIntervalViewModel
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
        public DateTimeIntervalViewModel()
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
        public DateTimeIntervalViewModel(DateTime? dateTimeStarted, DateTime? dateTimeEnded)
        {
            // Assign the value for each property.
            DateTimeStarted = dateTimeStarted;
            DateTimeEnded = dateTimeEnded;
        }

        /// <summary>
        /// Returns the duration of the interval.
        /// </summary>
        /// <returns>The duration of the interval.</returns>
        public TimeSpan GetDuration()
        {
            // Get the current time.
            var dateTimeCurrent = DateTime.Now;
            // Get the start and end times of the interval.
            var dateTimeStarted = DateTimeStarted.HasValue ? DateTimeStarted.Value : dateTimeCurrent;
            var dateTimeEnded = DateTimeEnded.HasValue ? DateTimeEnded.Value : dateTimeCurrent;
            // Return the duration of the interval.
            return dateTimeEnded - dateTimeStarted;
        }
    }
}
