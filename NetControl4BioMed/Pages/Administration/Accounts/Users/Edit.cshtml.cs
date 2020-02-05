using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Administration.Accounts.Users
{
    [Authorize(Roles = "Administrator")]
    public class EditModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public EditModel(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string Id { get; set; }

            [DataType(DataType.EmailAddress)]
            [Required(ErrorMessage = "This field is required.")]
            public string Email { get; set; }

            [Required(ErrorMessage = "This field is required.")]
            public bool EmailConfirmed { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public User User { get; set; }
        }

        public IActionResult OnGet(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Users/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                User = _context.Users
                    .Where(item => item.Id == id)
                    .Include(item => item.UserRoles)
                        .ThenInclude(item => item.Role)
                    .Include(item => item.DatabaseUsers)
                        .ThenInclude(item => item.Database)
                    .FirstOrDefault()
            };
            // Check if the item hasn't been found.
            if (View.User == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item could be found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Users/Index");
            }
            // Define the input.
            Input = new InputModel
            {
                Id = View.User.Id,
                Email = View.User.Email,
                EmailConfirmed = View.User.EmailConfirmed
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Check if the provided model is not valid.
            if (!ModelState.IsValid)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Users/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                User = _context.Users
                    .Where(item => item.Id == Input.Id)
                    .Include(item => item.UserRoles)
                        .ThenInclude(item => item.Role)
                    .Include(item => item.DatabaseUsers)
                        .ThenInclude(item => item.Database)
                    .Include(item => item.NetworkUsers)
                    .Include(item => item.AnalysisUsers)
                    .FirstOrDefault()
            };
            // Check if the item hasn't been found.
            if (View.User == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item could be found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Accounts/Users/Index");
            }
            // Check if the e-mail is different from the current one.
            if (Input.Email != View.User.Email)
            {
                // Try to update the username.
                var result = await _userManager.SetUserNameAsync(View.User, Input.Email);
                // Try to update the e-mail.
                result = result.Succeeded ? await _userManager.SetEmailAsync(View.User, Input.Email) : result;
                // Check if the update was not successful.
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
            }
            // Check if we should set the e-mail as confirmed.
            if (!View.User.EmailConfirmed && Input.EmailConfirmed)
            {
                // Generate the token and try to set it.
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(View.User);
                var result = await _userManager.ConfirmEmailAsync(View.User, token);
                // Check if the update was not successful.
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
            }
            // Save the changes to the database.
            await _context.SaveChangesAsync();
            // Display a message.
            TempData["StatusMessage"] = $"Success: 1 user updated successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Accounts/Users/Index");
        }
    }
}
