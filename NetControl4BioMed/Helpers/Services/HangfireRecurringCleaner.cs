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
    /// Implements a Hangfire recurring job runner.
    /// </summary>
    public class HangfireRecurringJobRunner : IHangfireRecurringJobRunner
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
        public HangfireRecurringJobRunner(ApplicationDbContext context, ISendGridEmailSender emailSender, LinkGenerator linkGenerator)
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
            await AlertDelete(scheme: viewModel.Scheme, host: new HostString(viewModel.HostValue), numberOfDays: 31, numberOfDaysLeft: 7);
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
            // Get the limit date.
            var limitDate = DateTime.Today - TimeSpan.FromDays(numberOfDays);
            // Get the analyses.
            var analyses = _context.Analyses
                .Where(item => item.Status == AnalysisStatus.Initializing || item.Status == AnalysisStatus.Ongoing)
                .Where(item => item.DateTimeStarted < limitDate);
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
            // Get the limit date.
            var limitDate = DateTime.Today - TimeSpan.FromDays(numberOfDays + numberOfDaysLeft);
            // Get the analyses.
            var analyses = _context.Analyses
                .Where(item => item.Status == AnalysisStatus.Initializing || item.Status == AnalysisStatus.Ongoing || item.Status == AnalysisStatus.Stopping)
                .Where(item => item.DateTimeStarted < limitDate);
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
        private async Task AlertDelete(string scheme, HostString host, int numberOfDays = 31, int numberOfDaysLeft = 7)
        {
            // Get the limit date.
            var limitDate = DateTime.Today - TimeSpan.FromDays(numberOfDays - numberOfDaysLeft);
            // Get the networks and analyses.
            var networks = _context.Networks
                .Where(item => item.DateTimeCreated < limitDate);
            var analyses = _context.Analyses
                .Where(item => item.Status == AnalysisStatus.Stopped || item.Status == AnalysisStatus.Completed || item.Status == AnalysisStatus.Error)
                .Where(item => item.DateTimeEnded < limitDate)
                .Concat(networks
                    .Select(item => item.AnalysisNetworks)
                    .SelectMany(item => item)
                    .Select(item => item.Analysis))
                .Distinct();
            // Get the users.
            var networkUsers = networks
                .Select(item => item.NetworkUsers)
                .SelectMany(item => item)
                .Select(item => item.User);
            var analysisUsers = analyses
                .Select(item => item.AnalysisUsers)
                .SelectMany(item => item)
                .Select(item => item.User);
            // Get the users that have access to the items.
            var users = networkUsers
                .Concat(analysisUsers);
            // Go over each of the users.
            foreach (var user in users)
            {
                // Send an alert delete analyses e-mail.
                await _emailSender.SendAlertDeleteEmailAsync(new EmailAlertDeleteViewModel
                {
                    Email = user.Email,
                    DateTime = DateTime.Today + TimeSpan.FromDays(numberOfDaysLeft),
                    NetworkItems = user.NetworkUsers
                        .Select(item => item.Network)
                        .Where(item => networks.Contains(item))
                        .Select(item => new EmailAlertDeleteViewModel.ItemModel
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Url = _linkGenerator.GetUriByPage("/Content/Created/Networks/Details/Index", handler: null, values: new { id = item.Id }, scheme: scheme, host: host)
                        }),
                    AnalysisItems = user.AnalysisUsers
                        .Select(item => item.Analysis)
                        .Where(item => analyses.Contains(item))
                        .Select(item => new EmailAlertDeleteViewModel.ItemModel
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Url = _linkGenerator.GetUriByPage("/Content/Created/Analyses/Details/Index", handler: null, values: new { id = item.Id }, scheme: scheme, host: host)
                        }),
                    ApplicationUrl = _linkGenerator.GetUriByPage("/Index", handler: null, values: null, scheme: scheme, host: host)
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
            // Get the limit date.
            var limitDate = DateTime.Today - TimeSpan.FromDays(numberOfDays);
            // Get the networks.
            var networks = _context.Networks
                .Where(item => item.DateTimeCreated < limitDate);
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
            // Get the limit date.
            var limitDate = DateTime.Today - TimeSpan.FromDays(numberOfDays);
            // Get the analyses.
            var analyses = _context.Analyses
                .Where(item => item.Status == AnalysisStatus.Stopped || item.Status == AnalysisStatus.Completed || item.Status == AnalysisStatus.Error)
                .Where(item => item.DateTimeEnded < limitDate);
            // Mark all of the items for deletion.
            _context.Analyses.RemoveRange(analyses);
            // Save the changes to the database.
            await _context.SaveChangesAsync();
        }
    }
}
