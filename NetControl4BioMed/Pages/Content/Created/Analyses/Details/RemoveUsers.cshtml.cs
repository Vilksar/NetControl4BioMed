using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Content.Created.Analyses.Details
{
    [Authorize]
    public class RemoveUsersModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public RemoveUsersModel(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public string Id { get; set; }

            public IEnumerable<string> Emails { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public Analysis Analysis { get; set; }

            public bool IsGeneric { get; set; }

            public IEnumerable<ItemModel> Items { get; set; }

            public bool IsCurrentUserSelected { get; set; }

            public bool AreAllUsersSelected { get; set; }
        }

        public class ItemModel
        {
            public string Email { get; set; }

            public DateTime DateTimeCreated { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id, string emails)
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
            // Get the items with the provided ID.
            var items = _context.Analyses
                .Where(item => item.AnalysisUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == id)
                .Include(item => item.AnalysisDatabases)
                    .ThenInclude(item => item.Database)
                        .ThenInclude(item => item.DatabaseType)
                .Include(item => item.AnalysisUsers)
                    .ThenInclude(item => item.User)
                .Include(item => item.AnalysisUserInvitations)
                .AsQueryable();
            // Check if there were no items found.
            if (items == null || items.Count() != 1)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Analysis = items.First(),
                IsGeneric = items.First().AnalysisDatabases
                    .Any(item => item.Database.DatabaseType.Name == "Generic")
            };
            // Get the items for the view.
            var items1 = View.Analysis.AnalysisUsers
                .Where(item => emails.Contains(item.User.Email))
                .Select(item => new ItemModel
                {
                    Email = item.User.Email,
                    DateTimeCreated = item.DateTimeCreated
                });
            var items2 = View.Analysis.AnalysisUserInvitations
                .Where(item => emails.Contains(item.Email))
                .Select(item => new ItemModel
                {
                    Email = item.Email,
                    DateTimeCreated = item.DateTimeCreated
                });
            // Define the view items.
            View.Items = items1
                .Concat(items2);
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No users have been found with the provided e-mails.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Details/Index", new { id = View.Analysis.Id });
            }
            // Define the view status of the selected items.
            View.IsCurrentUserSelected = View.Items.Any(item => item.Email == user.Email);
            View.AreAllUsersSelected = !View.Analysis.AnalysisUsers.Select(item => item.User.Email).Except(View.Items.Select(item => item.Email)).Any();
            // Check if there aren't any emails provided.
            if (emails == null || !emails.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid emails have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Details/Index", new { id = View.Analysis.Id });
            }
            // Define the input.
            Input = new InputModel
            {
                Id = View.Analysis.Id
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
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
            if (string.IsNullOrEmpty(Input.Id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Get the items with the provided ID.
            var items = _context.Analyses
                .Where(item => item.AnalysisUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == Input.Id)
                .Include(item => item.AnalysisDatabases)
                    .ThenInclude(item => item.Database)
                        .ThenInclude(item => item.DatabaseType)
                .Include(item => item.AnalysisNodes)
                    .ThenInclude(item => item.Node)
                .Include(item => item.AnalysisEdges)
                    .ThenInclude(item => item.Edge)
                .Include(item => item.AnalysisNetworks)
                    .ThenInclude(item => item.Network)
                .Include(item => item.AnalysisUsers)
                    .ThenInclude(item => item.User)
                .Include(item => item.AnalysisUserInvitations)
                .AsQueryable();
            // Check if there were no items found.
            if (items == null || items.Count() != 1)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Analysis = items.First(),
                IsGeneric = items.First().AnalysisDatabases
                    .Any(item => item.Database.DatabaseType.Name == "Generic")
            };
            // Get the items for the view.
            var items1 = View.Analysis.AnalysisUsers
                .Where(item => Input.Emails.Contains(item.User.Email))
                .Select(item => new ItemModel
                {
                    Email = item.User.Email,
                    DateTimeCreated = item.DateTimeCreated
                });
            var items2 = View.Analysis.AnalysisUserInvitations
                .Where(item => Input.Emails.Contains(item.Email))
                .Select(item => new ItemModel
                {
                    Email = item.Email,
                    DateTimeCreated = item.DateTimeCreated
                });
            // Define the view items.
            View.Items = items1
                .Concat(items2);
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No users have been found with the provided e-mails.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Details/Index", new { id = View.Analysis.Id });
            }
            // Define the status of the selected items.
            View.IsCurrentUserSelected = View.Items.Any(item => item.Email == user.Email);
            View.AreAllUsersSelected = !View.Analysis.AnalysisUsers.Select(item => item.User.Email).Except(View.Items.Select(item => item.Email)).Any();
            // Check if there aren't any emails provided.
            if (Input.Emails == null || !Input.Emails.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid emails have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Details/Index", new { id = View.Analysis.Id });
            }
            // Check if the provided model isn't valid.
            if (!ModelState.IsValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error has been encountered. Please check again the input fields.");
                // Redisplay the page.
                return Page();
            }
            // Save the number of items found.
            var userCount = View.Items.Count();
            // Check if all of the users have been selected.
            if (View.AreAllUsersSelected)
            {
                // Mark the network for deletion.
                _context.Analyses.Remove(View.Analysis);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
                // Display a message.
                TempData["StatusMessage"] = $"Success: 1 analysis deleted successfully.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Get all of the network users and network user invitations.
            var networkUsers = View.Analysis.AnalysisUsers
                .Where(item => Input.Emails.Contains(item.User.Email));
            var networkUserInvitations = View.Analysis.AnalysisUserInvitations
                .Where(item => Input.Emails.Contains(item.Email));
            // Mark the items for deletion.
            _context.AnalysisUsers.RemoveRange(networkUsers);
            _context.AnalysisUserInvitations.RemoveRange(networkUserInvitations);
            // Save the changes to the database.
            await _context.SaveChangesAsync();
            // Display a message.
            TempData["StatusMessage"] = $"Success: {userCount.ToString()} user{(userCount != 1 ? "s" : string.Empty)} removed successfully.";
            // Check if the current user was selected.
            if (View.IsCurrentUserSelected)
            {
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Redirect to the index page.
            return RedirectToPage("/Content/Created/Analyses/Details/Users", new { id = View.Analysis.Id });
        }
    }
}
