using NetControl4BioMed.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnumerationSampleType = NetControl4BioMed.Data.Enumerations.SampleType;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between a sample and a type.
    /// </summary>
    public class SampleType : ISampleDependent
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
        /// Gets or sets the type of the relationship.
        /// </summary>
        public EnumerationSampleType Type { get; set; }
    }
}
