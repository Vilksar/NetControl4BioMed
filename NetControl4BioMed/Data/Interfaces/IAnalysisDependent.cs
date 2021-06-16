using NetControl4BioMed.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Interfaces
{
    /// <summary>
    /// Provides an abstraction for a model depending on an analysis.
    /// </summary>
    public interface IAnalysisDependent
    {
        /// <summary>
        /// Represents the analysis ID of the model.
        /// </summary>
        string AnalysisId { get; set; }

        /// <summary>
        /// Represents the analysis of the model.
        /// </summary>
        Analysis Analysis { get; set; }
    }
}
