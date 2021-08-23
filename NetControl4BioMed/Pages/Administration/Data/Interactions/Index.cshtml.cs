using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetControl4BioMed.Pages.Administration.Data.Interactions
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
            public SearchViewModel<Interaction> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "Name", "Name" },
                    { "Description", "Description" }
                },
                Filter = new Dictionary<string, string>
                {
                    { "HasDatabaseInteractions", "Has database interactions" },
                    { "HasNoDatabaseInteractions", "Does not have database interactions" },
                    { "HasDatabaseInteractionFieldInteractions", "Has database interaction field interactions" },
                    { "HasNoDatabaseInteractionFieldInteractions", "Does not have database interaction field interactions" },
                    { "HasInteractionProteins", "Has interaction proteins" },
                    { "HasNoInteractionProteins", "Does not have interaction proteins" },
                    { "HasNetworkInteractions", "Has network interactions" },
                    { "HasNoNetworkInteractions", "Does not have network interactions" },
                    { "HasAnalysisInteractions", "Has analysis interactions" },
                    { "HasNoAnalysisInteractions", "Does not have analysis interactions" },
                    { "HasPathInteractions", "Has path interactions" },
                    { "HasNoPathInteractions", "Does not have path interactions" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "DateTimeCreated", "Date created" },
                    { "Name", "Name" },
                    { "DatabaseInteractionCount", "Number of database interactions" },
                    { "DatabaseInteractionFieldInteractionCount", "Number of database interaction field interactions" },
                    { "InteractionProteinCount", "Number of interaction proteins" },
                    { "NetworkInteractionCount", "Number of network interactions" },
                    { "AnalysisInteractionCount", "Number of analysis interactions" },
                    { "PathInteractionCount", "Number of path interactions" }
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
            var query = _context.Interactions
                .Where(item => item.DatabaseInteractions.Any());
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Name") && item.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Description") && item.Description.Contains(input.SearchString));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("HasDatabaseInteractions") ? item.DatabaseInteractions.Any() : true)
                .Where(item => input.Filter.Contains("HasNoDatabaseInteractions") ? !item.DatabaseInteractions.Any() : true)
                .Where(item => input.Filter.Contains("HasDatabaseInteractionFieldInteractions") ? item.DatabaseInteractionFieldInteractions.Any() : true)
                .Where(item => input.Filter.Contains("HasNoDatabaseInteractionFieldInteractions") ? !item.DatabaseInteractionFieldInteractions.Any() : true)
                .Where(item => input.Filter.Contains("HasInteractionProteins") ? item.InteractionProteins.Any() : true)
                .Where(item => input.Filter.Contains("HasNoInteractionProteins") ? !item.InteractionProteins.Any() : true)
                .Where(item => input.Filter.Contains("HasNetworkInteractions") ? item.NetworkInteractions.Any() : true)
                .Where(item => input.Filter.Contains("HasNoNetworkInteractions") ? !item.NetworkInteractions.Any() : true)
                .Where(item => input.Filter.Contains("HasAnalysisInteractions") ? item.AnalysisInteractions.Any() : true)
                .Where(item => input.Filter.Contains("HasNoAnalysisInteractions") ? !item.AnalysisInteractions.Any() : true)
                .Where(item => input.Filter.Contains("HasPathInteractions") ? item.PathInteractions.Any() : true)
                .Where(item => input.Filter.Contains("HasNoPathInteractions") ? !item.PathInteractions.Any() : true);
            // Sort it according to the parameters.
            switch ((input.SortBy, input.SortDirection))
            {
                case var sort when sort == ("Id", "Ascending"):
                    query = query.OrderBy(item => item.Id);
                    break;
                case var sort when sort == ("Id", "Descending"):
                    query = query.OrderByDescending(item => item.Id);
                    break;
                case var sort when sort == ("DateTimeCreated", "Ascending"):
                    query = query.OrderBy(item => item.DateTimeCreated);
                    break;
                case var sort when sort == ("DateTimeCreated", "Descending"):
                    query = query.OrderByDescending(item => item.DateTimeCreated);
                    break;
                case var sort when sort == ("Name", "Ascending"):
                    query = query.OrderBy(item => item.Name);
                    break;
                case var sort when sort == ("Name", "Descending"):
                    query = query.OrderByDescending(item => item.Name);
                    break;
                case var sort when sort == ("DatabaseInteractionCount", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseInteractions.Count());
                    break;
                case var sort when sort == ("DatabaseInteractionCount", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseInteractions.Count());
                    break;
                case var sort when sort == ("DatabaseInteractionFieldInteractionCount", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseInteractionFieldInteractions.Count());
                    break;
                case var sort when sort == ("DatabaseInteractionFieldInteractionCount", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseInteractionFieldInteractions.Count());
                    break;
                case var sort when sort == ("InteractionProteinCount", "Ascending"):
                    query = query.OrderBy(item => item.InteractionProteins.Count());
                    break;
                case var sort when sort == ("InteractionProteinCount", "Descending"):
                    query = query.OrderByDescending(item => item.InteractionProteins.Count());
                    break;
                case var sort when sort == ("NetworkInteractionCount", "Ascending"):
                    query = query.OrderBy(item => item.NetworkInteractions.Count());
                    break;
                case var sort when sort == ("NetworkInteractionCount", "Descending"):
                    query = query.OrderByDescending(item => item.NetworkInteractions.Count());
                    break;
                case var sort when sort == ("AnalysisInteractionCount", "Ascending"):
                    query = query.OrderBy(item => item.AnalysisInteractions.Count());
                    break;
                case var sort when sort == ("AnalysisInteractionCount", "Descending"):
                    query = query.OrderByDescending(item => item.AnalysisInteractions.Count());
                    break;
                case var sort when sort == ("PathInteractionCount", "Ascending"):
                    query = query.OrderBy(item => item.PathInteractions.Count());
                    break;
                case var sort when sort == ("PathInteractionCount", "Descending"):
                    query = query.OrderByDescending(item => item.PathInteractions.Count());
                    break;
                default:
                    break;
            }
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<Interaction>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
