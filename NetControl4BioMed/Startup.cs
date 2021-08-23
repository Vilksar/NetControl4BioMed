using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Services;
using System;

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
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Lax;
                options.Secure = CookieSecurePolicy.SameAsRequest;
            });
            // Enable cookies for temporary data.
            services.Configure<CookieTempDataProviderOptions>(options =>
            {
                options.Cookie.IsEssential = true;
            });
            // Add the database context and connection.
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration["ConnectionStrings:SqlConnection"]);
            });
            services.AddHangfire(options =>
            {
                options.UseSqlServerStorage(Configuration["ConnectionStrings:SqlConnection"]);
            });
            // Add the default Identity functions for users and roles.
            services.AddIdentity<User, Role>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            // Configure the path options.
            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/Identity/AccessDenied";
                options.LoginPath = "/Identity/Login";
                options.LogoutPath = "/Identity/Logout";
            });
            // Add the external authentication options.
            services.AddAuthentication()
                .AddGoogle(googleOptions =>
                {
                    googleOptions.ClientId = Configuration["Authentication:Google:ClientId"];
                    googleOptions.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                    googleOptions.AccessDeniedPath = "/Identity/AccessDenied";
                })
                .AddMicrosoftAccount(microsoftOptions =>
                {
                    microsoftOptions.ClientId = Configuration["Authentication:Microsoft:ClientId"];
                    microsoftOptions.ClientSecret = Configuration["Authentication:Microsoft:ClientSecret"];
                    microsoftOptions.AccessDeniedPath = "/Identity/AccessDenied";
                });
            // Add the HTTP client dependency.
            services.AddHttpClient();
            // Add the dependency injections.
            services.AddTransient<IPartialViewRenderer, PartialViewRenderer>();
            services.AddTransient<IReCaptchaChecker, ReCaptchaChecker>();
            services.AddTransient<ISendGridEmailSender, SendGridEmailSender>();
            services.AddTransient<IRecurringTaskManager, RecurringTaskManager>();
            services.AddTransient<IAdministrationTaskManager, AdministrationTaskManager>();
            services.AddTransient<IContentTaskManager, ContentTaskManager>();
            // Add Razor pages.
            services.AddRazorPages();
        }

        /// <summary>
        /// Configures the application options at startup.
        /// </summary>
        /// <remarks>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </remarks>
        /// <param name="applicationBuilder">The application builder.</param>
        /// <param name="webHostEnvironment">The hosting environment of the application.</param>
        public void Configure(IApplicationBuilder applicationBuilder, IWebHostEnvironment webHostEnvironment)
        {
            // Check the environment in which it is running.
            if (webHostEnvironment.IsDevelopment())
            {
                // Display more details about the errors.
                applicationBuilder.UseDeveloperExceptionPage();
            }
            else
            {
                // Redirect to a generic "Error" page.
                applicationBuilder.UseExceptionHandler("/Error");
                // Use re-execution for the HTTP error status codes, to the same "Error" page.
                applicationBuilder.UseStatusCodePagesWithReExecute("/Error", "?errorCode={0}");
                // The default HSTS value is 30 days. This may change for production scenarios, as in https://aka.ms/aspnetcore-hsts.
                applicationBuilder.UseHsts();
            }
            // Parameters for the application.
            applicationBuilder.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto
            });
            applicationBuilder.UseHttpsRedirection();
            applicationBuilder.UseStaticFiles();
            applicationBuilder.UseCookiePolicy();
            applicationBuilder.UseRouting();
            // Use authentication.
            applicationBuilder.UseAuthentication();
            applicationBuilder.UseAuthorization();
            // Use Razor pages.
            applicationBuilder.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
            // Use Hangfire.
            applicationBuilder.UseHangfireDashboard("/Hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() }
            });
            applicationBuilder.UseHangfireServer(new BackgroundJobServerOptions
            {
                WorkerCount = 4 < Environment.ProcessorCount ? Environment.ProcessorCount - 3 : 1,
                Queues = new[] { "recurring", "default" }
            });
            applicationBuilder.UseHangfireServer(new BackgroundJobServerOptions
            {
                WorkerCount = 4 < Environment.ProcessorCount ? 2 : 1,
                Queues = new[] { "administration", "background" }
            });
            // Seed the database.
            applicationBuilder.SeedDatabaseAsync(Configuration).Wait();
        }
    }
}
