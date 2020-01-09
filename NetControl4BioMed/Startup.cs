using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NetControl4BioMed
{
    /// <summary>
    /// Represents the actions to perform by the application at startup.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Gets the configuration options for the application.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Initializes a new instance of the application startup.
        /// </summary>
        /// <param name="configuration">Represents the application configuration options.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Configures the services at the application startup.
        /// </summary>
        /// <remarks>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </remarks>
        /// <param name="services">Represents the service collection to be configured.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure the cookie options.
            services.Configure<CookiePolicyOptions>(options =>
            {
                // Determine whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            // Add Razor pages.
            services.AddRazorPages();
        }

        /// <summary>
        /// Configures the application options at startup.
        /// </summary>
        /// <remarks>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </remarks>
        /// <param name="app">Represents the application builder.</param>
        /// <param name="env">Represents the hosting environment of the application.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Check the environment in which it is running.
            if (env.IsDevelopment())
            {
                // Display more details about the errors.
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Redirect to a generic "Error" page.
                app.UseExceptionHandler("/Error");
                // Use re-execution for the HTTP error status codes, to the same "Error" page.
                app.UseStatusCodePagesWithReExecute("/Error", "?errorCode={0}");
                // The default HSTS value is 30 days. This may change for production scenarios, as in https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            // Parameters for the application.
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseRouting();
            // Use authentication.
            app.UseAuthorization();
            // Use Razor pages.
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
