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

namespace NetControl4BioMed.Pages.Content.Created.Networks.Details.Accounts.Users
{
    [Authorize]
    public class RemoveModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public RemoveModel(UserManager<User> userManager, ApplicationDbContext context)
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
            public Network Network { get; set; }

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
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Get the items with the provided ID.
            var items = _context.Networks
                .Where(item => item.NetworkUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == id);
            // Check if there were no items found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Network = items
                    .Include(item => item.NetworkUsers)
                        .ThenInclude(item => item.User)
                    .Include(item => item.NetworkUserInvitations)
                    .First(),
                IsGeneric = items
                    .Any(item => item.NetworkDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
            };
            // Get the items for the view.
            var items1 = View.Network.NetworkUsers
                .Where(item => emails.Contains(item.User.Email))
                .Select(item => new ItemModel
                {
                    Email = item.User.Email,
                    DateTimeCreated = item.DateTimeCreated
                });
            var items2 = View.Network.NetworkUserInvitations
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
                return RedirectToPage("/Content/Created/Networks/Details/Accounts/Users/Index", new { id = View.Network.Id });
            }
            // Define the view status of the selected items.
            View.IsCurrentUserSelected = View.Items.Any(item => item.Email == user.Email);
            View.AreAllUsersSelected = !View.Network.NetworkUsers.Select(item => item.User.Email).Except(View.Items.Select(item => item.Email)).Any();
            // Check if there aren't any emails provided.
            if (emails == null || !emails.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid emails have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Details/Accounts/Users/Index", new { id = View.Network.Id });
            }
            // Define the input.
            Input = new InputModel
            {
                Id = View.Network.Id
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
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Get the items with the provided ID.
            var items = _context.Networks
                .Where(item => item.NetworkUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == Input.Id);
            // Check if there were no items found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                IsGeneric = items
                    .Any(item => item.NetworkDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic")),
                Network = items
                    .Include(item => item.NetworkNodes)
                        .ThenInclude(item => item.Node)
                    .Include(item => item.NetworkEdges)
                        .ThenInclude(item => item.Edge)
                    .Include(item => item.NetworkUsers)
                        .ThenInclude(item => item.User)
                    .Include(item => item.NetworkUserInvitations)
                    .First()
            };
            // Get the items for the view.
            var items1 = View.Network.NetworkUsers
                .Where(item => Input.Emails.Contains(item.User.Email))
                .Select(item => new ItemModel
                {
                    Email = item.User.Email,
                    DateTimeCreated = item.DateTimeCreated
                });
            var items2 = View.Network.NetworkUserInvitations
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
                return RedirectToPage("/Content/Created/Networks/Details/Accounts/Users/Index", new { id = View.Network.Id });
            }
            // Define the status of the selected items.
            View.IsCurrentUserSelected = View.Items.Any(item => item.Email == user.Email);
            View.AreAllUsersSelected = !View.Network.NetworkUsers.Select(item => item.User.Email).Except(View.Items.Select(item => item.Email)).Any();
            // Check if there aren't any emails provided.
            if (Input.Emails == null || !Input.Emails.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid emails have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Details/Accounts/Users/Index", new { id = View.Network.Id });
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
                // Get the related entities that use the current network.
                var analyses = View.Network.AnalysisNetworks.Select(item => item.Analysis);
                // Mark the items for deletion.
                _context.Analyses.RemoveRange(analyses);
                // Check if the network is generic.
                if (View.IsGeneric)
                {
                    // Get the related entities for the current network.
                    var genericNodes = View.Network.NetworkNodes.Select(item => item.Node);
                    var genericEdges = View.Network.NetworkEdges.Select(item => item.Edge);
                    // Mark the items for deletion.
                    _context.Edges.RemoveRange(genericEdges);
                    _context.Nodes.RemoveRange(genericNodes);
                }
                // Mark the network for deletion.
                _context.Networks.Remove(View.Network);
                // Save the changes to the database.
                await _context.SaveChangesAsync();
                // Display a message.
                TempData["StatusMessage"] = $"Success: 1 network deleted successfully.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Get all of the network users and network user invitations.
            var networkUsers = View.Network.NetworkUsers
                .Where(item => Input.Emails.Contains(item.User.Email));
            var networkUserInvitations = View.Network.NetworkUserInvitations
                .Where(item => Input.Emails.Contains(item.Email));
            // Mark the items for deletion.
            _context.NetworkUsers.RemoveRange(networkUsers);
            _context.NetworkUserInvitations.RemoveRange(networkUserInvitations);
            // Save the changes to the database.
            await _context.SaveChangesAsync();
            // Display a message.
            TempData["StatusMessage"] = $"Success: {userCount.ToString()} user{(userCount != 1 ? "s" : string.Empty)} removed successfully.";
            // Check if the current user was selected.
            if (View.IsCurrentUserSelected)
            {
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Redirect to the index page.
            return RedirectToPage("/Content/Created/Networks/Details/Accounts/Users/Index", new { id = View.Network.Id });
        }
    }
}
