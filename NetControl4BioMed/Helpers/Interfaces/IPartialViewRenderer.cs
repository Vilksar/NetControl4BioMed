using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Interfaces
{
    /// <summary>
    /// Provides an abstraction for a partial view renderer.
    /// </summary>
    public interface IPartialViewRenderer
    {
        /// <summary>
        /// Renders the partial view indicated by the specified name to an HTML string, using the specified model.
        /// </summary>
        /// <typeparam name="TModel">Represents the model type with which to render the partial view.</typeparam>
        /// <param name="partialName">Represents the name of the partial view to be rendered.</param>
        /// <param name="model">Represents the model with which to render the partial view.</param>
        Task<string> RenderPartialToStringAsync<TModel>(string partialName, TModel model);
    }
}
