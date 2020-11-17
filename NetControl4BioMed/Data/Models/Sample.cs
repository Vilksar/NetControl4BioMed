using NetControl4BioMed.Data.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a sample.
    /// </summary>
    public class Sample
    {
        /// <summary>
        /// Gets or sets the unique internal ID of the sample.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the date when the sample was created.
        /// </summary>
        public DateTime DateTimeCreated { get; set; }

        /// <summary>
        /// Gets or sets the name of the sample.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the sample.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the type of the sample.
        /// </summary>
        public SampleType Type { get; set; }

        /// <summary>
        /// Gets or sets the data of the sample.
        /// </summary>
        public string Data { get; set; }
    }
}
