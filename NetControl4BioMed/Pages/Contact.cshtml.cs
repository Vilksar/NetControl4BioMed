using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.ViewModels;

namespace NetControl4BioMed.Pages
{
    [AllowAnonymous]
    public class ContactModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ISendGridEmailSender _emailSender;
        private readonly LinkGenerator _linkGenerator;
        private readonly IReCaptchaChecker _reCaptchaChecker;

        public ContactModel(UserManager<User> userManager, ISendGridEmailSender emailSender, LinkGenerator linkGenerator, IReCaptchaChecker reCaptchaChecker)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _linkGenerator = linkGenerator;
            _reCaptchaChecker = reCaptchaChecker;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string Name { get; set; }

            [DataType(DataType.EmailAddress)]
            [Required(ErrorMessage = "This field is required.")]
            public string Email { get; set; }

            [DataType(DataType.MultilineText)]
            [Required(ErrorMessage = "This field is required.")]
            public string Message { get; set; }

            public string ReCaptchaToken { get; set; }
        }

        public async Task<IActionResult> OnGet(string message = null)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Define the input.
            Input = new InputModel
            {
                Name = Uri.UnescapeDataString(string.Empty),
                Email = Uri.UnescapeDataString(user?.Email ?? string.Empty),
                Message = Uri.UnescapeDataString(message ?? string.Empty)
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
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
            // Define a new view model for the e-mail.
            var emailViewModel = new EmailContactViewModel
            {
                Name = Input.Name,
                Email = Input.Email,
                Message = Input.Message,
                ApplicationUrl = _linkGenerator.GetUriByPage(HttpContext, "/Index", handler: null, values: null)
            };
            // Send an e-mail to the predefined administrator address.
            await _emailSender.SendContactEmailAsync(emailViewModel);
            // Display a message to the user.
            TempData["StatusMessage"] = "Success: The message was sent successfully.";
            // Redirect to page.
            return RedirectToPage();
        }
    }
}
