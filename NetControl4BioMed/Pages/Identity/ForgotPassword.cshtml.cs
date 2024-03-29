using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.Identity
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly LinkGenerator _linkGenerator;
        private readonly IReCaptchaChecker _reCaptchaChecker;

        public ForgotPasswordModel(UserManager<User> userManager, IEmailSender emailSender, LinkGenerator linkGenerator, IReCaptchaChecker reCaptchaChecker)
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
            [DataType(DataType.EmailAddress)]
            [Required(ErrorMessage = "This field is required.")]
            public string Email { get; set; }

            public string ReCaptchaToken { get; set; }
        }

        public IActionResult OnGet()
        {
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
            // Get the user with the provided e-mail.
            var user = await _userManager.FindByEmailAsync(Input.Email);
            // Check if there wasn't any user found.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The provided e-mail address is not associated with any user.";
                // Redirect to the page.
                return RedirectToPage("/Index");
            }
            // Generate the password reset code for the user.
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            // Get the callback URL to be encoded in the e-mail.
            var callbackUrl = _linkGenerator.GetUriByPage(HttpContext, "/Identity/ResetPassword", handler: null, values: new { code = code });
            // Define a new view model for the e-mail.
            var emailViewModel = new EmailPasswordResetViewModel
            {
                Email = user.Email,
                Url = callbackUrl,
                ApplicationUrl = _linkGenerator.GetUriByPage(HttpContext, "/Index", handler: null, values: null)
            };
            // Send the password reset e-mail for the user.
            await _emailSender.SendPasswordResetEmailAsync(emailViewModel);
            // Display a message.
            TempData["StatusMessage"] = "Success: Please check your e-mail. A message containing instructions for resetting your password has been sent.";
            // Redirect to the home page.
            return RedirectToPage("/Index");
        }
    }
}
