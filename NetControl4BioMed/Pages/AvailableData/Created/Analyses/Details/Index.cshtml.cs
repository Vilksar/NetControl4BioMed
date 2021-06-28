using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Algorithms = NetControl4BioMed.Helpers.Algorithms;

namespace NetControl4BioMed.Pages.AvailableData.Created.Analyses.Details
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public IndexModel(UserManager<User> userManager, ApplicationDbContext context, IConfiguration configuration)
        {
            _userManager = userManager;
            _context = context;
            _configuration = configuration;
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public bool IsUserOwner { get; set; }

            public Analysis Analysis { get; set; }

            public bool ShowVisualization { get; set; }

            public Algorithms.Analyses.Greedy.Parameters GreedyAlgorithmParameters { get; set; }

            public Algorithms.Analyses.Genetic.Parameters GeneticAlgorithmParameters { get; set; }

            public Dictionary<string, int?> ItemCount { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id, bool loadDemonstration)
        {
            // Check if the demonstration should be loaded.
            if (loadDemonstration)
            {
                // Check if there are no demonstration items configured.
                if (string.IsNullOrEmpty(_configuration["Data:Demonstration:AnalysisId"]))
                {
                    // Try to get a demonstration control path.
                    var controlPath = _context.ControlPaths
                        .Include(item => item.Analysis)
                            .ThenInclude(item => item.Network)
                        .Where(item => item.Analysis.IsPublic && item.Analysis.IsDemonstration && item.Analysis.Network.IsPublic && item.Analysis.Network.IsDemonstration)
                        .AsNoTracking()
                        .FirstOrDefault();
                    // Check if there was no demonstration control path found.
                    if (controlPath == null || controlPath.Analysis == null || controlPath.Analysis.Network == null)
                    {
                        // Display a message.
                        TempData["StatusMessage"] = "Error: There are no demonstration analyses available.";
                        // Redirect to the index page.
                        return RedirectToPage("/AvailableData/Created/Analyses/Index");
                    }
                    // Update the demonstration item IDs.
                    _configuration["Data:Demonstration:NetworkId"] = controlPath.Analysis.Network.Id;
                    _configuration["Data:Demonstration:AnalysisId"] = controlPath.Analysis.Id;
                    _configuration["Data:Demonstration:ControlPathId"] = controlPath.Id;
                }
                // Get the ID of the configured demonstration item.
                id = _configuration["Data:Demonstration:AnalysisId"];
            }
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/AvailableData/Created/Analyses/Index");
            }
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Get the item with the provided ID.
            var items = _context.Analyses
                .Where(item => item.IsPublic || (user != null && item.AnalysisUsers.Any(item1 => item1.Email == user.Email)))
                .Where(item => item.Id == id);
            // Check if there was no item found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No analysis has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/AvailableData/Created/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                IsUserOwner = items
                    .Any(item => user != null && item.AnalysisUsers.Any(item1 => item1.Email == user.Email)),
                Analysis = items
                    .Include(item => item.Network)
                    .First(),
                ShowVisualization = items
                    .All(item => (item.Status == AnalysisStatus.Stopped || item.Status == AnalysisStatus.Completed) && item.AnalysisProteins
                        .Where(item1 => item1.Type == AnalysisProteinType.None)
                        .Count() < AnalysisExtensions.MaximumSizeForVisualization),
                GreedyAlgorithmParameters = null,
                GeneticAlgorithmParameters = null,
                ItemCount = new Dictionary<string, int?>
                {
                    { "Proteins", items.Select(item => item.AnalysisProteins).SelectMany(item => item).Count(item => item.Type == AnalysisProteinType.None) },
                    { "Interactions", items.Select(item => item.AnalysisInteractions).SelectMany(item => item).Count() },
                    { "Databases", items.Select(item => item.AnalysisDatabases).SelectMany(item => item).Count() },
                    { "ProteinCollections", items.Select(item => item.AnalysisProteinCollections).SelectMany(item => item).Count() }
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
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Return an empty result.
                return new JsonResult(new { });
            }
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Get the item with the provided ID.
            var item = _context.Analyses
                .Where(item => item.IsPublic || (user != null && item.AnalysisUsers.Any(item1 => item1.Email == user.Email)))
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
