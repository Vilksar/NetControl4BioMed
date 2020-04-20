using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Interfaces
{
    /// <summary>
    /// Provides an abstraction for the data deleter.
    /// </summary>
    public interface IDataDeleter
    {
        /// <summary>
        /// Deletes the data specified by the model.
        /// </summary>
        /// <param name="viewModel">The model of the data to remove.</param>
        /// <returns></returns>
        Task Run(DataDeleterViewModel viewModel);
    }
}
