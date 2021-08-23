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

namespace NetControl4BioMed.Pages.Administration.Relationships.ProteinCollectionTypes
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
            public SearchViewModel<ProteinCollectionType> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "ProteinCollectionId", "Protein collection ID" },
                    { "ProteinCollectionName", "Protein collection name" }
                },
                Filter = new Dictionary<string, string>
                {
                    { "ContainsSeedProteins", "Contains seed proteins" },
                    { "ContainsNotSeedProteins", "Does not contain seed proteins" },
                    { "ContainsSourceProteins", "Contains source proteins" },
                    { "ContainsNotSourceProteins", "Does not contain source proteins" },
                    { "ContainsTargetProteins", "Contains target proteins" },
                    { "ContainsNotTargetProteins", "Does not contain target proteins" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "ProteinCollectionId", "Protein collection ID" },
                    { "ProteinCollectionName", "Protein collection name" },
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
            var query = _context.ProteinCollectionTypes
                .Where(item => true);
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("ProteinCollectionId") && item.ProteinCollection.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("ProteinCollectionName") && item.ProteinCollection.Name.Contains(input.SearchString));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("ContainsSeedProteins") ? item.Type == EnumerationProteinCollectionType.Seed : true)
                .Where(item => input.Filter.Contains("ContainsNotSeedProteins") ? item.Type != EnumerationProteinCollectionType.Seed : true)
                .Where(item => input.Filter.Contains("ContainsIsSourceProteins") ? item.Type == EnumerationProteinCollectionType.Source : true)
                .Where(item => input.Filter.Contains("ContainsIsNotSourceProteins") ? item.Type != EnumerationProteinCollectionType.Source : true)
                .Where(item => input.Filter.Contains("ContainsIsTargetProteins") ? item.Type == EnumerationProteinCollectionType.Target : true)
                .Where(item => input.Filter.Contains("ContainsNotTargetProteins") ? item.Type != EnumerationProteinCollectionType.Target : true);
            // Sort it according to the parameters.
            switch ((input.SortBy, input.SortDirection))
            {
                case var sort when sort == ("ProteinCollectionId", "Ascending"):
                    query = query.OrderBy(item => item.ProteinCollection.Id);
                    break;
                case var sort when sort == ("ProteinCollectionId", "Descending"):
                    query = query.OrderByDescending(item => item.ProteinCollection.Id);
                    break;
                case var sort when sort == ("ProteinCollectionName", "Ascending"):
                    query = query.OrderBy(item => item.ProteinCollection.Name);
                    break;
                case var sort when sort == ("ProteinCollectionName", "Descending"):
                    query = query.OrderByDescending(item => item.ProteinCollection.Name);
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
                .Include(item => item.ProteinCollection);
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<ProteinCollectionType>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
