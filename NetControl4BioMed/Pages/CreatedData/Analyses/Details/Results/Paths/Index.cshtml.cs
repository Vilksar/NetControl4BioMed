using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.CreatedData.Analyses.Details.Results.Paths
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly LinkGenerator _linkGenerator;

        public IndexModel(UserManager<User> userManager, ApplicationDbContext context, LinkGenerator linkGenerator)
        {
            _userManager = userManager;
            _context = context;
            _linkGenerator = linkGenerator;
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public Analysis Analysis { get; set; }

            public bool HasNetworkDatabases { get; set; }

            public SearchViewModel<Path> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "ProteinId", "Protein ID" },
                    { "ProteinName", "Protein name" },
                    { "SourceProteinId", "Source protein ID" },
                    { "SourceProteinName", "Source protein name" },
                    { "TargetProteinId", "Target protein ID" },
                    { "TargetProteinName", "Target protein name" },
                    { "InteractionId", "Interaction ID" },
                    { "InteractionName", "Interaction name" }
                },
                Filter = new Dictionary<string, string>
                {
                    { "IsInControlPath", "Is in the selected control path" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "SourceProteinId", "Source protein ID" },
                    { "SourceProteinName", "Source protein name" },
                    { "TargetProteinId", "Target protein ID" },
                    { "TargetProteinName", "Target protein name" },
                    { "Length", "Length" }
                }
            };
        }

        public async Task<IActionResult> OnGetAsync(string id, string controlPathId = null, string searchString = null, IEnumerable<string> searchIn = null, IEnumerable<string> filter = null, string sortBy = null, string sortDirection = null, int? itemsPerPage = null, int? currentPage = 1)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Analyses/Index");
            }
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Get the items with the provided ID.
            var items = _context.Analyses
                .Where(item => item.IsPublic || (user != null && item.AnalysisUsers.Any(item1 => item1.Email == user.Email)))
                .Where(item => item.Id == id);
            // Check if there were no items found.
            if (items == null || items.Count() != 1)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Analyses/Index");
            }
            // Define the search input.
            var input = new SearchInputViewModel(ViewModel.SearchOptions, id, searchString, searchIn, filter, sortBy, sortDirection, itemsPerPage, currentPage);
            // Check if any of the provided variables was null before the reassignment.
            if (input.NeedsRedirect)
            {
                // Redirect to the page where they are all explicitly defined.
                return RedirectToPage(new { id = input.Id, controlPathId = controlPathId, searchString = input.SearchString, searchIn = input.SearchIn, filter = input.Filter, sortBy = input.SortBy, sortDirection = input.SortDirection, itemsPerPage = input.ItemsPerPage, currentPage = input.CurrentPage });
            }
            // Start with all of the items.
            var query = items
                .Select(item => item.ControlPaths)
                .SelectMany(item => item)
                .Select(item => item.Paths)
                .SelectMany(item => item);
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("ProteinId") && item.PathProteins.Any(item1 => item1.Protein.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("ProteinName") && item.PathProteins.Any(item1 => item1.Protein.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("SourceProteinId") && item.PathProteins.Where(item1 => item1.Type == PathProteinType.Source).Any(item1 => item1.Protein.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("SourceProteinName") && item.PathProteins.Where(item1 => item1.Type == PathProteinType.Source).Any(item1 => item1.Protein.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("TargetProteinId") && item.PathProteins.Where(item1 => item1.Type == PathProteinType.Target).Any(item1 => item1.Protein.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("TargetProteinName") && item.PathProteins.Where(item1 => item1.Type == PathProteinType.Target).Any(item1 => item1.Protein.Name.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("InteractionId") && item.PathInteractions.Any(item1 => item1.Interaction.Id.Contains(input.SearchString)) ||
                    input.SearchIn.Contains("InteractionName") && item.PathInteractions.Any(item1 => item1.Interaction.Name.Contains(input.SearchString)));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("IsInControlPath") ? !string.IsNullOrEmpty(controlPathId) && item.ControlPath.Id == controlPathId : true);
            // Sort it according to the parameters.
            switch ((input.SortBy, input.SortDirection))
            {
                case var sort when sort == ("Id", "Ascending"):
                    query = query.OrderBy(item => item.Id);
                    break;
                case var sort when sort == ("Id", "Descending"):
                    query = query.OrderByDescending(item => item.Id);
                    break;
                case var sort when sort == ("SourceProteinId", "Ascending"):
                    query = query.OrderBy(item => item.PathProteins.Where(item1 => item1.Type == PathProteinType.Source).First().Protein.Id);
                    break;
                case var sort when sort == ("SourceProteinId", "Descending"):
                    query = query.OrderByDescending(item => item.PathProteins.Where(item1 => item1.Type == PathProteinType.Source).First().Protein.Id);
                    break;
                case var sort when sort == ("SourceProteinName", "Ascending"):
                    query = query.OrderBy(item => item.PathProteins.Where(item1 => item1.Type == PathProteinType.Source).First().Protein.Name);
                    break;
                case var sort when sort == ("SourceProteinName", "Descending"):
                    query = query.OrderByDescending(item => item.PathProteins.Where(item1 => item1.Type == PathProteinType.Source).First().Protein.Name);
                    break;
                case var sort when sort == ("TargetProteinId", "Ascending"):
                    query = query.OrderBy(item => item.PathProteins.Where(item1 => item1.Type == PathProteinType.Target).First().Protein.Id);
                    break;
                case var sort when sort == ("TargetProteinId", "Descending"):
                    query = query.OrderByDescending(item => item.PathProteins.Where(item1 => item1.Type == PathProteinType.Target).First().Protein.Id);
                    break;
                case var sort when sort == ("TargetProteinName", "Ascending"):
                    query = query.OrderBy(item => item.PathProteins.Where(item1 => item1.Type == PathProteinType.Target).First().Protein.Name);
                    break;
                case var sort when sort == ("TargetProteinName", "Descending"):
                    query = query.OrderByDescending(item => item.PathProteins.Where(item1 => item1.Type == PathProteinType.Target).First().Protein.Name);
                    break;
                case var sort when sort == ("Length", "Ascending"):
                    query = query.OrderBy(item => item.PathInteractions.Count());
                    break;
                case var sort when sort == ("Length", "Descending"):
                    query = query.OrderByDescending(item => item.PathInteractions.Count());
                    break;
                default:
                    break;
            }
            // Include the related entities.
            query = query
                .Include(item => item.PathProteins)
                    .ThenInclude(item => item.Protein);
            // Define the view.
            View = new ViewModel
            {
                Analysis = items
                    .First(),
                Search = new SearchViewModel<Path>(_linkGenerator, HttpContext, input, query)
            };
            // Update the view.
            View.HasNetworkDatabases = _context.NetworkDatabases
                .Any(item => item.Network.Id == View.Analysis.NetworkId);
            // Return the page.
            return Page();
        }
    }
}
