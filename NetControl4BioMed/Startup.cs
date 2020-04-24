using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Services;
using NetControl4BioMed.Helpers.ViewModels;

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
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
                options.Secure = CookieSecurePolicy.SameAsRequest;
            });
            // Add the database context and connection.
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration["ConnectionStrings:DefaultConnection"]);
            });
            services.AddHangfire(options =>
            {
                options.UseSqlServerStorage(Configuration["ConnectionStrings:DefaultConnection"]);
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
                })
                .AddMicrosoftAccount(microsoftOptions =>
                {
                    microsoftOptions.ClientId = Configuration["Authentication:Microsoft:ClientId"];
                    microsoftOptions.ClientSecret = Configuration["Authentication:Microsoft:ClientSecret"];
                })
                .AddFacebook(facebookOptions =>
                {
                    facebookOptions.AppId = Configuration["Authentication:Facebook:AppId"];
                    facebookOptions.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
                });
            // Add the HTTP client dependency.
            services.AddHttpClient();
            // Add the dependency injections.
            services.AddTransient<IPartialViewRenderer, PartialViewRenderer>();
            services.AddTransient<IReCaptchaChecker, ReCaptchaChecker>();
            services.AddTransient<ISendGridEmailSender, SendGridEmailSender>();
            services.AddTransient<IHangfireRecurringJobRunner, HangfireRecurringJobRunner>();
            services.AddTransient<IDatabaseDataManager, DatabaseDataManager>();
            services.AddTransient<IAnalysisRunner, AnalysisRunner>();
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
            app.UseAuthentication();
            app.UseAuthorization();
            // Use Razor pages.
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
            // Use Hangfire.
            app.UseHangfireDashboard("/Hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() }
            });
            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                WorkerCount = Environment.ProcessorCount > 1 ? (Environment.ProcessorCount - 1) * 2 : 1
            });
            // Seed the database.
            app.SeedDatabaseAsync(Configuration).Wait();
        }
    }
}
