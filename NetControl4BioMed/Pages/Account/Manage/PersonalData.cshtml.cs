using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Pages.Account.Manage
{
    [Authorize]
    public class PersonalDataModel : PageModel
    {
        private readonly UserManager<User> _userManager;

        public PersonalDataModel(UserManager<User> userManager)
        {
            _userManager = userManager;
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
                // Redirect to the home page.
                return RedirectToPage("/Index");
            }
            // Define the personal data dictionary and include all of the defined properties.
            var personalData = new Dictionary<string, string>();
            // Iterate over the personal data properties.
            foreach (var property in user.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute))))
            {
                // Add to the personal data dictionary the name and value (if it exists).
                personalData.Add(property.Name, property.GetValue(user)?.ToString());
            }
            // Define a new memory stream.
            var stream = new MemoryStream();
            // Serialize the personal data to the stream.
            await JsonSerializer.SerializeAsync(stream, personalData, new JsonSerializerOptions { WriteIndented = true });
            // Reset the stream position.
            stream.Position = 0;
            // Return a new JSON file.
            return new FileStreamResult(stream, MediaTypeNames.Application.Json) { FileDownloadName = $"PersonalData.json" };
        }
    }
}
