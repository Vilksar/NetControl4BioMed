using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Tasks;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.AvailableData.Created.Networks.Create
{
    [RequestFormLimits(ValueLengthLimit = 16 * 1024 * 1024)]
    public class UploadModel : PageModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IReCaptchaChecker _reCaptchaChecker;

        public UploadModel(IServiceProvider serviceProvider, UserManager<User> userManager, ApplicationDbContext context, IReCaptchaChecker reCaptchaChecker)
        {
            _serviceProvider = serviceProvider;
            _userManager = userManager;
            _context = context;
            _reCaptchaChecker = reCaptchaChecker;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            [RegularExpression("cyjs", ErrorMessage = "The value is not valid.")]
            public string Type { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public bool IsPublic { get; set; }

            [DataType(DataType.MultilineText)]
            [Required(ErrorMessage = "This field is required.")]
            public string Data { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string ReCaptchaToken { get; set; }
        }

        public class ItemModel
        {
            public string SourceNode { get; set; }

            public string TargetNode { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Define the JSON serializer options.
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                IgnoreNullValues = true
            };
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Define the input.
            Input = new InputModel
            {
                IsPublic = user == null,
                Data = JsonSerializer.Serialize(new { }, jsonSerializerOptions)
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if the reCaptcha is valid.
            if (!await _reCaptchaChecker.IsValid(Input.ReCaptchaToken))
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The reCaptcha verification failed.");
                // Return the page.
                return Page();
            }
            // Check if the provided model isn't valid.
            if (!ModelState.IsValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error has been encountered. Please check again the input fields.");
                // Redisplay the page.
                return Page();
            }
            // Check if the public availability isn't valid.
            if (user == null && !Input.IsPublic)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "You are not logged in, so the network must be set as public.");
                // Redisplay the page.
                return Page();
            }
            // Define the task to create.
            var task = new NetworksTask
            {
                Scheme = HttpContext.Request.Scheme,
                HostValue = HttpContext.Request.Host.Value
            };
            // Check the type of the file that was provided.
            if (Input.Type == "cyjs")
            {
                // Try to deserialize the data.
                if (!Input.Data.TryDeserializeJsonObject<CytoscapeViewModel>(out var viewModel) || viewModel == null)
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "The provided file does not have the required format.");
                    // Redisplay the page.
                    return Page();
                }
                // Get the protein dictionary from the data.
                var proteinDictionary = viewModel.Elements.Nodes.ToDictionary(item => item.Data.Id, item => item.Data.Name);
                // Get the interactions from the data.
                var interactions = viewModel.Elements.Edges.Select(item => new ItemModel
                {
                    SourceNode = proteinDictionary.GetValueOrDefault(item.Data.Source),
                    TargetNode = proteinDictionary.GetValueOrDefault(item.Data.Target)
                })
                    .Where(item => !string.IsNullOrEmpty(item.SourceNode) && !string.IsNullOrEmpty(item.TargetNode));
                // Check if there were no interactions found within the data.
                if (interactions == null || !interactions.Any())
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "The provided file does not contain any valid interactions.");
                    // Redisplay the page.
                    return Page();
                }
                // Serialize the seed data.
                var data = JsonSerializer.Serialize(interactions
                    .Select(item => new NetworkInteractionInputModel
                    {
                        Interaction = new InteractionInputModel
                        {
                            InteractionProteins = new List<InteractionProteinInputModel>
                            {
                                new InteractionProteinInputModel
                                {
                                    Protein = new ProteinInputModel
                                    {
                                        Id = item.SourceNode
                                    },
                                    Type = "Source"
                                },
                                new InteractionProteinInputModel
                                {
                                    Protein = new ProteinInputModel
                                    {
                                        Id = item.TargetNode
                                    },
                                    Type = "Target"
                                }
                            }
                        }
                    }));
                // Define a new task.
                task.Items = new List<NetworkInputModel>
                {
                    new NetworkInputModel
                    {
                        Name = viewModel.Data.Name,
                        Description = viewModel.Data.Description,
                        IsPublic = Input.IsPublic,
                        Algorithm = NetworkAlgorithm.None.ToString(),
                        Data = data,
                        NetworkUsers = user != null ?
                            new List<NetworkUserInputModel>
                            {
                                new NetworkUserInputModel
                                {
                                    User = new UserInputModel
                                    {
                                        Id = user.Id
                                    },
                                    Email = user.Email
                                }
                            } :
                            new List<NetworkUserInputModel>()
                    }
                };
            }
            // Check if the type is not valid.
            else
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The provided file type is not valid.");
                // Redisplay the page.
                return Page();
            }
            // Define the IDs of the created items.
            var ids = Enumerable.Empty<string>();
            // Try to run the task.
            try
            {
                // Run the task.
                ids = await task.CreateAsync(_serviceProvider, CancellationToken.None);
            }
            catch (Exception exception)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, exception.Message);
                // Redisplay the page.
                return Page();
            }
            // Check if there wasn't any ID returned.
            if (ids == null || !ids.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = $"Success: 1 network uploaded successfully and scheduled for generation.";
                // Redirect to the index page.
                return RedirectToPage("/AvailableData/Created/Networks/Index");
            }
            // Display a message.
            TempData["StatusMessage"] = $"Success: 1 network uploaded successfully with the ID \"{ids.First()}\" and scheduled for generation.";
            // Redirect to the index page.
            return RedirectToPage("/AvailableData/Created/Networks/Details/Index", new { id = ids.First() });
        }
    }
}
