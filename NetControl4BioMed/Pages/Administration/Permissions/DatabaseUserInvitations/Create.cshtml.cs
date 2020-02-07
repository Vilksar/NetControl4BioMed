using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Administration.Permissions.DatabaseUserInvitations
{
    [Authorize(Roles = "Administrator")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.EmailAddress)]
            [Required(ErrorMessage = "This field is required.")]
            public string Email { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string DatabaseString { get; set; }
        }

        public IActionResult OnGet(string email = null, string databaseString = null)
        {
            // Define the input.
            Input = new InputModel
            {
                Email = email,
                DatabaseString = databaseString
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
            // Try to get the user with the provided e-mail.
            var user = _context.Users.FirstOrDefault(item => item.Email == Input.Email);
            // Check if there was any user found.
            if (user != null)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "A user with the provided e-mail already exists.");
                // Redisplay the page.
                return Page();
            }
            // Get the database based on the provided string.
            var database = _context.Databases.FirstOrDefault(item => item.Id == Input.DatabaseString || item.Name == Input.DatabaseString);
            // Check if there was no database found.
            if (database == null)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "No database could be found with the given string.");
                // Redisplay the page.
                return Page();
            }
            // Create a new database user.
            var databaseUserInvitation = new DatabaseUserInvitation
            {
                DatabaseId = database.Id,
                Database = database,
                Email = Input.Email,
                DateTimeCreated = DateTime.Now
            };
            // Mark it for addition to the database.
            _context.DatabaseUserInvitations.Add(databaseUserInvitation);
            // Save the changes.
            await _context.SaveChangesAsync();
            // Display a message.
            TempData["StatusMessage"] = "Success: 1 database user invitation created successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Permissions/DatabaseUserInvitations/Index");
        }
    }
}
