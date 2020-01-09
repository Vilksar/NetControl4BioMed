using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using NetControl4BioMed.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Services
{
    /// <summary>
    /// Implements a renderer for returning partial views as HTML strings.
    /// </summary>
    public class PartialViewRenderer : IPartialViewRenderer
    {
        /// <summary>
        /// Represents the Razor view engine.
        /// </summary>
        private readonly IRazorViewEngine _viewEngine;

        /// <summary>
        /// Represents the temporary data provider.
        /// </summary>
        private readonly ITempDataProvider _tempDataProvider;

        /// <summary>
        /// Represents the service provider.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="viewEngine">Represents the Razor engine for the views.</param>
        /// <param name="tempDataProvider">Represents the temporary data provider.</param>
        /// <param name="serviceProvider">Represents the application service provider.</param>
        public PartialViewRenderer(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Renders the partial view indicated by the specified name to an HTML string, using the specified model.
        /// </summary>
        /// <typeparam name="TModel">Represents the model type with which to render the partial view.</typeparam>
        /// <param name="partialName">Represents the name of the partial view to be rendered.</param>
        /// <param name="model">Represents the model with which to render the partial view.</param>
        public async Task<string> RenderPartialToStringAsync<TModel>(string partialName, TModel model)
        {
            // Get the action context.
            var actionContext = new ActionContext(new DefaultHttpContext { RequestServices = _serviceProvider }, new RouteData(), new ActionDescriptor());
            // Try to get the partial view with the provided name.
            var getPartialResult = _viewEngine.FindView(actionContext, partialName, false);
            // Check if there were any errors getting the partial view.
            if (!getPartialResult.Success)
            {
                // Throw an error.
                throw new InvalidOperationException($"Error: Unable to find the requested partial {partialName} in {string.Join(", ", getPartialResult.SearchedLocations)}.");
            }
            // Get the actual partial view.
            var partial = getPartialResult.View;
            // Define the variable to store the string.
            var viewContent = string.Empty;
            // Define a new string writer.
            using (var output = new StringWriter())
            {
                // Get the view context.
                var viewContext = new ViewContext
                (
                    actionContext,
                    partial,
                    new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model },
                    new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                    output,
                    new HtmlHelperOptions()
                );
                // Render the partial within the view context.
                await partial.RenderAsync(viewContext);
                // And return the string.
                viewContent = output.ToString();
            }
            // Check if there were any errors in rendering the partial view.
            if (string.IsNullOrEmpty(viewContent))
            {
                // Return an error.
                throw new InvalidOperationException($"Error: An error occured while rendering the requested partial {partialName}.");
            }
            // Return the string.
            return viewContent;
        }
    }
}
