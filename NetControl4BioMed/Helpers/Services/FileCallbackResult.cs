using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Services
{
    /// <summary>
    /// Represents a FileResult implementation that can stream file content while generating it.
    /// </summary>
    /// <remarks>Implementation taken from https://blog.stephencleary.com/2016/11/streaming-zip-on-aspnet-core.html, with small modifications.</remarks>
    public class FileCallbackResult : FileResult
    {
        /// <summary>
        /// Gets or sets the callback function to be executed.
        /// </summary>
        public Func<Stream, ActionContext, Task> Callback { get; set; }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="contentType">Represents the content type for the file result.</param>
        /// <param name="callback">Represents the callback function to be executed.</param>
        public FileCallbackResult(string contentType, Func<Stream, ActionContext, Task> callback) : base(contentType)
        {
            // Assign the values.
            Callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        /// <summary>
        /// Executes the current file callback result.
        /// </summary>
        /// <param name="context">Represents the action context.</param>
        /// <returns></returns>
        public override Task ExecuteResultAsync(ActionContext context)
        {
            // Check if the context doesn't exist.
            if (context == null)
            {
                // Throw an exception.
                throw new ArgumentNullException(nameof(context));
            }
            // Allow synchronous IO for the response stream.
            var syncIOFeature = context.HttpContext.Features.Get<IHttpBodyControlFeature>();
            syncIOFeature.AllowSynchronousIO = true;
            // Define a new file result executor.
            var executor = new FileCallbackResultExecutor(context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>());
            // Return the execution of the file callback.
            return executor.ExecuteAsync(context, this);
        }
    }
}
