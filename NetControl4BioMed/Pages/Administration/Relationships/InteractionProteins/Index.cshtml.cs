using Microsoft.AspNetCore.Authorization;
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

namespace NetControl4BioMed.Pages.Administration.Relationships.InteractionProteins
{
    [Authorize(Roles = "Administrator")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly LinkGenerator _linkGenerator;

        public IndexModel(ApplicationDbContext context, LinkGenerator linkGenerator)
        {
            _context = context;
            _linkGenerator = linkGenerator;
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public SearchViewModel<InteractionProtein> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "InteractionId", "Interaction ID" },
                    { "InteractionName", "Interaction name" },
                    { "ProteinId", "Protein ID" },
                    { "ProteinName", "Protein name" }
                },
                Filter = new Dictionary<string, string>
                {
                    { "IsSource", "Is source" },
                    { "IsNotSource", "Is not source" },
                    { "IsTarget", "Is target" },
                    { "IsNotTarget", "Is not target" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "InteractionId", "Interaction ID" },
                    { "InteractionName", "Interaction name" },
                    { "ProteinId", "Protein ID" },
                    { "ProteinName", "ProteinName" },
                    { "Type", "Type" }
                }
            };
        }

        public IActionResult OnGet(string searchString = null, IEnumerable<string> searchIn = null, IEnumerable<string> filter = null, string sortBy = null, string sortDirection = null, int? itemsPerPage = null, int? currentPage = 1)
        {
            // Define the search input.
            var input = new SearchInputViewModel(ViewModel.SearchOptions, null, searchString, searchIn, filter, sortBy, sortDirection, itemsPerPage, currentPage);
            // Check if any of the provided variables was null before the reassignment.
            if (input.NeedsRedirect)
            {
                // Redirect to the page where they are all explicitly defined.
                return RedirectToPage(new { searchString = input.SearchString, searchIn = input.SearchIn, filter = input.Filter, sortBy = input.SortBy, sortDirection = input.SortDirection, itemsPerPage = input.ItemsPerPage, currentPage = input.CurrentPage });
            }
            // Start with all of the items in the non-generic databases.
            var query = _context.InteractionProteins
                .Where(item => item.Protein.DatabaseProteins.Any())
                .Where(item => item.Interaction.DatabaseInteractions.Any());
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("InteractionId") && item.Interaction.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("InteractionName") && item.Interaction.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("ProteinId") && item.Protein.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("ProteinName") && item.Protein.Name.Contains(input.SearchString));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("IsSource") ? item.Type == InteractionProteinType.Source : true)
                .Where(item => input.Filter.Contains("IsNotSource") ? item.Type != InteractionProteinType.Source : true)
                .Where(item => input.Filter.Contains("IsTarget") ? item.Type == InteractionProteinType.Target : true)
                .Where(item => input.Filter.Contains("IsNotTarget") ? item.Type != InteractionProteinType.Target : true);
            // Sort it according to the parameters.
            switch ((input.SortBy, input.SortDirection))
            {
                case var sort when sort == ("InteractionId", "Ascending"):
                    query = query.OrderBy(item => item.Interaction.Id);
                    break;
                case var sort when sort == ("InteractionId", "Descending"):
                    query = query.OrderByDescending(item => item.Interaction.Id);
                    break;
                case var sort when sort == ("InteractionName", "Ascending"):
                    query = query.OrderBy(item => item.Interaction.Name);
                    break;
                case var sort when sort == ("InteractionName", "Descending"):
                    query = query.OrderByDescending(item => item.Interaction.Name);
                    break;
                case var sort when sort == ("ProteinId", "Ascending"):
                    query = query.OrderBy(item => item.Protein.Id);
                    break;
                case var sort when sort == ("ProteinId", "Descending"):
                    query = query.OrderByDescending(item => item.Protein.Id);
                    break;
                case var sort when sort == ("ProteinName", "Ascending"):
                    query = query.OrderBy(item => item.Protein.Name);
                    break;
                case var sort when sort == ("ProteinName", "Descending"):
                    query = query.OrderByDescending(item => item.Protein.Name);
                    break;
                case var sort when sort == ("Type", "Ascending"):
                    query = query.OrderBy(item => item.Type);
                    break;
                case var sort when sort == ("Type", "Descending"):
                    query = query.OrderByDescending(item => item.Type);
                    break;
                default:
                    break;
            }
            // Include the related entitites.
            query = query
                .Include(item => item.Protein)
                .Include(item => item.Interaction);
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<InteractionProtein>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
