using Microsoft.AspNetCore.Routing;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.ViewModels;
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
                    await Algorithms.Algorithm1.Algorithm.RunAsync(context, analysis);
                    // End the switch.
                    break;
                case AnalysisAlgorithm.Algorithm2:
                    // Run the algorithm on the analysis.
                    await Algorithms.Algorithm2.Algorithm.RunAsync(context, analysis);
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
        public static List<string> GetLog(this Analysis analysis)
        {
            // Return the list of log entries.
            return JsonSerializer.Deserialize<List<string>>(analysis.Log);
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
            return JsonSerializer.Serialize(analysis.GetLog().Append(message));
        }

        /// <summary>
        /// Gets the Cytoscape view model corresponding to the provided analysis.
        /// </summary>
        /// <param name="analysis">The current analysis.</param>
        /// <param name="linkGenerator">Represents the link generator.</param>
        /// <returns>Returns the Cytoscape view model corresponding to the provided network.</returns>
        public static CytoscapeViewModel GetCytoscapeViewModel(this Analysis analysis, LinkGenerator linkGenerator)
        {
            // Get the default values.
            var interactionType = analysis.AnalysisDatabases.FirstOrDefault().Database.DatabaseType.Name.ToLower();
            var isGeneric = interactionType == "generic";
            // Return the view model.
            return new CytoscapeViewModel
            {
                Elements = new CytoscapeViewModel.CytoscapeElements
                {
                    Nodes = analysis.AnalysisNodes
                        .Select(item => item.Node)
                        .Select(item => new CytoscapeViewModel.CytoscapeElements.CytoscapeNode
                        {
                            Data = new CytoscapeViewModel.CytoscapeElements.CytoscapeNode.CytoscapeNodeData
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Href = isGeneric ? string.Empty : linkGenerator.GetPathByPage(page: "/Content/Data/Nodes/Details", values: new { id = item.Id }),
                                Alias = item.DatabaseNodeFieldNodes
                                    .Where(item1 => item1.DatabaseNodeField.IsSearchable)
                                    .Select(item1 => item1.Value)
                            },
                            Classes = item.AnalysisNodes
                                .Where(item1 => item1.Analysis == analysis)
                                .Select(item1 => item1.Type.ToString().ToLower())
                        }),
                    Edges = analysis.AnalysisEdges
                        .Select(item => item.Edge)
                        .Select(item => new CytoscapeViewModel.CytoscapeElements.CytoscapeEdge
                        {
                            Data = new CytoscapeViewModel.CytoscapeElements.CytoscapeEdge.CytoscapeEdgeData
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Source = item.EdgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Source)?.Node.Id,
                                Target = item.EdgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Target)?.Node.Id,
                                Interaction = interactionType
                            },
                            Classes = Enumerable.Empty<string>()
                        })
                },
                Layout = CytoscapeViewModel.DefaultLayout,
                Styles = CytoscapeViewModel.DefaultStyles.Concat(CytoscapeViewModel.DefaultAnalysisStyles)
            };
        }
    }
}
