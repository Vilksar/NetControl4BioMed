using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between a sample and a database which it uses.
    /// </summary>
    public class SampleDatabase : ISampleDependent
    {
        /// <summary>
        /// Gets or sets the sample ID of the relationship.
        /// </summary>
        public string SampleId { get; set; }

        /// <summary>
        /// Gets or sets the sample of the relationship.
        /// </summary>
        public Sample Sample { get; set; }

        /// <summary>
        /// Gets or sets the database ID of the relationship.
        /// </summary>
        public string DatabaseId { get; set; }

        /// <summary>
        /// Gets or sets the database of the relationship.
        /// </summary>
        public Database Database { get; set; }
    }
}
