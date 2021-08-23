using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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
using EnumerationProteinCollectionType = NetControl4BioMed.Data.Enumerations.ProteinCollectionType;

namespace NetControl4BioMed.Pages.CreatedData.Networks
{
    [RequestFormLimits(ValueLengthLimit = 16 * 1024 * 1024)]
    public class BuildModel : PageModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IReCaptchaChecker _reCaptchaChecker;

        public BuildModel(IServiceProvider serviceProvider, UserManager<User> userManager, ApplicationDbContext context, IReCaptchaChecker reCaptchaChecker)
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

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string Algorithm { get; set; }

            [DataType(DataType.MultilineText)]
            [Required(ErrorMessage = "This field is required.")]
            public string InteractionDatabaseData { get; set; }

            public bool UseSeedProteinData { get; set; }

            [DataType(DataType.MultilineText)]
            [Required(ErrorMessage = "This field is required.")]
            public string SeedProteinData { get; set; }

            public bool UseSeedProteinCollectionData { get; set; }

            [DataType(DataType.MultilineText)]
            [Required(ErrorMessage = "This field is required.")]
            public string SeedProteinCollectionData { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string ReCaptchaToken { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string networkId)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Get the required query.
            var proteinDatabases = _context.Databases
                .Where(item => item.IsPublic || (user != null && item.DatabaseUsers.Any(item1 => item1.Email == user.Email)))
                .Where(item => item.DatabaseProteins.Any());
            // Check if there aren't any protein databases available.
            if (proteinDatabases == null || !proteinDatabases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A new network can't be created, as there are no protein databases available.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Networks/Index");
            }
            // Get the required query.
            var interactionDatabases = _context.Databases
                .Where(item => item.IsPublic || (user != null && item.DatabaseUsers.Any(item1 => item1.Email == user.Email)))
                .Where(item => item.DatabaseInteractions.Any());
            // Check if there aren't any interaction databases available.
            if (interactionDatabases == null || !interactionDatabases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A new network can't be created, as there are no interaction databases available.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Networks/Index");
            }
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
                // Check if the network found does not have any databases.
                if (!networks.Select(item => item.NetworkDatabases).SelectMany(item => item).Any())
                {
                    // Redirect to the upload page.
                    return RedirectToPage("/CreatedData/Networks/Define", new { networkId = networkId });
                }
                // Get the related data.
                var interactionDatabaseIds = _context.NetworkDatabases
                    .Where(item => item.Type == NetworkDatabaseType.Interaction && item.Network.Id == networkId)
                    .Where(item => item.Database.IsPublic || (user != null && item.Database.DatabaseUsers.Any(item1 => item1.Email == user.Email)))
                    .Where(item => item.Database.DatabaseInteractions.Any())
                    .Select(item => item.Database.Id)
                    .AsNoTracking()
                    .ToList();
                var seedProteinNames = _context.NetworkProteins
                    .Where(item => item.Type == NetworkProteinType.Seed && item.Network.Id == networkId)
                    .Select(item => item.Protein.Name)
                    .AsNoTracking()
                    .ToList();
                var seedProteinCollectionIds = _context.NetworkProteinCollections
                    .Where(item => item.Type == NetworkProteinCollectionType.Seed && item.Network.Id == networkId)
                    .Select(item => item.ProteinCollection.Id)
                    .AsNoTracking()
                    .ToList();
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
                    InteractionDatabaseData = JsonSerializer.Serialize(interactionDatabaseIds),
                    UseSeedProteinData = seedProteinNames != null && seedProteinNames.Any(),
                    SeedProteinData = JsonSerializer.Serialize(seedProteinNames),
                    UseSeedProteinCollectionData = seedProteinCollectionIds != null && seedProteinCollectionIds.Any(),
                    SeedProteinCollectionData = JsonSerializer.Serialize(seedProteinCollectionIds)
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
                UseSeedProteinData = false,
                SeedProteinData = JsonSerializer.Serialize(Enumerable.Empty<string>()),
                UseSeedProteinCollectionData = true,
                SeedProteinCollectionData = JsonSerializer.Serialize(Enumerable.Empty<string>())
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Get the required query.
            var proteinDatabases = _context.Databases
                .Where(item => item.IsPublic || (user != null && item.DatabaseUsers.Any(item1 => item1.Email == user.Email)))
                .Where(item => item.DatabaseProteins.Any());
            // Check if there aren't any protein databases available.
            if (proteinDatabases == null || !proteinDatabases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A new network can't be created, as there are no protein databases available.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Networks/Index");
            }
            // Get the required query.
            var interactionDatabases = _context.Databases
                .Where(item => item.IsPublic || (user != null && item.DatabaseUsers.Any(item1 => item1.Email == user.Email)))
                .Where(item => item.DatabaseInteractions.Any());
            // Check if there aren't any interaction databases available.
            if (interactionDatabases == null || !interactionDatabases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A new network can't be created, as there are no interaction databases available.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Networks/Index");
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
            // Check if no seed data providing method was selected.
            if (!Input.UseSeedProteinData && !Input.UseSeedProteinCollectionData)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "At least one method for providing seed proteins needs to be selected.");
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
            catch (Exception exception)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, exception.Message);
                // Redisplay the page.
                return Page();
            }
            // Try to deserialize the interaction database data.
            if (!Input.InteractionDatabaseData.TryDeserializeJsonObject<List<string>>(out var interactionDatabaseIds) || interactionDatabaseIds == null)
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
                ModelState.AddModelError(string.Empty, "At least one interaction database must be provided.");
                // Redisplay the page.
                return Page();
            }
            // Keep only the valid IDs.
            interactionDatabaseIds = _context.Databases
                .Where(item => item.IsPublic || (user != null && item.DatabaseUsers.Any(item1 => item1.Email == user.Email)))
                .Where(item => item.DatabaseInteractions.Any())
                .Where(item => interactionDatabaseIds.Contains(item.Id))
                .Select(item => item.Id)
                .ToList();
            // Check if there weren't any interaction databases found.
            if (interactionDatabaseIds == null || !interactionDatabaseIds.Any())
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "No interaction databases could be found with the provided ID(s).");
                // Redisplay the page.
                return Page();
            }
            // Define the related data.
            var seedProteinIdentifiers = new List<string>();
            var seedProteinCollectionIds = new List<string>();
            // Check if seed proteins should be used.
            if (Input.UseSeedProteinData)
            {
                // Try to deserialize the seed data.
                if (!Input.SeedProteinData.TryDeserializeJsonObject<List<string>>(out seedProteinIdentifiers) || seedProteinIdentifiers == null)
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "The provided seed data could not be deserialized.");
                    // Redisplay the page.
                    return Page();
                }
            }
            // Check if seed protein collection data should be used.
            if (Input.UseSeedProteinCollectionData)
            {
                // Try to deserialize the seed protein collection data.
                if (!Input.SeedProteinCollectionData.TryDeserializeJsonObject<List<string>>(out seedProteinCollectionIds) || seedProteinCollectionIds == null)
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "The provided seed protein collection data could not be deserialized.");
                    // Redisplay the page.
                    return Page();
                }
                // Keep only the valid IDs.
                seedProteinCollectionIds = _context.ProteinCollections
                    .Where(item => item.ProteinCollectionTypes.Any(item1 => item1.Type == EnumerationProteinCollectionType.Seed))
                    .Where(item => seedProteinCollectionIds.Contains(item.Id))
                    .Select(item => item.Id)
                    .ToList();
            }
            // Check if there weren't any items found.
            if (!seedProteinIdentifiers.Any() && !seedProteinCollectionIds.Any())
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "No seed proteins could be found within the provided seed data or the selected seed protein collections.");
                // Redisplay the page.
                return Page();
            }
            // Get the related data.
            var proteinDatabaseIds = proteinDatabases
                .Select(item => item.Id)
                .ToList();
            // Serialize the seed data.
            var data = JsonSerializer.Serialize(seedProteinIdentifiers
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
                        NetworkDatabases = proteinDatabaseIds
                            .Select(item => new NetworkDatabaseInputModel
                            {
                                Database = new DatabaseInputModel
                                {
                                    Id = item
                                },
                                Type = "Protein"
                            }).Concat(interactionDatabaseIds
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
                        NetworkProteinCollections = seedProteinCollectionIds
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
                return RedirectToPage("/CreatedData/Networks/Index");
            }
            // Display a message.
            TempData["StatusMessage"] = $"Success: 1 network defined successfully with the ID \"{ids.First()}\" and scheduled for generation.";
            // Redirect to the index page.
            return RedirectToPage("/CreatedData/Networks/Details/Index", new { id = ids.First() });
        }

        public async Task<IActionResult> OnGetInteractionDatabasesAsync(DataTableParametersViewModel parameters)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Start with all of the items to which the user has access.
            var query = _context.Databases
                .Where(item => item.IsPublic || (user != null && item.DatabaseUsers.Any(item1 => item1.Email == user.Email)))
                .Where(item => item.DatabaseInteractions.Any());
            // Get the total count.
            var totalCount = query.Count();
            // Check if there is search applied.
            if (parameters.Search != null && !string.IsNullOrEmpty(parameters.Search.Value))
            {
                // Select the results matching the search string.
                query = query
                    .Where(item => item.Id.Contains(parameters.Search.Value) || item.Name.Contains(parameters.Search.Value));
            }
            // Check if there is sorting applied.
            if (parameters.Order != null && parameters.Order.Any())
            {
                // Get the sorting column and direction.
                var column = parameters.Columns.ElementAtOrDefault(parameters.Order.FirstOrDefault()?.Column ?? -1)?.Name;
                var direction = parameters.Order.FirstOrDefault()?.Direction;
                // Switch based on the ordering parameters.
                switch ((column, direction))
                {
                    case var sort when sort == ("IsSelected", "Ascending"):
                        query = query.OrderBy(item => parameters.SelectedItems.Contains(item.Id));
                        break;
                    case var sort when sort == ("IsSelected", "Descending"):
                        query = query.OrderByDescending(item => parameters.SelectedItems.Contains(item.Id));
                        break;
                    case var sort when sort == ("Name", "Ascending"):
                        query = query.OrderBy(item => item.Name);
                        break;
                    case var sort when sort == ("Name", "Descending"):
                        query = query.OrderByDescending(item => item.Name);
                        break;
                    default:
                        break;
                }
            }
            // Get the filtered count.
            var filteredCount = query.Count();
            // Take only the results on the current page.
            query = query
                .Skip(parameters.Start)
                .Take(parameters.Length)
                .AsNoTracking();
            // Return the JSON response.
            return new JsonResult(new DataTableResponseViewModel
            {
                Data = query
                    .Select(item => new List<string>
                    {
                        item.Id,
                        item.Name,
                        parameters.SelectedItems.Contains(item.Id).ToString()
                    })
                    .ToList(),
                Draw = parameters.Draw,
                RecordsFiltered = filteredCount,
                RecordsTotal = totalCount
            });
        }

        public IActionResult OnGetSeedProteinCollections(DataTableParametersViewModel parameters)
        {
            // Start with all of the items to which the user has access.
            var query = _context.ProteinCollections
                .Where(item => item.ProteinCollectionTypes.Any(item1 => item1.Type == EnumerationProteinCollectionType.Seed));
            // Get the total count.
            var totalCount = query.Count();
            // Check if there is search applied.
            if (parameters.Search != null && !string.IsNullOrEmpty(parameters.Search.Value))
            {
                // Select the results matching the search string.
                query = query
                    .Where(item => item.Id.Contains(parameters.Search.Value) || item.Name.Contains(parameters.Search.Value));
            }
            // Check if there is sorting applied.
            if (parameters.Order != null && parameters.Order.Any())
            {
                // Get the sorting column and direction.
                var column = parameters.Columns.ElementAtOrDefault(parameters.Order.FirstOrDefault()?.Column ?? -1)?.Name;
                var direction = parameters.Order.FirstOrDefault()?.Direction;
                // Switch based on the ordering parameters.
                switch ((column, direction))
                {
                    case var sort when sort == ("IsSelected", "Ascending"):
                        query = query.OrderBy(item => parameters.SelectedItems.Contains(item.Id));
                        break;
                    case var sort when sort == ("IsSelected", "Descending"):
                        query = query.OrderByDescending(item => parameters.SelectedItems.Contains(item.Id));
                        break;
                    case var sort when sort == ("Name", "Ascending"):
                        query = query.OrderBy(item => item.Name);
                        break;
                    case var sort when sort == ("Name", "Descending"):
                        query = query.OrderByDescending(item => item.Name);
                        break;
                    default:
                        break;
                }
            }
            // Get the filtered count.
            var filteredCount = query.Count();
            // Take only the results on the current page.
            query = query
                .Skip(parameters.Start)
                .Take(parameters.Length)
                .AsNoTracking();
            // Return the JSON response.
            return new JsonResult(new DataTableResponseViewModel
            {
                Data = query
                    .Select(item => new List<string>
                    {
                        item.Id,
                        item.Name,
                        parameters.SelectedItems.Contains(item.Id).ToString()
                    })
                    .ToList(),
                Draw = parameters.Draw,
                RecordsFiltered = filteredCount,
                RecordsTotal = totalCount
            });
        }
    }
}
