using NetControl4BioMed.Helpers.Models;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Interfaces
{
    /// <summary>
    /// Provides an abstraction for the analysis runner.
    /// </summary>
    public interface IAnalysisRunner
    {
        /// <summary>
        /// Runs the analysis with the given ID.
        /// </summary>
        /// <param name="viewModel">The model of the analysis to run.</param>
        /// <returns></returns>
        Task Run(AnalysisRunnerModel model);
    }
}
