using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Tasks;
using EnumerationProteinCollectionType = NetControl4BioMed.Data.Enumerations.ProteinCollectionType;

namespace NetControl4BioMed.Pages.AvailableData.Created.Networks
{
    [RequestFormLimits(ValueLengthLimit = 16 * 1024 * 1024)]
    public class CreateModel : PageModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IReCaptchaChecker _reCaptchaChecker;

        public CreateModel(IServiceProvider serviceProvider, UserManager<User> userManager, ApplicationDbContext context, IConfiguration configuration, IReCaptchaChecker reCaptchaChecker)
        {
            _serviceProvider = serviceProvider;
            _userManager = userManager;
            _context = context;
            _configuration = configuration;
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

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string Algorithm { get; set; }

            [DataType(DataType.MultilineText)]
            [Required(ErrorMessage = "This field is required.")]
            public string InteractionDatabaseData { get; set; }

            [DataType(DataType.MultilineText)]
            [Required(ErrorMessage = "This field is required.")]
            public string SeedProteinData { get; set; }

            [DataType(DataType.MultilineText)]
            [Required(ErrorMessage = "This field is required.")]
            public string SeedProteinCollectionData { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string ReCaptchaToken { get; set; }
        }
        public ViewModel View { get; set; }

        public class ViewModel
        {
            public IEnumerable<Database> ProteinDatabases { get; set; }

            public IEnumerable<Database> InteractionDatabases { get; set; }

            public IEnumerable<ProteinCollection> SeedProteinCollections { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string networkId = null, bool loadDemonstration = false)
        {
            // Check if the demonstration should be loaded.
            if (loadDemonstration)
            {
                // Check if there are no demonstration items configured.
                if (string.IsNullOrEmpty(_configuration["Data:Demonstration:NetworkId"]))
                {
                    // Try to get a demonstration control path.
                    var controlPath = _context.ControlPaths
                        .Include(item => item.Analysis)
                            .ThenInclude(item => item.Network)
                        .Where(item => item.Analysis.IsPublic && item.Analysis.IsDemonstration && item.Analysis.Network.IsPublic && item.Analysis.Network.IsDemonstration)
                        .AsNoTracking()
                        .FirstOrDefault();
                    // Check if there was no demonstration control path found.
                    if (controlPath == null || controlPath.Analysis == null || controlPath.Analysis.Network == null)
                    {
                        // Display a message.
                        TempData["StatusMessage"] = "Error: There are no demonstration networks available.";
                        // Redirect to the index page.
                        return RedirectToPage("/AvailableData/Created/Networks/Index");
                    }
                    // Update the demonstration item IDs.
                    _configuration["Data:Demonstration:NetworkId"] = controlPath.Analysis.Network.Id;
                    _configuration["Data:Demonstration:AnalysisId"] = controlPath.Analysis.Id;
                    _configuration["Data:Demonstration:ControlPathId"] = controlPath.Id;
                }
                // Get the ID of the configured demonstration item.
                networkId = _configuration["Data:Demonstration:NetworkId"];
            }
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Define the view.
            View = new ViewModel
            {
                ProteinDatabases = _context.Databases
                    .Where(item => item.IsPublic || item.DatabaseUsers.Any(item1 => item1.Email == user.Email))
                    .Where(item => item.DatabaseProteins.Any()),
                InteractionDatabases = _context.Databases
                    .Where(item => item.IsPublic || item.DatabaseUsers.Any(item1 => item1.Email == user.Email))
                    .Where(item => item.DatabaseInteractions.Any()),
                SeedProteinCollections = _context.ProteinCollections
                    .Where(item => item.ProteinCollectionTypes.Any(item1 => item1.Type == EnumerationProteinCollectionType.Seed))
            };
            // Check if there weren't any protein databases available.
            if (View.ProteinDatabases == null || !View.ProteinDatabases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A new network can't be created, as there are no protein databases available.";
                // Redirect to the index page.
                return RedirectToPage("/AvailableData/Created/Networks/Index");
            }
            // Check if there weren't any interaction databases available.
            if (View.InteractionDatabases == null || !View.InteractionDatabases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A new network can't be created, as there are no interaction databases available.";
                // Redirect to the index page.
                return RedirectToPage("/AvailableData/Created/Networks/Index");
            }
            // Check if there was a network provided.
            if (!string.IsNullOrEmpty(networkId))
            {
                // Try to get the network with the provided ID.
                var networks = _context.Networks
                    .Where(item => item.IsPublic || item.NetworkUsers.Any(item1 => item1.Email == user.Email))
                    .Where(item => item.Id == networkId);
                // Check if there was an ID provided, but there was no network found.
                if (networks == null || !networks.Any())
                {
                    // Display a message.
                    TempData["StatusMessage"] = "Error: No network could be found with the provided ID, or you don't have access to it.";
                    // Redirect to the index page.
                    return RedirectToPage("/AvailableData/Created/Networks/Index");
                }
                // Check if the network found does not have any databases.
                if (!networks.Select(item => item.NetworkDatabases).SelectMany(item => item).Any())
                {
                    // Redirect to the upload page.
                    return RedirectToPage("/AvailableData/Created/Networks/Upload", new { networkId = networkId });
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
                    Algorithm = networks
                        .Select(item => item.Algorithm)
                        .FirstOrDefault()
                        .ToString(),
                    InteractionDatabaseData = JsonSerializer.Serialize(networks
                        .Select(item => item.NetworkDatabases)
                        .SelectMany(item => item)
                        .Select(item => item.Database)
                        .Intersect(View.InteractionDatabases)
                        .Select(item => item.Id)),
                    SeedProteinData = JsonSerializer.Serialize(networks
                        .Select(item => item.NetworkProteins)
                        .SelectMany(item => item)
                        .Where(item => item.Type == NetworkProteinType.Seed)
                        .Select(item => item.Protein.Name)),
                    SeedProteinCollectionData = JsonSerializer.Serialize(networks
                        .Select(item => item.NetworkProteinCollections)
                        .SelectMany(item => item)
                        .Select(item => item.ProteinCollection)
                        .Intersect(View.SeedProteinCollections)
                        .Select(item => item.Id))
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
                Algorithm = null,
                InteractionDatabaseData = JsonSerializer.Serialize(Enumerable.Empty<string>()),
                SeedProteinData = JsonSerializer.Serialize(Enumerable.Empty<string>()),
                SeedProteinCollectionData = JsonSerializer.Serialize(Enumerable.Empty<string>())
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Define the view.
            View = new ViewModel
            {
                ProteinDatabases = _context.Databases
                    .Where(item => item.IsPublic || item.DatabaseUsers.Any(item1 => item1.Email == user.Email))
                    .Where(item => item.DatabaseProteins.Any()),
                InteractionDatabases = _context.Databases
                    .Where(item => item.IsPublic || item.DatabaseUsers.Any(item1 => item1.Email == user.Email))
                    .Where(item => item.DatabaseInteractions.Any()),
                SeedProteinCollections = _context.ProteinCollections
                    .Where(item => item.ProteinCollectionTypes.Any(item1 => item1.Type == EnumerationProteinCollectionType.Seed))
            };
            // Check if there weren't any protein databases available.
            if (View.ProteinDatabases == null || !View.ProteinDatabases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A new network can't be created, as there are no protein databases available.";
                // Redirect to the index page.
                return RedirectToPage("/AvailableData/Created/Networks/Index");
            }
            // Check if there weren't any interaction databases available.
            if (View.InteractionDatabases == null || !View.InteractionDatabases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A new network can't be created, as there are no interaction databases available.";
                // Redirect to the index page.
                return RedirectToPage("/AvailableData/Created/Networks/Index");
            }
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
            // Try to get the algorithm.
            try
            {
                // Get the algorithm.
                var algorithm = EnumerationExtensions.GetEnumerationValue<NetworkAlgorithm>(Input.Algorithm);
                // Check if the algorithm is not valid.
                if (algorithm == NetworkAlgorithm.None)
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "The network generation algorithm is not valid.");
                    // Redisplay the page.
                    return Page();
                }
            }
            catch (Exception)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, $"The network generation algorithm couldn't be determined from the provided string.");
                // Redisplay the page.
                return Page();
            }
            // Try to deserialize the interaction database data.
            if (!Input.InteractionDatabaseData.TryDeserializeJsonObject<IEnumerable<string>>(out var interactionDatabaseIds) || interactionDatabaseIds == null)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The provided interaction database data could not be deserialized.");
                // Redisplay the page.
                return Page();
            }
            // Check if there weren't any interaction database IDs provided.
            if (!interactionDatabaseIds.Any())
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "At least one interaction database ID must be provided.");
                // Redisplay the page.
                return Page();
            }
            // Try to get the interaction databases with the provided IDs.
            var interactionDatabases = View.InteractionDatabases
                .Where(item => interactionDatabaseIds.Contains(item.Id));
            // Check if there weren't any interaction databases found.
            if (interactionDatabases == null || !interactionDatabases.Any())
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "No interaction databases could be found with the provided ID(s).");
                // Redisplay the page.
                return Page();
            }
            // Try to deserialize the seed protein collection data.
            if (!Input.SeedProteinCollectionData.TryDeserializeJsonObject<IEnumerable<string>>(out var seedProteinCollectionIds) || seedProteinCollectionIds == null)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The provided seed protein collection data could not be deserialized.");
                // Redisplay the page.
                return Page();
            }
            // Try to get the seed protein collections with the provided IDs.
            var seedProteinCollections = View.SeedProteinCollections
                .Where(item => seedProteinCollectionIds.Contains(item.Id));
            // Try to deserialize the seed data.
            if (!Input.SeedProteinData.TryDeserializeJsonObject<IEnumerable<string>>(out var items) || items == null)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The provided seed data could not be deserialized.");
                // Redisplay the page.
                return Page();
            }
            // Check if there weren't any items found.
            if (!items.Any() && !seedProteinCollections.Any())
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "No items could be found within the provided seed data or the selected seed protein collections.");
                // Redisplay the page.
                return Page();
            }
            // Serialize the seed data.
            var data = JsonSerializer.Serialize(items
                .Select(item => new NetworkProteinInputModel
                {
                    Protein = new ProteinInputModel
                    {
                        Id = item
                    },
                    Type = "Seed"
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
                        Algorithm = Input.Algorithm,
                        Data = data,
                        NetworkDatabases = View.ProteinDatabases
                            .Select(item => item.Id)
                            .Select(item => new NetworkDatabaseInputModel
                            {
                                Database = new DatabaseInputModel
                                {
                                    Id = item
                                },
                                Type = "Protein"
                            }).Concat(interactionDatabases
                                .Select(item => item.Id)
                                .Select(item => new NetworkDatabaseInputModel
                                {
                                    Database = new DatabaseInputModel
                                    {
                                        Id = item
                                    },
                                    Type = "Interaction"
                                })),
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
                            new List<NetworkUserInputModel>(),
                        NetworkProteinCollections = seedProteinCollections
                            .Select(item => item.Id)
                            .Select(item => new NetworkProteinCollectionInputModel
                            {
                                ProteinCollection = new ProteinCollectionInputModel
                                {
                                    Id = item
                                },
                                Type = "Seed"
                            })
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
                return RedirectToPage("/AvailableData/Created/Networks/Index");
            }
            // Display a message.
            TempData["StatusMessage"] = $"Success: 1 network defined successfully with the ID \"{ids.First()}\" and scheduled for generation.";
            // Redirect to the index page.
            return RedirectToPage("/AvailableData/Created/Networks/Details/Index", new { id = ids.First() });
        }
    }
}
