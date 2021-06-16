using NetControl4BioMed.Data.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a background task.
    /// </summary>
    public class BackgroundTask
    {
        /// <summary>
        /// Gets or sets the unique internal ID of the background task.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the date when the background task was created.
        /// </summary>
        public DateTime DateTimeCreated { get; set; }

        /// <summary>
        /// Gets or sets the name of the background task.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the recurring status of the background task.
        /// </summary>
        public bool IsRecurring { get; set; }

        /// <summary>
        /// Gets or sets the data of the background task.
        /// </summary>
        public string Data { get; set; }
    }
}
