using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Interfaces
{
    /// <summary>
    /// Provides an abstraction for the Hangfire recurring job runner.
    /// </summary>
    public interface IHangfireRecurringJobRunner
    {
        /// <summary>
        /// Performs the daily cleaning of the database.
        /// </summary>
        /// <param name="viewModel">Represents the view model for the Hangfire recurring cleaner.</param>
        Task Run(HangfireRecurringCleanerViewModel viewModel);
    }
}
