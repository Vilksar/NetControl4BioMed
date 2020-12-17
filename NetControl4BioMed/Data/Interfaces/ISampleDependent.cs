using NetControl4BioMed.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Interfaces
{
    /// <summary>
    /// Provides an abstraction for a model depending on a sample.
    /// </summary>
    public interface ISampleDependent
    {
        /// <summary>
        /// Represents the sample ID of the model.
        /// </summary>
        string SampleId { get; set; }

        /// <summary>
        /// Represents the sample of the model.
        /// </summary>
        Sample Sample { get; set; }
    }
}
