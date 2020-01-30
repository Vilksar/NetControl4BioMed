using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
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
    /// Implements a Hangfire task for running an analysis.
    /// </summary>
    public class AnalysisRunner : IAnalysisRunner
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
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="emailSender">The e-mail sender.</param>
        public AnalysisRunner(ApplicationDbContext context, ISendGridEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        /// <summary>
        /// Runs the analysis with the given ID.
        /// </summary>
        /// <param name="id">The ID of the analysis to run.</param>
        /// <returns></returns>
        public async Task Run(AnalysisRunnerViewModel model)
        {
            // Get the analysis with the given ID.
            var analysis = _context.Analyses
                .Include(item => item.AnalysisNodes)
                    .ThenInclude(item => item.Node)
                .Include(item => item.AnalysisEdges)
                    .ThenInclude(item => item.Edge)
                        .ThenInclude(item => item.EdgeNodes)
                            .ThenInclude(item => item.Node)
                .Include(item => item.AnalysisUsers)
                    .ThenInclude(item => item.User)
                .FirstOrDefault(item => item.Id == model.Id);
            // Check if the analysis hasn't been found.
            if (analysis == null)
            {
                // End the function.
                return;
            }
            // Run the analysis.
            await analysis.Run(_context);
            // Reload the analysis.
            await _context.Entry(analysis).ReloadAsync();
            // Go over each registered user in the analysis.
            foreach (var user in analysis.AnalysisUsers.Where(item => item.User != null).Select(item => item.User))
            {
                // Send an analysis ending e-mail.
                await _emailSender.SendAnalysisEndedEmailAsync(new EmailAnalysisEndedViewModel
                {
                    Email = user.Email,
                    Id = analysis.Id,
                    Name = analysis.Name,
                    Status = analysis.Status.GetDisplayName(),
                    Url = model.Url,
                    ApplicationUrl = model.ApplicationUrl
                });
            }
        }
    }
}
