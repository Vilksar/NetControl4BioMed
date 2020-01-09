using Hangfire.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Services
{
    /// <summary>
    /// Represents an authorization filter for accessing the Hangfire dashboard.
    /// </summary>
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        /// <summary>
        /// Checks if the current user is an administrator.
        /// </summary>
        /// <param name="context">Represents the Hangfire dashboard context.</param>
        /// <returns>Returns "true" if the user is authorized to access the dashboard, "false" otherwise.</returns>
        public bool Authorize(DashboardContext context)
        {
            // Allow only administrator users to see the Dashboard.
            return context.GetHttpContext().User.IsInRole("Administrator");
        }
    }
}
