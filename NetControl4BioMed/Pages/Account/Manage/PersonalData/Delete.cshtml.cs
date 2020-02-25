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
using NetControl4BioMed.Helpers.Interfaces;

namespace NetControl4BioMed.Pages.Account.Manage.PersonalData
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IReCaptchaChecker _reCaptchaChecker;

        public DeleteModel(UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext context, IReCaptchaChecker reCaptchaChecker)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _reCaptchaChecker = reCaptchaChecker;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.Password)]
            [Required(ErrorMessage = "This field is required.")]
            public string Password { get; set; }

            public string ReCaptchaToken { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public bool HasPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if the user exists or not.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: An error occured while trying to load the user data. If you are already logged in, please log out and try again.";
                // Redirect to the home page.
                return RedirectToPage("/Index");
            }
            // Define the variables for the view.
            View = new ViewModel
            {
                HasPassword = await _userManager.HasPasswordAsync(user)
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if the user exists or not.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: An error occured while trying to load the user data. If you are already logged in, please log out and try again.";
                // Redirect to the login page.
                return RedirectToPage("/Index");
            }
            // Define the variables to return to the view.
            View = new ViewModel
            {
                HasPassword = await _userManager.HasPasswordAsync(user)
            };
            // Check if the reCaptcha is valid.
            if (!await _reCaptchaChecker.IsValid(Input.ReCaptchaToken))
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The reCaptcha verification failed.");
                // Return the page.
                return Page();
            }
            // Check if the provided model is not valid.
            if (!ModelState.IsValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error was encountered. Please check again the input fields.");
                // Return the page.
                return Page();
            }
            // Check if the user is the last administrator user.
            if ((await _userManager.IsInRoleAsync(user, "Administrator")) && (await _userManager.GetUsersInRoleAsync("Administrator")).Count() == 1)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "If you delete your account, then there will be no administrator users left. Please add a new administrator, and then try again.");
                // Return the page.
                return Page();
            }
            // Check if the user has password and the provided password is incorrect.
            if (View.HasPassword && !await _userManager.CheckPasswordAsync(user, Input.Password))
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The provided password is not correct.");
                // Return the page.
                return Page();
            }
            // Try to delete the user.
            var result = await _userManager.DeleteAsync(user);
            // Check if the user deletion was not successful.
            if (!result.Succeeded)
            {
                // Go over the encountered errors
                foreach (var error in result.Errors)
                {
                    // and add them to the model
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                // Return the page.
                return Page();
            }
            // Sign out the user.
            await _signInManager.SignOutAsync();
            // Go over all of the networks and analyses and find the ones without any assigned users.
            var networks = _context.Networks.Where(item => !item.NetworkUsers.Any());
            var analyses = _context.Analyses.Where(item => !item.AnalysisUsers.Any() || item.AnalysisNetworks.Any(item1 => networks.Contains(item1.Network)));
            // Get the generic entities among them.
            var genericNetworks = networks.Where(item => item.NetworkDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"));
            var genericNodes = _context.Nodes.Where(item => item.NetworkNodes.Any(item1 => genericNetworks.Contains(item1.Network)));
            var genericEdges = _context.Edges.Where(item => item.NetworkEdges.Any(item1 => genericNetworks.Contains(item1.Network)) || item.EdgeNodes.Any(item1 => genericNodes.Contains(item1.Node)));
            // Mark the items for deletion.
            _context.Analyses.RemoveRange(analyses);
            _context.Networks.RemoveRange(networks);
            _context.Edges.RemoveRange(genericEdges);
            _context.Nodes.RemoveRange(genericNodes);
            // Save the changes in the database.
            await _context.SaveChangesAsync();
            // Redirect to the home page.
            return RedirectToPage("/Index");
        }
    }
}
