using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Extensions
{
    /// <summary>
    /// Provides extensions for the analyses.
    /// </summary>
    public static class AnalysisExtensions
    {
        /// <summary>
        /// Runs the current analysis within the given database context.
        /// </summary>
        /// <param name="analysis">The analysis to run.</param>
        /// <param name="context">The database context containing the analysis.</param>
        public static async Task Run(this Analysis analysis, ApplicationDbContext context)
        {
            // Mark the analysis for updating.
            context.Update(analysis);
            // The code for running the analysis will come here.
            // Save the changes in the database.
            await context.SaveChangesAsync();
        }
    }
}
