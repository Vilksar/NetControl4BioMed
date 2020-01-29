using System;
using System.Collections.Generic;
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
            var personalDataProps = typeof(User).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
            // Iterate over the personal data properties.
            foreach (var property in personalDataProps)
            {
                // Add to the personal data dictionary the name and value (if it exists).
                personalData.Add(property.Name, property.GetValue(user)?.ToString() ?? "null");
            }
            // Add to the response header the information about the attachment and the file to be downloaded.
            Response.Headers.Add("Content-Disposition", "attachment; filename=PersonalData.json");
            // Return a new JSON file from the personal data dictionary.
            return File(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(personalData, new JsonSerializerOptions { WriteIndented = true })), MediaTypeNames.Application.Octet, "PersonalData.json");
        }
    }
}
