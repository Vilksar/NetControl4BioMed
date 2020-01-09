using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NetControl4BioMed
{
    /// <summary>
    /// Represents the main class of the application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Initializes the application.
        /// </summary>
        /// <param name="args">Represents the arguments of the application.</param>
        public static void Main(string[] args)
        {
            // Create a web host builder, build the host and run it.
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates a web host builder with the given arguments.
        /// </summary>
        /// <param name="args">Represents the arguments for the web host builder.</param>
        /// <returns>Returns a new web host as defined in the "Startup" class.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            // Return a default web host with the given arguments and parameters.
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Warning);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}
