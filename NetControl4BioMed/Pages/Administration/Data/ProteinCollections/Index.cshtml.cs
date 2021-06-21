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
using EnumerationProteinCollectionType = NetControl4BioMed.Data.Enumerations.ProteinCollectionType;

namespace NetControl4BioMed.Pages.Administration.Data.ProteinCollections
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
            public SearchViewModel<ProteinCollection> Search { get; set; }

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
                    { "ContainsSeedProteins", "Contains seed proteins"},
                    { "ContainsNotSeedProteins", "Does not contain seed proteins"},
                    { "ContainsSourceProteins", "Contains source proteins"},
                    { "ContainsNotSourceProteins", "Does not contain source proteins"},
                    { "ContainsTargetProteins", "Contains target proteins"},
                    { "ContainsNotTargetProteins", "Does not contain target proteins"},
                    { "HasProteinCollectionTypes", "Has protein collection types" },
                    { "HasNoProteinCollectionTypes", "Does not have protein collection types" },
                    { "HasProteinCollectionProteins", "Has protein collection proteins" },
                    { "HasNoProteinCollectionProteins", "Does not have protein collection proteins" },
                    { "HasNetworkProteinColections", "Has network protein collections" },
                    { "HasNoNetworkProteinCollections", "Does not have network protein collections" },
                    { "HasAnalysisProteinCollections", "Has analysis protein collections" },
                    { "HasNoAnalysisProteinCollections", "Does not have analysis protein collections" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "Id", "ID" },
                    { "DateTimeCreated", "Date created" },
                    { "Name", "Name" },
                    { "ProteinCollectionTypeCount", "Number of protein collection types" },
                    { "ProteinCollectionProteinCount", "Number of protein collection proteins" },
                    { "NetworkProteinCollectionCount", "Number of network protein collections" },
                    { "AnalysisProteinCollectionCount", "Number of analysis protein collections" }
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
            // Start with all of the items in the database.
            var query = _context.ProteinCollections
                .Where(item => true);
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("Id") && item.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Name") && item.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Description") && item.Description.Contains(input.SearchString));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("ContainsSeedProteins") ? item.ProteinCollectionTypes.Any(item1 => item1.Type == EnumerationProteinCollectionType.Seed) : true)
                .Where(item => input.Filter.Contains("ContainsNotSeedProteins") ? !item.ProteinCollectionTypes.Any(item1 => item1.Type == EnumerationProteinCollectionType.Seed) : true)
                .Where(item => input.Filter.Contains("ContainsSourceProteins") ? item.ProteinCollectionTypes.Any(item1 => item1.Type == EnumerationProteinCollectionType.Source) : true)
                .Where(item => input.Filter.Contains("ContainsNotSourceProteins") ? !item.ProteinCollectionTypes.Any(item1 => item1.Type == EnumerationProteinCollectionType.Source) : true)
                .Where(item => input.Filter.Contains("ContainsTargetProteins") ? item.ProteinCollectionTypes.Any(item1 => item1.Type == EnumerationProteinCollectionType.Target) : true)
                .Where(item => input.Filter.Contains("ContainsNotTargetProteins") ? !item.ProteinCollectionTypes.Any(item1 => item1.Type == EnumerationProteinCollectionType.Target) : true)
                .Where(item => input.Filter.Contains("HasProteinCollectionTypes") ? item.ProteinCollectionTypes.Any() : true)
                .Where(item => input.Filter.Contains("HasNoProteinCollectionTypes") ? !item.ProteinCollectionTypes.Any() : true)
                .Where(item => input.Filter.Contains("HasProteinCollectionProteins") ? item.ProteinCollectionProteins.Any() : true)
                .Where(item => input.Filter.Contains("HasNoProteinCollectionProteins") ? !item.ProteinCollectionProteins.Any() : true)
                .Where(item => input.Filter.Contains("HasNetworkProteinColections") ? item.NetworkProteinCollections.Any() : true)
                .Where(item => input.Filter.Contains("HasNoNetworkProteinColections") ? !item.NetworkProteinCollections.Any() : true)
                .Where(item => input.Filter.Contains("HasAnalysisProteinCollections") ? item.AnalysisProteinCollections.Any() : true)
                .Where(item => input.Filter.Contains("HasNoAnalysisProteinCollections") ? !item.AnalysisProteinCollections.Any() : true);
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
                case var sort when sort == ("ProteinCollectionTypeCount", "Ascending"):
                    query = query.OrderBy(item => item.ProteinCollectionTypes.Count());
                    break;
                case var sort when sort == ("ProteinCollectionTypeCount", "Descending"):
                    query = query.OrderByDescending(item => item.ProteinCollectionTypes.Count());
                    break;
                case var sort when sort == ("ProteinCollectionProteinCount", "Ascending"):
                    query = query.OrderBy(item => item.ProteinCollectionProteins.Count());
                    break;
                case var sort when sort == ("ProteinCollectionProteinCount", "Descending"):
                    query = query.OrderByDescending(item => item.ProteinCollectionProteins.Count());
                    break;
                case var sort when sort == ("NetworkProteinCollectionCount", "Ascending"):
                    query = query.OrderBy(item => item.NetworkProteinCollections.Count());
                    break;
                case var sort when sort == ("NetworkProteinCollectionCount", "Descending"):
                    query = query.OrderByDescending(item => item.NetworkProteinCollections.Count());
                    break;
                case var sort when sort == ("AnalysisProteinCollectionCount", "Ascending"):
                    query = query.OrderBy(item => item.AnalysisProteinCollections.Count());
                    break;
                case var sort when sort == ("AnalysisProteinCollectionCount", "Descending"):
                    query = query.OrderByDescending(item => item.AnalysisProteinCollections.Count());
                    break;
                default:
                    break;
            }
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<ProteinCollection>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
