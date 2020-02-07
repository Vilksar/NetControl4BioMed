using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Administration.Accounts.Users
{
    [Authorize(Roles = "Administrator")]
    public class CreateModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public CreateModel(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.EmailAddress)]
            [Required(ErrorMessage = "This field is required.")]
            public string Email { get; set; }

            [DataType(DataType.Password)]
            [Required(ErrorMessage = "This field is required.")]
            [StringLength(100, ErrorMessage = "The password must be at least {2} characters long.", MinimumLength = 6)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Required(ErrorMessage = "This field is required.")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "This field is required.")]
            public bool EmailConfirmed { get; set; }
        }

        public IActionResult OnGet()
        {
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Check if the provided model isn't valid.
            if (!ModelState.IsValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error has been encountered. Please check again the input fields.");
                // Redisplay the page.
                return Page();
            }
            // Define the new user.
            var user = new User
            {
                UserName = Input.Email,
                Email = Input.Email,
                DateTimeCreated = DateTime.Now
            };
            // Try to create the new user.
            var result = await _userManager.CreateAsync(user, Input.Password);
            // Check if we should set the e-mail as confirmed.
            if (result.Succeeded && Input.EmailConfirmed)
            {
                // Generate the token for e-mail confirmation.
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                // Confirm the e-mail address.
                result = await _userManager.ConfirmEmailAsync(user, token);
            }
            // Check if any of the operations has failed.
            if (!result.Succeeded)
            {
                // Go over each of the encountered errors.
                foreach (var error in result.Errors)
                {
                    // Add the error to the model.
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                // Redisplay the page.
                return Page();
            }
            // Get all the databases, networks and analyses to which the user already has access.
            var databaseUserInvitations = _context.DatabaseUserInvitations.Where(item => item.Email == user.Email);
            var networkUserInvitations = _context.NetworkUserInvitations.Where(item => item.Email == user.Email);
            var analysisUserInvitations = _context.AnalysisUserInvitations.Where(item => item.Email == user.Email);
            // Create, for each, a corresponding user entry.
            var databaseUsers = databaseUserInvitations.Select(item => new DatabaseUser { DatabaseId = item.DatabaseId, Database = item.Database, UserId = user.Id, User = user, DateTimeCreated = item.DateTimeCreated });
            var networkUsers = networkUserInvitations.Select(item => new NetworkUser { NetworkId = item.NetworkId, Network = item.Network, UserId = user.Id, User = user, DateTimeCreated = item.DateTimeCreated });
            var analysisUsers = analysisUserInvitations.Select(item => new AnalysisUser { AnalysisId = item.AnalysisId, Analysis = item.Analysis, UserId = user.Id, User = user, DateTimeCreated = item.DateTimeCreated });
            // Mark the new items for addition.
            _context.DatabaseUsers.AddRange(databaseUsers);
            _context.NetworkUsers.AddRange(networkUsers);
            _context.AnalysisUsers.AddRange(analysisUsers);
            // Mark the old items for deletion.
            _context.DatabaseUserInvitations.RemoveRange(databaseUserInvitations);
            _context.NetworkUserInvitations.RemoveRange(networkUserInvitations);
            _context.AnalysisUserInvitations.RemoveRange(analysisUserInvitations);
            // Save the changes in the database.
            await _context.SaveChangesAsync();
            // Display a message.
            TempData["StatusMessage"] = "Success: 1 user created successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Accounts/Users/Index");
        }
    }
}
