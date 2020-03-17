using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Services
{
    /// <summary>
    /// Implements a Hangfire recurring cleaner.
    /// </summary>
    public class HangfireRecurringCleaner : IHangfireRecurringCleaner
    {
        /// <summary>
        /// Represents the application database context.
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Represents the SendGrid e-mail sender.
        /// </summary>
        private readonly ISendGridEmailSender _emailSender;

        /// <summary>
        /// Represents the link generator.
        /// </summary>
        private readonly LinkGenerator _linkGenerator;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="emailSender">The e-mail sender.</param>
        /// <param name="logger">The application logger.</param>
        public HangfireRecurringCleaner(ApplicationDbContext context, ISendGridEmailSender emailSender, LinkGenerator linkGenerator)
        {
            _context = context;
            _emailSender = emailSender;
            _linkGenerator = linkGenerator;
        }

        /// <summary>
        /// Performs several actions aimed at cleaning the database.
        /// </summary>
        /// <returns></returns>
        public async Task Run(HangfireRecurringCleanerViewModel viewModel)
        {
            // Stop the ongoing long running analyses.
            await StopAnalyses(numberOfDays: 7);
            // Delete the ongoing long running analyses.
            await ForceStopAnalyses(numberOfDays: 7, numberOfDaysLeft: 1);
            // Alert about the items close to deletion.
            await AlertDelete(httpContext: viewModel.HttpContext, numberOfDays: 31, numberOfDaysLeft: 7);
            // Delete the items.
            await DeleteNetworks(numberOfDays: 31);
            await DeleteAnalyses(numberOfDays: 31);
        }

        /// <summary>
        /// Stops all analyses that have been ongoing for more than 7 days.
        /// </summary>
        /// <param name="numberOfDays">The number of days for which an analysis is allowed to run.</param>
        /// <returns></returns>
        private async Task StopAnalyses(int numberOfDays = 7)
        {
            // Get the analyses.
            var analyses = _context.Analyses
                .Where(item => item.Status == AnalysisStatus.Initializing || item.Status == AnalysisStatus.Ongoing)
                .Where(item => item.DateTimeStarted < DateTime.Today - TimeSpan.FromDays(numberOfDays));
            // Mark all of the items for updating.
            _context.Analyses.UpdateRange(analyses);
            // Go over each of the analyses.
            foreach (var analysis in analyses)
            {
                // Update the log.
                analysis.Log = analysis.AppendToLog($"The analysis has been running for {numberOfDays}, so it will now be automatically scheduled to stop.");
                // Update the status.
                analysis.Status = AnalysisStatus.Stopping;
            }
            // Save the changes to the database.
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Forces to stop all analyses that have been started more than 8 days prior to the current date, but haven't ended yet.
        /// </summary>
        /// <param name="numberOfDays">The number of days for which an analysis is allowed to run.</param>
        /// <param name="numberOfDaysLeft">The number of days for which an analysis is allowed to stop.</param>
        /// <returns></returns>
        private async Task ForceStopAnalyses(int numberOfDays = 7, int numberOfDaysLeft = 1)
        {
            // Get the analyses.
            var analyses = _context.Analyses
                .Where(item => item.Status == AnalysisStatus.Initializing || item.Status == AnalysisStatus.Ongoing || item.Status == AnalysisStatus.Stopping)
                .Where(item => item.DateTimeStarted < DateTime.Today - TimeSpan.FromDays(numberOfDays + numberOfDaysLeft));
            // Mark all of the items for updating.
            _context.Analyses.UpdateRange(analyses);
            // Go over each of the analyses.
            foreach (var analysis in analyses)
            {
                // Update the log.
                analysis.Log = analysis.AppendToLog($"The analysis could not be gracefully stopped within the alotted time of {numberOfDaysLeft} days, so it will now be forcefully stopped.");
                // Update the status.
                analysis.Status = AnalysisStatus.Error;
                // Update the analysis end time.
                analysis.DateTimeEnded = DateTime.Now;
                // Stop and delete the Hangfire background job.
                BackgroundJob.Delete(analysis.JobId);
            }
            // Save the changes to the database.
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Sends e-mails to users with access to any analysis that ended 24 days prior to the current date and will be deleted after 7 days.
        /// </summary>
        /// <param name="numberOfDays">The number of days for which an analysis is stored in the database.</param>
        /// <param name="numberOfDaysLeft">The number of days until the deletion will take place.</param>
        /// <returns></returns>
        private async Task AlertDelete(HttpContext httpContext, int numberOfDays = 31, int numberOfDaysLeft = 7)
        {
            // Get the grouping of users, and the networks that they have access to.
            var groupingUserNetworks = _context.NetworkUsers
                .Where(item => item.Network.DateTimeCreated == DateTime.Today - TimeSpan.FromDays(numberOfDays - numberOfDaysLeft))
                .GroupBy(item => item.User)
                .Where(item => item.Any());
            // Get the grouping of nodes, and the analyses that they have access to.
            var groupingUserAnalyses = _context.AnalysisUsers
                .Where(item => item.Analysis.Status == AnalysisStatus.Stopped || item.Analysis.Status == AnalysisStatus.Completed || item.Analysis.Status == AnalysisStatus.Error)
                .Where(item => item.Analysis.DateTimeEnded == DateTime.Today - TimeSpan.FromDays(numberOfDays - numberOfDaysLeft))
                .GroupBy(item => item.User)
                .Where(item => item.Any());
            // Get the users that have access to the items.
            var users = groupingUserNetworks
                .Select(item => item.Key)
                .Concat(groupingUserAnalyses.Select(item => item.Key))
                .Distinct();
            // Go over each of the users.
            foreach (var user in users)
            {
                // Send an alert delete analyses e-mail.
                await _emailSender.SendAlertDeleteEmailAsync(new EmailAlertDeleteViewModel
                {
                    Email = user.Email,
                    DateTime = DateTime.Today + TimeSpan.FromDays(numberOfDaysLeft),
                    NetworkItems = groupingUserNetworks
                        .Where(item => item.Key == user)
                        .SelectMany(item => item)
                        .AsEnumerable()
                        .Select(item => new EmailAlertDeleteViewModel.ItemModel
                        {
                            Id = item.Network.Id,
                            Name = item.Network.Name,
                            Url = _linkGenerator.GetUriByPage(httpContext, "/Content/Created/Networks/Details/Index", handler: null, values: new { id = item.Network.Id })
                        }),
                    AnalysisItems = groupingUserAnalyses
                        .Where(item => item.Key == user)
                        .SelectMany(item => item)
                        .AsEnumerable()
                        .Select(item => new EmailAlertDeleteViewModel.ItemModel
                        {
                            Id = item.Analysis.Id,
                            Name = item.Analysis.Name,
                            Url = _linkGenerator.GetUriByPage(httpContext, "/Content/Created/Analyses/Details/Index", handler: null, values: new { id = item.Analysis.Id })
                        }),
                    ApplicationUrl = _linkGenerator.GetUriByPage(httpContext, "/Index", handler: null, values: null)
                });
            }
        }

        /// <summary>
        /// Deletes all networks that ended more than 31 days prior to the current date.
        /// </summary>
        /// <param name="numberOfDays">The number of days for which an analysis is stored in the database.</param>
        /// <returns></returns>
        private async Task DeleteNetworks(int numberOfDays = 31)
        {
            // Get the networks.
            var networks = _context.Networks
                .Where(item => item.DateTimeCreated < DateTime.Now - TimeSpan.FromDays(numberOfDays));
            // Get the corresponding analyses.
            var analyses = networks
                .Select(item => item.AnalysisNetworks)
                .SelectMany(item => item)
                .Select(item => item.Analysis)
                .Distinct();
            // Get the generic corresponding networks, nodes and edges.
            var genericNetworks = networks
                .Where(item => item.NetworkDatabases.Any(item => item.Database.DatabaseType.Name == "Generic"));
            var genericNodes = genericNetworks
                .Select(item => item.NetworkNodes)
                .SelectMany(item => item)
                .Select(item => item.Node)
                .Distinct();
            var genericEdges = genericNetworks
                .Select(item => item.NetworkEdges)
                .SelectMany(item => item)
                .Select(items => items.Edge)
                .Distinct();
            // Mark all of the items for deletion.
            _context.Analyses.RemoveRange(analyses);
            _context.Networks.RemoveRange(networks);
            _context.Edges.RemoveRange(genericEdges);
            _context.Nodes.RemoveRange(genericNodes);
            // Save the changes to the database.
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes all analyses that ended more than 31 days prior to the current date.
        /// </summary>
        /// <param name="numberOfDays">The number of days for which an analysis is stored in the database.</param>
        /// <returns></returns>
        private async Task DeleteAnalyses(int numberOfDays = 31)
        {
            // Get the analyses.
            var analyses = _context.Analyses
                .Where(item => item.Status == AnalysisStatus.Stopped || item.Status == AnalysisStatus.Completed || item.Status == AnalysisStatus.Error)
                .Where(item => item.DateTimeEnded < DateTime.Today - TimeSpan.FromDays(numberOfDays));
            // Mark all of the items for deletion.
            _context.Analyses.RemoveRange(analyses);
            // Save the changes to the database.
            await _context.SaveChangesAsync();
        }
    }
}
