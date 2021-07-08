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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.CreatedData.Networks
{
    [RequestFormLimits(ValueLengthLimit = 16 * 1024 * 1024)]
    public class DefineModel : PageModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IReCaptchaChecker _reCaptchaChecker;

        public DefineModel(IServiceProvider serviceProvider, UserManager<User> userManager, ApplicationDbContext context, IReCaptchaChecker reCaptchaChecker)
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
            public string Name { get; set; }

            [DataType(DataType.MultilineText)]
            public string Description { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public bool IsPublic { get; set; }

            [DataType(DataType.MultilineText)]
            [Required(ErrorMessage = "This field is required.")]
            public string SeedInteractionData { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string ReCaptchaToken { get; set; }
        }

        public class ItemModel
        {
            public string SourceNode { get; set; }

            public string TargetNode { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string networkId)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if there was a network provided.
            if (!string.IsNullOrEmpty(networkId))
            {
                // Try to get the network with the provided ID.
                var networks = _context.Networks
                    .Where(item => item.IsPublic || (user != null && item.NetworkUsers.Any(item1 => item1.Email == user.Email)))
                    .Where(item => item.Id == networkId);
                // Check if there was an ID provided, but there was no network found.
                if (networks == null || !networks.Any())
                {
                    // Display a message.
                    TempData["StatusMessage"] = "Error: No network could be found with the provided ID, or you don't have access to it.";
                    // Redirect to the index page.
                    return RedirectToPage("/CreatedData/Networks/Index");
                }
                // Check if the network found has any databases.
                if (networks.Select(item => item.NetworkDatabases).SelectMany(item => item).Any())
                {
                    // Redirect to the create page.
                    return RedirectToPage("/CreatedData/Networks/Build", new { networkId = networkId });
                }
                // Define the input.
                Input = new InputModel
                {
                    Name = networks
                        .Select(item => item.Name)
                        .FirstOrDefault(),
                    Description = networks
                        .Select(item => item.Description)
                        .FirstOrDefault(),
                    IsPublic = user == null,
                    SeedInteractionData = JsonSerializer.Serialize(networks
                        .Select(item => item.NetworkInteractions)
                        .SelectMany(item => item)
                        .Select(item => item.Interaction)
                        .Select(item => new ItemModel
                        {
                            SourceNode = item.InteractionProteins
                                .Where(item1 => item1.Type == InteractionProteinType.Source)
                                .Select(item1 => item1.Protein.Name)
                                .FirstOrDefault(),
                            TargetNode = item.InteractionProteins
                                .Where(item1 => item1.Type == InteractionProteinType.Target)
                                .Select(item1 => item1.Protein.Name)
                                .FirstOrDefault()
                        })
                        .Where(item => !string.IsNullOrEmpty(item.SourceNode) && !string.IsNullOrEmpty(item.TargetNode)))
                };
                // Display a message.
                TempData["StatusMessage"] = "Success: The network has been loaded successfully.";
                // Return the page.
                return Page();
            }
            // Define the input.
            Input = new InputModel
            {
                IsPublic = user == null,
                SeedInteractionData = JsonSerializer.Serialize(Enumerable.Empty<ItemModel>())
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
            // Try to deserialize the seed data.
            if (!Input.SeedInteractionData.TryDeserializeJsonObject<List<ItemModel>>(out var items) || items == null)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The provided seed data could not be deserialized.");
                // Redisplay the page.
                return Page();
            }
            // Check if there weren't any items found.
            if (!items.Any())
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "No items could be found within the provided seed data.");
                // Redisplay the page.
                return Page();
            }
            // Serialize the seed data.
            var data = JsonSerializer.Serialize(items
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
            var task = new NetworksTask
            {
                Scheme = HttpContext.Request.Scheme,
                HostValue = HttpContext.Request.Host.Value,
                Items = new List<NetworkInputModel>
                {
                    new NetworkInputModel
                    {
                        Name = Input.Name,
                        Description = Input.Description,
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
                }
            };
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
                TempData["StatusMessage"] = $"Success: 1 network defined successfully and scheduled for generation.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Networks/Index");
            }
            // Display a message.
            TempData["StatusMessage"] = $"Success: 1 network defined successfully with the ID \"{ids.First()}\" and scheduled for generation.";
            // Redirect to the index page.
            return RedirectToPage("/CreatedData/Networks/Details/Index", new { id = ids.First() });
        }
    }
}
