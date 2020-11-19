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
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Tasks;
using NetControl4BioMed.Helpers.ViewModels;

namespace NetControl4BioMed.Pages.Content.Created.Analyses.Details.Accounts.Users
{
    public class AddModel : PageModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ISendGridEmailSender _emailSender;
        private readonly LinkGenerator _linkGenerator;
        private readonly IReCaptchaChecker _reCaptchaChecker;

        public AddModel(IServiceProvider serviceProvider, UserManager<User> userManager, ApplicationDbContext context, ISendGridEmailSender emailSender, LinkGenerator linkGenerator, IReCaptchaChecker reCaptchaChecker)
        {
            _serviceProvider = serviceProvider;
            _userManager = userManager;
            _context = context;
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
            public string Id { get; set; }

            [DataType(DataType.EmailAddress)]
            [Required(ErrorMessage = "This field is required.")]
            public string Email { get; set; }

            public string ReCaptchaToken { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public Analysis Analysis { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Get the items with the provided ID.
            var items = _context.Analyses
                .Where(item => item.IsPublic || item.AnalysisUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == id);
            // Check if there were no items found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Analysis = items
                    .First()
            };
            // Check if the user does not exist.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: You need to be logged in to add a user to the analysis.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Details/Accounts/Users/Index", new { id = View.Analysis.Id });
            }
            // Define the input.
            Input = new InputModel
            {
                Id = View.Analysis.Id
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if there isn't any ID provided.
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Get the items with the provided ID.
            var items = _context.Analyses
                .Where(item => item.IsPublic || item.AnalysisUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == id);
            // Check if there were no items found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Analysis = items
                    .Include(item => item.AnalysisUsers)
                        .ThenInclude(item => item.User)
                    .Include(item => item.AnalysisUserInvitations)
                    .First()
            };
            // Check if the user does not exist.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: You need to be logged in to add a user to the analysis.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Details/Accounts/Users/Index", new { id = View.Analysis.Id });
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
            // Check if the provided e-mail address already has access to the network.
            if (View.Analysis.AnalysisUsers.Any(item => item.User.Email == Input.Email) || View.Analysis.AnalysisUserInvitations.Any(item => item.Email == Input.Email))
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The user with the provided e-mail already has access to the network.");
                // Return the page.
                return Page();
            }
            // Try to get the user with the provided e-mail address.
            var userToAdd = _context.Users
                .FirstOrDefault(item => item.Email == Input.Email);
            // Check if any user has been found.
            if (userToAdd != null)
            {
                // Define a new task.
                var task = new AnalysisUsersTask
                {
                    Items = new List<AnalysisUserInputModel>
                    {
                        new AnalysisUserInputModel
                        {
                            Analysis = new AnalysisInputModel
                            {
                                Id = View.Analysis.Id
                            },
                            User = new UserInputModel
                            {
                                Id = userToAdd.Id
                            }
                        }
                    }
                };
                // Try to run the task.
                try
                {
                    // Run the task.
                    await task.CreateAsync(_serviceProvider, CancellationToken.None);
                }
                catch (Exception exception)
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, exception.Message);
                    // Redisplay the page.
                    return Page();
                }
            }
            else
            {
                // Define a new task.
                var task = new AnalysisUserInvitationsTask
                {
                    Items = new List<AnalysisUserInvitationInputModel>
                    {
                        new AnalysisUserInvitationInputModel
                        {
                            Analysis = new AnalysisInputModel
                            {
                                Id = View.Analysis.Id
                            },
                            Email = Input.Email
                        }
                    }
                };
                // Try to run the task.
                try
                {
                    // Run the task.
                    await task.CreateAsync(_serviceProvider, CancellationToken.None);
                }
                catch (Exception exception)
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, exception.Message);
                    // Redisplay the page.
                    return Page();
                }
            }
            // Define the view model for the e-mail.
            var emailAddedToAnalysisViewModel = new EmailAddedToAnalysisViewModel
            {
                Email = user.Email,
                Name = View.Analysis.Name,
                Url = _linkGenerator.GetUriByPage(HttpContext, "/Content/Created/Analyses/Details/Index", handler: null, values: new { id = View.Analysis.Id }),
                AddedEmail = Input.Email,
                ApplicationUrl = _linkGenerator.GetUriByPage(HttpContext, "/Index", handler: null, values: null)
            };
            // Send the defined e-mails.
            await _emailSender.SendAddedToAnalysisEmailAsync(emailAddedToAnalysisViewModel);
            // Define the view model for the e-mail.
            var emailWasAddedToAnalysisViewModel = new EmailWasAddedToAnalysisViewModel
            {
                Email = Input.Email,
                Name = View.Analysis.Name,
                Url = _linkGenerator.GetUriByPage(HttpContext, "/Content/Created/Analyses/Details/Index", handler: null, values: new { id = View.Analysis.Id }),
                AddedByEmail = user.Email,
                ApplicationUrl = _linkGenerator.GetUriByPage(HttpContext, "/Index", handler: null, values: null)
            };
            // Send the defined e-mails.
            await _emailSender.SendWasAddedToAnalysisEmailAsync(emailWasAddedToAnalysisViewModel);
            // Display a message to the user.
            TempData["StatusMessage"] = "Success: 1 user added successfully to the analysis.";
            // Redirect to the users page.
            return RedirectToPage("/Content/Created/Analyses/Details/Accounts/Users/Index", new { id = View.Analysis.Id });
        }
    }
}
