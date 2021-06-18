using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.Account.Manage.PersonalData
{
    [Authorize]
    public class DownloadModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IReCaptchaChecker _reCaptchaChecker;

        public DownloadModel(UserManager<User> userManager, IReCaptchaChecker reCaptchaChecker)
        {
            _userManager = userManager;
            _reCaptchaChecker = reCaptchaChecker;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public string ReCaptchaToken { get; set; }
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
            // Return the streamed file.
            return new FileCallbackResult(MediaTypeNames.Application.Json, async (stream, _) =>
            {
                // Define the personal data dictionary and include all of the defined properties.
                var personalData = new Dictionary<string, string>();
                // Iterate over the personal data properties.
                foreach (var property in user.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute))))
                {
                    // Add to the personal data dictionary the name and value (if it exists).
                    personalData.Add(property.Name, property.GetValue(user)?.ToString());
                }
                // Serialize the personal data to the stream.
                await JsonSerializer.SerializeAsync(stream, personalData, new JsonSerializerOptions { WriteIndented = true });
            })
            {
                FileDownloadName = $"PersonalData.json"
            };
        }
    }
}
