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

namespace NetControl4BioMed.Pages.Administration.Relationships.DatabaseInteractionFieldInteractions
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
            public SearchViewModel<DatabaseInteractionFieldInteraction> Search { get; set; }

            public static SearchOptionsViewModel SearchOptions { get; } = new SearchOptionsViewModel
            {
                SearchIn = new Dictionary<string, string>
                {
                    { "DatabaseInteractionFieldId", "Database interaction field ID" },
                    { "DatabaseInteractionFieldName", "Database interaction field name" },
                    { "InteractionId", "Interaction ID" },
                    { "InteractionName", "Interaction name" },
                    { "Value", "Value" }
                },
                Filter = new Dictionary<string, string>
                {
                },
                SortBy = new Dictionary<string, string>
                {
                    { "DatabaseInteractionFieldId", "Database interaction field ID" },
                    { "DatabaseInteractionFieldName", "Database interaction field name" },
                    { "InteractionId", "Interaction ID" },
                    { "InteractionName", "InteractionName" },
                    { "Value", "Value" }
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
            var query = _context.DatabaseInteractionFieldInteractions
                .Where(item => item.Interaction.DatabaseInteractions.Any());
            // Select the results matching the search string.
            query = query
                .Where(item => !input.SearchIn.Any() ||
                    input.SearchIn.Contains("DatabaseInteractionFieldId") && item.DatabaseInteractionField.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("DatabaseInteractionFieldName") && item.DatabaseInteractionField.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("InteractionId") && item.Interaction.Id.Contains(input.SearchString) ||
                    input.SearchIn.Contains("InteractionName") && item.Interaction.Name.Contains(input.SearchString) ||
                    input.SearchIn.Contains("Value") && item.Value.Contains(input.SearchString));
            // Sort it according to the parameters.
            switch ((input.SortBy, input.SortDirection))
            {
                case var sort when sort == ("DatabaseInteractionFieldId", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseInteractionField.Id);
                    break;
                case var sort when sort == ("DatabaseInteractionFieldId", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseInteractionField.Id);
                    break;
                case var sort when sort == ("DatabaseInteractionFieldName", "Ascending"):
                    query = query.OrderBy(item => item.DatabaseInteractionField.Name);
                    break;
                case var sort when sort == ("DatabaseInteractionFieldName", "Descending"):
                    query = query.OrderByDescending(item => item.DatabaseInteractionField.Name);
                    break;
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
                case var sort when sort == ("Value", "Ascending"):
                    query = query.OrderBy(item => item.Value);
                    break;
                case var sort when sort == ("Value", "Descending"):
                    query = query.OrderByDescending(item => item.Value);
                    break;
                default:
                    break;
            }
            // Include the related entitites.
            query = query
                .Include(item => item.DatabaseInteractionField)
                .Include(item => item.Interaction);
            // Define the view.
            View = new ViewModel
            {
                Search = new SearchViewModel<DatabaseInteractionFieldInteraction>(_linkGenerator, HttpContext, input, query)
            };
            // Return the page.
            return Page();
        }
    }
}
