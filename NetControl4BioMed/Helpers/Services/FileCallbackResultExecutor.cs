using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Services
{
    /// <summary>
    /// Represents a FileResultExecutor implementation for the FileCallbackResult.
    /// </summary>
    /// <remarks>Implementation taken from https://blog.stephencleary.com/2016/11/streaming-zip-on-aspnet-core.html, with small modifications.</remarks>
    public class FileCallbackResultExecutor : FileResultExecutorBase
    {
        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="loggerFactory">Represents the logger factory.</param>
        public FileCallbackResultExecutor(ILoggerFactory loggerFactory) : base(CreateLogger<FileCallbackResultExecutor>(loggerFactory)) { }

        /// <summary>
        /// Executes the callback function and returns the result.
        /// </summary>
        /// <param name="context">Represents the action context.</param>
        /// <param name="result">Represents the file callback result.</param>
        /// <returns></returns>
        public Task ExecuteAsync(ActionContext context, FileCallbackResult result)
        {
            // Set the headers
            SetHeadersAndLog(context, result, null, false);
            // Return the callback function execution.
            return result.Callback(context.HttpContext.Response.Body, context);
        }
    }
}
