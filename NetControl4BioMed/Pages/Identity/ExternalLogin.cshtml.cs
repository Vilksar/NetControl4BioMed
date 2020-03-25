using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
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
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ISendGridEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        private readonly LinkGenerator _linkGenerator;

        public ExternalLoginModel(SignInManager<User> signInManager, UserManager<User> userManager, ISendGridEmailSender emailSender, ApplicationDbContext context, LinkGenerator linkGenerator)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
            _context = context;
            _linkGenerator = linkGenerator;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.EmailAddress)]
            [Required(ErrorMessage = "This field is required.")]
            public string Email { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public string LoginProvider { get; set; }

            public string ReturnUrl { get; set; }
        }

        public IActionResult OnGet()
        {
            // Redirect to the login page. All "get" requests for external login should come from the external providers.
            return RedirectToPage("/Identity/Login");
        }

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Define the variables to return to the view.
            View = new ViewModel
            {
                LoginProvider = provider,
                ReturnUrl = returnUrl ?? Url.Content("~/")
            };
            // Check if there is any external provider given.
            if (string.IsNullOrEmpty(provider))
            {
                // Display an error.
                TempData["StatusMessage"] = "Error: A valid external provider is needed in order log in.";
                // Redirect to the home page.
                return RedirectToPage("/Identity/Login");
            }
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("/Identity/ExternalLogin", "Callback", new { returnUrl = View.ReturnUrl });
            // Get the current properties of the external authentication configuration.
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            // Apply the new authentication schema.
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            // Check if there was any error with the external provider.
            if (!string.IsNullOrEmpty(remoteError))
            {
                // Display an error.
                TempData["StatusMessage"] = $"Error: There was an error with the external provider: {remoteError}.";
                // Redirect to the login page.
                return RedirectToPage("/Identity/Login");
            }
            // Get the information provided by the external authentication for the current user.
            var info = await _signInManager.GetExternalLoginInfoAsync();
            // Check if there wasn't any information received.
            if (info == null)
            {
                // Display an error.
                TempData["StatusMessage"] = "Error: There was an error loading information from the external provider.";
                // Redirect to the login page.
                return RedirectToPage("/Identity/Login");
            }
            // Define the variables for the view.
            View = new ViewModel
            {
                LoginProvider = info.LoginProvider,
                ReturnUrl = returnUrl ?? Url.Content("~/")
            };
            // Get the ID of the user trying to log in.
            var userId = info.Principal != null ? info.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value : string.Empty;
            // Check if there wasn't any user ID found.
            if (string.IsNullOrEmpty(userId))
            {
                // Display an error.
                TempData["StatusMessage"] = "Error: There was an error loading information from the external provider.";
                // Redirect to the login page.
                return RedirectToPage("/Identity/Login");
            }
            // Get the user trying to log in.
            var user = await _userManager.FindByIdAsync(userId);
            // Check if any user has been found.
            if (user == null)
            {
                // Display an error.
                TempData["StatusMessage"] = "Error: There was an error loading information from the external provider.";
                // Redirect to the login page.
                return RedirectToPage("/Identity/Login");
            }
            // Try to sign in the user with the external login provider information. It will work only if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false, true);
            // Check if the login was successful.
            if (result.Succeeded)
            {
                // Redirect to the return URL.
                return LocalRedirect(View.ReturnUrl);
            }
            // Check if the account is locked out.
            if (result.IsLockedOut)
            {
                // Display an error.
                TempData["StatusMessage"] = "Error: This account has been locked out. Please try again later.";
                // Redirect to the home page.
                return RedirectToPage("/Identity/Login");
            }
            // Check if the user is not allowed to sign in because the e-mail is not confirmed.
            if (result.IsNotAllowed && !user.EmailConfirmed)
            {
                // Generate an e-mail confirmation code.
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                // Create the callback URL to be encoded in the confirmation email.
                var callbackUrl = _linkGenerator.GetUriByPage(HttpContext, "/Identity/ConfirmEmail", handler: null, values: new { userId = user.Id, code = code });
                // Define a new view model for the e-mail.
                var emailViewModel = new EmailEmailConfirmationViewModel
                {
                    Email = user.Email,
                    Url = callbackUrl,
                    ApplicationUrl = _linkGenerator.GetUriByPage(HttpContext, "/Index", handler: null, values: null)
                };
                // Send the confirmation e-mail for the user.
                await _emailSender.SendEmailConfirmationEmailAsync(emailViewModel);
                // Display an error.
                TempData["StatusMessage"] = "Error: You are not allowed to log in because your e-mail address is not yet confirmed. A new e-mail containing instructions on how to confirm it has been sent to the specified e-mail address.";
                // Redirect to the login page.
                return RedirectToPage("/Identity/Login");
            }
            // If the user does not have an account, then ask to create one. Retrieve the e-mail from the external provider, if it exists.
            Input = new InputModel
            {
                Email = info.Principal.HasClaim(item => item.Type == ClaimTypes.Email) ? info.Principal.FindFirstValue(ClaimTypes.Email) : string.Empty
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            // Get the information provided by the external authentication for the current user.
            var info = await _signInManager.GetExternalLoginInfoAsync();
            // Check if there wasn't any information received.
            if (info == null)
            {
                // Display an error.
                TempData["StatusMessage"] = "Error: There was an error loading information from the external provider.";
                // Redirect to the login page.
                return RedirectToPage("/Identity/Login");
            }
            // Define the variables for the view.
            View = new ViewModel
            {
                LoginProvider = info.LoginProvider,
                ReturnUrl = returnUrl ?? Url.Content("~/")
            };
            // Check if the provided model is not valid.
            if (!ModelState.IsValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error was encountered. Please check again the input fields.");
                // Return the page.
                return Page();
            }
            // Define a new user with the provided e-mail.
            var user = new User
            {
                UserName = Input.Email,
                Email = Input.Email,
                DateTimeCreated = DateTime.Now
            };
            // Try to create the new user.
            var result = await _userManager.CreateAsync(user);
            // Check if the creation was not successful.
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
            // Add external login to the user.
            result = await _userManager.AddLoginAsync(user, info);
            // Check if the adding was not successful.
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
            // Define a new view model for the e-mail.
            var emailViewModel = new EmailEmailConfirmationViewModel
            {
                Email = user.Email,
                Url = callbackUrl,
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
            // Save the changes in the database.
            await _context.SaveChangesAsync();
            // Sign in the user.
            await _signInManager.SignInAsync(user, false);
            // Display a message to the user.
            TempData["StatusMessage"] = $"Success: The account has been created successfully. Please check the e-mail address associated with your {View.LoginProvider} account for instructions on confirming your e-mail.";
            // Redirect to the return URL.
            return LocalRedirect(View.ReturnUrl);
        }
    }
}
