using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.ViewModels;

namespace NetControl4BioMed.Pages.Identity
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ISendGridEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        private readonly LinkGenerator _linkGenerator;
        private readonly IReCaptchaChecker _reCaptchaChecker;

        public RegisterModel(UserManager<User> userManager, ISendGridEmailSender emailSender, ApplicationDbContext context, LinkGenerator linkGenerator, IReCaptchaChecker reCaptchaChecker)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _context = context;
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

            [DataType(DataType.Password)]
            [Required(ErrorMessage = "This field is required.")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Required(ErrorMessage = "This field is required.")]
            [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public string ReCaptchaToken { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public string ReturnUrl { get; set; }
        }

        public IActionResult OnGet(string returnUrl = null)
        {
            // Define the variables for the view.
            View = new ViewModel
            {
                ReturnUrl = returnUrl ?? Url.Content("~/")
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // Define the variables for the view.
            View = new ViewModel
            {
                ReturnUrl = returnUrl ?? Url.Content("~/")
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
            // Define the new user.
            var user = new User
            {
                UserName = Input.Email,
                Email = Input.Email,
                DateTimeCreated = DateTime.Now
            };
            // Try to create the user with the given details.
            var result = await _userManager.CreateAsync(user, Input.Password);
            // Check if the user creation has failed.
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
            // Generate an e-mail confirmation code.
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            // Create the callback URL to be encoded in the confirmation email.
            var callbackUrl = _linkGenerator.GetUriByPage(HttpContext, "/Identity/ConfirmEmail", handler: null, values: new { userId = user.Id, code = code });
            var encodedUrl = HtmlEncoder.Default.Encode(callbackUrl);
            // Define a new view model for the e-mail.
            var emailViewModel = new EmailEmailConfirmationViewModel
            {
                Email = user.Email,
                Url = encodedUrl,
                ApplicationUrl = _linkGenerator.GetUriByPage(HttpContext, "/Index", handler: null, values: null)
            };
            // Send the confirmation e-mail for the user.
            await _emailSender.SendEmailConfirmationEmailAsync(emailViewModel);
            // Get all the databases, networks and analyses to which the user already has access.
            var databaseUserInvitations = _context.DatabaseUserInvitations.Where(item => item.Email == user.Email);
            var networkUserInvitations = _context.NetworkUserInvitations.Where(item => item.Email == user.Email);
            var analysisUserInvitations = _context.AnalysisUserInvitations.Where(item => item.Email == user.Email);
            // Create, for each, a corresponding user entry.
            var databaseUsers = databaseUserInvitations.Select(item => new DatabaseUser { DatabaseId = item.DatabaseId, Database = item.Database, UserId = user.Id, User = user, DateTimeCreated = item.DateTimeCreated });
            var networkUsers = networkUserInvitations.Select(item => new NetworkUser { NetworkId = item.NetworkId, Network = item.Network, UserId = user.Id, User = user, DateTimeCreated = item.DateTimeCreated });
            var analysisUsers = analysisUserInvitations.Select(item => new AnalysisUser { AnalysisId = item.AnalysisId, Analysis = item.Analysis, UserId = user.Id, User = user, DateTimeCreated = item.DateTimeCreated });
            // Mark the new items for addition.
            _context.DatabaseUsers.AddRange(databaseUsers);
            _context.NetworkUsers.AddRange(networkUsers);
            _context.AnalysisUsers.AddRange(analysisUsers);
            // Mark the old items for deletion.
            _context.DatabaseUserInvitations.RemoveRange(databaseUserInvitations);
            _context.NetworkUserInvitations.RemoveRange(networkUserInvitations);
            _context.AnalysisUserInvitations.RemoveRange(analysisUserInvitations);
            // Try to save the changes in the database.
            try
            {
                // Save the changes in the database.
                await _context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, exception.Message);
                // And re-display the page.
                return Page();
            }
            // Display a message to the user.
            TempData["StatusMessage"] = "Success: The account has been created successfully. Please check the provided e-mail address for instructions on confirming your e-mail, in order to log in.";
            // Redirect to the return URL.
            return LocalRedirect(View.ReturnUrl);
        }
    }
}
