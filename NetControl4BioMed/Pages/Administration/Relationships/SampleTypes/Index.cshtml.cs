using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.ViewModels;
using EnumerationSampleType = NetControl4BioMed.Data.Enumerations.SampleType;

namespace NetControl4BioMed.Pages.Administration.Relationships.SampleTypes
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
            public SearchViewModel<SampleType> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "SampleId", "Node collection ID" },
                    { "SampleName", "Node collection name" }
                },
                Filter = new Dictionary<string, string>
                {
                    { "ContainsSeedNodes", "Contains seed nodes" },
                    { "ContainsNotSeedNodes", "Does not contain seed nodes" },
                    { "ContainsSeedEdges", "Contains seed edges" },
                    { "ContainsNotSeedEdges", "Does not contain seed edges" },
                    { "ContainsSourceNodes", "Contains source nodes" },
                    { "ContainsNotSourceNodes", "Does not contain source nodes" },
                    { "ContainsTargetNodes", "Contains target nodes" },
                    { "ContainsNotTargetNodes", "Does not contain target nodes" }
                },
                SortBy = new Dictionary<string, string>
                {
                    { "SampleId", "Node collection ID" },
                    { "SampleName", "Node collection name" },
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
            var query = _context.SampleTypes
                .Where(item => true);
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("SampleId") && item.Sample.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("SampleName") && item.Sample.Name.Contains(input.SearchString));
            // Select the results matching the filter parameter.
            query = query
                .Where(item => input.Filter.Contains("ContainsSeedNodes") ? item.Type == EnumerationSampleType.SeedNodes : true)
                .Where(item => input.Filter.Contains("ContainsNotSeedNodes") ? item.Type != EnumerationSampleType.SeedNodes : true)
                .Where(item => input.Filter.Contains("ContainsSeedEdges") ? item.Type == EnumerationSampleType.SeedEdges : true)
                .Where(item => input.Filter.Contains("ContainsNotSeedEdges") ? item.Type != EnumerationSampleType.SeedEdges : true)
                .Where(item => input.Filter.Contains("ContainsIsSourceNodes") ? item.Type == EnumerationSampleType.SourceNodes : true)
                .Where(item => input.Filter.Contains("ContainsIsNotSourceNodes") ? item.Type != EnumerationSampleType.SourceNodes : true)
                .Where(item => input.Filter.Contains("ContainsIsTargetNodes") ? item.Type == EnumerationSampleType.TargetNodes : true)
                .Where(item => input.Filter.Contains("ContainsNotTargetNodes") ? item.Type != EnumerationSampleType.TargetNodes : true);
            // Sort it according to the parameters.
            switch ((input.SortBy, input.SortDirection))
            {
                case var sort when sort == ("SampleId", "Ascending"):
                    query = query.OrderBy(item => item.Sample.Id);
                    break;
                case var sort when sort == ("SampleId", "Descending"):
                    query = query.OrderByDescending(item => item.Sample.Id);
                    break;
                case var sort when sort == ("SampleName", "Ascending"):
                    query = query.OrderBy(item => item.Sample.Name);
                    break;
                case var sort when sort == ("SampleName", "Descending"):
                    query = query.OrderByDescending(item => item.Sample.Name);
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
                .Include(item => item.Sample);
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<SampleType>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
