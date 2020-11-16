using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using Algorithms = NetControl4BioMed.Helpers.Algorithms;

namespace NetControl4BioMed.Pages.Content.Created.Analyses.Details
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public IndexModel(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public Analysis Analysis { get; set; }

            public string DatabaseTypeId { get; set; }

            public bool ShowVisualization { get; set; }

            public Algorithms.Analyses.Greedy.Parameters GreedyAlgorithmParameters { get; set; }

            public Algorithms.Analyses.Genetic.Parameters GeneticAlgorithmParameters { get; set; }

            public Dictionary<string, int?> ItemCount { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if the user does not exist.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: An error occured while trying to load the user data. If you are already logged in, please log out and try again.";
                // Redirect to the home page.
                return RedirectToPage("/Index");
            }
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Get the item with the provided ID.
            var items = _context.Analyses
                .Where(item => item.AnalysisUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == id);
            // Check if there was no item found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No analysis has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Analysis = items
                    .First(),
                DatabaseTypeId = items
                    .Select(item => item.AnalysisDatabases)
                    .SelectMany(item => item)
                    .Select(item => item.Database.DatabaseType.Id)
                    .Distinct()
                    .FirstOrDefault(),
                ShowVisualization = items
                    .All(item => (item.Status == AnalysisStatus.Stopped || item.Status == AnalysisStatus.Completed) && item.AnalysisNodes
                        .Where(item1 => item1.Type == AnalysisNodeType.None)
                        .Count() < AnalysisExtensions.MaximumSizeForVisualization),
                GreedyAlgorithmParameters = null,
                GeneticAlgorithmParameters = null,
                ItemCount = new Dictionary<string, int?>
                {
                    { "Nodes", items.Select(item => item.AnalysisNodes).SelectMany(item => item).Count(item => item.Type == AnalysisNodeType.None) },
                    { "Edges", items.Select(item => item.AnalysisEdges).SelectMany(item => item).Count() },
                    { "Databases", items.Select(item => item.AnalysisDatabases).SelectMany(item => item).Count() },
                    { "NodeCollections", items.Select(item => item.AnalysisNodeCollections).SelectMany(item => item).Count() },
                    { "Users", items.Select(item => item.AnalysisUsers).SelectMany(item => item).Count() + items.Select(item => item.AnalysisUserInvitations).SelectMany(item => item).Count() },
                    { "Networks", items.Select(item => item.AnalysisNetworks).SelectMany(item => item).Count() }
                }
            };
            // Check which algorithm is used and try to deserialize the parameters.
            if (View.Analysis.Algorithm == AnalysisAlgorithm.Greedy)
            {
                // Try to deserialize the parameters
                if (View.Analysis.Parameters.TryDeserializeJsonObject<Algorithms.Analyses.Greedy.Parameters>(out var parameters))
                {
                    // Assign the parameters.
                    View.GreedyAlgorithmParameters = parameters;
                }
            }
            else if (View.Analysis.Algorithm == AnalysisAlgorithm.Genetic)
            {
                // Try to deserialize the parameters
                if (View.Analysis.Parameters.TryDeserializeJsonObject<Algorithms.Analyses.Genetic.Parameters>(out var parameters))
                {
                    // Assign the parameters.
                    View.GeneticAlgorithmParameters = parameters;
                }
            }
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnGetRefreshAsync(string id)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if the user does not exist.
            if (user == null)
            {
                // Return an empty result.
                return new JsonResult(new { });
            }
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Return an empty result.
                return new JsonResult(new { });
            }
            // Get the item with the provided ID.
            var item = _context.Analyses
                .Where(item => item.AnalysisUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == id)
                .FirstOrDefault();
            // Return the analysis data.
            return new JsonResult(new
            {
                Status = item != null ? item.Status.ToString() : string.Empty,
                Progress = item != null ? ((double)item.CurrentIteration * 100 / item.MaximumIterations).ToString("0.00") : "0.00",
                ProgressWithoutImprovement = item != null ? ((double)item.CurrentIterationWithoutImprovement * 100 / item.MaximumIterationsWithoutImprovement).ToString("0.00") : "0.00",
                DateTimeElapsed = item != null && item.DateTimeStarted != null ? ((item.DateTimeEnded ?? DateTime.UtcNow) - item.DateTimeStarted).ToString() : "--:--:--.-------"
            });
        }
    }
}
