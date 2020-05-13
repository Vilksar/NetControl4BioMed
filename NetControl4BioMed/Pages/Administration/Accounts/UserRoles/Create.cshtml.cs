using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Tasks;

namespace NetControl4BioMed.Pages.Administration.Accounts.UserRoles
{
    [Authorize(Roles = "Administrator")]
    public class CreateModel : PageModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;

        public CreateModel(IServiceProvider serviceProvider, UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext context)
        {
            _serviceProvider = serviceProvider;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string UserString { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string RoleString { get; set; }
        }

        public IActionResult OnGet(string userString = null, string roleString = null)
        {
            // Define the input.
            Input = new InputModel
            {
                UserString = userString,
                RoleString = roleString
            };
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
            // Get the user based on the provided string.
            var user = _context.Users.FirstOrDefault(item => item.Id == Input.UserString || item.Email == Input.UserString);
            // Check if there was no user found.
            if (user == null)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "No user could be found matching the provided string.");
                // Redisplay the page.
                return Page();
            }
            // Get the role based on the provided string.
            var role = _context.Roles.FirstOrDefault(item => item.Id == Input.RoleString || item.Name == Input.RoleString);
            // Check if there was no role found.
            if (role == null)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "No role could be found matching the provided string.");
                // Redisplay the page.
                return Page();
            }
            // Define a new task.
            var task = new UserRolesTask
            {
                Items = new List<UserRoleInputModel>
                {
                    new UserRoleInputModel
                    {
                        UserId = user.Id,
                        RoleId = role.Id
                    }
                }
            };
            // Try to run the task.
            try
            {
                // Run the task.
                task.Create(_serviceProvider, CancellationToken.None);
            }
            catch (Exception exception)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, exception.Message);
                // Redisplay the page.
                return Page();
            }
            // Check if the found user is the current one.
            if (await _userManager.GetUserAsync(User) == user)
            {
                // Log out the user.
                await _signInManager.SignOutAsync();
                // Display a message.
                TempData["StatusMessage"] = $"Info: 1 user role created successfully. The roles assigned to your account have changed, so you have been signed out.";
                // Redirect to the index page.
                return RedirectToPage("/Index");
            }
            // Display a message.
            TempData["StatusMessage"] = "Success: 1 user role created successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Accounts/UserRoles/Index");
        }
    }
}
