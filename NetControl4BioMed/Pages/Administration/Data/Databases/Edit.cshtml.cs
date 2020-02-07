using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Administration.Data.Databases
{
    [Authorize(Roles = "Administrator")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string Id { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string Name { get; set; }

            [DataType(DataType.MultilineText)]
            public string Description { get; set; }

            [DataType(DataType.Url)]
            public string Url { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public bool IsPublic { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string DatabaseTypeId { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public IEnumerable<DatabaseType> DatabaseTypes { get; set; }

            public Database Database { get; set; }
        }

        public IActionResult OnGet(string id)
        {
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Data/Databases/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                DatabaseTypes = _context.DatabaseTypes
                    .Where(item => item.Name != "Generic")
                    .AsEnumerable(),
                Database = _context.Databases
                    .Where(item => item.Id == id)
                    .Include(item => item.DatabaseType)
                    .Include(item => item.DatabaseUsers)
                    .Include(item => item.DatabaseUserInvitations)
                    .Include(item => item.DatabaseNodeFields)
                    .Include(item => item.DatabaseEdgeFields)
                    .Include(item => item.DatabaseNodes)
                    .Include(item => item.DatabaseEdges)
                    .Include(item => item.NetworkDatabases)
                    .Include(item => item.AnalysisDatabases)
                    .FirstOrDefault()
            };
            // Check if the item hasn't been found.
            if (View.Database == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item could be found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Data/Databases/Index");
            }
            // Check if the database is the generic database.
            if (View.Database.Name == "Generic")
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The \"Generic\" database can't be edited.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Data/Databases/Index");
            }
            // Define the input.
            Input = new InputModel
            {
                Id = View.Database.Id,
                Name = View.Database.Name,
                Description = View.Database.Description,
                Url = View.Database.Url,
                IsPublic = View.Database.IsPublic,
                DatabaseTypeId = View.Database.DatabaseType.Id
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
                return RedirectToPage("/Administration/Data/Databases/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                DatabaseTypes = _context.DatabaseTypes
                    .Where(item => item.Name != "Generic")
                    .AsEnumerable(),
                Database = _context.Databases
                    .Where(item => item.Id == Input.Id)
                    .Include(item => item.DatabaseType)
                    .Include(item => item.DatabaseUsers)
                    .Include(item => item.DatabaseUserInvitations)
                    .Include(item => item.DatabaseNodeFields)
                    .Include(item => item.DatabaseEdgeFields)
                    .Include(item => item.DatabaseNodes)
                    .Include(item => item.DatabaseEdges)
                    .Include(item => item.NetworkDatabases)
                    .Include(item => item.AnalysisDatabases)
                    .FirstOrDefault()
            };
            // Check if the item hasn't been found.
            if (View.Database == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item could be found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Data/Databases/Index");
            }
            // Check if the database is the generic database.
            if (View.Database.Name == "Generic")
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The \"Generic\" database can't be edited.";
                // Redirect to the index page.
                return RedirectToPage("/Administration/Data/Databases/Index");
            }
            // Check if there is another database with the same name.
            if (_context.Databases.Any(item => item.Id != View.Database.Id && item.Name == Input.Name))
            {
                // Add an error to the model
                ModelState.AddModelError(string.Empty, $"A database with the name \"{Input.Name}\" already exists.");
                // Redisplay the page.
                return Page();
            }
            // Check if the database type is to be changed, and the database has networks.
            if (View.Database.DatabaseType.Id != Input.DatabaseTypeId && View.Database.NetworkDatabases.Any())
            {
                // Add an error to the model
                ModelState.AddModelError(string.Empty, "The database type can't be changed while the database has networks.");
                // Redisplay the page.
                return Page();
            }
            // Get the database type.
            var databaseType = _context.DatabaseTypes.FirstOrDefault(item => item.Id == Input.DatabaseTypeId);
            // Check if no database type has been found or if the database type is generic.
            if (databaseType == null || databaseType.Name == "Generic")
            {
                // Add an error to the model
                ModelState.AddModelError(string.Empty, "No non-generic database type could be found with the provided ID.");
                // Redisplay the page.
                return Page();
            }
            // Mark the item for updating.
            _context.Databases.Update(View.Database);
            // Update the data.
            View.Database.Name = Input.Name;
            View.Database.Description = Input.Description;
            View.Database.Url = Input.Url;
            View.Database.IsPublic = Input.IsPublic;
            View.Database.DatabaseTypeId = databaseType.Id;
            View.Database.DatabaseType = databaseType;
            // Save the changes to the database.
            await _context.SaveChangesAsync();
            // Display a message.
            TempData["StatusMessage"] = "Success: 1 database type updated successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Administration/Data/Databases/Index");
        }
    }
}
