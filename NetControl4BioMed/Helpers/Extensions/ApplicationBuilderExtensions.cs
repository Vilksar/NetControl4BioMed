using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Data.Seed;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Extensions
{
    /// <summary>
    /// Provides extensions for the application builder.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Seeds the database, if needed, with default administrator role and users, and with the defined seed data.
        /// </summary>
        /// <param name="applicationBuilder">The application builder.</param>
        /// <param name="configuration">The application configuration options.</param>
        /// <returns></returns>
        public static async Task<IApplicationBuilder> SeedDatabaseAsync(this IApplicationBuilder applicationBuilder, IConfiguration configuration)
        {
            // Create a new scope for the application.
            using var scope = applicationBuilder.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
            // Get the required services.
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Check if the administrator role doesn't already exist.
            if (!await roleManager.RoleExistsAsync("Administrator"))
            {
                // Define a new administrator role.
                var role = new Role
                {
                    Name = "Administrator",
                    DateTimeCreated = DateTime.UtcNow
                };
                // Save it into the database.
                await roleManager.CreateAsync(role);
            }
            // Check if the administrator user doesn't already exist.
            if (await userManager.FindByEmailAsync(configuration.GetSection("Administrator:Email").Value) == null)
            {
                // Define a new task.
                var task = new UsersTask
                {
                    Items = new List<UserInputModel>
                    {
                        new UserInputModel
                        {
                            Email = configuration.GetSection("Administrator:Email").Value,
                            Type = "Password",
                            Data = JsonSerializer.Serialize(configuration.GetSection("Administrator:Password").Value),
                            EmailConfirmed = true
                        }
                    }
                };
                // Run the task.
                await task.CreateAsync(scope.ServiceProvider, CancellationToken.None);
                // Get the newly created user.
                var user = await userManager.FindByEmailAsync(configuration.GetSection("Administrator:Email").Value);
                // Add the user to the administrator role.
                await userManager.AddToRoleAsync(user, "Administrator");
            }
            // Check if no databases exist.
            if (!context.Databases.Any())
            {
                // Mark the seed data for addition.
                context.Databases.AddRange(Databases.Seed);
                // Save the changes.
                await context.SaveChangesAsync();
                // Mark the seed data for addition.
                context.DatabaseProteinFields.AddRange(DatabaseProteinFields.Seed);
                // Save the changes.
                await context.SaveChangesAsync();
                // Mark the seed data for addition.
                context.DatabaseInteractionFields.AddRange(DatabaseInteractionFields.Seed);
                // Save the changes.
                await context.SaveChangesAsync();
            }
            // Return the application builder.
            return applicationBuilder;
        }
    }
}
