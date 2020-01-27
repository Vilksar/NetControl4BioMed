using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.ViewModels;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Models;
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
        /// <param name="analysis">The current analysis.</param>
        /// <param name="context">The database context containing the analysis.</param>
        public static async Task Run(this Analysis analysis, ApplicationDbContext context)
        {
            // Mark the analysis for updating.
            context.Update(analysis);
            // Check if the analysis doesn't exist.
            if (analysis == null)
            {
                // End the function.
                return;
            }
            // Check the algorithm to run the analysis.
            switch (analysis.Algorithm)
            {
                case AnalysisAlgorithm.Algorithm1:
                    // Run the algorithm on the analysis.
                    await Algorithms.Algorithm1.Algorithm.Run(context, analysis);
                    // End the switch.
                    break;
                case AnalysisAlgorithm.Algorithm2:
                    // Run the algorithm on the analysis.
                    await Algorithms.Algorithm2.Algorithm.Run(context, analysis);
                    // End the switch.
                    break;
                default:
                    // Update the analysis log.
                    analysis.Log = analysis.AppendToLog("The algorithm is not defined.");
                    // Update the analysis status.
                    analysis.Status = AnalysisStatus.Error;
                    // Save the changes in the database.
                    await context.SaveChangesAsync();
                    // End the switch.
                    break;
            }
            // Reload the analysis.
            await context.Entry(analysis).ReloadAsync();
            // Add an ending message to the log.
            analysis.Log = analysis.AppendToLog($"The analysis has ended with the status \"{analysis.Status.GetDisplayName()}\".");
            // Save the changes in the database.
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Gets the log of the analysis.
        /// </summary>
        /// <param name="analysis">The current analysis.</param>
        /// <returns>Returns the log of the analysis.</returns>
        public static List<LogEntry> GetLog(this Analysis analysis)
        {
            // Return the list of log entries.
            return JsonSerializer.Deserialize<List<LogEntry>>(analysis.Log);
        }

        /// <summary>
        /// Appends to the list a new entry to the log of the analysis and returns the updated list.
        /// </summary>
        /// <param name="analysis">The current analysis.</param>
        /// <param name="message">The message to add as a new entry to the analysis log.</param>
        /// <returns>Returns the updated log of the analysis.</returns>
        public static string AppendToLog(this Analysis analysis, string message)
        {
            // Return the log entries.
            return JsonSerializer.Serialize(analysis.GetLog().Append(new LogEntry(message)));
        }

        /// <summary>
        /// Gets the intervals in which the analysis was running.
        /// </summary>
        /// <param name="analysis">The current analysis.</param>
        /// <returns>Returns the log of the analysis.</returns>
        public static List<DateTimeInterval> GetDateTimeIntervals(this Analysis analysis)
        {
            // Return the list of intervals.
            return JsonSerializer.Deserialize<List<DateTimeInterval>>(analysis.DateTimeIntervals);
        }

        /// <summary>
        /// Appends to the list a new interval in which the analysis was running and returns the updated list.
        /// </summary>
        /// <param name="analysis">The current analysis.</param>
        /// <param name="dateTimeStarted">The starting date and time of the interval.</param>
        /// <param name="dateTimeEnded">The ending date and time of the interval.</param>
        /// <returns>Returns the updated log of the analysis.</returns>
        public static string AppendToDateTimeIntervals(this Analysis analysis, DateTime? dateTimeStarted, DateTime? dateTimeEnded)
        {
            // Return the intervals.
            return JsonSerializer.Serialize(analysis.GetDateTimeIntervals().Append(new DateTimeInterval(dateTimeStarted, dateTimeEnded)));
        }
    }
}
