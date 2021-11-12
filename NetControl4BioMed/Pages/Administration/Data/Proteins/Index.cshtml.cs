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

namespace NetControl4BioMed.Pages.Administration.Data.Proteins
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
            public SearchViewModel<Protein> Search { get; set; }

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
                    { "HasDatabaseProteins", "Has database proteins" },
                    { "HasNoDatabaseProteins", "Does not have database proteins" },
                    { "HasDatabaseProteinFieldProteins", "Has database protein field proteins" },
                    { "HasNoDatabaseProteinFieldProteins", "Does not have database protein field proteins" },
                    { "HasInteractionProteins", "Has interaction proteins" },
                    { "HasNoInteractionProteins", "Does not have interaction proteins" },
                    { "HasProteinCollectionProteins", "Has protein collection proteins" },
                    { "HasNoProteinCollectionProteins", "Does not have protein collection proteins" },
                    { "HasNetworkProteins", "Has network proteins" },
                    { "HasNoNetworkProteins", "Does not have network proteins" },
                    { "HasAnalysisProteins", "Has analysis proteins" },
                    { "HasNoAnalysisProteins", "Does not have analysis proteins" },
                    { "HasPathProteins", "Has path proteins" },
                    { "HasNoPathProteins", "Does not have path proteins" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "DateTimeCreated", "Date created" },
                    { "Name", "Name" },
                    { "DatabaseProteinCount", "Number of database proteins" },
                    { "DatabaseProteinFieldProteinCount", "Number of database protein field proteins" },
                    { "InteractionProteinCount", "Number of interaction proteins" },
                    { "ProteinCollectionProteinCount", "Number of protein collection proteins" },
                    { "NetworkProteinCount", "Number of network proteins" },
                    { "AnalysisProteinCount", "Number of analysis proteins" },
                    { "PathProteinCount", "Number of path proteins" }
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
            var query = _context.Proteins
                .Where(item => item.DatabaseProteins.Any());
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Name") && item.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Description") && item.Description.Contains(input.SearchString));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("HasDatabaseProteins") ? item.DatabaseProteins.Any() : true)
                .Where(item => input.Filter.Contains("HasNoDatabaseProteins") ? !item.DatabaseProteins.Any() : true)
                .Where(item => input.Filter.Contains("HasDatabaseProteinFieldProteins") ? item.DatabaseProteinFieldProteins.Any() : true)
                .Where(item => input.Filter.Contains("HasNoDatabaseProteinFieldProteins") ? !item.DatabaseProteinFieldProteins.Any() : true)
                .Where(item => input.Filter.Contains("HasInteractionProteins") ? item.InteractionProteins.Any() : true)
                .Where(item => input.Filter.Contains("HasNoInteractionProteins") ? !item.InteractionProteins.Any() : true)
                .Where(item => input.Filter.Contains("HasProteinCollectionProteins") ? item.ProteinCollectionProteins.Any() : true)
                .Where(item => input.Filter.Contains("HasNoProteinCollectionProteins") ? !item.ProteinCollectionProteins.Any() : true)
                .Where(item => input.Filter.Contains("HasNetworkProteins") ? item.NetworkProteins.Any() : true)
                .Where(item => input.Filter.Contains("HasNoNetworkProteins") ? !item.NetworkProteins.Any() : true)
                .Where(item => input.Filter.Contains("HasAnalysisProteins") ? item.AnalysisProteins.Any() : true)
                .Where(item => input.Filter.Contains("HasNoAnalysisProteins") ? !item.AnalysisProteins.Any() : true)
                .Where(item => input.Filter.Contains("HasPathProteins") ? item.PathProteins.Any() : true)
                .Where(item => input.Filter.Contains("HasNoPathProteins") ? !item.PathProteins.Any() : true);
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
                case var sort when sort == ("DatabaseProteinCount", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseProteins.Count());
                    break;
                case var sort when sort == ("DatabaseProteinCount", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseProteins.Count());
                    break;
                case var sort when sort == ("DatabaseProteinFieldProteinCount", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseProteinFieldProteins.Count());
                    break;
                case var sort when sort == ("DatabaseProteinFieldProteinCount", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseProteinFieldProteins.Count());
                    break;
                case var sort when sort == ("InteractionProteinCount", "Ascending"):
                    query = query.OrderBy(item => item.InteractionProteins.Count());
                    break;
                case var sort when sort == ("InteractionProteinCount", "Descending"):
                    query = query.OrderByDescending(item => item.InteractionProteins.Count());
                    break;
                case var sort when sort == ("NetworkProteinCount", "Ascending"):
                    query = query.OrderBy(item => item.NetworkProteins.Count());
                    break;
                case var sort when sort == ("NetworkProteinCount", "Descending"):
                    query = query.OrderByDescending(item => item.NetworkProteins.Count());
                    break;
                case var sort when sort == ("AnalysisProteinCount", "Ascending"):
                    query = query.OrderBy(item => item.AnalysisProteins.Count());
                    break;
                case var sort when sort == ("AnalysisProteinCount", "Descending"):
                    query = query.OrderByDescending(item => item.AnalysisProteins.Count());
                    break;
                case var sort when sort == ("PathProteinCount", "Ascending"):
                    query = query.OrderBy(item => item.PathProteins.Count());
                    break;
                case var sort when sort == ("PathProteinCount", "Descending"):
                    query = query.OrderByDescending(item => item.PathProteins.Count());
                    break;
                case var sort when sort == ("ProteinCollectionProteinCount", "Ascending"):
                    query = query.OrderBy(item => item.ProteinCollectionProteins.Count());
                    break;
                case var sort when sort == ("ProteinCollectionProteinCount", "Descending"):
                    query = query.OrderByDescending(item => item.ProteinCollectionProteins.Count());
                    break;
                default:
                    break;
            }
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<Protein>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
